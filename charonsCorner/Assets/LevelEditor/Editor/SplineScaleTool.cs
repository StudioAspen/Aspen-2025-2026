using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

#if UNITY_EDITOR
using Unity.Mathematics;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEditor.Splines;
using UnityEngine.PlayerLoop;

namespace CharonsCorner.LevelEditor.Editor
{
    [EditorTool("Scale Path Tool", typeof(SplinePath), typeof(SplineToolContext))]
    public class SplineScaleTool : SplineTool
    {
        /// <summary>
        /// On activated, register OnSceneGUI
        /// </summary>
        public override void OnActivated()
        {
            SceneView.duringSceneGui += OnSceneGUI;
        }

        /// <summary>
        /// On deactivated, unregister OnSceneGUI
        /// </summary>
        public override void OnWillBeDeactivated()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
        }

        /// <summary>
        /// Handle spline scale tool logic in scene view.
        /// </summary>
        /// <param name="sceneView"></param>
        private void OnSceneGUI(SceneView sceneView)
        {
            // Cache event and necessary spline components
            Event e = Event.current;
            SplinePath splinePath = (SplinePath)target;
            SplineContainer splineContainer = splinePath.splineContainer;
            
            // Get the nearest hit position on all the splines
            List<SplineInfo> splineInfos = new List<SplineInfo>();
            for (int i = 0; i < splineContainer.Splines.Count; i++)
            {
                splineInfos.Add(new SplineInfo(splineContainer, i));
            }
            SplineToolEditorUtility.TryGetNearestPositionOnCurve(splineInfos, out SplineHit splineHit);
            
            // Draw preview handle for creating a new scale handle
            if ((Vector3)splineHit.Position != Vector3.zero)
            {
                Handles.DrawWireCube(splineHit.Position, Vector3.one);
            }

            // If mouse down, create spline handle at mouse location
            if (e.type == EventType.MouseDown && e.button == 0 && !e.alt)
            {
                // Return if no spline was hit
                Spline spline = splineHit.NextKnot.SplineInfo.Spline;
                if (spline == null)
                {
                    return;
                }
                
                // Register undo
                Undo.RecordObject(splineContainer, "Added path scale handle"); 
                
                // Get spline scale data
                SplineData<float> xScaleData = spline.GetOrCreateFloatData(SplineScale.X_SCALE_KEY);
                SplineData<float> yScaleData = spline.GetOrCreateFloatData(SplineScale.Y_SCALE_KEY);

                // Set new scale data point at hit normalized position
                Vector3 localPosition = splinePath.transform.InverseTransformPoint(splineHit.Position);
                SplineUtility.GetNearestPoint(spline, localPosition, out float3 nearestPoint, out float t);
                xScaleData.SetFloatAtT(t, 1);
                yScaleData.SetFloatAtT(t, 1);
            }
        }

        /// <summary>
        /// Handle tool logic in scene view.
        /// </summary>
        /// <param name="window"></param>
        public override void OnToolGUI(EditorWindow window)
        {
            // Return if active window is not scene view
            if (!(window is SceneView))
            {
                return;
            }

            foreach (var obj in targets)
            {
                // Continue if target is NOT a ISplineContainer
                if (!(obj is SplinePath splinePath))
                {
                    continue;
                }

                SplineContainer splineContainer = splinePath.splineContainer;

                // Handle scaling on each scale data point on the splines
                foreach (Spline spline in splineContainer.Splines)
                {
                    // Query scale data
                    SplineData<float> xScaleData = spline.GetOrCreateFloatData(SplineScale.X_SCALE_KEY);
                    SplineData<float> yScaleData = spline.GetOrCreateFloatData(SplineScale.Y_SCALE_KEY);

                    for (int i = 0; i < xScaleData.Count; i++)
                    {
                        EditorGUI.BeginChangeCheck();
                        
                        // Register undo
                        Undo.RecordObject(splineContainer, "Scaled spline path");
                        
                        // Evaluate position, quaternion, and scale of spline at t for scale handle, then draw scale handle
                        float t = xScaleData[i].Index;
                        spline.Evaluate(t, out float3 position, out float3 tangent, out float3 up);
                        position = splineContainer.transform.TransformPoint(position);
                        Quaternion quaternion = Quaternion.LookRotation(tangent, up);
                        Vector3 scale = new Vector3(xScaleData[i].Value, yScaleData[i].Value, 1);
                        
                        scale = Handles.ScaleHandle(scale, position, quaternion);

                        // If the scale handle was changed by the user, update spline scale data
                        bool changed = EditorGUI.EndChangeCheck();
                        if (changed)
                        {
                            xScaleData.SetFloatAtT(t, scale.x);
                            yScaleData.SetFloatAtT(t, scale.y);
                            
                            splinePath.CookSplinePath();
                        }
                    }
                }
            }
        }
    }
}

#endif // UNITY_EDITOR