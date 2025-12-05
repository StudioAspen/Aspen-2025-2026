using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Timers;
using UnityEngine;

// There are 3 conditions for working gamepad support in Nice Vibrations:
//
// 1. NICE_VIBRATIONS_INPUTSYSTEM_INSTALLED - The input system package needs to be installed.
//    See https://docs.unity3d.com/Packages/com.unity.inputsystem@1.0/manual/Installation.html#installing-the-package
//    This is set by Nice Vibrations' assembly definition file, using a version define.
//    See https://docs.unity3d.com/Manual/ScriptCompilationAssemblyDefinitionFiles.html#define-symbols
//    about version defines, and see Lofelt.NiceVibrations.asmdef for the usage in Nice Vibrations.
//
// 2. ENABLE_INPUT_SYSTEM - The input system needs to be enabled in the project settings.
//    See https://docs.unity3d.com/Packages/com.unity.inputsystem@1.0/manual/Installation.html#enabling-the-new-input-backends
//    This define is set by Unity, see https://docs.unity3d.com/Manual/PlatformDependentCompilation.html
//
// 3. NICE_VIBRATIONS_DISABLE_GAMEPAD_SUPPORT - This is a user-defined define which needs to be not set.
//    NICE_VIBRATIONS_DISABLE_GAMEPAD_SUPPORT is not set by default. It can be set by a user in the
//    player settings to disable gamepad support completely. One reason to do this is to reduce the
//    size of a HapticClip asset, as setting this define changes to HapticImporter to not add the
//    GamepadRumble to the HapticClip. Changing this define requires re-importing all .haptic clip
//    assets to update HapticClip's GamepadRumble.
//
// If any of the 3 conditions is not met, GamepadRumbler doesn't contain any calls into
// UnityEngine.InputSystem, and CanPlay() always returns false.
#if ((!UNITY_ANDROID && !UNITY_IOS) || UNITY_EDITOR) && NICE_VIBRATIONS_INPUTSYSTEM_INSTALLED && ENABLE_INPUT_SYSTEM && !NICE_VIBRATIONS_DISABLE_GAMEPAD_SUPPORT
using UnityEngine.InputSystem;
#endif

namespace Lofelt.NiceVibrations
{
    /// <summary>
    /// Contains a vibration pattern to make a gamepad rumble.
    /// </summary>
    ///
    /// GamepadRumble contains the information on when to set what motor speeds on a gamepad
    /// to make it rumble with a specific pattern.
    ///
    /// GamepadRumble has three arrays of the same length representing the rumble pattern. The
    /// entries for each array index describe for how long to turn on the gamepad's vibration
    /// motors, at what speed.
    [Serializable]
    public struct GamepadRumble
    {
        /// <summary>
        /// The duration, in milliseconds, that the motors will be turned on at the speed set
        /// in \ref lowFrequencyMotorSpeeds and \ref highFrequencyMotorSpeeds at the same array
        /// index
        /// </summary>
        [SerializeField]
        public int[] durationsMs;

        /// <summary>
        /// The total duration of the GamepadRumble, in milliseconds
        /// </summary>
        [SerializeField]
        public int totalDurationMs;

        /// <summary>
        /// The motor speeds of the low frequency motor
        /// </summary>
        [SerializeField]
        public float[] lowFrequencyMotorSpeeds;

        /// <summary>
        /// The motor speeds of the high frequency motor
        /// </summary>
        [SerializeField]
        public float[] highFrequencyMotorSpeeds;

        /// <summary>
        /// Checks if the GamepadRumble is valid and also not empty
        /// </summary>
        /// <returns>Whether the GamepadRumble is valid</returns>
        public bool IsValid()
        {
            return durationsMs != null &&
                   lowFrequencyMotorSpeeds != null &&
                   highFrequencyMotorSpeeds != null &&
                   durationsMs.Length == lowFrequencyMotorSpeeds.Length &&
                   durationsMs.Length == highFrequencyMotorSpeeds.Length &&
                   durationsMs.Length > 0;
        }
    }

