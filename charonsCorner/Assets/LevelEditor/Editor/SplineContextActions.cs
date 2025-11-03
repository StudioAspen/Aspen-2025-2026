using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Splines;
using UnityEngine;
using UnityEngine.Splines;

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
            List<SelectedSplineElementInfo> elementInfos = SplineToolEditorUtility.GetSelection();
            
            return elementInfos.Count >= 2;
        }

        /// <summary>
        /// Creates an intersection from the selected spline elements and add it to the SplinePath.
        /// </summary>
        [MenuItem("CONTEXT/SplineToolContext/Create Intersection")]
        private static void CreateIntersectionAction()
        {
            // Get selected spline elements
            List<SelectedSplineElementInfo> elementInfos = SplineToolEditorUtility.GetSelection();
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
        
        /// <summary>
        /// 
        /// </summary>
        [MenuItem("CONTEXT/SplineToolContext/Create Scale Handle")]
        private static void CreateScaleHandle(MenuCommand cmd)
        {
            // Get selected spline elements
            SplineContainer splineContainer = (SplineContainer)cmd.context; // <- the component you clicked
            List<SplineInfo> splineInfos = new List<SplineInfo>();
            // Get closest spline point on each spline
            for (int i = 0; i < splineContainer.Splines.Count; i++)
            {
                splineInfos.Add(new SplineInfo(splineContainer, i));
            }

            SplineToolEditorUtility.TryGetNearestPositionOnCurve(splineInfos, out SplineHit splineHit);
            Debug.Log(splineHit.Position);
        }

        /// <summary>
        /// Recreate the mesh used for the spline, useful to break duplicated mesh references.
        /// </summary>
        [MenuItem("CONTEXT/SplineToolContext/Recreate Spline Mesh")]
        private static void RecreateSplineMesh(MenuCommand cmd)
        {
            // Get selected spline elements
            SplineContainer splineContainer = (SplineContainer)cmd.context; // <- the component you clicked

            if (splineContainer != null)
            {
                // Create new mesh filter
                MeshFilter meshFilter = splineContainer.gameObject.GetComponent<MeshFilter>();
                meshFilter.sharedMesh = new Mesh();
                
                // Recook spline path
                SplinePath splinePath = splineContainer.gameObject.GetComponent<SplinePath>();
                splinePath.CookSplinePath();
            }
        } 
    }
}

#endif // UNITY_EDITOR
