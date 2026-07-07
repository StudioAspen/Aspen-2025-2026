using UnityEngine;
using UnityEngine.Splines;

#if UNITY_EDITOR
using UnityEditor;

namespace CharonsCorner.LevelEditor
{
	
    [CustomEditor(typeof(SplinePath))]
    public class SplinePathEditor : UnityEditor.Editor
    {
	    [SerializeField] private Material _defaultMaterial;
	    
	    private SplinePath _splinePath;
	    private SplineContainer _splineContainer;
	    
	    /// <summary>
	    /// Called on enable in the editor
	    /// </summary>
        private void OnEnable()
        {
	        _splinePath = (SplinePath)target;
	        _splineContainer = _splinePath.GetComponent<SplineContainer>();
	        
	        if (_splinePath.splineContainer == null)
	        {
		        _splinePath.splineContainer = _splineContainer;
	        }
	        
	        Spline.Changed += OnSplineChanged;
	        
	        _splinePath.CookSplinePath();
        }

	    /// <summary>
	    /// Called on disable in the editor
	    /// </summary>
        private void OnDisable()
        { 
	        Spline.Changed -= OnSplineChanged;
        }
        
	    /// <summary>
	    /// Called on validate
	    /// </summary>
        private void OnValidate()
        {
            _splinePath.CookSplinePath();
        }
        
	    /// <summary>
	    /// Called when the spline is changed.
	    /// </summary>
	    /// <param name="spline">Default arg</param>
	    /// <param name="i">Default arg</param>
	    /// <param name="arg3">Default arg</param>
        private void OnSplineChanged(Spline spline, int i, SplineModification arg3)
        {
            if (_splineContainer != null)
            {
                bool belongsToContainer = false;
                foreach (var s in _splineContainer.Splines)
                {
                    if (s == spline)
                    {
                        belongsToContainer = true;
                        break;
                    }
                }
                
                if (belongsToContainer)
                {
                    _splinePath.CookSplinePath();
                }
            }
        }
    }
}
#endif // UNITY_EDITOR