    /// <summary>
    /// Vibrates a gamepad based on a GamepadRumble rumble pattern.
    /// </summary>
    ///
    /// GamepadRumbler can load and play back a GamepadRumble pattern on the current
    /// gamepad.
    ///
    /// This is a low-level class that normally doesn't need to be used directly. Instead,
    /// you can use HapticSource and HapticController to play back haptic clips, as those
    /// classes support gamepads by using GamepadRumbler internally.
    public static class GamepadRumbler
    {
#if ((!UNITY_ANDROID && !UNITY_IOS) || UNITY_EDITOR) && NICE_VIBRATIONS_INPUTSYSTEM_INSTALLED && ENABLE_INPUT_SYSTEM && !NICE_VIBRATIONS_DISABLE_GAMEPAD_SUPPORT
        // Per-gamepad state tracking
        private class GamepadState
        {
            public GamepadRumble loadedRumble;
            public bool rumbleLoaded = false;
            // This Timer is used to wait until it is time to advance to the next entry in loadedRumble.
            // When the Timer is elapsed, ProcessNextRumble() is called to set new motor speeds to the
            // gamepad.
            public Timer rumbleTimer;
            // The index of the entry of loadedRumble that is currently being played back
            public int rumbleIndex = -1;
            // The total duration of rumble entries that have been played back so far
            public long rumblePositionMs = 0;
            // Keeps track of how much time elapsed since playback was started
            public Stopwatch playbackWatch = new Stopwatch();
            /// <summary>
            /// A multiplication factor applied to the motor speeds of the low frequency motor.
            /// </summary>
            ///
            /// The multiplication factor is applied to the low frequency motor speed of every
            /// GamepadRumble entry before playing it.
            ///
            /// In other words, this applies a gain (for factors greater than 1.0) or an attenuation
            /// (for factors less than 1.0) to the clip. If the resulting speed of an entry is
            /// greater than 1.0, it is clipped to 1.0. The speed is clipped hard, no limiter is
            /// used.
            ///
            /// The motor speed multiplication is reset when calling Load(), so Load() needs to be
            /// called first before setting the multiplication.
            ///
            /// A change of the multiplication is applied to a currently playing rumble, but only
            /// for the next rumble entry, not the one currently playing.
            public float lowFrequencyMotorSpeedMultiplication = 1.0f;
            /// <summary>
            /// Same as \ref lowFrequencyMotorSpeedMultiplication, but for the high frequency speed
            /// motor.
            public float highFrequencyMotorSpeedMultiplication = 1.0f;
            public int gamepadID;

            public GamepadState(int id)
            {
                gamepadID = id;
                rumbleTimer = new Timer();
            }
        }

        static Dictionary<int, GamepadState> gamepadStates = new Dictionary<int, GamepadState>();
        static int currentGamepadID = -1;

        /// <summary>
        /// Gets the currently selected gamepad ID.
        /// </summary>
        /// <returns>The current gamepad ID, or -1 if none is set</returns>
        public static int GetCurrentGamepadID()
        {
            return currentGamepadID;
        }
#endif

        /// <summary>
        /// Initializes the GamepadRumbler.
        /// </summary>
        ///
        /// This needs to be called from the main thread, which is the reason why this is a method
        /// instead of a static constructor: Sometimes Unity calls static constructors from a
        /// different thread, and an explicit Init() method gives us more control over this.
        public static void Init()
        {
#if ((!UNITY_ANDROID && !UNITY_IOS) || UNITY_EDITOR) && NICE_VIBRATIONS_INPUTSYSTEM_INSTALLED && ENABLE_INPUT_SYSTEM && !NICE_VIBRATIONS_DISABLE_GAMEPAD_SUPPORT
            var syncContext = System.Threading.SynchronizationContext.Current;
            // Initialization is now handled per-gamepad when state is created
#endif
        }

