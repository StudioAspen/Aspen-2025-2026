// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

using Animancer.TransitionLibraries;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using Object = UnityEngine.Object;

namespace Animancer
{
    /// <summary>The root node which manages Animancer's <see cref="UnityEngine.Playables.PlayableGraph"/>.</summary>
    /// 
    /// <remarks>
    /// This class can be used as a custom yield instruction to wait until all animations finish playing.
    /// <para></para>
    /// The most common way to access this class is via <see cref="AnimancerComponent.Graph"/>.
    /// <para></para>
    /// <strong>Documentation:</strong>
    /// <see href="https://kybernetik.com.au/animancer/docs/manual/playing">
    /// Playing Animations</see>
    /// </remarks>
    /// 
    /// https://kybernetik.com.au/animancer/api/Animancer/AnimancerGraph
    /// 
    public partial class AnimancerGraph : AnimancerNodeBase,
        IAnimationClipCollection,
        ICopyable<AnimancerGraph>,
        IEnumerator,
        IHasDescription
    {
        /************************************************************************************************************************/
        #region Fields and Properties
        /************************************************************************************************************************/

        private static float _DefaultFadeDuration = 0.25f;

        /************************************************************************************************************************/

#if UNITY_EDITOR
        /// <summary>[Editor-Only]
        /// The namespace that should be used for a class which sets the <see cref="DefaultFadeDuration"/>.
        /// </summary>
        public const string DefaultFadeDurationNamespace = nameof(Animancer);

        /// <summary>[Editor-Only]
        /// The name that should be used for a class which sets the <see cref="DefaultFadeDuration"/>.
        /// </summary>
        public const string DefaultFadeDurationClass = nameof(DefaultFadeDuration);

        /// <summary>[Editor-Only]
        /// Initializes the <see cref="DefaultFadeDuration"/> (see its example for more information).
        /// </summary>
        /// <remarks>
        /// This method takes about 2 milliseconds if a <see cref="DefaultFadeDuration"/> class exists, or 0 if it
        /// doesn't (less than 0.5 rounded off according to a <see cref="System.Diagnostics.Stopwatch"/>).
        /// <para></para>
        /// The <see cref="DefaultFadeDuration"/> can't simply be stored in the
        /// <see cref="Editor.AnimancerSettings"/> because it needs to be initialized before Unity is able to load
        /// <see cref="ScriptableObject"/>s.
        /// </remarks>
        static AnimancerGraph()
        {
            var typeName = $"{DefaultFadeDurationNamespace}.{DefaultFadeDurationClass}";

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            // Iterate backwards since it's more likely to be towards the end.
            for (int iAssembly = assemblies.Length - 1; iAssembly >= 0; iAssembly--)
            {
                var type = assemblies[iAssembly].GetType(typeName);
                if (type != null)
                {
                    var methods = type.GetMethods(AnimancerReflection.StaticBindings);
                    for (int iMethod = 0; iMethod < methods.Length; iMethod++)
                    {
                        var method = methods[iMethod];
                        if (method.IsDefined(typeof(RuntimeInitializeOnLoadMethodAttribute), false))
                        {
                            method.Invoke(null, null);
                            return;
                        }
                    }
                }
            }
        }
#endif

        /************************************************************************************************************************/

        /// <summary>The fade duration to use if not specified. Default is 0.25.</summary>
        /// 
        /// <exception cref="UnityEngine.Assertions.AssertionException">The value is negative or infinity.</exception>
        /// 
        /// <remarks>
        /// <em>Animancer Lite doesn't allow this value to be changed in runtime builds (except to 0).</em>
        /// <para></para>
        /// <strong>Example:</strong>
        /// <see cref="Sprite"/> based games often have no use for fading so you could set this value to 0 using the
        /// following script so that you don't need to manually set the <see cref="ITransition.FadeDuration"/> of all
        /// your transitions.
        /// <para></para>
        /// To set this value automatically on startup, put the following class into any script:
        /// <para></para><code>
        /// namespace Animancer
        /// {
        ///     internal static class DefaultFadeDuration
        ///     {
        ///         [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.BeforeSceneLoad)]
        ///         private static void Initialize() => AnimancerGraph.DefaultFadeDuration = 0;
        ///     }
        /// }
        /// </code>
        /// Using that specific namespace (<see cref="DefaultFadeDurationNamespace"/>) and class name
        /// (<see cref="DefaultFadeDurationClass"/>) allows Animancer to find and run it immediately in the Unity
        /// Editor so that newly created transition fields can start with the correct value (using a
        /// <c>[UnityEditor.InitializeOnLoadMethod]</c> attribute would run it too late).
        /// </remarks>
        public static float DefaultFadeDuration
        {
            get => _DefaultFadeDuration;
            set
            {
                AnimancerUtilities.Assert(value >= 0 && value < float.PositiveInfinity,
                    $"{nameof(AnimancerGraph)}.{nameof(DefaultFadeDuration)} must not be negative or infinity.");

                _DefaultFadeDuration = value;
            }
        }

        /************************************************************************************************************************/

        /// <summary>[Internal]
        /// The <see cref="UnityEngine.Playables.PlayableGraph"/> containing this <see cref="AnimancerGraph"/>.
        /// </summary>
        internal PlayableGraph _PlayableGraph;

        /// <summary>[Pro-Only]
        /// The <see cref="UnityEngine.Playables.PlayableGraph"/> containing this <see cref="AnimancerGraph"/>.
        /// </summary>
        public PlayableGraph PlayableGraph
            => _PlayableGraph;

        /// <summary>Returns the <see cref="PlayableGraph"/>.</summary>
        public static implicit operator PlayableGraph(AnimancerGraph animancer)
            => animancer.PlayableGraph;

        /************************************************************************************************************************/

        /// <summary>[Internal]
        /// The <see cref="Playable"/> of the pre-update <see cref="UpdatableListPlayable"/>.
        /// </summary>
        /// <remarks>
        /// This is the final <see cref="Playable"/> connected to the output of the <see cref="PlayableGraph"/>.
        /// </remarks>
        internal Playable _PreUpdatePlayable;// Internal for AnimancerLayerList.

        /************************************************************************************************************************/

        private PlayableOutput _Output;

        /// <summary>The <see cref="PlayableOutput"/> connected to this <see cref="AnimancerGraph"/>.</summary>
        public PlayableOutput Output
        {
            get
            {
                if (!_Output.IsOutputValid())
                    _Output = _PlayableGraph.FindOutput(_PreUpdatePlayable);

                return _Output;
            }
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override AnimancerLayer Layer
            => null;

        /// <inheritdoc/>
        public override int ChildCount
            => _Layers.Count;

        /// <inheritdoc/>
        protected internal override AnimancerNode GetChildNode(int index)
            => _Layers[index];

        /************************************************************************************************************************/

        private AnimancerLayerList _Layers;

        /// <summary>The <see cref="AnimancerLayer"/>s which each manage their own set of animations.</summary>
        /// <remarks>
        /// <strong>Documentation:</strong>
        /// <see href="https://kybernetik.com.au/animancer/docs/manual/blending/layers">
        /// Layers</see>
        /// </remarks>
        public AnimancerLayerList Layers
        {
            get => _Layers;
            set
            {
#if UNITY_ASSERTIONS
                if (value.Graph != this)
                    throw new ArgumentException(
                        $"{nameof(AnimancerGraph)}.{nameof(AnimancerLayerList)}.{nameof(AnimancerLayerList.Graph)}" +
                        $" mismatch: cannot use a list in an {nameof(AnimancerGraph)} that is not its" +
                        $" {nameof(AnimancerLayerList.Graph)}");
#endif

                _Playable = value.Playable;

                if (_Layers != null && _Playable.IsValid())
                {
                    var count = _Layers.Count;
                    for (int i = 0; i < count; i++)
                    {
                        var layer = _Playable.GetInput(i);
                        if (layer.IsValid())
                        {
                            _Playable.DisconnectInput(i);
                            _PlayableGraph.Connect(_Playable, layer, i, _Layers[i].Weight);
                        }
                    }

                    _PreUpdatePlayable.DisconnectInput(0);

                    // Don't destroy the old Playable since it could still be reused later.
                }

                _Layers = value;

                _PlayableGraph.Connect(_PreUpdatePlayable, _Playable, 0, 1);
            }
        }

        /************************************************************************************************************************/

        /// <summary>The <see cref="AnimancerState"/>s managed by this graph.</summary>
        /// <remarks>
        /// <strong>Documentation:</strong>
        /// <see href="https://kybernetik.com.au/animancer/docs/manual/playing/states">
        /// States</see>
        /// </remarks>
        public readonly AnimancerStateDictionary States;

        /************************************************************************************************************************/

        private ParameterDictionary _Parameters;

        /// <summary>Dynamic parameters which anything can get or set.</summary>
        public ParameterDictionary Parameters
            => _Parameters ??= new();

        /// <summary>Has the <see cref="Parameters"/> dictionary been initialized?</summary>
        public bool HasParameters
            => _Parameters != null;

        /************************************************************************************************************************/

        /// <summary>[Internal] The backing field of <see cref="Events"/>.</summary>
        internal NamedEventDictionary _Events;

        /// <summary>A dictionary of callbacks to be triggered by any event with a matching name.</summary>
        public NamedEventDictionary Events
            => _Events ??= new();

        /// <summary>Has the <see cref="Events"/> dictionary been initialized?</summary>
        public bool HasEvents
            => _Events != null;

        /************************************************************************************************************************/

        /// <summary>[Pro-Only] The optional <see cref="TransitionLibrary"/> this graph can use.</summary>
        public TransitionLibrary Transitions { get; set; }

        /************************************************************************************************************************/

        private readonly IUpdatable.List _PreUpdatables = new();
        private readonly IUpdatable.List _PostUpdatables = new();

        /// <summary>Objects to be updated before time advances.</summary>
        public IReadOnlyIndexedList<IUpdatable> PreUpdatables
            => _PreUpdatables;

        /// <summary>Objects to be updated after time advances.</summary>
        public IReadOnlyIndexedList<IUpdatable> PostUpdatables
            => _PostUpdatables;

        /************************************************************************************************************************/

        /// <summary>The component that is playing this <see cref="AnimancerGraph"/>.</summary>
        public IAnimancerComponent Component { get; private set; }

        /************************************************************************************************************************/

        /// <summary>Determines what time source is used to update the <see cref="UnityEngine.Playables.PlayableGraph"/>.</summary>
        public DirectorUpdateMode UpdateMode
        {
            get => _PlayableGraph.GetTimeUpdateMode();
            set => _PlayableGraph.SetTimeUpdateMode(value);
        }

        /************************************************************************************************************************/

        private bool _KeepChildrenConnected;

        /// <summary>
        /// Should playables stay connected to the graph at all times?
        /// Otherwise they will be disconnected when their  <see cref="AnimancerNode.Weight"/> is 0.
        /// </summary>
        /// 
        /// <remarks>
        /// This value defaults to <c>false</c> and can be set by <see cref="SetKeepChildrenConnected"/>.
        /// <para></para>
        /// <strong>Example:</strong><code>
        /// [SerializeField]
        /// private AnimancerComponent _Animancer;
        /// 
        /// public void Initialize()
        /// {
        ///     _Animancer.Graph.SetKeepChildrenConnected(true);
        /// }
        /// </code></remarks>
        public override bool KeepChildrenConnected
            => _KeepChildrenConnected;

        /// <summary>Sets <see cref="KeepChildrenConnected"/>.</summary>
        /// <remarks>This method exists because the <see cref="KeepChildrenConnected"/> override can't add a setter.</remarks>
        public void SetKeepChildrenConnected(bool value)
        {
            if (_KeepChildrenConnected == value)
                return;

            _KeepChildrenConnected = value;

            if (value)
            {
                for (int i = _Layers.Count - 1; i >= 0; i--)
                    _Layers.GetLayer(i).ConnectAllStates();
            }
            else
            {
                for (int i = _Layers.Count - 1; i >= 0; i--)
                    _Layers.GetLayer(i).DisconnectInactiveStates();
            }
        }

        /************************************************************************************************************************/

        private bool _SkipFirstFade;

        /// <summary>
        /// Normally the first animation on the Base Layer should not fade in because there is nothing fading out. But
        /// sometimes that is undesirable, such as if the <see cref="Animator.runtimeAnimatorController"/> is assigned
        /// since Animancer can blend with that.
        /// </summary>
        /// <remarks>
        /// Setting this value to false ensures that the <see cref="AnimationLayerMixerPlayable"/> has at least two
        /// inputs because it ignores the <see cref="AnimancerNode.Weight"/> of the layer when there is only one.
        /// </remarks>
        public bool SkipFirstFade
        {
            get => _SkipFirstFade;
            set
            {
                _SkipFirstFade = value;

                if (!value && _Layers.Count < 2)
                {
                    _Layers.Count = 1;
                    Playable.SetInputCount(2);
                }
            }
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Initialization
        /************************************************************************************************************************/

        /// <summary>
        /// Creates an <see cref="AnimancerGraph"/> in an existing
        /// <see cref="UnityEngine.Playables.PlayableGraph"/>.
        /// </summary>
        public AnimancerGraph(
            PlayableGraph graph,
            TransitionLibrary transitions = null)
        {
            _PlayableGraph = graph;
            Transitions = transitions;
            Graph = this;

            _PreUpdatePlayable = UpdatableListPlayable.Create(this, 2, _PreUpdatables);

            var postUpdate = UpdatableListPlayable.Create(this, 0, _PostUpdatables);
            _PlayableGraph.Connect(postUpdate, 0, _PreUpdatePlayable, 1);

            States = new(this);

#if UNITY_EDITOR
            Editor.AnimancerGraphCleanup.AddGraph(this);
#endif
        }

        /// <summary>
        /// Creates an <see cref="AnimancerGraph"/> in a new
        /// <see cref="UnityEngine.Playables.PlayableGraph"/>.
        /// </summary>
        public AnimancerGraph(TransitionLibrary transitions = null)
            : this(CreateGraph(), transitions)
        {
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Initializes this graph and plays it on the on the
        /// <see cref="IAnimancerComponent.Animator"/> if `createOutput` is true.
        /// </summary>
        public void Initialize(
            IAnimancerComponent animancer,
            bool createOutput = true)
        {
            var animator = animancer.Animator;

#if UNITY_ASSERTIONS
            if (animator == null)
                throw new ArgumentNullException(nameof(animator),
                    $"An {nameof(Animator)} component is required to play animations.");

#if UNITY_EDITOR
            if (UnityEditor.EditorUtility.IsPersistent(animator))
                throw new ArgumentException(
                    $"The specified {nameof(Animator)} component is a prefab which means it cannot play animations.",
                    nameof(animator));
#endif

            if (animancer != null)
            {
                Debug.Assert(animancer.IsGraphInitialized && animancer.Graph == this,
                    $"{nameof(Initialize)} was called on an {nameof(AnimancerGraph)} which does not match the" +
                    $" {nameof(IAnimancerComponent)}.{nameof(IAnimancerComponent.Graph)}.");
                Debug.Assert(animator == animancer.Animator,
                    $"{nameof(Initialize)} was called with an {nameof(Animator)} which does not match the" +
                    $" {nameof(IAnimancerComponent)}.{nameof(IAnimancerComponent.Animator)}.");

#if UNITY_EDITOR
                CaptureInactiveInitializationStackTrace(animancer);
#endif
            }

            if (Output.IsOutputValid())
            {
                Debug.LogWarning(
                    $"A {nameof(PlayableGraph)} output is already connected to the {nameof(AnimancerGraph)}." +
                    $" The old output should be destroyed using `animancerComponent.Graph.DestroyOutput();`" +
                    $" before calling {nameof(Initialize)}.", animator);
            }
#endif

            Layers ??= new AnimancerLayerMixerList(this);

            Component = animancer;

            // Generic Rigs can blend with an underlying Animator Controller but Humanoids can't.
            SkipFirstFade = animator.isHuman || animator.runtimeAnimatorController == null;

            AnimancerEvent.Invoker.Initialize(animator.updateMode);

            if (createOutput)
            {
#pragma warning disable CS0618 // Type or member is obsolete.
                // Unity 2022 marked this method as [Obsolete] even though it's the only way to use Animate Physics mode.
                AnimationPlayableUtilities.Play(animator, _PreUpdatePlayable, _PlayableGraph);
#pragma warning restore CS0618 // Type or member is obsolete.
            }
        }

        /************************************************************************************************************************/
        #region Graph Creation
        /************************************************************************************************************************/

        /// <summary>
        /// Creates a new empty <see cref="UnityEngine.Playables.PlayableGraph"/>
        /// and consumes the name set by <see cref="SetNextGraphName"/> if it was called.
        /// </summary>
        /// <remarks>
        /// The caller is responsible for calling <see cref="Destroy()"/> on the returned object,
        /// except in Edit Mode where it will be called automatically.
        /// </remarks>
        public static PlayableGraph CreateGraph()
        {
#if UNITY_EDITOR
            var name = _NextGraphName;
            _NextGraphName = null;

            return name != null
                ? PlayableGraph.Create(name)
                : PlayableGraph.Create();
#else
            return PlayableGraph.Create();
#endif
        }

        /************************************************************************************************************************/

#if UNITY_EDITOR
        private static string _NextGraphName;
#endif

        /// <summary>[Editor-Conditional]
        /// Sets the display name for the next instance to give its <see cref="UnityEngine.Playables.PlayableGraph"/>.
        /// </summary>
        [System.Diagnostics.Conditional(Strings.UnityEditor)]
        public static void SetNextGraphName(string name)
        {
#if UNITY_EDITOR
            _NextGraphName = name;
#endif
        }

        /************************************************************************************************************************/

#if UNITY_EDITOR
        /// <summary>[Editor-Only] Returns "Component Name (Animancer)".</summary>
        public override string ToString()
            => !Component.IsNullOrDestroyed()
            ? $"{Component.gameObject.name} ({nameof(Animancer)})"
            : _PlayableGraph.IsValid()
            ? _PlayableGraph.GetEditorName()
            : $"Destroyed ({nameof(Animancer)})";
#endif

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/

        /// <summary>[Pro-Only]
        /// Inserts a `playable` after the root of the <see cref="PlayableGraph"/>
        /// so that it can modify the final output.
        /// </summary>
        /// <remarks>It can be removed using <see cref="AnimancerUtilities.RemovePlayable"/>.</remarks>
        public void InsertOutputPlayable(Playable playable)
        {
            var output = Output;
            _PlayableGraph.Connect(playable, output.GetSourcePlayable(), 0, 1);
            output.SetSourcePlayable(playable);
        }

        /// <summary>[Pro-Only]
        /// Inserts an animation job after the root of the <see cref="PlayableGraph"/>
        /// so that it can modify the final output.
        /// </summary>
        /// <remarks>
        /// It can can be removed by passing the returned value into <see cref="AnimancerUtilities.RemovePlayable"/>.
        /// </remarks>
        public AnimationScriptPlayable InsertOutputJob<T>(T data)
            where T : struct, IAnimationJob
        {
            var playable = AnimationScriptPlayable.Create(_PlayableGraph, data, 1);
            InsertOutputPlayable(playable);
            return playable;
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        void ICopyable<AnimancerGraph>.CopyFrom(AnimancerGraph copyFrom, CloneContext context)
            => CopyFrom(copyFrom, context);

        /// <summary>Copies all layers and states from `copyFrom` into this graph.</summary>
        public void CopyFrom(AnimancerGraph copyFrom, bool includeInactiveStates = false)
        {
            var context = CloneContext.Pool.Instance.Acquire();
            CopyFrom(copyFrom, context, includeInactiveStates);
            CloneContext.Pool.Instance.Release(context);
        }

        /// <summary>Copies all layers and states from `copyFrom` into this graph.</summary>
        public void CopyFrom(AnimancerGraph copyFrom, CloneContext context, bool includeInactiveStates = false)
        {
            if (copyFrom == this)
                return;

            var wouldCloneUpdatables = context.WillCloneUpdatables;
            context.WillCloneUpdatables = true;

            Speed = copyFrom.Speed;
            SetKeepChildrenConnected(copyFrom.KeepChildrenConnected);
            SkipFirstFade = copyFrom.SkipFirstFade;
            IsGraphPlaying = copyFrom.IsGraphPlaying;
            FrameID = copyFrom.FrameID;

            context[copyFrom] = this;

            // Register states in the context.
            foreach (var copyFromState in copyFrom.States)
                if (States.TryGet(copyFromState.Key, out var copyToState))
                    context[copyFromState] = copyToState;

            var layerCount = copyFrom._Layers.Count;

            // Register layers in the context.
            for (int i = 0; i < layerCount; i++)
            {
                var copyFromLayer = copyFrom._Layers[i];
                var copyToLayer = _Layers[i];
                context[copyFromLayer] = copyToLayer;
            }

            // Copy existing layers.
            for (int i = 0; i < layerCount; i++)
            {
                var copyFromLayer = copyFrom._Layers[i];
                var copyToLayer = _Layers[i];
                copyToLayer.CopyFrom(copyFromLayer, context);
                copyToLayer.CopyStatesFrom(copyFromLayer, context, includeInactiveStates);
            }

            // Stop any extra layers.
            for (int i = layerCount; i < _Layers.Count; i++)
                _Layers[i].Stop();

            _PreUpdatables.CloneFrom(copyFrom._PreUpdatables, context);
            _PostUpdatables.CloneFrom(copyFrom._PostUpdatables, context);

            context.WillCloneUpdatables = wouldCloneUpdatables;
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Cleanup
        /************************************************************************************************************************/

        /// <summary>Is this <see cref="AnimancerGraph"/> currently usable (not destroyed)?</summary>
        /// <remarks>Calls <see cref="DisposeAll"/> if the <see cref="PlayableGraph"/> was already destroyed.</remarks>
        public bool IsValidOrDispose()
        {
            if (_PlayableGraph.IsValid())
                return true;

            DisposeAll();
            return false;
        }

        /************************************************************************************************************************/

        /// <summary>Destroys the <see cref="PlayableGraph"/> and everything in it. This operation cannot be undone.</summary>
        /// <remarks>If the <see cref="PlayableGraph"/> is owned by another system, use <see cref="DestroyPlayables"/> instead.</remarks>
        public void Destroy()
        {
            OnGraphDestroyed();

            if (_PlayableGraph.IsValid())
            {
                _PlayableGraph.Destroy();

#if UNITY_EDITOR
                Editor.AnimancerGraphCleanup.RemoveGraph(this);
#endif
            }
        }

        /// <summary>
        /// Destroys this <see cref="AnimancerGraph"/> and everything inside it (layers, states, etc.)
        /// without destroying the <see cref="PlayableGraph"/>.
        /// </summary>
        /// <remarks>
        /// This can be useful if Animancer was initialized inside a <see cref="UnityEngine.Playables.PlayableGraph"/> owned by another
        /// system such as Unity's Animation Rigging package. Otherwise, use <see cref="Destroy"/>.
        /// </remarks>
        public void DestroyPlayables()
        {
            OnGraphDestroyed();

            if (_PlayableGraph.IsValid())
                _PlayableGraph.DestroySubgraph(_PreUpdatePlayable);
        }

        /************************************************************************************************************************/

        /// <summary>Destroys the <see cref="Output"/> if it exists and returns true if successful.</summary>
        public bool DestroyOutput()
        {
            var output = Output;
            if (!output.IsOutputValid())
                return false;

            _PlayableGraph.DestroyOutput(output);
            return true;
        }

        /************************************************************************************************************************/

        /// <summary>Calls <see cref="OnGraphDestroyed"/> in case <see cref="Destroy"/> wasn't called.</summary>
        ~AnimancerGraph()
            => OnGraphDestroyed();

        /// <summary>Calls <see cref="AnimancerLayer.OnGraphDestroyed"/> on all layers.</summary>
        private void OnGraphDestroyed()
        {
            if (_Layers != null)
            {
                var layerCount = _Layers.Count;
                for (int i = 0; i < layerCount; i++)
                    _Layers[i].OnGraphDestroyed();
            }

            DisposeAll();
        }

        /************************************************************************************************************************/

        private List<IDisposable> _Disposables;

        /// <summary>A list of objects that need to be disposed when this <see cref="AnimancerGraph"/> is destroyed.</summary>
        /// <remarks>This list is primarily used to dispose native arrays used by Animation Jobs.</remarks>
        public List<IDisposable> Disposables
            => _Disposables ??= new();

        /************************************************************************************************************************/

        /// <summary>Calls <see cref="IDisposable.Dispose"/> on all <see cref="Disposables"/> and discards them.</summary>
        private void DisposeAll()
        {
            if (_Disposables == null ||
                _Disposables.Count == 0)
                return;

            GC.SuppressFinalize(this);

            var previous = Current;
            Current = this;

            var i = _Disposables.Count;
            DisposeNext:
            try
            {
                while (--i >= 0)
                {
                    _Disposables[i].Dispose();
                }

                _Disposables.Clear();
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, Component as Object);
                goto DisposeNext;
            }

            Current = previous;
        }

        /************************************************************************************************************************/
        #region Inverse Kinematics
        // These fields are stored here but accessed via the LayerList.
        /************************************************************************************************************************/

        private bool _ApplyAnimatorIK;

        /// <inheritdoc/>
        public override bool ApplyAnimatorIK
        {
            get => _ApplyAnimatorIK;
            set
            {
                _ApplyAnimatorIK = value;

                for (int i = _Layers.Count - 1; i >= 0; i--)
                    _Layers.GetLayer(i).ApplyAnimatorIK = value;
            }
        }

        /************************************************************************************************************************/

        private bool _ApplyFootIK;

        /// <inheritdoc/>
        public override bool ApplyFootIK
        {
            get => _ApplyFootIK;
            set
            {
                _ApplyFootIK = value;

                for (int i = _Layers.Count - 1; i >= 0; i--)
                    _Layers.GetLayer(i).ApplyFootIK = value;
            }
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Playing
        /************************************************************************************************************************/

        /// <summary>Calls <see cref="IAnimancerComponent.GetKey"/> on the <see cref="Component"/>.</summary>
        /// <remarks>If the <see cref="Component"/> is null, this method returns the `clip` itself.</remarks>
        public object GetKey(AnimationClip clip)
            => Component != null
            ? Component.GetKey(clip)
            : clip;

        /************************************************************************************************************************/

        /// <summary>
        /// Gets the state registered with the <see cref="IHasKey.Key"/>,
        /// stops and rewinds it to the start, then returns it.
        /// </summary>
        /// <remarks>Note that playing something new will automatically stop the old animation.</remarks>
        public AnimancerState Stop(IHasKey hasKey)
            => Stop(hasKey.Key);

        /// <summary>
        /// Calls <see cref="AnimancerNode.Stop"/> on the state registered with the `key`
        /// to stop it from playing and rewind it to the start.
        /// </summary>
        /// <remarks>Note that playing something new will automatically stop the old animation.</remarks>
        public AnimancerState Stop(object key)
        {
            if (States.TryGet(key, out var state))
                state.Stop();

            return state;
        }

        /// <summary>
        /// Calls <see cref="AnimancerNode.Stop"/> on all animations
        /// to stop them from playing and rewind them to the start.
        /// </summary>
        /// <remarks>Note that playing something new will automatically stop the old animation.</remarks>
        public void Stop()
        {
            for (int i = _Layers.Count - 1; i >= 0; i--)
                _Layers.GetLayer(i).Stop();
        }

        /************************************************************************************************************************/

        /// <summary>Is a state registered with the <see cref="IHasKey.Key"/> and currently playing?</summary>
        public bool IsPlaying(IHasKey hasKey)
            => IsPlaying(hasKey.Key);

        /// <summary>Is a state registered with the `key` and currently playing?</summary>
        public bool IsPlaying(object key)
            => States.TryGet(key, out var state)
            && state.IsPlaying;

        /// <summary>Is least one animation being played?</summary>
        public bool IsPlaying()
        {
            if (!IsGraphPlaying)
                return false;

            for (int i = _Layers.Count - 1; i >= 0; i--)
                if (_Layers.GetLayer(i).IsAnyStatePlaying())
                    return true;

            return false;
        }

        /************************************************************************************************************************/

        /// <summary>Is the `clip` currently being played by at least one state in the specified layer?</summary>
        /// <remarks>
        /// This method is inefficient because it searches through every state,
        /// unlike <see cref="IsPlaying(object)"/> which only checks the state registered using the specified key.
        /// </remarks>
        public bool IsPlayingClip(AnimationClip clip)
        {
            if (!IsGraphPlaying)
                return false;

            for (int i = _Layers.Count - 1; i >= 0; i--)
                if (_Layers.GetLayer(i).IsPlayingClip(clip))
                    return true;

            return false;
        }

        /************************************************************************************************************************/

        /// <summary>Calculates the total <see cref="AnimancerNode.Weight"/> of all states in all layers.</summary>
        public float GetTotalWeight()
        {
            float weight = 0;

            for (int i = _Layers.Count - 1; i >= 0; i--)
                weight += _Layers.GetLayer(i).GetTotalChildWeight();

            return weight;
        }

        /************************************************************************************************************************/

        /// <summary>[<see cref="IAnimationClipCollection"/>] Gathers all the animations in all layers.</summary>
        public void GatherAnimationClips(ICollection<AnimationClip> clips)
            => _Layers.GatherAnimationClips(clips);

        /************************************************************************************************************************/
        // IEnumerator for yielding in a coroutine to wait until animations have stopped.
        /************************************************************************************************************************/

        /// <summary>Are any animations playing?</summary>
        /// <remarks>This allows this object to be used as a custom yield instruction.</remarks>
        bool IEnumerator.MoveNext()
        {
            for (int i = _Layers.Count - 1; i >= 0; i--)
                if (_Layers.GetLayer(i).IsPlayingAndNotEnding())
                    return true;

            return false;
        }

        /// <summary>Returns null.</summary>
        object IEnumerator.Current => null;

        /// <summary>Does nothing.</summary>
        void IEnumerator.Reset() { }

        /************************************************************************************************************************/
        #region Key Error Methods
#if UNITY_EDITOR
        /************************************************************************************************************************/
        // These are overloads of other methods that take a System.Object key to ensure the user doesn't try to use an
        // AnimancerState as a key, since the whole point of a key is to identify a state in the first place.
        /************************************************************************************************************************/

        /// <summary>[Warning]
        /// You should not use an <see cref="AnimancerState"/> as a key.
        /// Just call <see cref="AnimancerNode.Stop"/>.
        /// </summary>
        [Obsolete("You should not use an AnimancerState as a key. Just call AnimancerState.Stop().", true)]
        public AnimancerState Stop(AnimancerState key)
        {
            key.Stop();
            return key;
        }

        /// <summary>[Warning]
        /// You should not use an <see cref="AnimancerState"/> as a key.
        /// Just check <see cref="AnimancerState.IsPlaying"/>.
        /// </summary>
        [Obsolete("You should not use an AnimancerState as a key. Just check AnimancerState.IsPlaying.", true)]
        public bool IsPlaying(AnimancerState key) => key.IsPlaying;

        /************************************************************************************************************************/
#endif
        #endregion
        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Evaluation
        /************************************************************************************************************************/

        /// <summary>Indicates whether the <see cref="UnityEngine.Playables.PlayableGraph"/> is currently playing.</summary>
        public bool IsGraphPlaying
        {
            get => _PlayableGraph.IsPlaying();
            set
            {
                if (value)
                    UnpauseGraph();
                else
                    PauseGraph();
            }
        }

        /// <summary>
        /// Resumes playing the <see cref="UnityEngine.Playables.PlayableGraph"/>.
        /// </summary>
        public void UnpauseGraph()
        {
            _PlayableGraph.Play();

#if UNITY_EDITOR
            // In Edit Mode, unpausing the graph doesn't work properly unless we force it to change.
            if (!UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode)
                Evaluate(0.00001f);
#endif
        }

        /// <summary>
        /// Freezes the <see cref="UnityEngine.Playables.PlayableGraph"/> at its current state.
        /// </summary>
        /// <remarks>
        /// If you call this method, you are responsible for calling <see cref="UnpauseGraph"/> to resume playing.
        /// </remarks>
        public void PauseGraph()
            => _PlayableGraph.Stop();

        /************************************************************************************************************************/

        /// <summary>
        /// Evaluates all of the currently playing animations to apply their states to the animated objects.
        /// </summary>
        public void Evaluate()
        {
            _PlayableGraph.Evaluate();
            AnimancerEvent.Invoker.InvokeAllAndClear();
        }

        /// <summary>
        /// Advances all currently playing animations by the specified amount of time (in seconds) and evaluates the
        /// graph to apply their states to the animated objects.
        /// </summary>
        public void Evaluate(float deltaTime)
        {
            _PlayableGraph.Evaluate(deltaTime);
            AnimancerEvent.Invoker.InvokeAllAndClear();
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Description
        /************************************************************************************************************************/

        /// <inheritdoc/>
        public void AppendDescription(
            StringBuilder text,
            string separator = "\n")
        {
            text.AppendField(null, nameof(AnimancerGraph), Component);

            separator += Strings.Indent;
            var layerDetailsSeparator = separator + Strings.Indent;

            text.AppendField(separator, "Layer Count", _Layers.Count);
            AnimancerNode.AppendIKDetails(text, separator, this);

            var count = _Layers.Count;
            for (int i = 0; i < count; i++)
            {
                text.Append(separator);
                _Layers[i].AppendDescription(text, layerDetailsSeparator);
            }

            States.AppendDescriptionOrOrphans(text, separator);
            text.AppendLine();
            AppendInternalDetails(text, Strings.Indent, separator + Strings.Indent);
            AppendGraphDetails(text, separator, Strings.Indent);
        }

        /************************************************************************************************************************/

        /// <summary>Appends all registered <see cref="IUpdatable"/>s and <see cref="IDisposable"/>s.</summary>
        public void AppendInternalDetails(
            StringBuilder text,
            string sectionPrefix,
            string itemPrefix)
        {
            AppendAll(text, sectionPrefix, itemPrefix, _PreUpdatables, "Pre Updatables");
            text.AppendLine();
            AppendAll(text, sectionPrefix, itemPrefix, _PostUpdatables, "Post Updatables");
            text.AppendLine();
            AppendAll(text, sectionPrefix, itemPrefix, _Disposables, "Disposables");
        }

        /************************************************************************************************************************/

        /// <summary>Appends everything in the `collection`.</summary>
        private static void AppendAll(
            StringBuilder text,
            string sectionPrefix,
            string itemPrefix,
            ICollection collection,
            string name)
        {
            var count = collection != null
                ? collection.Count
                : 0;

            text.AppendField(sectionPrefix, name, count);
            if (collection == null)
                return;

            var separator = $"{itemPrefix}{Strings.Indent}";
            foreach (var item in collection)
            {
                text.Append(itemPrefix);

                if (item is AnimancerNode node)
                    text.Append(node.GetPath());
                else
                    text.AppendDescription(item, separator);
            }
        }

        /************************************************************************************************************************/

        private const string NoPlayable = "No Playable";

        /// <summary>Appends the structure of the <see cref="PlayableGraph"/>.</summary>
        public void AppendGraphDetails(
            StringBuilder text,
            string itemPrefix = "\n",
            string indent = Strings.Indent)
        {
            var indentedPrefix = itemPrefix + indent;

            var outputCount = _PlayableGraph.GetOutputCount();
            for (int i = 0; i < outputCount; i++)
            {
                var output = _PlayableGraph.GetOutput(i);

                text.Append(itemPrefix)
                    .Append(output.GetPlayableOutputType().Name);

#if UNITY_EDITOR
                text.Append(" \"")
                    .Append(UnityEditor.Playables.PlayableOutputEditorExtensions.GetEditorName(output))
                    .Append('"');
#endif

                text.Append(": ");

                var playable = output.GetSourcePlayable();
                AppendGraphDetails(text, playable, indentedPrefix, indent);
            }
        }

        /// <summary>Appends the structure of the <see cref="PlayableGraph"/>.</summary>
        private void AppendGraphDetails(
            StringBuilder text,
            Playable playable,
            string itemPrefix = "\n",
            string indent = Strings.Indent)
        {
            text.Append(itemPrefix);

            if (!playable.IsValid())
            {
                text.Append(NoPlayable);
                return;
            }

            text.Append(playable.GetPlayableType().Name);

            var inputCount = playable.GetInputCount();
            if (inputCount == 0)
                return;

            itemPrefix += indent;

            for (int i = 0; i < inputCount; i++)
            {
                var child = playable.GetInput(i);
                AppendGraphDetails(text, child, itemPrefix, indent);
            }
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Update
        /************************************************************************************************************************/

        /// <summary>[Pro-Only]
        /// Adds the `updatable` to the list that need to be updated before the playables if it wasn't there already.
        /// </summary>
        /// <remarks>
        /// The <see cref="Animator.updateMode"/> determines the update rate.
        /// <para></para>
        /// This method is safe to call at any time, even during an update.
        /// <para></para>
        /// The execution order is non-deterministic. Specifically, the most recently added will be updated first and
        /// <see cref="CancelPreUpdate"/> will change the order by swapping the last one into the place of the removed
        /// object.
        /// </remarks>
        public void RequirePreUpdate(IUpdatable updatable)
        {
#if UNITY_ASSERTIONS
            if (updatable is AnimancerNode node)
            {
                Validate.AssertPlayable(node);
                Validate.AssertGraph(node, this);
            }
#endif

            _PreUpdatables.Add(updatable);
        }

        /************************************************************************************************************************/

        /// <summary>[Pro-Only]
        /// Adds the `updatable` to the list that need to be updated after the playables if it wasn't there already.
        /// </summary>
        /// <remarks>
        /// The <see cref="Animator.updateMode"/> determines the update rate.
        /// <para></para>
        /// This method is safe to call at any time, even during an update.
        /// <para></para>
        /// The execution order is non-deterministic.
        /// Specifically, the most recently added will be updated first and <see cref="CancelPostUpdate"/>
        /// will change the order by swapping the last one into the place of the removed object.
        /// </remarks>
        public void RequirePostUpdate(IUpdatable updatable)
        {
#if UNITY_ASSERTIONS
            if (updatable is AnimancerNode node)
            {
                Validate.AssertPlayable(node);
                Validate.AssertGraph(node, this);
            }
#endif

            _PostUpdatables.Add(updatable);
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Removes the `updatable` from the list of objects
        /// that need to be updated before the playables.
        /// </summary>
        /// <remarks>
        /// This method is safe to call at any time, even during an update.
        /// <para></para>
        /// The last element is swapped into the place of the one being removed
        /// so that the rest of them don't need to be moved down one place to fill the gap.
        /// This is more efficient, but means that the update order can change.
        /// </remarks>
        public bool CancelPreUpdate(IUpdatable updatable)
            => _PreUpdatables.Remove(updatable);

        /// <summary>
        /// Removes the `updatable` from the list of objects
        /// that need to be updated after the playebles.
        /// </summary>
        /// <remarks>
        /// This method is safe to call at any time, even during an update.
        /// <para></para>
        /// The last element is swapped into the place of the one being removed
        /// so that the rest of them don't need to be moved down one place to fill the gap.
        /// This is more efficient, but means that the update order can change.
        /// </remarks>
        public bool CancelPostUpdate(IUpdatable updatable)
            => _PostUpdatables.Remove(updatable);

        /************************************************************************************************************************/

        /// <summary>The graph currently being executed.</summary>
        /// <remarks>
        /// During <see cref="AnimancerEvent"/> invocations,
        /// use <c>AnimancerEvent.Current.State.Graph</c> instead.
        /// </remarks>
        public static AnimancerGraph Current { get; private set; }

        /// <summary>The current <see cref="FrameData.deltaTime"/>.</summary>
        /// <remarks>After each update, this property will be left at its most recent value.</remarks>
        public static float DeltaTime { get; private set; }

        /// <summary>The current <see cref="FrameData.frameId"/>.</summary>
        /// <remarks>
        /// After each update, this property will be left at its most recent value.
        /// <para></para>
        /// <see cref="AnimancerState.Time"/> uses this value to determine
        /// whether it has accessed the playable's time
        /// since it was last updated in order to cache its value.
        /// </remarks>
        public ulong FrameID { get; private set; }

        /************************************************************************************************************************/

        /// <summary>[Internal] Calls <see cref="IUpdatable.Update"/> on each of the `updatables`.</summary>
        internal void UpdateAll(IUpdatable.List updatables, float deltaTime, ulong frameID)
        {
            var previous = Current;
            Current = this;

            DeltaTime = deltaTime;

            updatables.UpdateAll();

            if (FrameID != frameID)// Pre-Update.
            {
                // Any time before or during this method will still have all Playables at their time from last frame,
                // so we don't want them to think their time is dirty until we' a're done with the pre-update.
                FrameID = frameID;

                AssertPreUpdate();
            }
            else// Post-Update.
            {
                for (int i = _Layers.Count - 1; i >= 0; i--)
                    _Layers[i].UpdateEvents();
            }

            Current = previous;
        }

        /************************************************************************************************************************/

        /// <summary>[Assert-Conditional] Called during the pre-update to perform msome safety checks.</summary>
        [System.Diagnostics.Conditional(Strings.Assertions)]
        private void AssertPreUpdate()
        {
#if UNITY_ASSERTIONS
            if (OptionalWarning.AnimatorSpeed.IsEnabled() &&
                Component != null)
            {
                var animator = Component.Animator;
                if (animator != null &&
                    animator.speed != 1 &&
                    animator.runtimeAnimatorController == null)
                {
                    animator.speed = 1;
                    OptionalWarning.AnimatorSpeed.Log(
                        $"{nameof(Animator)}.{nameof(Animator.speed)} doesn't affect {nameof(Animancer)}." +
                        $" Use {nameof(AnimancerGraph)}.{nameof(Speed)} instead.", animator);
                }
            }
#endif
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
    }
}

