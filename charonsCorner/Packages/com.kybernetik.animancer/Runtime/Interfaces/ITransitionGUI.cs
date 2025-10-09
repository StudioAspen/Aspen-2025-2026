// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

#if UNITY_EDITOR

using UnityEngine;

namespace Animancer.Editor
{
    /// <summary>[Editor-Only] An object that can draw custom GUI elements relating to transitions.</summary>
    /// <remarks>
    /// Implement this in a custom transition type to give it custom GUI elements.
    /// <para></para>
    /// <strong>Example:</strong><code>
    /// using Animancer;
    /// using UnityEngine;
    /// 
    /// // AttackTransition.cs contains your custom transition type.
    /// public partial class AttackTransition : ClipTransition
    /// {
    ///     [SerializeField] private Bounds _HitBox;
    ///     [SerializeField] private float _HitStartTime;
    ///     [SerializeField] private float _HitEndTime;
    /// 
    ///     // Damage, Knockback, etc.
    /// }
    /// 
    /// // AttackTransition.Drawer.cs contains the custom GUI for it.
    /// #if UNITY_EDITOR
    ///     
    /// using Animancer.Editor;
    /// using UnityEditor;
    /// using UnityEngine;
    /// 
    /// public partial class AttackTransition : ITransitionGUI
    /// {
    ///     // See each method for an example.
    ///     public void OnPreviewSceneGUI(TransitionPreviewDetails details) { }
    ///     public void OnTimelineBackgroundGUI() { }
    ///     public void OnTimelineForegroundGUI() { }
    /// }
    /// 
    /// #endif
    /// </code></remarks>
    /// https://kybernetik.com.au/animancer/api/Animancer.Editor/ITransitionGUI
    public interface ITransitionGUI
    {
        /************************************************************************************************************************/

        /// <summary>Called while drawing the GUI for the <see cref="Previews.TransitionPreviewWindow"/> scene.</summary>
        /// <remarks>
        /// <strong>Example:</strong><code>
        /// // For the AttackTransition example from ITransitionGUI.
        /// // Draw the hit box as a wireframe cube.
        /// public void OnPreviewSceneGUI(TransitionPreviewDetails details)
        /// {
        ///     Color color = Handles.color;
        ///     Handles.color = new(0.5f, 1, 0.5f);
        ///     
        ///     Transform transform = details.Transform;
        ///     
        ///     Handles.DrawWireCube(
        ///         transform.TransformPoint(_HitBox.center),
        ///         _HitBox.size);
        ///     
        ///     Handles.color = color;
        /// }
        /// </code></remarks>
        void OnPreviewSceneGUI(TransitionPreviewDetails details);

        /// <summary>
        /// Called while drawing the background GUI for the <see cref="TimelineGUI"/> for the
        /// <see cref="IHasEvents.Events"/>.
        /// </summary>
        /// <remarks>
        /// <strong>Example:</strong><code>
        /// // For the AttackTransition example from ITransitionGUI.
        /// // Draw the hit time as a highlighted area.
        /// public void OnTimelineBackgroundGUI()
        /// {
        ///     if (Event.current.type != EventType.Repaint)
        ///         return;
        /// 
        ///     Color previousColor = GUI.color;
        ///     TimelineGUI timelineGUI = TimelineGUI.Current;
        /// 
        ///     float start = timelineGUI.SecondsToPixels(_HitStartTime);
        ///     float end = timelineGUI.SecondsToPixels(_HitEndTime);
        ///     Rect area = new Rect(
        ///         start,
        ///         0,
        ///         end - start,
        ///         timelineGUI.Area.height - timelineGUI.TickHeight);
        /// 
        ///     Color color = new Color(0.9f, 0.4f, 0.25f, 0.5f);
        /// 
        ///     EditorGUI.DrawRect(area, color);
        /// 
        ///     GUI.color = previousColor;
        /// }
        /// </code></remarks>
        void OnTimelineBackgroundGUI();

        /// <summary>
        /// Called while drawing the foreground GUI for the <see cref="TimelineGUI"/> for the
        /// <see cref="IHasEvents.Events"/>.
        /// </summary>
        /// <remarks>
        /// This method can be used similarly to the <see cref="OnTimelineBackgroundGUI"/> example,
        /// except that it draws in front of everything else.
        /// </remarks>
        void OnTimelineForegroundGUI();

        /************************************************************************************************************************/
    }

    /// <summary>[Editor-Only] Details about the current preview used by <see cref="ITransitionGUI.OnPreviewSceneGUI"/>.</summary>
    /// https://kybernetik.com.au/animancer/api/Animancer.Editor/TransitionPreviewDetails
    public readonly struct TransitionPreviewDetails
    {
        /************************************************************************************************************************/

        /// <summary>The <see cref="AnimancerGraph"/> used to play the preview.</summary>
        public readonly AnimancerGraph Animancer;

        /// <summary>The <see cref="UnityEngine.Transform"/> of the <see cref="Animator"/> used to play the preview.</summary>
        public Transform Transform => Animancer.Component.Animator.transform;

        /************************************************************************************************************************/

        /// <summary>Creates a new <see cref="TransitionPreviewDetails"/>.</summary>
        public TransitionPreviewDetails(AnimancerGraph animancer)
        {
            Animancer = animancer;
        }

        /************************************************************************************************************************/
    }
}

#endif

