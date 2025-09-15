using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine.Splines;

namespace UnityEditor.Splines
{
    #if UNITY_EDITOR
    
    public struct SelectedSplineElementInfo
    {
        public SplineContainer target;
        public int targetIndex;
        public int knotIndex;

        public SelectedSplineElementInfo(SplineContainer argSplineContainer, int argIndex, int argKnotIndex)
        {
            target = argSplineContainer;
            targetIndex = argIndex;
            knotIndex = argKnotIndex;
        }
    }

    public struct SplineHit
    {
        public float T;
        public float3 Normal;
        public float3 Position;
        public SelectableKnot PreviousKnot;
        public SelectableKnot NextKnot;
    }
    
    /// <summary>
    /// Editor utility for the spline.
    /// </summary>
    public static class SplineToolEditorUtility
    {
        /// <summary>
        /// Check if there are spline elements selected.
        /// </summary>
        /// <returns>Returns true if spline elements aer selected.</returns>
        public static bool HasSelection()
        {
            return SplineSelection.HasActiveSplineSelection();
        }

        /// <summary>
        /// Get the selected spline elements.
        /// </summary>
        /// <returns>Returns a list of the selected spline elements.</returns>
        public static List<SelectedSplineElementInfo> GetSelection()
        {
            // Get internal struct data
            List<SelectableSplineElement> elements = SplineSelection.selection;

            // Create empty list to store the info of each element
            List<SelectedSplineElementInfo> elementInfos = new List<SelectedSplineElementInfo>();

            // Store the needed info of each element
            foreach (SelectableSplineElement element in elements)
            {
                elementInfos.Add(new SelectedSplineElementInfo(
                    element.target as SplineContainer, element.targetIndex, element.knotIndex));
            }

            return elementInfos;
        }

        public static bool TryGetNearestPositionOnCurve(IReadOnlyList<SplineInfo> splines, out SplineHit hit, float maxDistance = SplineHandleUtility.pickingDistance)
        {
            bool success = EditorSplineUtility.TryGetNearestPositionOnCurve(splines, out SplineCurveHit splineCurveHit, maxDistance);

            hit = new SplineHit()
            {
                T = splineCurveHit.T,
                Normal = splineCurveHit.Normal,
                Position = splineCurveHit.Position,
                PreviousKnot = splineCurveHit.PreviousKnot,
                NextKnot = splineCurveHit.NextKnot
            };

            return success;
        }
    }
    
    #endif // UNITY_EDITOR
}
