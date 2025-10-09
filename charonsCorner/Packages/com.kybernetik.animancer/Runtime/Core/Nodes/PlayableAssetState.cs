// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Audio;
using UnityEngine.Playables;
using Object = UnityEngine.Object;

namespace Animancer
{
    /// <summary>[Pro-Only] An <see cref="AnimancerState"/> which plays a <see cref="PlayableAsset"/>.</summary>
    /// <remarks>
    /// <strong>Documentation:</strong>
    /// <see href="https://kybernetik.com.au/animancer/docs/manual/timeline">
    /// Timeline</see>
    /// </remarks>
    /// https://kybernetik.com.au/animancer/api/Animancer/PlayableAssetState
    public class PlayableAssetState : AnimancerState
    {
        /************************************************************************************************************************/
        #region Fields and Properties
        /************************************************************************************************************************/

        /// <summary>The <see cref="PlayableAsset"/> which this state plays.</summary>
        private PlayableAsset _Asset;

        /// <summary>The <see cref="PlayableAsset"/> which this state plays.</summary>
        public PlayableAsset Asset
        {
            get => _Asset;
            set => ChangeMainObject(ref _Asset, value);
        }

        /// <summary>The <see cref="PlayableAsset"/> which this state plays.</summary>
        public override Object MainObject
        {
            get => _Asset;
            set => _Asset = (PlayableAsset)value;
        }

#if UNITY_EDITOR
        /// <inheritdoc/>
        public override Type MainObjectType
            => typeof(PlayableAsset);
#endif

        /************************************************************************************************************************/

        private float _Length;