        /// <summary>
        /// Checks whether a call to Play() would trigger playback on a gamepad.
        /// </summary>
        ///
        /// Playing back a rumble pattern with Play() only works if a gamepad is connected and if
        /// a GamepadRumble has been loaded with Load() before.
        ///
        /// <returns>Whether a vibration can be triggered on a gamepad</returns>
        public static bool CanPlay()
        {
#if ((!UNITY_ANDROID && !UNITY_IOS) || UNITY_EDITOR) && NICE_VIBRATIONS_INPUTSYSTEM_INSTALLED && ENABLE_INPUT_SYSTEM && !NICE_VIBRATIONS_DISABLE_GAMEPAD_SUPPORT
            return CanPlay(currentGamepadID);
#else
            return false;
#endif
        }

        public static bool CanPlay(int gamepadID)
        {
#if ((!UNITY_ANDROID && !UNITY_IOS) || UNITY_EDITOR) && NICE_VIBRATIONS_INPUTSYSTEM_INSTALLED && ENABLE_INPUT_SYSTEM && !NICE_VIBRATIONS_DISABLE_GAMEPAD_SUPPORT
            if (!gamepadStates.ContainsKey(gamepadID))
                return false;
            
            var state = gamepadStates[gamepadID];
            return IsConnected(gamepadID) && state.rumbleLoaded && state.loadedRumble.IsValid();
#else
            return false;
#endif
        }

#if ((!UNITY_ANDROID && !UNITY_IOS) || UNITY_EDITOR) && NICE_VIBRATIONS_INPUTSYSTEM_INSTALLED && ENABLE_INPUT_SYSTEM && !NICE_VIBRATIONS_DISABLE_GAMEPAD_SUPPORT
        /// <summary>
        /// Gets the Gamepad object corresponding to the specified gamepad ID.
        /// </summary>
        ///
        /// If the specified ID is out of range of the connected gamepad(s),
        /// <c>InputSystem.Gamepad.current</c> will be returned.
        ///
        /// <param name="gamepadID">The ID of the gamepad to be returned.</c> </param>
        /// <returns> A <c> InputSystem.Gamepad</c> </returns>
        static UnityEngine.InputSystem.Gamepad GetGamepad(int gamepadID)
        {
            if (gamepadID >= 0)
            {
                if (gamepadID >= UnityEngine.InputSystem.Gamepad.all.Count)
                {
                    return UnityEngine.InputSystem.Gamepad.current;
                }
                else
                {
                    return UnityEngine.InputSystem.Gamepad.all[gamepadID];
                }
            }
            return UnityEngine.InputSystem.Gamepad.current;
        }

        static GamepadState GetOrCreateState(int gamepadID)
        {
            if (!gamepadStates.ContainsKey(gamepadID))
            {
                var state = new GamepadState(gamepadID);
                var syncContext = System.Threading.SynchronizationContext.Current;
                state.rumbleTimer.Elapsed += (object obj, System.Timers.ElapsedEventArgs args) =>
                {
                    syncContext.Post(_ =>
                    {
                        ProcessNextRumble(gamepadID);
                    }, null);
                };
                gamepadStates[gamepadID] = state;
            }
            return gamepadStates[gamepadID];
        }
#endif

        /// <summary>
        /// Set the current gamepad for haptics playback by ID.
        /// </summary>
        ///
        /// This method needs be called before haptics playback, e.g. \ref HapticController.Play(),
        /// \ref HapticPatterns.PlayEmphasis(), \ref HapticPatterns.PlayConstant(), etc, for
        /// for the gamepad to be properly selected.
        ///
        /// If this method isn't called, haptics will be played on <c>InputSystem.Gamepad.current</c>
        ///
        /// For example, if you have 3 controllers connected, you have to choose between values 0, 1,
        /// and 2.
        ///
        /// If the gamepad ID value doesn't match any connected gamepad, calling
        /// this method has no effect.
        /// <param name="gamepadID">The ID of the gamepad</param>
        public static void SetCurrentGamepad(int gamepadID)
        {
#if ((!UNITY_ANDROID && !UNITY_IOS) || UNITY_EDITOR) && NICE_VIBRATIONS_INPUTSYSTEM_INSTALLED && ENABLE_INPUT_SYSTEM && !NICE_VIBRATIONS_DISABLE_GAMEPAD_SUPPORT
            if (gamepadID < UnityEngine.InputSystem.Gamepad.all.Count)
            {
                currentGamepadID = gamepadID;
            }
#endif
        }

