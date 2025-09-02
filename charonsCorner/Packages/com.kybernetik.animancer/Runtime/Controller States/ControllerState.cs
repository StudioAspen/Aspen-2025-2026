// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using Object = UnityEngine.Object;

namespace Animancer
{
    /// <summary>[Pro-Only]
    /// An <see cref="AnimancerState"/> which plays a <see cref="RuntimeAnimatorController"/>.
    /// </summary>
    /// <remarks>
    /// This state can be controlled very similarly to an <see cref="Animator"/>
    /// via its <see cref="Playable"/> property.
    /// <para></para>
    /// <strong>Documentation:</strong>
    /// <see href="https://kybernetik.com.au/animancer/docs/manual/animator-controllers">
    /// Animator Controllers</see>
    /// </remarks>
    /// https://kybernetik.com.au/animancer/api/Animancer/ControllerState
    /// 
    public partial class ControllerState : AnimancerState,
        ICopyable<ControllerState>,
        IParametizedState,
        IUpdatable
    {
        /************************************************************************************************************************/
        #region Fields and Properties
        /************************************************************************************************************************/

        private RuntimeAnimatorController _Controller;

        /// <summary>The <see cref="RuntimeAnimatorController"/> which this state plays.</summary>
        public RuntimeAnimatorController Controller
        {
            get => _Controller;
            set => ChangeMainObject(ref _Controller, value);
        }

        /// <summary>The <see cref="RuntimeAnimatorController"/> which this state plays.</summary>
        public override Object MainObject
        {
            get => Controller;
            set => Controller = (RuntimeAnimatorController)value;
        }

#if UNITY_EDITOR
        /// <inheritdoc/>
        public override Type MainObjectType
            => typeof(RuntimeAnimatorController);
#endif

        /************************************************************************************************************************/

        private new AnimatorControllerPlayable _Playable;

        /// <summary>The internal system which plays the <see cref="RuntimeAnimatorController"/>.</summary>
        public new AnimatorControllerPlayable Playable
        {
            get
            {
                Validate.AssertPlayable(this);
                return _Playable;
            }
        }

        /************************************************************************************************************************/

        /// <summary>Determines what a layer does when <see cref="AnimancerNode.Stop"/> is called.</summary>
        public enum ActionOnStop
        {
            /// <summary>Reset the layer to the first state it was in.</summary>
            DefaultState,

            /// <summary>Rewind the current state's time to 0.</summary>
            RewindTime,

            /// <summary>Allow the current state to stay at its current time.</summary>
            Continue,
        }

        /// <summary>Determines what each layer does when <see cref="AnimancerNode.Stop"/> is called.</summary>
        /// <remarks>
        /// If empty, all layers will reset to their <see cref="ActionOnStop.DefaultState"/>.
        /// <para></para>
        /// If this array is smaller than the <see cref="AnimatorControllerPlayable.GetLayerCount"/>,
        /// any additional layers will use the last value in this array.
        /// </remarks>
        public ActionOnStop[] ActionsOnStop { get; set; }

        /// <summary>
        /// The <see cref="AnimatorStateInfo.shortNameHash"/> of the default state on each layer,
        /// used to reset to those states when <see cref="ApplyActionsOnStop"/>
        /// is called for layers using <see cref="ActionOnStop.DefaultState"/>.
        /// </summary>
        /// <remarks>Gathered automatically by <see cref="GatherDefaultStates"/>.</remarks>
        public int[] DefaultStateHashes { get; set; }

        /************************************************************************************************************************/
#if UNITY_ASSERTIONS
        /************************************************************************************************************************/

        /// <summary>[Assert-Only] Animancer Events work badly on <see cref="ControllerState"/>s.</summary>
        protected internal override string UnsupportedEventsMessage =>
            "Animancer Events on " + nameof(ControllerState) + "s will probably not work as expected." +
            " The events will be associated with the entire Animator Controller and be triggered by any of the" +
            " states inside it. If you want to use events in an Animator Controller you will likely need to use" +
            " Unity's regular Animation Event system.";

        /************************************************************************************************************************/
#endif
        /************************************************************************************************************************/

        /// <summary>[Assert-Conditional] Asserts that the `value` is valid for a parameter.</summary>
        /// <exception cref="ArgumentOutOfRangeException">The `value` is NaN or Infinity.</exception>
        [System.Diagnostics.Conditional(Strings.Assertions)]
        public void AssertParameterValue(float value, [CallerMemberName] string parameterName = null)
        {
            if (!value.IsFinite())
            {
                MarkAsUsed(this);
                throw new ArgumentOutOfRangeException(parameterName, Strings.MustBeFinite);
            }
        }

        /************************************************************************************************************************/

        /// <summary>IK cannot be dynamically enabled on a <see cref="ControllerState"/>.</summary>
        public override void CopyIKFlags(AnimancerNodeBase copyFrom) { }

        /************************************************************************************************************************/

        /// <summary>IK cannot be dynamically enabled on a <see cref="ControllerState"/>.</summary>
        public override bool ApplyAnimatorIK
        {
            get => false;
            set
            {
#if UNITY_ASSERTIONS
                if (value)
                    OptionalWarning.UnsupportedIK.Log(
                        $"IK cannot be dynamically enabled on a {nameof(ControllerState)}." +
                        " You must instead enable it on the desired layer inside the Animator Controller.",
                        _Controller);
#endif
            }
        }

        /************************************************************************************************************************/

        /// <summary>IK cannot be dynamically enabled on a <see cref="ControllerState"/>.</summary>
        public override bool ApplyFootIK
        {
            get => false;
            set
            {
#if UNITY_ASSERTIONS
                if (value)
                    OptionalWarning.UnsupportedIK.Log(
                        $"IK cannot be dynamically enabled on a {nameof(ControllerState)}." +
                        " You must instead enable it on the desired state inside the Animator Controller.",
                        _Controller);
#endif
            }
        }

        /************************************************************************************************************************/

        /// <summary>Returns the hash of a parameter being wrapped by this state.</summary>
        /// <exception cref="NotSupportedException">This state doesn't wrap any parameters.</exception>
        public virtual int GetParameterHash(int index)
        {
            MarkAsUsed(this);
            throw new NotSupportedException();
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public virtual void GetParameters(List<StateParameterDetails> parameters) { }

        /// <inheritdoc/>
        public virtual void SetParameters(List<StateParameterDetails> parameters) { }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Public API
        /************************************************************************************************************************/

        /// <summary>Creates a new <see cref="ControllerState"/> to play the `controller`.</summary>
        public ControllerState(RuntimeAnimatorController controller)
        {
            _Controller = controller != null
                ? controller
                : throw new ArgumentNullException(nameof(controller));
        }

        /// <summary>Creates a new <see cref="ControllerState"/> to play the `controller`.</summary>
        public ControllerState(RuntimeAnimatorController controller, params ActionOnStop[] actionsOnStop)
            : this(controller)
        {
            ActionsOnStop = actionsOnStop;
        }

        /************************************************************************************************************************/

        /// <summary>Creates and assigns the <see cref="AnimatorControllerPlayable"/> managed by this state.</summary>
        protected override void CreatePlayable(out Playable playable)
        {
            playable = _Playable = AnimatorControllerPlayable.Create(Graph._PlayableGraph, _Controller);
            GatherDefaultStates();
            DeserializeParameterBindings();

#if UNITY_ASSERTIONS
            var animator = Graph.Component?.Animator;
            if (animator != null && animator.runtimeAnimatorController != null)
            {
                var usingType = Graph.Component is HybridAnimancerComponent
                    ? Graph.Component.GetType()
                    : GetType();

                OptionalWarning.NativeControllerState.Log($"An Animator Controller is assigned to the" +
                    $" {nameof(Animator)} component while also using a {usingType.Name}." +
                    $" Most likely only one of them is being used so the other should be removed." +
                    $" See the documentation for more information: {Strings.DocsURLs.AnimatorControllers}", this);
            }
#endif
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Stores the values of all parameters and calls <see cref="AnimancerNode.RecreatePlayable"/>,
        /// then restores the parameter values.
        /// </summary>
        public override void RecreatePlayable()
        {
            if (!_Playable.IsValid())
            {
                CreatePlayable();
                return;
            }

            var parameterCount = _Playable.GetParameterCount();
            var values = new object[parameterCount];
            for (int i = 0; i < parameterCount; i++)
            {
                values[i] = AnimancerUtilities.GetParameterValue(_Playable, _Playable.GetParameter(i));
            }

            base.RecreatePlayable();

            for (int i = 0; i < parameterCount; i++)
            {
                AnimancerUtilities.SetParameterValue(_Playable, _Playable.GetParameter(i), values[i]);
            }
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Returns the current state on the specified `layer`,
        /// or the next state if it is currently in a transition.
        /// </summary>
        public AnimatorStateInfo GetStateInfo(int layerIndex)
        {
            if (!_Playable.IsValid())
                return default;

            Validate.AssertPlayable(this);
            return _Playable.IsInTransition(layerIndex)
                ? _Playable.GetNextAnimatorStateInfo(layerIndex)
                : _Playable.GetCurrentAnimatorStateInfo(layerIndex);
        }

        /************************************************************************************************************************/

        /// <summary>
        /// The <see cref="AnimatorStateInfo.normalizedTime"/> * <see cref="AnimatorStateInfo.length"/> of layer 0.
        /// </summary>
        public override double RawTime
        {
            get
            {
                var info = GetStateInfo(0);
                return info.normalizedTime * info.length;
            }
            set
            {
                Validate.AssertPlayable(this);
                _Playable.PlayInFixedTime(0, 0, (float)value);

                // Setting the time requires it to be playing.
                // This will leave it at the specified time.
                if (!IsPlaying)
                {
                    _Playable.Play();
                    Graph.RequirePostUpdate(this);
                }
            }
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        int IUpdatable.UpdatableIndex { get; set; } = IUpdatable.List.NotInList;

        /************************************************************************************************************************/

        /// <summary>Pauses the <see cref="Playable"/> if necessary after <see cref="RawTime"/> was set.</summary>
        void IUpdatable.Update()
        {
            if (!IsPlaying)
                _Playable.Pause();

            AnimancerGraph.Current.CancelPostUpdate(this);
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override void SetGraph(AnimancerGraph graph)
        {
            if (Graph == graph)
                return;

            Graph?.CancelPostUpdate(this);

            base.SetGraph(graph);
        }

        /************************************************************************************************************************/

        /// <summary>The current <see cref="AnimatorStateInfo.length"/> of layer 0.</summary>
        public override float Length => GetStateInfo(0).length;

        /************************************************************************************************************************/

        /// <summary>The current <see cref="AnimatorStateInfo.loop"/> of layer 0.</summary>
        public override bool IsLooping => GetStateInfo(0).loop;

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override void GetEventDispatchInfo(
            out float length,
            out float normalizedTime,
            out bool isLooping)
        {
            var state = GetStateInfo(0);
            length = state.length;
            normalizedTime = state.normalizedTime;
            isLooping = state.loop;
        }

        /************************************************************************************************************************/

        /// <summary>Gathers the <see cref="DefaultStateHashes"/> from the current states on each layer.</summary>
        /// <remarks>This is called by <see cref="CreatePlayable(out UnityEngine.Playables.Playable)"/>.</remarks>
        public void GatherDefaultStates()
        {
            Validate.AssertPlayable(this);

            var layerCount = _Playable.GetLayerCount();

            if (DefaultStateHashes == null || DefaultStateHashes.Length != layerCount)
                DefaultStateHashes = new int[layerCount];

            while (--layerCount >= 0)
                DefaultStateHashes[layerCount] = _Playable.GetCurrentAnimatorStateInfo(layerCount).shortNameHash;
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Stops the animation and makes it inactive immediately so it no longer affects the output.
        /// Also calls <see cref="ApplyActionsOnStop"/>.
        /// </summary>
        protected internal override void StopWithoutWeight()
        {
            // Don't call base.StopWithoutWeight(); because it sets Time = 0;
            // which uses PlayInFixedTime and interferes with resetting to the default states.

            SetIsPlaying(false);
            UpdateIsActive();

            ApplyActionsOnStop();

            _SmoothingVelocities?.Clear();
        }

        /// <summary>Applies the <see cref="ActionsOnStop"/> to their corresponding layers.</summary>
        /// <exception cref="NullReferenceException"><see cref="DefaultStateHashes"/> is null.</exception>
        public void ApplyActionsOnStop()
        {
            Validate.AssertPlayable(this);

            var layerCount = Math.Min(DefaultStateHashes.Length, _Playable.GetLayerCount());

            if (ActionsOnStop == null || ActionsOnStop.Length == 0)
            {
                for (int i = layerCount - 1; i >= 0; i--)
                    _Playable.Play(DefaultStateHashes[i], i, 0);
            }
            else
            {
                for (int i = layerCount - 1; i >= 0; i--)
                {
                    var index = i < ActionsOnStop.Length
                        ? i
                        : ActionsOnStop.Length - 1;

                    switch (ActionsOnStop[index])
                    {
                        case ActionOnStop.DefaultState:
                            _Playable.Play(DefaultStateHashes[i], i, 0);
                            break;

                        case ActionOnStop.RewindTime:
                            _Playable.Play(0, i, 0);
                            break;

                        case ActionOnStop.Continue:
                            break;
                    }
                }
            }
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override void GatherAnimationClips(ICollection<AnimationClip> clips)
        {
            if (_Controller != null)
                clips.Gather(_Controller.animationClips);
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override void Destroy()
        {
            _Controller = null;
            DisposeParameterBindings();
            base.Destroy();
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override AnimancerState Clone(CloneContext context)
        {
            var clone = new ControllerState(_Controller);
            clone.CopyFrom(this, context);
            return clone;
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public sealed override void CopyFrom(AnimancerState copyFrom, CloneContext context)
            => this.CopyFromBase(copyFrom, context);

        /// <inheritdoc/>
        public virtual void CopyFrom(ControllerState copyFrom, CloneContext context)
        {
            ActionsOnStop = copyFrom.ActionsOnStop;

            if (copyFrom.Graph != null &&
                Graph != null)
            {
                var layerCount = copyFrom._Playable.GetLayerCount();
                for (int i = 0; i < layerCount; i++)
                {
                    var info = copyFrom._Playable.GetCurrentAnimatorStateInfo(i);
                    _Playable.Play(info.shortNameHash, i, info.normalizedTime);
                }

                var parameterCount = copyFrom._Playable.GetParameterCount();
                for (int i = 0; i < parameterCount; i++)
                {
                    AnimancerUtilities.CopyParameterValue(
                        copyFrom._Playable,
                        _Playable,
                        copyFrom._Playable.GetParameter(i));
                }
            }

            CopySmoothingVelocitiesFrom(copyFrom);

            base.CopyFrom(copyFrom, context);
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Animator Controller Wrappers
        /************************************************************************************************************************/
        #region Cross Fade
        /************************************************************************************************************************/

        /// <summary>
        /// The default constant for fade duration parameters which causes it to use the
        /// <see cref="AnimancerGraph.DefaultFadeDuration"/> instead.
        /// </summary>
        public const float DefaultFadeDuration = -1;

        /************************************************************************************************************************/

        /// <summary>
        /// Returns the `fadeDuration` if it is zero or positive.
        /// Otherwise returns the <see cref="AnimancerGraph.DefaultFadeDuration"/>.
        /// </summary>
        public static float GetFadeDuration(float fadeDuration)
            => fadeDuration >= 0
            ? fadeDuration
            : AnimancerGraph.DefaultFadeDuration;

        /************************************************************************************************************************/

        /// <summary>Starts a transition from the current state to the specified state using normalized times.</summary>
        /// <remarks>If `fadeDuration` is negative, it uses the <see cref="AnimancerGraph.DefaultFadeDuration"/>.</remarks>
        public void CrossFade(
            int stateNameHash,
            float fadeDuration = DefaultFadeDuration,
            int layer = -1,
            float normalizedTime = float.NegativeInfinity)
            => Playable.CrossFade(stateNameHash, GetFadeDuration(fadeDuration), layer, normalizedTime);

        /************************************************************************************************************************/

        /// <summary>Starts a transition from the current state to the specified state using normalized times.</summary>
        /// <remarks>If `fadeDuration` is negative, it uses the <see cref="AnimancerGraph.DefaultFadeDuration"/>.</remarks>
        public void CrossFade(
            string stateName,
            float fadeDuration = DefaultFadeDuration,
            int layer = -1,
            float normalizedTime = float.NegativeInfinity)
            => Playable.CrossFade(stateName, GetFadeDuration(fadeDuration), layer, normalizedTime);

        /************************************************************************************************************************/

        /// <summary>Starts a transition from the current state to the specified state using times in seconds.</summary>
        /// <remarks>If `fadeDuration` is negative, it uses the <see cref="AnimancerGraph.DefaultFadeDuration"/>.</remarks>
        public void CrossFadeInFixedTime(
            int stateNameHash,
            float fadeDuration = DefaultFadeDuration,
            int layer = -1,
            float fixedTime = 0)
            => Playable.CrossFadeInFixedTime(stateNameHash, GetFadeDuration(fadeDuration), layer, fixedTime);

        /************************************************************************************************************************/

        /// <summary>Starts a transition from the current state to the specified state using times in seconds.</summary>
        /// <remarks>If `fadeDuration` is negative, it uses the <see cref="AnimancerGraph.DefaultFadeDuration"/>.</remarks>
        public void CrossFadeInFixedTime(
            string stateName,
            float fadeDuration = DefaultFadeDuration,
            int layer = -1,
            float fixedTime = 0)
            => Playable.CrossFadeInFixedTime(stateName, GetFadeDuration(fadeDuration), layer, fixedTime);

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Play
        /************************************************************************************************************************/

        /// <summary>Plays the specified state immediately, starting from a particular normalized time.</summary>
        public void Play(
            int stateNameHash,
            int layer = -1,
            float normalizedTime = float.NegativeInfinity)
            => Playable.Play(stateNameHash, layer, normalizedTime);

        /************************************************************************************************************************/

        /// <summary>Plays the specified state immediately, starting from a particular normalized time.</summary>
        public void Play(
            string stateName,
            int layer = -1,
            float normalizedTime = float.NegativeInfinity)
            => Playable.Play(stateName, layer, normalizedTime);

        /************************************************************************************************************************/

        /// <summary>Plays the specified state immediately, starting from a particular time (in seconds).</summary>
        public void PlayInFixedTime(
            int stateNameHash,
            int layer = -1,
            float fixedTime = 0)
            => Playable.PlayInFixedTime(stateNameHash, layer, fixedTime);

        /************************************************************************************************************************/

        /// <summary>Plays the specified state immediately, starting from a particular time (in seconds).</summary>
        public void PlayInFixedTime(
            string stateName,
            int layer = -1,
            float fixedTime = 0)
            => Playable.PlayInFixedTime(stateName, layer, fixedTime);

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Parameters
        /************************************************************************************************************************/

        /// <summary>Gets the value of the specified boolean parameter.</summary>
        public bool GetBool(int id)
            => Playable.GetBool(id);

        /// <summary>Gets the value of the specified boolean parameter.</summary>
        public bool GetBool(string name)
            => Playable.GetBool(name);

        /// <summary>Sets the value of the specified boolean parameter.</summary>
        public void SetBool(int id, bool value)
            => Playable.SetBool(id, value);

        /// <summary>Sets the value of the specified boolean parameter.</summary>
        public void SetBool(string name, bool value)
            => Playable.SetBool(name, value);

        /// <summary>Gets the value of the specified float parameter.</summary>
        public float GetFloat(int id)
            => Playable.GetFloat(id);

        /// <summary>Gets the value of the specified float parameter.</summary>
        public float GetFloat(string name)
            => Playable.GetFloat(name);

        /// <summary>Sets the value of the specified float parameter.</summary>
        public void SetFloat(int id, float value)
            => Playable.SetFloat(id, value);

        /// <summary>Sets the value of the specified float parameter.</summary>
        public void SetFloat(string name, float value)
            => Playable.SetFloat(name, value);

        /// <summary>Gets the value of the specified integer parameter.</summary>
        public int GetInteger(int id)
            => Playable.GetInteger(id);

        /// <summary>Gets the value of the specified integer parameter.</summary>
        public int GetInteger(string name)
            => Playable.GetInteger(name);

        /// <summary>Sets the value of the specified integer parameter.</summary>
        public void SetInteger(int id, int value)
            => Playable.SetInteger(id, value);

        /// <summary>Sets the value of the specified integer parameter.</summary>
        public void SetInteger(string name, int value)
            => Playable.SetInteger(name, value);

        /// <summary>Sets the specified trigger parameter to true.</summary>
        public void SetTrigger(int id)
            => Playable.SetTrigger(id);
        /// <summary>Sets the specified trigger parameter to true.</summary>
        /// 
        public void SetTrigger(string name)
            => Playable.SetTrigger(name);
        /// <summary>Resets the specified trigger parameter to false.</summary>
        /// 
        public void ResetTrigger(int id)
            => Playable.ResetTrigger(id);
        /// <summary>Resets the specified trigger parameter to false.</summary>
        /// 
        public void ResetTrigger(string name)
            => Playable.ResetTrigger(name);

        /// <summary>Indicates whether the specified parameter is controlled by an <see cref="AnimationClip"/>.</summary>
        public bool IsParameterControlledByCurve(int id)
            => Playable.IsParameterControlledByCurve(id);

        /// <summary>Indicates whether the specified parameter is controlled by an <see cref="AnimationClip"/>.</summary>
        public bool IsParameterControlledByCurve(string name)
            => Playable.IsParameterControlledByCurve(name);

        /// <summary>Gets the details of one of the <see cref="Controller"/>'s parameters.</summary>
        public AnimatorControllerParameter GetParameter(int index)
            => Playable.GetParameter(index);

        /// <summary>Gets the number of parameters in the <see cref="Controller"/>.</summary>
        public int GetParameterCount()
            => Playable.GetParameterCount();

        /************************************************************************************************************************/

        /// <summary>The number of parameters in the <see cref="Controller"/>.</summary>
        public int parameterCount => Playable.GetParameterCount();

        /************************************************************************************************************************/

        private AnimatorControllerParameter[] _Parameters;

        /// <summary>The parameters in the <see cref="Controller"/>.</summary>
        /// <remarks>
        /// This property allocates a new array when first accessed. To avoid that, you can use
        /// <see cref="GetParameterCount"/> and <see cref="GetParameter"/> instead.
        /// </remarks>
        public AnimatorControllerParameter[] parameters
        {
            get
            {
                if (_Parameters == null)
                {
                    var count = GetParameterCount();
                    _Parameters = new AnimatorControllerParameter[count];
                    for (int i = 0; i < count; i++)
                        _Parameters[i] = GetParameter(i);
                }

                return _Parameters;
            }
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Smoothed Set Float
        /************************************************************************************************************************/

        private Dictionary<int, float> _SmoothingVelocities;

        /************************************************************************************************************************/

        /// <summary>Sets the value of the specified float parameter with smoothing.</summary>
        /// <remarks>Consider using a <see cref="SmoothedFloatParameter"/> instead.</remarks>
        public float SetFloat(
            string name,
            float value,
            float dampTime,
            float deltaTime,
            float maxSpeed = float.PositiveInfinity)
            => SetFloat(Animator.StringToHash(name), value, dampTime, deltaTime, maxSpeed);

        /// <summary>Sets the value of the specified float parameter with smoothing.</summary>
        /// <remarks>Consider using a <see cref="SmoothedFloatParameter"/> instead.</remarks>
        public float SetFloat(
            int id,
            float value,
            float dampTime,
            float deltaTime,
            float maxSpeed = float.PositiveInfinity)
        {
            _SmoothingVelocities ??= new();

            _SmoothingVelocities.TryGetValue(id, out var velocity);

            value = Mathf.SmoothDamp(GetFloat(id), value, ref velocity, dampTime, maxSpeed, deltaTime);
            SetFloat(id, value);

            _SmoothingVelocities[id] = velocity;

            return value;
        }

        /************************************************************************************************************************/

        /// <summary>Copies the smoothing velocities.</summary>
        private void CopySmoothingVelocitiesFrom(ControllerState copyFrom)
        {
            if (copyFrom._SmoothingVelocities != null)
            {
                if (_SmoothingVelocities == null)
                    _SmoothingVelocities = new();
                else
                    _SmoothingVelocities.Clear();

                foreach (var item in copyFrom._SmoothingVelocities)
                    _SmoothingVelocities[item.Key] = item.Value;
            }
            else
            {
                _SmoothingVelocities?.Clear();
            }
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Misc
        /************************************************************************************************************************/
        // Layers.
        /************************************************************************************************************************/

        /// <summary>Gets the weight of the layer at the specified index.</summary>
        public float GetLayerWeight(int layerIndex)
            => Playable.GetLayerWeight(layerIndex);
        /// <summary>Sets the weight of the layer at the specified index.</summary>
        public void SetLayerWeight(int layerIndex, float weight)
            => Playable.SetLayerWeight(layerIndex, weight);

        /// <summary>Gets the number of layers in the <see cref="Controller"/>.</summary>
        public int GetLayerCount()
            => Playable.GetLayerCount();
        /// <summary>The number of layers in the <see cref="Controller"/>.</summary>
        public int layerCount
            => Playable.GetLayerCount();

        /// <summary>Gets the index of the layer with the specified name.</summary>
        public int GetLayerIndex(string layerName)
            => Playable.GetLayerIndex(layerName);
        /// <summary>Gets the name of the layer with the specified index.</summary>
        public string GetLayerName(int layerIndex)
            => Playable.GetLayerName(layerIndex);

        /************************************************************************************************************************/
        // States.
        /************************************************************************************************************************/

        /// <summary>Returns information about the current state.</summary>
        public AnimatorStateInfo GetCurrentAnimatorStateInfo(int layerIndex = 0)
            => Playable.GetCurrentAnimatorStateInfo(layerIndex);
        /// <summary>Returns information about the next state being transitioned towards.</summary>
        public AnimatorStateInfo GetNextAnimatorStateInfo(int layerIndex = 0)
            => Playable.GetNextAnimatorStateInfo(layerIndex);

        /// <summary>Indicates whether the specified layer contains the specified state.</summary>
        public bool HasState(int layerIndex, int stateID)
            => Playable.HasState(layerIndex, stateID);

        /************************************************************************************************************************/
        // Transitions.
        /************************************************************************************************************************/

        /// <summary>Indicates whether the specified layer is currently executing a transition.</summary>
        public bool IsInTransition(int layerIndex = 0)
            => Playable.IsInTransition(layerIndex);

        /// <summary>Gets information about the current transition.</summary>
        public AnimatorTransitionInfo GetAnimatorTransitionInfo(int layerIndex = 0)
            => Playable.GetAnimatorTransitionInfo(layerIndex);

        /************************************************************************************************************************/
        // Clips.
        /************************************************************************************************************************/

        /// <summary>Gets information about the <see cref="AnimationClip"/>s currently being played.</summary>
        public AnimatorClipInfo[] GetCurrentAnimatorClipInfo(int layerIndex = 0)
            => Playable.GetCurrentAnimatorClipInfo(layerIndex);

        /// <summary>Gets information about the <see cref="AnimationClip"/>s currently being played.</summary>
        public void GetCurrentAnimatorClipInfo(int layerIndex, List<AnimatorClipInfo> clips)
            => Playable.GetCurrentAnimatorClipInfo(layerIndex, clips);
        /// <summary>Gets the number of <see cref="AnimationClip"/>s currently being played.</summary>
        /// 
        public int GetCurrentAnimatorClipInfoCount(int layerIndex = 0)
            => Playable.GetCurrentAnimatorClipInfoCount(layerIndex);

        /// <summary>Gets information about the <see cref="AnimationClip"/>s currently being transitioned towards.</summary>
        public AnimatorClipInfo[] GetNextAnimatorClipInfo(int layerIndex = 0)
            => Playable.GetNextAnimatorClipInfo(layerIndex);

        /// <summary>Gets information about the <see cref="AnimationClip"/>s currently being transitioned towards.</summary>
        public void GetNextAnimatorClipInfo(int layerIndex, List<AnimatorClipInfo> clips)
            => Playable.GetNextAnimatorClipInfo(layerIndex, clips);

        /// <summary>Gets the number of <see cref="AnimationClip"/>s currently being transitioned towards.</summary>
        public int GetNextAnimatorClipInfoCount(int layerIndex = 0)
            => Playable.GetNextAnimatorClipInfoCount(layerIndex);

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
    }
}