        /// <summary>The <see cref="PlayableAsset.duration"/> (cached on initialization).</summary>
        public override float Length
            => _Length;

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override float Speed
        {
            get => base.Speed;
            set
            {
                base.Speed = value;

                for (int i = Outputs.Count - 1; i >= 0; i--)
                    Outputs[i].GetSourcePlayable().SetSpeed(value);
            }
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override void GetEventDispatchInfo(
            out float length,
            out float normalizedTime,
            out bool isLooping)
        {
            length = _Length;

            normalizedTime = length != 0
                ? Time / length
                : 0;

            isLooping = false;
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        protected override void OnSetIsPlaying()
        {
            if (!_Playable.IsValid())
                return;

            var inputCount = _Playable.GetInputCount();
            for (int i = 0; i < inputCount; i++)
            {
                var playable = _Playable.GetInput(i);
                if (!playable.IsValid())
                    continue;

                if (IsPlaying)
                    playable.Play();
                else
                    playable.Pause();
            }
        }

        /************************************************************************************************************************/

        /// <summary>IK cannot be dynamically enabled on a <see cref="PlayableAssetState"/>.</summary>
        public override void CopyIKFlags(AnimancerNodeBase copyFrom) { }

        /************************************************************************************************************************/

        /// <summary>IK cannot be dynamically enabled on a <see cref="PlayableAssetState"/>.</summary>
        public override bool ApplyAnimatorIK
        {
            get => false;
            set
            {
#if UNITY_ASSERTIONS
                if (value)
                    OptionalWarning.UnsupportedIK.Log(
                        $"IK cannot be dynamically enabled on a {nameof(PlayableAssetState)}.", Graph?.Component);
#endif
            }
        }

        /************************************************************************************************************************/

        /// <summary>IK cannot be dynamically enabled on a <see cref="PlayableAssetState"/>.</summary>
        public override bool ApplyFootIK
        {
            get => false;
            set
            {
#if UNITY_ASSERTIONS
                if (value)
                    OptionalWarning.UnsupportedIK.Log(
                        $"IK cannot be dynamically enabled on a {nameof(PlayableAssetState)}.", Graph?.Component);
#endif
            }
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Methods
        /************************************************************************************************************************/

        /// <summary>Creates a new <see cref="PlayableAssetState"/> to play the `asset`.</summary>
        /// <exception cref="ArgumentNullException">The `asset` is null.</exception>
        public PlayableAssetState(PlayableAsset asset)
        {
            if (asset == null)
                throw new ArgumentNullException(nameof(asset));

            _Asset = asset;
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        protected override void CreatePlayable(out Playable playable)
        {
            playable = _Asset.CreatePlayable(Graph._PlayableGraph, Graph.Component.gameObject);
            playable.SetDuration(9223372.03685477);// https://github.com/KybernetikGames/animancer/issues/111

            _Length = (float)_Asset.duration;

            if (!_HasInitializedBindings)
                InitializeBindings();
        }

        /************************************************************************************************************************/

        private readonly List<PlayableOutput>
            Outputs = new();

        private IList<Object> _Bindings;
        private bool _HasInitializedBindings;

        /************************************************************************************************************************/

        /// <summary>The objects controlled by each track in the asset.</summary>
        public IList<Object> Bindings
        {
            get => _Bindings;
            set
            {
                _Bindings = value;
                InitializeBindings();
            }
        }

        /************************************************************************************************************************/

        /// <summary>Sets the <see cref="Bindings"/>.</summary>
        public void SetBindings(params Object[] bindings)
        {
            Bindings = bindings;
        }

        /************************************************************************************************************************/

        private void InitializeBindings()
        {
            if (Graph == null)
                return;

            _HasInitializedBindings = true;

            Validate.AssertPlayable(this);

            var graph = Graph._PlayableGraph;

            var bindableIndex = 0;
            var bindableCount = _Bindings != null
                ? _Bindings.Count
                : 0;

            var speed = Speed;

            foreach (var binding in _Asset.outputs)
            {
                GetBindingDetails(binding, out var trackName, out var trackType, out var isMarkers);

                var bindable = bindableIndex < bindableCount
                    ? _Bindings[bindableIndex]
                    : null;

#if UNITY_ASSERTIONS
                if (!isMarkers &&
                    trackType != null &&
                    bindable != null &&
                    !trackType.IsAssignableFrom(bindable.GetType()))
                {
                    Debug.LogError(
                        $"Binding Type Mismatch: bindings[{bindableIndex}] is '{bindable}'" +
                        $" but should be a {trackType.FullName} for {trackName}",
                        Graph.Component as Object);
                    bindableIndex++;
                    continue;
                }
#endif

                var playable = _Playable.GetInput(bindableIndex);

                if (speed != 1)
                    playable.SetSpeed(speed);

                if (trackType == typeof(Animator))// AnimationTrack.
                {
                    if (bindable != null)
                    {
#if UNITY_ASSERTIONS
                        if (bindable == Graph.Component?.Animator)
                            Debug.LogError(
                                $"{nameof(PlayableAsset)} tracks should not be bound to the same {nameof(Animator)} as" +
                                $" Animancer. Leaving the binding of the first Animation Track empty will automatically" +
                                $" apply its animation to the object being controlled by Animancer.",
                                Graph.Component as Object);
#endif

                        var playableOutput = AnimationPlayableOutput.Create(graph, trackName, (Animator)bindable);
                        playableOutput.SetReferenceObject(binding.sourceObject);
                        playableOutput.SetSourcePlayable(playable);
                        playableOutput.SetWeight(1);
                        Outputs.Add(playableOutput);
                    }
                }
#if UNITY_AUDIO
                else if (trackType == typeof(AudioSource))// AudioTrack.
                {
                    if (bindable != null)
                    {
                        var playableOutput = AudioPlayableOutput.Create(graph, trackName, (AudioSource)bindable);
                        playableOutput.SetReferenceObject(binding.sourceObject);
                        playableOutput.SetSourcePlayable(playable);
                        playableOutput.SetWeight(1);
                        Outputs.Add(playableOutput);
                    }
                }
#endif
                else if (isMarkers)// Markers.
                {
                    var animancer = Graph.Component as Component;
                    var playableOutput = ScriptPlayableOutput.Create(graph, trackName);
                    playableOutput.SetReferenceObject(binding.sourceObject);
                    playableOutput.SetSourcePlayable(playable);
                    playableOutput.SetWeight(1);
                    playableOutput.SetUserData(animancer);
                    Outputs.Add(playableOutput);

                    var receivers = ListPool.Acquire<INotificationReceiver>();
                    animancer.GetComponents(receivers);
                    for (int i = 0; i < receivers.Count; i++)
                        playableOutput.AddNotificationReceiver(receivers[i]);
                    ListPool.Release(receivers);

                    continue;// Don't increment the bindingIndex.
                }
                else// ActivationTrack, ControlTrack, PlayableTrack, SignalTrack.
                {
                    var playableOutput = ScriptPlayableOutput.Create(graph, trackName);
                    playableOutput.SetReferenceObject(binding.sourceObject);
                    playableOutput.SetSourcePlayable(playable);
                    playableOutput.SetWeight(1);
                    playableOutput.SetUserData(bindable);
                    if (bindable is INotificationReceiver receiver)
                        playableOutput.AddNotificationReceiver(receiver);
                    Outputs.Add(playableOutput);
                }

                bindableIndex++;
            }
        }

        /************************************************************************************************************************/

        /// <summary>Gathers details about the `binding`.</summary>
        public static void GetBindingDetails(
            PlayableBinding binding,
            out string name,
            out Type type,
            out bool isMarkers)
        {
            name = binding.streamName;
            type = binding.outputTargetType;
            isMarkers = type == typeof(GameObject) && name == "Markers";
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override void Destroy()
        {
            _Asset = null;

            if (Graph != null)
            {
                var graph = Graph._PlayableGraph;
                if (graph.IsValid())
                {
                    for (int i = Outputs.Count - 1; i >= 0; i--)
                    {
                        var output = Outputs[i];

                        var playable = output.GetSourcePlayable();
                        if (playable.IsValid())
                            graph.DestroySubgraph(playable);

                        graph.DestroyOutput(output);
                    }

                    Outputs.Clear();

                    if (_Playable.IsValid())
                        graph.DestroySubgraph(_Playable);
                }
            }

            base.Destroy();
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override AnimancerState Clone(CloneContext context)
        {
            var asset = context.GetCloneOrOriginal(_Asset);
            var clone = new PlayableAssetState(asset);
            clone.CopyFrom(this, context);
            return clone;
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        protected override void AppendDetails(StringBuilder text, string separator)
        {
            base.AppendDetails(text, separator);

            text.Append(separator)
                .Append($"{nameof(Bindings)}: ");

            int count;
            if (_Bindings == null)
            {
                text.Append("Null");
                count = 0;
            }
            else
            {
                count = _Bindings.Count;
                text.Append('[')
                    .Append(count)
                    .Append(']');
            }

            text.Append(_HasInitializedBindings
                ? " (Initialized)"
                : " (Not Initialized)");

            for (int i = 0; i < count; i++)
            {
                text.Append(separator)
                    .Append($"{nameof(Bindings)}[")
                    .Append(i)
                    .Append("] = ")
                    .Append(AnimancerUtilities.ToStringOrNull(_Bindings[i]));
            }
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
    }
}