        /// <summary>
        /// Checks whether a gamepad is connected and recognized by Unity's input system.
        /// </summary>
        ///
        /// If the input system package is not installed or not enabled, the gamepad is not
        /// recognized and treated as not connected here.
        ///
        /// If the <c>NICE_VIBRATIONS_DISABLE_GAMEPAD_SUPPORT</c> define is set in the player settings,
        /// this function pretends no gamepad is connected.
        ///
        /// <returns>Whether a gamepad is connected</returns>
        public static bool IsConnected()
        {
#if ((!UNITY_ANDROID && !UNITY_IOS) || UNITY_EDITOR) && NICE_VIBRATIONS_INPUTSYSTEM_INSTALLED && ENABLE_INPUT_SYSTEM && !NICE_VIBRATIONS_DISABLE_GAMEPAD_SUPPORT
            return IsConnected(currentGamepadID);
#else
            return false;
#endif
        }

        public static bool IsConnected(int gamepadID)
        {
#if ((!UNITY_ANDROID && !UNITY_IOS) || UNITY_EDITOR) && NICE_VIBRATIONS_INPUTSYSTEM_INSTALLED && ENABLE_INPUT_SYSTEM && !NICE_VIBRATIONS_DISABLE_GAMEPAD_SUPPORT
            return GetGamepad(gamepadID) != null;
#else
            return false;
#endif
        }

        /// <summary>
        /// Loads a rumble pattern for later playback.
        /// </summary>
        ///
        /// <param name="rumble">The rumble pattern to load</param>
        public static void Load(GamepadRumble rumble)
        {
#if ((!UNITY_ANDROID && !UNITY_IOS) || UNITY_EDITOR) && NICE_VIBRATIONS_INPUTSYSTEM_INSTALLED && ENABLE_INPUT_SYSTEM && !NICE_VIBRATIONS_DISABLE_GAMEPAD_SUPPORT
            Load(rumble, currentGamepadID);
#endif
        }

        public static void Load(GamepadRumble rumble, int gamepadID)
        {
#if ((!UNITY_ANDROID && !UNITY_IOS) || UNITY_EDITOR) && NICE_VIBRATIONS_INPUTSYSTEM_INSTALLED && ENABLE_INPUT_SYSTEM && !NICE_VIBRATIONS_DISABLE_GAMEPAD_SUPPORT
            var state = GetOrCreateState(gamepadID);
            
            if (rumble.IsValid())
            {
                state.loadedRumble = rumble;
                state.rumbleLoaded = true;
                state.lowFrequencyMotorSpeedMultiplication = 1.0f;
                state.highFrequencyMotorSpeedMultiplication = 1.0f;
            }
            else
            {
                Unload(gamepadID);
            }
#endif
        }

        /// <summary>
        /// Plays back the rumble pattern loaded previously with Load().
        /// </summary>
        ///
        /// If no rumble pattern has been loaded, or if no gamepad is connected, this method does
        /// nothing.
        public static void Play()
        {
#if ((!UNITY_ANDROID && !UNITY_IOS) || UNITY_EDITOR) && NICE_VIBRATIONS_INPUTSYSTEM_INSTALLED && ENABLE_INPUT_SYSTEM && !NICE_VIBRATIONS_DISABLE_GAMEPAD_SUPPORT
            Play(currentGamepadID);
#endif
        }

        public static void Play(int gamepadID)
        {
#if ((!UNITY_ANDROID && !UNITY_IOS) || UNITY_EDITOR) && NICE_VIBRATIONS_INPUTSYSTEM_INSTALLED && ENABLE_INPUT_SYSTEM && !NICE_VIBRATIONS_DISABLE_GAMEPAD_SUPPORT
            if (CanPlay(gamepadID))
            {
                var state = gamepadStates[gamepadID];
                state.rumbleIndex = 0;
                state.rumblePositionMs = 0;
                state.playbackWatch.Restart();
                ProcessNextRumble(gamepadID);
            }
#endif
        }

