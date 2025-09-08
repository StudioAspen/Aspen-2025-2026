using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Splines;

namespace CharonsCorner.LevelEditor.Editor
{
    public static class SplineContextActions
    {
        /// <summary>
        /// Validate the Create Intersection context action.
        /// </summary>
        /// <returns>Returns true if at least two spline elements are selected.</returns>
        [MenuItem("CONTEXT/SplineToolContext/Create Intersection", validate = true)]
        private static bool ValidateCreateIntersectionAction()
        {
            List<SelectedSplineElementInfo> elementInfos = SplineEditorUtility.GetSelection();
            
            return elementInfos.Count >= 2;
        }

        /// <summary>
        /// Creates an intersection from the selected spline elements and add it to the SplinePath.
        /// </summary>
        [MenuItem("CONTEXT/SplineToolContext/Create Intersection")]
        private static void CreateIntersectionAction()
        {
            // Get selected spline elements
            List<SelectedSplineElementInfo> elementInfos = SplineEditorUtility.GetSelection();
            SplinePath splinePath = elementInfos[0].target.GetComponent<SplinePath>();

            Intersection intersection = new Intersection();
            intersection.junctions = new List<Junction>();
            
            // Create a list of all the intersection information
            foreach (SelectedSplineElementInfo elementInfo in elementInfos)
            {
                intersection.junctions.Add(new Junction(elementInfo.targetIndex, 
                    elementInfo.knotIndex, elementInfo.target));
            }

            // Add intersection and cook the spline path
            splinePath.AddIntersection(intersection);
            splinePath.CookSplinePath();
        }
    }
}

#endif // UNITY_EDITOR
