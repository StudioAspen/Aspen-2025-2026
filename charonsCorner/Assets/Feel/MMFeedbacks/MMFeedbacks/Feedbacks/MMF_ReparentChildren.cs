using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// This feedback will let you reparent all children of a list of target transforms to their current parent's level.
    /// </summary>
    [AddComponentMenu("")]
    [FeedbackHelp("This feedback will let you reparent all children of a list of target transforms to their current parent's level.")]
    [MovedFrom(false, null, "MoreMountains.Feedbacks")]
    [System.Serializable]
    [FeedbackPath("Custom/Reparent Children")]
    public class MMF_ReparentChildren : MMF_Feedback
    {
        /// a static bool used to disable all feedbacks of this type at once
        public static bool FeedbackTypeAuthorized = true;
        
        /// sets the inspector color for this feedback
        #if UNITY_EDITOR
        public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.TransformColor; } }
        public override bool EvaluateRequiresSetup() { return (TargetTransforms == null) || (TargetTransforms.Count == 0); }
        public override string RequiredTargetText { get { return TargetTransforms != null && TargetTransforms.Count > 0 ? TargetTransforms.Count + " transforms" : ""; } }
        public override string RequiresSetupText { get { return "This feedback requires at least one TargetTransform to be set to be able to work properly. You can set them below."; } }
        #endif

        [MMFInspectorGroup("Reparent Children", true, 61)]
        /// the list of transforms to reparent children from
        [Tooltip("the list of transforms to reparent children from")]
        public List<Transform> TargetTransforms;

        /// <summary>
        /// On Play we reparent children from each target transform
        /// </summary>
        /// <param name="position"></param>
        /// <param name="feedbacksIntensity"></param>
        protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
        {
            if (!Active || !FeedbackTypeAuthorized || (TargetTransforms == null))
            {
                return;
            }

            foreach (Transform target in TargetTransforms)
            {
                if (target == null)
                {
                    continue;
                }

                int childCount = target.childCount;
                for (int i = childCount - 1; i >= 0; i--)
                {
                    target.GetChild(i).SetParent(target.parent);
                }
            }
        }
    }
}