        /// <summary>
        /// Stops playback previously started with Play() by turning off the gamepad's motors.
        /// </summary>
        public static void Stop()
        {
#if ((!UNITY_ANDROID && !UNITY_IOS) || UNITY_EDITOR) && NICE_VIBRATIONS_INPUTSYSTEM_INSTALLED && ENABLE_INPUT_SYSTEM && !NICE_VIBRATIONS_DISABLE_GAMEPAD_SUPPORT
            Stop(currentGamepadID);
#endif
        }

        public static void Stop(int gamepadID)
        {
#if ((!UNITY_ANDROID && !UNITY_IOS) || UNITY_EDITOR) && NICE_VIBRATIONS_INPUTSYSTEM_INSTALLED && ENABLE_INPUT_SYSTEM && !NICE_VIBRATIONS_DISABLE_GAMEPAD_SUPPORT
            if (GetGamepad(gamepadID) != null)
            {
                GetGamepad(gamepadID).ResetHaptics();
            }
            
            if (gamepadStates.ContainsKey(gamepadID))
            {
                var state = gamepadStates[gamepadID];
                state.rumbleTimer.Enabled = false;
                state.rumbleIndex = -1;
                state.rumblePositionMs = 0;
                state.playbackWatch.Stop();
            }
#endif
        }

        public static void StopAll()
        {
#if ((!UNITY_ANDROID && !UNITY_IOS) || UNITY_EDITOR) && NICE_VIBRATIONS_INPUTSYSTEM_INSTALLED && ENABLE_INPUT_SYSTEM && !NICE_VIBRATIONS_DISABLE_GAMEPAD_SUPPORT
            foreach (var kvp in gamepadStates)
            {
                Stop(kvp.Key);
            }
#endif
        }

        /// <summary>
        /// Stops playback and unloads the currently loaded GamepadRumble from memory.
        /// </summary>
        public static void Unload()
        {
#if ((!UNITY_ANDROID && !UNITY_IOS) || UNITY_EDITOR) && NICE_VIBRATIONS_INPUTSYSTEM_INSTALLED && ENABLE_INPUT_SYSTEM && !NICE_VIBRATIONS_DISABLE_GAMEPAD_SUPPORT
            Unload(currentGamepadID);
#endif
        }

        public static void Unload(int gamepadID)
        {
#if ((!UNITY_ANDROID && !UNITY_IOS) || UNITY_EDITOR) && NICE_VIBRATIONS_INPUTSYSTEM_INSTALLED && ENABLE_INPUT_SYSTEM && !NICE_VIBRATIONS_DISABLE_GAMEPAD_SUPPORT
            if (gamepadStates.ContainsKey(gamepadID))
            {
                var state = gamepadStates[gamepadID];
                state.loadedRumble.highFrequencyMotorSpeeds = null;
                state.loadedRumble.lowFrequencyMotorSpeeds = null;
                state.loadedRumble.durationsMs = null;
                state.rumbleLoaded = false;
                Stop(gamepadID);
            }
#endif
        }

        public static void SetMotorSpeedMultiplication(float lowFreq, float highFreq)
        {
#if ((!UNITY_ANDROID && !UNITY_IOS) || UNITY_EDITOR) && NICE_VIBRATIONS_INPUTSYSTEM_INSTALLED && ENABLE_INPUT_SYSTEM && !NICE_VIBRATIONS_DISABLE_GAMEPAD_SUPPORT
            SetMotorSpeedMultiplication(lowFreq, highFreq, currentGamepadID);
#endif
        }

