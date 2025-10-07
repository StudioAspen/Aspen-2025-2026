// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

using System;
using Unity.Collections;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace Animancer
{
    /// <summary>A scripted animation for an <see cref="AnimationJobState{T}"/>.</summary>
    /// <remarks>
    /// If <see cref="IDisposable.Dispose"/> is implemented,
    /// it will be called when the state is destroyed or disposed.
    /// </remarks>
    public interface IAnimancerStateJob : IDisposable
    {
        /************************************************************************************************************************/

        /// <summary>The total time this job would take to play in seconds at normal speed.</summary>
        float Length { get; }

        /// <summary>Defines what do to when processing the root motion.</summary>
        /// <remarks>
        /// This is called by <see cref="IAnimationJob.ProcessRootMotion"/>
        /// and receives the <see cref="AnimancerState.TimeD"/>.
        /// </remarks>
        void ProcessRootMotion(AnimationStream stream, double time) { }

        /// <summary>Defines what do to when processing the animation.</summary>
        /// <remarks>
        /// This is called by <see cref="IAnimationJob.ProcessAnimation"/>
        /// and receives the <see cref="AnimancerState.TimeD"/>.
        /// </remarks>
        void ProcessAnimation(AnimationStream stream, double time);

        /// <summary>Disposes of any resources used by this job.</summary>
        void IDisposable.Dispose() { }

        /************************************************************************************************************************/
    }

    /// <summary>An <see cref="AnimancerState"/> which plays an <see cref="IAnimancerStateJob"/>.</summary>
    /// <remarks>
    /// <strong>Documentation:</strong>
    /// <see href="https://kybernetik.com.au/animancer/docs/manual/playing/states">
    /// States</see>
    /// </remarks>
    /// https://kybernetik.com.au/animancer/api/Animancer/AnimationJobState_1
    /// 
    public class AnimationJobState<T> : AnimancerState, IUpdatable, IDisposable
        where T : struct, IAnimancerStateJob
    {
        /************************************************************************************************************************/

        /// <summary>
        /// An <see cref="IAnimancerStateJob"/> which wraps an <see cref="IAnimancerStateJob"/>
        /// in order to manage a <see cref="Time"/> array which can feed the <see cref="AnimancerState.TimeD"/>
        /// into the job.
        /// </summary>
        public struct TimedJob : IAnimationJob, IDisposable
        {
            /// <summary>The <see cref="IAnimancerStateJob"/> data.</summary>
            public T Job;

            /// <summary>The <see cref="AnimancerState.TimeD"/> to be passed to the job.</summary>
            public NativeArray<double> Time;

            /// <summary>Cleans up the unmanaged resources used by this job.</summary>
            public readonly void Dispose()
            {
                if (Time.IsCreated)
                    Time.Dispose();

                Job.Dispose();
            }

            /// <summary>Defines what do to when processing the root motion.</summary>
            public readonly void ProcessRootMotion(AnimationStream stream)
                => Job.ProcessRootMotion(stream, Time[0]);

            /// <summary>Defines what do to when processing the animation.</summary>
            public readonly void ProcessAnimation(AnimationStream stream)
                => Job.ProcessAnimation(stream, Time[0]);
        }

        /************************************************************************************************************************/

        private new AnimationScriptPlayable _Playable;
        private TimedJob _Job;

        /// <summary>The data of the job to be executed by this state.</summary>
        /// <remarks>
        /// Setting this value has a minor performance cost. If it needs to be changed frequently,
        /// consider using a single-item <c>NativeArray</c> in your job as demonstrated in the
        /// <see href="https://kybernetik.com.au/animancer/docs/samples/jobs/hit-impacts#angle">
        /// Hit Impacts</see> sample.
        /// </remarks>
        public T Job
        {
            get => _Job.Job;
            set
            {
                _Job.Job = value;
                _Playable.SetJobData(_Job);
            }
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override float Length
            => _Job.Job.Length;

        /************************************************************************************************************************/

        /// <inheritdoc/>
        int IUpdatable.UpdatableIndex { get; set; } = IUpdatable.List.NotInList;

        /************************************************************************************************************************/

        /// <summary>Creates a new <see cref="AnimationJobState{T}"/>.</summary>
        public AnimationJobState(T job)
        {
            _Job.Job = job;
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override void SetGraph(AnimancerGraph graph)
        {
            Graph?.Disposables.Remove(this);
            base.SetGraph(graph);
            Graph?.Disposables.Add(this);
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        protected override void CreatePlayable(out Playable playable)
        {
            if (!_Job.Time.IsCreated)
                _Job.Time = AnimancerUtilities.CreateNativeReference<double>();

            playable = _Playable = AnimationScriptPlayable.Create(Graph._PlayableGraph, _Job);
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        protected override void OnSetIsPlaying()
        {
            base.OnSetIsPlaying();

            if (IsPlaying)
                Graph.RequirePreUpdate(this);
            else
                Graph.CancelPreUpdate(this);
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Called every frame before the job is applied to send the
        /// <see cref="AnimancerState.Time"/> to the <see cref="Job"/>.
        /// </summary>
        public virtual void Update()
        {
            var time = _Job.Time;
            time[0] = TimeD;
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        void IDisposable.Dispose()
            => _Job.Dispose();

        /// <inheritdoc/>
        public override void Destroy()
        {
            base.Destroy();

            if (Graph != null)
            {
                Graph.CancelPreUpdate(this);
                Graph.Disposables.Remove(this);
            }

            _Job.Dispose();
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override string ToString()
            => _Job.Job.ToString();

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override AnimancerState Clone(CloneContext context)
        {
            var clone = new AnimationJobState<T>(_Job.Job);
            clone.CopyFrom(this, context);
            return clone;
        }

        /************************************************************************************************************************/
    }
}

