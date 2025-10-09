// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Animancer
{
    /// <summary>
    /// Bitwise flags used to determine which warnings Animancer should give.
    /// <para></para>
    /// <strong>These warnings are all optional</strong>.
    /// Feel free to disable any of them if you understand the <em>potential</em> issues they're referring to.
    /// </summary>
    /// 
    /// <remarks>
    /// All warnings are enabled by default, but are compiled out of runtime builds (except development builds).
    /// <para></para>
    /// You can manually disable warnings using the <c>AnimancerSettings</c> asset
    /// or the Animancer Settings panel in the Animancer Tools Window (<c>Window/Animation/Animancer Tools</c>).
    /// <para></para>
    /// <strong>Example:</strong>
    /// You can put a method like this in any class to disable whatever warnings you don't want on startup:
    /// <para></para><code>
    /// #if UNITY_ASSERTIONS
    /// [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.BeforeSceneLoad)]
    /// private static void DisableAnimancerWarnings()
    /// {
    ///     Animancer.OptionalWarning.ProOnly.Disable();
    ///     
    ///     // You could disable OptionalWarning.All, but that's not recommended for obvious reasons.
    /// }
    /// #endif
    /// </code></remarks>
    /// 
    /// https://kybernetik.com.au/animancer/api/Animancer/OptionalWarning
    /// 
    [Flags]
    public enum OptionalWarning
    {
        /// <summary><c>default</c></summary>
        None = 0,

        /// <summary>
        /// A <see href="https://kybernetik.com.au/animancer/docs/introduction/features">Pro-Only Feature</see>
        /// has been used in <see href="https://kybernetik.com.au/animancer/redirect/lite">Animancer Lite</see>.
        /// </summary>
        /// 
        /// <remarks>
        /// Some <see href="https://kybernetik.com.au/animancer/docs/introduction/features">Features</see>
        /// are only available in <see href="https://kybernetik.com.au/animancer/redirect/pro">Animancer Pro</see>.
        /// <para></para>
        /// <see href="https://kybernetik.com.au/animancer/redirect/lite">Animancer Lite</see>
        /// allows you to try out those features in the Unity Editor and gives this warning the
        /// first time each one is used to inform you that they will not work in runtime builds.
        /// </remarks>
        ProOnly = 1 << 0,

        /// <summary>
        /// An <see cref="AnimancerComponent.Graph"/> is being initialized
        /// during a type of GUI event that isn't supposed to cause side effects.
        /// </summary>
        /// 
        /// <remarks>
        /// <see cref="EventType.Layout"/> and <see cref="EventType.Repaint"/>
        /// should display the current details of things, but they should not modify things.
        /// </remarks>
        CreateGraphDuringGuiEvent = 1 << 1,

        /// <summary>
        /// The <see cref="AnimancerComponent.Animator"/> is disabled so Animancer won't be able to play animations.
        /// </summary>
        /// 
        /// <remarks>
        /// The <see cref="Animator"/> doesn't need an Animator Controller,
        /// it just needs to be enabled via the checkbox in the Inspector
        /// or by setting <c>animancerComponent.Animator.enabled = true;</c> in code.
        /// </remarks>
        AnimatorDisabled = 1 << 2,

        /// <summary>
        /// An <see cref="Animator.runtimeAnimatorController"/> is assigned
        /// but the Rig is Humanoid so it can't be blended with Animancer.
        /// </summary>
        /// 
        /// <remarks>
        /// <see href="https://kybernetik.com.au/animancer/docs/manual/animator-controllers#native">Native</see>
        /// Animator Controllers can blend with Animancer on Generic Rigs, but not on Humanoid Rigs
        /// (you can swap back and forth between the Animator Controller and Animancer,
        /// but it won't smoothly blend between them).
        /// <para></para>
        /// If you don't intend to blend between them, you can just disable this warning.
        /// </remarks>
        NativeControllerHumanoid = 1 << 3,

        /// <summary>
        /// An <see cref="Animator.runtimeAnimatorController"/> is being used at the same time as a
        /// <see cref="ControllerState"/> or <see cref="HybridAnimancerComponent"/>.
        /// </summary>
        /// 
        /// <remarks>
        /// It is possible to use both, but it usually only happens when misunderstanding how the system works.
        /// The differences are explained on the
        /// <see href="https://kybernetik.com.au/animancer/docs/manual/animator-controllers">
        /// Animator Controllers</see> page.
        /// If you do want both systems, just disable this warning.
        /// </remarks>
        NativeControllerState = 1 << 4,

        /// <summary>
        /// An <see href="https://kybernetik.com.au/animancer/docs/manual/events/end">End Event</see>
        /// didn't actually end the animation.
        /// </summary>
        /// 
        /// <remarks>
        /// Animancer doesn't automatically do anything during an End Event
        /// so it's up to you to end the animation (usually by playing something else).
        /// <para></para>
        /// This warning is given when the event isn't used to stop the animation that caused it
        /// (usually by playing something else). This often indicates that the event hasn't been
        /// configured correctly, however it is sometimes intentional such as if the event doesn't
        /// immediately stop the animation but sets a flag to indicate the animation has ended for
        /// another system to act on at a later time. In that case this warning should be disabled.
        /// <para></para>
        /// <see href="https://kybernetik.com.au/animancer/docs/manual/events/end">End Events</see>
        /// are triggered every frame after their time has passed, so in this case it might be
        /// necessary to clear the event or simply use a regular
        /// <see href="https://kybernetik.com.au/animancer/docs/manual/events/animancer">Animancer Event</see>.
        /// </remarks>
        EndEventInterrupt = 1 << 5,

        /// <summary>
        /// An <see href="https://kybernetik.com.au/animancer/docs/manual/events/animancer">Animancer Event</see>
        /// triggered by one character was used to play an animation on a different character.
        /// </summary>
        /// 
        /// <remarks>
        /// This most commonly happens when a Transition is shared by multiple characters and they
        /// all register their own callbacks to its events which leads to those events controlling
        /// the wrong character.
        /// </remarks>
        EventPlayMismatch = 1 << 6,

        /// <summary>
        /// An <see cref="AnimancerEvent"/> that does nothing was invoked.
        /// Most likely it was not configured correctly.
        /// </summary>
        /// 
        /// <remarks>
        /// Unused events should be removed to avoid wasting performance checking and invoking them.
        /// </remarks>
        UselessEvent = 1 << 7,

        /// <summary>
        /// <see href="https://kybernetik.com.au/animancer/docs/manual/events/animancer">Animancer Events</see>
        /// are being used on a state which does not properly support them so they might not work as intended.
        /// </summary>
        /// 
        /// <remarks>
        /// <see href="https://kybernetik.com.au/animancer/docs/manual/events/animancer">Animancer Events</see> on a
        /// <see cref="ControllerState"/> will be triggered based on its <see cref="AnimancerState.NormalizedTime"/>,
        /// which comes from the current state of its Animator Controller regardless of which state that may be.
        /// <para></para>
        /// If you intend for the event to be associated with a specific state inside the Animator Controller,
        /// you need to use Unity's regular
        /// <see href="https://kybernetik.com.au/animancer/docs/manual/events/animation">Animation Events</see>
        /// instead.
        /// <para></para>
        /// But if you intend the event to be triggered by any state inside the Animator Controller,
        /// then you can simply disable this warning.
        /// </remarks>
        UnsupportedEvents = 1 << 8,

        /// <summary>
        /// <see href="https://kybernetik.com.au/animancer/docs/manual/ik">Inverse Kinematics</see>
        /// cannot be dynamically enabled on some
        /// <see href="https://kybernetik.com.au/animancer/docs/manual/playing/states">State</see> types.
        /// </summary>
        /// 
        /// <remarks>
        /// To use IK on a <see cref="ControllerState"/>
        /// you must instead enable it on the desired layer inside the Animator Controller.
        /// <para></para>
        /// IK is not supported by <see cref="PlayableAssetState"/>.
        /// <para></para>
        /// Setting <see cref="AnimancerNode.ApplyAnimatorIK"/> on such a state will simply do nothing,
        /// so feel free to disable this warning if you are enabling IK on states without checking their type.
        /// </remarks>
        UnsupportedIK = 1 << 9,

        /// <summary>
        /// A Mixer State is being initialized with its <see cref="AnimancerNode.ChildCount"/> &lt;= 1.
        /// </summary>
        /// 
        /// <remarks>
        /// The purpose of a mixer is to mix multiple child states
        /// so you are probably initializing it with incorrect parameters.
        /// <para></para>
        /// A mixer with only one child will simply play that child,
        /// so feel free to disable this warning if that's what you intend to do.
        /// </remarks>
        MixerMinChildren = 1 << 10,

        /// <summary>
        /// A Mixer State is synchronizing a child with <see cref="AnimancerState.Length"/> = 0.
        /// </summary>
        /// 
        /// <remarks>
        /// Synchronization is based on the <see cref="AnimancerState.NormalizedTime"/>
        /// which can't be calculated if the <see cref="AnimancerState.Length"/> is 0.
        /// <para></para>
        /// Some state types can change their <see cref="AnimancerState.Length"/>,
        /// in which case you can just disable this warning.
        /// But otherwise, the indicated state should not be added to the synchronization list.
        /// </remarks>
        MixerSynchronizeZeroLength = 1 << 11,

        /// <summary>
        /// When a transition with a non-zero <see cref="ITransition.FadeDuration"/>
        /// creates a state, that state will log this warning if it's ever played
        /// without a fade duration.
        /// </summary>
        /// <remarks>
        /// This helps identify situations where a state is accidentally played directly
        /// where the transition should be played instead to allow it to apply its fade
        /// and any other details.
        /// </remarks>
        ExpectFade = 1 << 12,

        /// <summary>
        /// A <see href="https://kybernetik.com.au/animancer/docs/manual/blending/fading/custom">Custom Easing</see>
        /// is being started but its weight calculation does not go from 0 to 1.
        /// </summary>
        /// 
        /// <remarks>
        /// The <see cref="FadeGroup.Easing"/> method is expected to return 0 when the parameter is 0 and
        /// 1 when the parameter is 1. It can do anything you want with other values,
        /// but starting or ending at different values will likely lead to undesirable results.
        /// <para></para>
        /// If your <see cref="FadeGroup.Easing"/> method is expensive you could disable this warning to save
        /// some performance, but violating the above guidelines is not recommended.
        /// </remarks>
        FadeEasingBounds = 1 << 13,

        /// <summary>
        /// The <see cref="Animator.speed"/> property does not affect Animancer. 
        /// Use <see cref="AnimancerGraph.Speed"/> instead.
        /// </summary>
        /// 
        /// <remarks>
        /// The <see cref="Animator.speed"/> property only works with Animator Controllers but does not affect the
        /// Playables API so Animancer has its own <see cref="AnimancerGraph.Speed"/> property.
        /// </remarks>
        AnimatorSpeed = 1 << 14,

        /// <summary>
        /// An <see cref="AnimancerNodeBase.Graph"/> is null during finalization (garbage collection).
        /// </summary>
        /// 
        /// <remarks>
        /// This probably means that node was never used for anything and should not have been created.
        /// <para></para>
        /// This warning can be prevented for a specific node by calling <see cref="AnimancerNodeBase.MarkAsUsed"/>.
        /// <para></para>
        /// To minimise the performance cost of checking this warning, it does not capture the stack trace of the
        /// node's creation by default. However, you can enable <see cref="AnimancerNode.TraceConstructor"/> on startup
        /// so that it can include the stack trace in the warning message for any nodes that end up being unused.
        /// </remarks>
        UnusedNode = 1 << 15,

        /// <summary>
        /// An <see cref="AnimancerState.MainObject"/> doesn't match the <see cref="Transition{TState}.MainObject"/>
        /// of the transition being applied to it.
        /// </summary>
        /// <remarks>
        /// If you set the <see cref="Transition{TState}.MainObject"/> of a transition, you should also call
        /// <see cref="Transition{TState}.ReconcileMainObject"/> to create a new state for the new object.
        /// </remarks>
        MainObjectMismatch = 1 << 16,

        /// <summary>
        /// <see cref="PlayableAssetState.InitializeBindings"/> is trying to bind to the same <see cref="Animator"/>
        /// that is being used by Animancer.
        /// </summary>
        /// <remarks>
        /// Doing this will replace Animancer's output so its animations would not work anymore.
        /// </remarks>
        PlayableAssetAnimatorBinding = 1 << 17,

        /// <summary>
        /// <see cref="AnimancerLayer.GetOrCreateWeightlessState"/> is cloning a complex state such as a
        /// <see cref="ManualMixerState"/> or <see cref="ControllerState"/>.
        /// This has a larger performance cost than cloning a <see cref="ClipState"/> and these states
        /// generally have parameters that need to be controlled which may result in undesired behaviour
        /// if your scripts are only expecting to have one state to control.
        /// </summary>
        /// <remarks>
        /// The <see href="https://kybernetik.com.au/animancer/docs/manual/blending/fading/modes">Fade Modes</see>
        /// page explains why clones are created.
        /// </remarks>
        CloneComplexState = 1 << 18,

        /// <summary>
        /// Unity doesn't suppport dynamically creating animations for Animancer in runtime builds
        /// so this warning is given when attempting to use an animation which isn't saved as an
        /// asset to explain this limitation as early as possible.
        /// </summary>
        /// 
        /// <remarks>
        /// This warning should be disabled if you're loading animations from Asset Bundles or Addressables
        /// or if you only intend to use the animation in the Unity Editor.
        /// </remarks>
        DynamicAnimation = 1 << 19,

        /// <summary>
        /// <see cref="Animancer.StringReference"/>s are generally more efficient for comparisons
        /// than raw <see cref="string"/>s and are not interchangeable so references should be preferred.
        /// </summary>
        StringReference = 1 << 20,

        /// <summary>All warning types.</summary>
        All = ~0,
    }

    /// https://kybernetik.com.au/animancer/api/Animancer/Validate
    public static partial class Validate
    {
        /************************************************************************************************************************/

#if UNITY_ASSERTIONS
        /// <summary>[Assert-Only]
        /// The <see cref="OptionalWarning"/> flags that are currently disabled (default none).
        /// </summary>
        private static OptionalWarning _DisabledWarnings;
#endif

        /************************************************************************************************************************/

        /// <summary>[Animancer Extension] [Assert-Conditional]
        /// Disables the specified warning type. Supports bitwise combinations.
        /// </summary>
        /// <remarks>
        /// <strong>Example:</strong>
        /// You can put a method like this in any class to disable whatever warnings you don't want on startup:
        /// <para></para><code>
        /// #if UNITY_ASSERTIONS
        /// [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.BeforeSceneLoad)]
        /// private static void DisableAnimancerWarnings()
        /// {
        ///     Animancer.OptionalWarning.ProOnly.Disable();
        ///     
        ///     // You could disable OptionalWarning.All, but that's not recommended for obvious reasons.
        /// }
        /// #endif
        /// </code></remarks>
        [System.Diagnostics.Conditional(Strings.Assertions)]
        public static void Disable(this OptionalWarning type)
        {
#if UNITY_ASSERTIONS
            _DisabledWarnings |= type;
#endif
        }

        /************************************************************************************************************************/

        /// <summary>[Animancer Extension] [Assert-Conditional]
        /// Enables the specified warning type. Supports bitwise combinations.
        /// </summary>
        [System.Diagnostics.Conditional(Strings.Assertions)]
        public static void Enable(this OptionalWarning type)
        {
#if UNITY_ASSERTIONS
            _DisabledWarnings &= ~type;
#endif
        }

        /************************************************************************************************************************/

        /// <summary>[Animancer Extension] [Assert-Conditional]
        /// Enables or disables the specified warning type. Supports bitwise combinations.
        /// </summary>
        [System.Diagnostics.Conditional(Strings.Assertions)]
        public static void SetEnabled(this OptionalWarning type, bool enable)
        {
#if UNITY_ASSERTIONS
            if (enable)
                type.Enable();
            else
                type.Disable();
#endif
        }

        /************************************************************************************************************************/

        /// <summary>[Animancer Extension] [Assert-Conditional]
        /// Logs the `message` as a warning if the `type` is enabled.
        /// </summary>
        /// <remarks>Does nothing if the `message` is <c>null</c>.</remarks>
        [System.Diagnostics.Conditional(Strings.Assertions)]
        [HideInCallstack]
        public static void Log(this OptionalWarning type, string message, object context = null)
        {
#if UNITY_ASSERTIONS
            if (message == null || type.IsDisabled())
                return;

            Debug.LogWarning($"Possible Issue Detected: {message}" +
                $"\n\nThis warning can be disabled via '{Strings.AnimancerSettingsPath}'" +
                $" or by calling {nameof(Animancer)}.{nameof(OptionalWarning)}.{type}.{nameof(Disable)}()" +
                " and it will automatically be compiled out of Runtime Builds (except for Development Builds)." +
                $" More information can be found at {Strings.DocsURLs.OptionalWarning}\n",
                context as Object);
#endif
        }

        /************************************************************************************************************************/
#if UNITY_ASSERTIONS
        /************************************************************************************************************************/

        /// <summary>[Animancer Extension] [Assert-Only] Are none of the specified warning types disabled?</summary>
        public static bool IsEnabled(this OptionalWarning type) => (_DisabledWarnings & type) == 0;

        /************************************************************************************************************************/

        /// <summary>[Animancer Extension] [Assert-Only] Are all of the specified warning types disabled?</summary>
        public static bool IsDisabled(this OptionalWarning type) => (_DisabledWarnings & type) == type;

        /************************************************************************************************************************/

        /// <summary>[Animancer Extension] [Assert-Only]
        /// Disables the specified warnings and returns those that were previously enabled.
        /// </summary>
        /// <remarks>Call <see cref="Enable"/> on the returned value to re-enable it.</remarks>
        public static OptionalWarning DisableTemporarily(this OptionalWarning type)
        {
            var previous = _DisabledWarnings;
            type.Disable();
            return ~previous & type;
        }

        /************************************************************************************************************************/

        private const string PermanentlyDisabledWarningsKey = nameof(Animancer) + "." + nameof(PermanentlyDisabledWarnings);

        /// <summary>[Assert-Only] Warnings that are automatically disabled</summary>
        /// <remarks>
        /// This value is stored in <see cref="PlayerPrefs"/>
        /// and can be manually edited via <see cref="Strings.AnimancerSettingsPath"/>.
        /// </remarks>
        public static OptionalWarning PermanentlyDisabledWarnings
        {
#if NO_RUNTIME_PLAYER_PREFS && ! UNITY_EDITOR
            get => default;
            set
            {
                _DisabledWarnings = value;
            }
#else
            get => (OptionalWarning)PlayerPrefs.GetInt(PermanentlyDisabledWarningsKey);
            set
            {
                _DisabledWarnings = value;
                PlayerPrefs.SetInt(PermanentlyDisabledWarningsKey, (int)value);
            }
#endif
        }

#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
#endif
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void InitializePermanentlyDisabledWarnings()
        {
            _DisabledWarnings = PermanentlyDisabledWarnings;
        }

        /************************************************************************************************************************/
#endif
        /************************************************************************************************************************/
    }
}

