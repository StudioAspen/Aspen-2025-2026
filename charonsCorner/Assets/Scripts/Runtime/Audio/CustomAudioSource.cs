using System.Collections;
using Animancer;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Audio;

namespace CharonsCorner.Runtime
{
    /// <summary>
    /// AudioSource wrapper that routes all playback through AudioManager.
    /// For simple one-shots, delegates directly to AudioManager.Play().
    /// For stateful playback (pause, resume, loop, fade), claims a dedicated
    /// AudioSource from AudioManager's mixer and manages it locally.
    /// </summary>
    public class CustomAudioSource : MonoBehaviour
    {
        public enum PlaybackMode
        {
            /// <summary>
            /// Delegates to AudioManager.Play() — fire and forget, no pause/resume/fade.
            /// </summary>
            OneShot,
            /// <summary>
            /// Claims a persistent AudioSource routed through AudioManager's mixer.
            /// Supports pause, resume, loop, and volume control.
            /// </summary>
            Managed
        }

        [Title("Audio Settings")]
        [SerializeField, Required] private StringAsset _audioId;
        [SerializeField] private AudioManager.MixerTarget _mixerTarget = AudioManager.MixerTarget.Default;
        [SerializeField] private PlaybackMode _playbackMode = PlaybackMode.Managed;

        [Title("Playback")]
        [SerializeField] private bool _playOnAwake = false;
        [SerializeField] private bool _loop = false;
        [SerializeField] private bool _mute = false;

        [Title("Volume & Pitch")]
        [SerializeField, Range(0f, 1f)] private float _volume = 1f;
        [SerializeField, Range(0f, 3f)] private float _pitch = 1f;
        [SerializeField, Range(-1f, 1f)] private float _stereoPan = 0f;

        [Title("3D Audio")]
        [InfoBox("Set Spatial Blend > 0 to enable positional audio")]
        [SerializeField, Range(0f, 1f)] private float _spatialBlend = 0f;
        [SerializeField] private AudioRolloffMode _rolloffMode = AudioRolloffMode.Logarithmic;
        [SerializeField] private float _minDistance = 1f;
        [SerializeField] private float _maxDistance = 500f;
        [SerializeField] private float _dopplerLevel = 1f;
        [SerializeField] private float _spread = 0f;

        [Title("Priority & Scheduling")]
        [SerializeField, Range(0, 256)] private int _priority = 128;

        [Title("Reverb & Effects")]
        [SerializeField, Range(0f, 1.1f)] private float _reverbZoneMix = 1f;

        [Title("State")]
        [SerializeField, ReadOnly] private bool _isPlaying;

        // The managed AudioSource — only exists in Managed mode.
        // Hidden in inspector since it's an implementation detail.
        private AudioSource _source;
        private AudioClip _resolvedClip;
        private Coroutine _fadeCoroutine;

        public AudioClip ResolvedClip => _resolvedClip;
        public bool IsPlaying => _source != null && _source.isPlaying;

        // ── Properties ────────────────────────────────────────────────────────

        public float Volume
        {
            get => _volume;
            set { _volume = value; if (_source) _source.volume = value; }
        }

        public float Pitch
        {
            get => _pitch;
            set { _pitch = value; if (_source) _source.pitch = value; }
        }

        public bool Loop
        {
            get => _loop;
            set { _loop = value; if (_source) _source.loop = value; }
        }

        public bool Mute
        {
            get => _mute;
            set { _mute = value; if (_source) _source.mute = value; }
        }

        public float Time
        {
            get => _source ? _source.time : 0f;
            set { if (_source) _source.time = value; }
        }

        public int TimeSamples
        {
            get => _source ? _source.timeSamples : 0;
            set { if (_source) _source.timeSamples = value; }
        }

        // ── Lifecycle ─────────────────────────────────────────────────────────

        private void Awake()
        {
            ResolveClip();

            if (_playbackMode == PlaybackMode.Managed)
                SetupManagedSource();

            if (_playOnAwake)
                Play();
        }

        private void Update()
        {
            if (_playbackMode != PlaybackMode.Managed) return;
            _isPlaying = IsPlaying;
        }

        private void OnDestroy()
        {
            TeardownManagedSource();
        }

        // ── Playback Controls ─────────────────────────────────────────────────

        [Button("Play", ButtonSizes.Large), ButtonGroup("Controls")]
        public void Play()
        {
            ResolveClip();

            if (_playbackMode == PlaybackMode.OneShot)
            {
                AudioManager.Instance.Play(
                    _audioId,
                    _mixerTarget,
                    _spatialBlend > 0f ? transform.position : null,
                    _pitch
                );
                _isPlaying = true;
                return;
            }

            // Managed path
            if (_source == null) SetupManagedSource();
            if (_resolvedClip == null)
            {
                Debug.LogWarning($"[CustomAudioSource] Clip '{_audioId}' not found in AudioManager sound bank.", this);
                return;
            }

            _source.Play();
            _isPlaying = true;
        }

        /// <summary>Plays with a delay (seconds). Managed mode only.</summary>
        public void PlayDelayed(float delay)
        {
            if (_playbackMode == PlaybackMode.OneShot)
            {
                Debug.LogWarning("[CustomAudioSource] PlayDelayed is not supported in OneShot mode.", this);
                return;
            }

            ResolveClip();
            if (_source == null) SetupManagedSource();
            if (_resolvedClip != null)
                _source.PlayDelayed(delay);
        }