        public static void SetMotorSpeedMultiplication(float lowFreq, float highFreq, int gamepadID)
        {
#if ((!UNITY_ANDROID && !UNITY_IOS) || UNITY_EDITOR) && NICE_VIBRATIONS_INPUTSYSTEM_INSTALLED && ENABLE_INPUT_SYSTEM && !NICE_VIBRATIONS_DISABLE_GAMEPAD_SUPPORT
            if (gamepadStates.ContainsKey(gamepadID))
            {
                var state = gamepadStates[gamepadID];
                state.lowFrequencyMotorSpeedMultiplication = lowFreq;
                state.highFrequencyMotorSpeedMultiplication = highFreq;
            }
#endif
        }

#if ((!UNITY_ANDROID && !UNITY_IOS) || UNITY_EDITOR) && NICE_VIBRATIONS_INPUTSYSTEM_INSTALLED && ENABLE_INPUT_SYSTEM && !NICE_VIBRATIONS_DISABLE_GAMEPAD_SUPPORT
        private static bool IncreaseRumbleIndex(int gamepadID)
        {
            if (!gamepadStates.ContainsKey(gamepadID))
                return false;

            var state = gamepadStates[gamepadID];
            state.rumblePositionMs += state.loadedRumble.durationsMs[state.rumbleIndex];
            state.rumbleIndex++;
            if (state.rumbleIndex == state.loadedRumble.durationsMs.Length)
            {
                Stop(gamepadID);
                return false;
            }

            return true;
        }

        private static void ProcessNextRumble(int gamepadID)
        {
            if (!gamepadStates.ContainsKey(gamepadID))
                return;

            var state = gamepadStates[gamepadID];

            if (state.rumbleIndex == -1)
            {
                return;
            }

            if (state.rumbleIndex == state.loadedRumble.durationsMs.Length)
            {
                Stop(gamepadID);
                return;
            }

            // Figure out for how long the current rumble entry should be played (durationToWait).
            // Due to the timer not waiting for exactly the same amount of time that we requested,
            // there can be a bit of error that we need to compensate for. For example, if the timer
            // waited for 3ms longer than we requested, we play the next rumble entry for a 3ms
            // less to compensate for that.
            // In fact, Unity triggers the timer only once per frame, so at 30 FPS, the timer
            // resolution is 32ms. That means that the timing error can be bigger than the duration
            // of the whole rumble entry, and to compensate for that, the entire rumble entry needs
            // to be skipped. That's what the loop does: It skips rumble entries to compensate for
            // timer error.
            UnityEngine.Debug.Assert(state.loadedRumble.IsValid());
            UnityEngine.Debug.Assert(state.rumbleLoaded);
            UnityEngine.Debug.Assert(state.rumbleIndex >= 0 && state.rumbleIndex <= state.loadedRumble.durationsMs.Length);

            long elapsed = state.playbackWatch.ElapsedMilliseconds;
            long durationToWait = 0;
            while (true)
            {
                long rumbleEntryDuration = state.loadedRumble.durationsMs[state.rumbleIndex];
                long error = elapsed - state.rumblePositionMs;
                durationToWait = rumbleEntryDuration - error;

                // If durationToWait is <= 0, the current rumble entry needs to be skipped to
                // compensate for timer error. Otherwise break and play the current rumble entry.
                if (durationToWait > 0)
                {
                    break;
                }

                // If the end of the rumble has been reached, return, as playback has stopped.
                if (!IncreaseRumbleIndex(gamepadID))
                {
                    return;
                }
            }

            float lowFrequencySpeed = state.loadedRumble.lowFrequencyMotorSpeeds[state.rumbleIndex] * 
                Mathf.Max(state.lowFrequencyMotorSpeedMultiplication, 0.0f);
            float highFrequencySpeed = state.loadedRumble.highFrequencyMotorSpeeds[state.rumbleIndex] * 
                Mathf.Max(state.highFrequencyMotorSpeedMultiplication, 0.0f);

            UnityEngine.InputSystem.Gamepad currentGamepad = GetGamepad(gamepadID);
            if (currentGamepad != null)
            {
                currentGamepad.SetMotorSpeeds(lowFrequencySpeed, highFrequencySpeed);
            }
            else
            {
                return;
            }

            // Set up the timer to call ProcessNextRumble() again with the next rumble entry, after
            // the duration of the current rumble entry.
            state.rumblePositionMs += state.loadedRumble.durationsMs[state.rumbleIndex];
            state.rumbleIndex++;
            state.rumbleTimer.Interval = durationToWait;
            state.rumbleTimer.AutoReset = false;
            state.rumbleTimer.Enabled = true;
        }
#endif
    }
}