        /// <summary>Plays a one-shot of the assigned clip through AudioManager, ignoring loop and mode.</summary>
        [Button("One Shot", ButtonSizes.Medium), ButtonGroup("Controls")]
        public void PlayOneShot() => PlayOneShot(_volume);

        public void PlayOneShot(float volumeScale)
        {
            AudioManager.Instance.Play(
                _audioId,
                _mixerTarget,
                _spatialBlend > 0f ? transform.position : null,
                _pitch
            );
        }

        [Button("Pause", ButtonSizes.Medium), ButtonGroup("Controls")]
        public void Pause()
        {
            if (_playbackMode == PlaybackMode.OneShot)
            {
                Debug.LogWarning("[CustomAudioSource] Pause is not supported in OneShot mode.", this);
                return;
            }

            _source?.Pause();
            _isPlaying = false;
        }

        [Button("Unpause", ButtonSizes.Medium), ButtonGroup("Controls")]
        public void UnPause()
        {
            if (_playbackMode == PlaybackMode.OneShot)
            {
                Debug.LogWarning("[CustomAudioSource] UnPause is not supported in OneShot mode.", this);
                return;
            }

            _source?.UnPause();
            _isPlaying = IsPlaying;
        }

        [Button("Stop", ButtonSizes.Large), ButtonGroup("Controls")]
        public void Stop()
        {
            StopFade();
            _source?.Stop();
            _isPlaying = false;
        }

        // ── Fade ──────────────────────────────────────────────────────────────

        /// <summary>Fades volume to <paramref name="targetVolume"/> over <paramref name="duration"/> seconds.</summary>
        public void FadeTo(float targetVolume, float duration, AnimationCurve curve = null)
        {
            if (_playbackMode == PlaybackMode.OneShot)
            {
                Debug.LogWarning("[CustomAudioSource] Fade is not supported in OneShot mode.", this);
                return;
            }

            StopFade();
            _fadeCoroutine = StartCoroutine(FadeRoutine(targetVolume, duration, curve));
        }

        /// <summary>Fades in from 0 to the configured volume.</summary>
        public void FadeIn(float duration, AnimationCurve curve = null)
        {
            if (_source != null) _source.volume = 0f;
            if (!IsPlaying) Play();
            FadeTo(_volume, duration, curve);
        }

        /// <summary>Fades out to 0 then stops.</summary>
        public void FadeOut(float duration, AnimationCurve curve = null)
        {
            StopFade();
            _fadeCoroutine = StartCoroutine(FadeOutRoutine(duration, curve));
        }

        public void StopFade()
        {
            if (_fadeCoroutine != null)
            {
                StopCoroutine(_fadeCoroutine);
                _fadeCoroutine = null;
            }
        }

        private IEnumerator FadeRoutine(float targetVolume, float duration, AnimationCurve curve)
        {
            if (_source == null) yield break;

            float startVolume = _source.volume;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += UnityEngine.Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float curvedT = curve != null ? curve.Evaluate(t) : t;
                _source.volume = Mathf.Lerp(startVolume, targetVolume, curvedT);
                yield return null;
            }

            _source.volume = targetVolume;
            _volume = targetVolume;
            _fadeCoroutine = null;
        }

        private IEnumerator FadeOutRoutine(float duration, AnimationCurve curve)
        {
            yield return FadeRoutine(0f, duration, curve);
            Stop();
        }

        // ── Setup ─────────────────────────────────────────────────────────────

        private void SetupManagedSource()
        {
            if (_source != null) return;

            _source = gameObject.AddComponent<AudioSource>();
            _source.hideFlags = HideFlags.HideInInspector;
            ApplyAllSettings();
        }

        private void TeardownManagedSource()
        {
            if (_source == null) return;
            _source.Stop();
            Destroy(_source);
            _source = null;
        }

        public void ResolveClip()
        {
            if (_audioId == null || AudioManager.Instance == null) return;
            if (!AudioManager.Instance.TryGetClip(_audioId, out _resolvedClip))
            {
                Debug.LogWarning($"[CustomAudioSource] Clip '{_audioId}' not found in AudioManager sound bank.", this);
                _resolvedClip = null;
            }

            if (_source != null)
                _source.clip = _resolvedClip;
        }

        /// <summary>Directly assigns a clip, bypassing bank resolution. Useful for runtime overrides.</summary>
        public void SetClipDirectly(AudioClip clip)
        {
            _resolvedClip = clip;
            if (_source != null)
                _source.clip = clip;
        }

        private void ApplyAllSettings()
        {
            if (_source == null) return;

            _source.clip = _resolvedClip;
            _source.volume = _volume;
            _source.pitch = _pitch;
            _source.loop = _loop;
            _source.mute = _mute;
            _source.panStereo = _stereoPan;
            _source.spatialBlend = _spatialBlend;
            _source.rolloffMode = _rolloffMode;
            _source.minDistance = _minDistance;
            _source.maxDistance = _maxDistance;
            _source.dopplerLevel = _dopplerLevel;
            _source.spread = _spread;
            _source.priority = _priority;
            _source.reverbZoneMix = _reverbZoneMix;
            _source.playOnAwake = false;
            _source.outputAudioMixerGroup = AudioManager.Instance.GetMixerGroup(_mixerTarget);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_source == null) return;
            ApplyAllSettings();
        }
#endif
    }
}