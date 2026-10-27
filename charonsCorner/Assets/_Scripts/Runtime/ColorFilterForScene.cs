using UnityEngine;
using UnityEngine.Rendering;
#if MM_URP
using UnityEngine.Rendering.Universal;
#endif
using UnityEngine.SceneManagement;

/// <summary>
/// This script intakes a Volume component and edits the ColorFilter property in Color Adjustments for the scene.
/// It reverts the color adjustments back to what it was before when the scene is unloaded.
/// </summary>
public class ColorFilterForScene : MonoBehaviour
{
    [Header("Volume Settings")]
#if MM_URP
    [SerializeField] private Volume _targetVolume;
#endif
    [SerializeField] private Color _newColorFilter = Color.white;

#if MM_URP
    private ColorAdjustments _colorAdjustments;
    private Color _originalColor;
    private bool _hasOriginalColor = false;
    private VolumeProfile _profile;
#endif

    private void Awake()
    {
#if MM_URP
        if (_targetVolume == null)
        {
            _targetVolume = GetComponent<Volume>();
        }

        if (_targetVolume != null)
        {
            _profile = _targetVolume.profile;
            if (_profile.TryGet(out _colorAdjustments))
            {
                _originalColor = _colorAdjustments.colorFilter.value;
                _hasOriginalColor = true;
            }
        }
        else
        {
            Debug.LogWarning("[ColorFilterForScene] No Volume component assigned or found on this object.");
        }
#endif
    }

    private void Start()
    {
        ApplyFilter();
    }

    private void OnEnable()
    {
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
        RevertFilter();
    }

    private void ApplyFilter()
    {
#if MM_URP
        if (_colorAdjustments != null)
        {
            _colorAdjustments.colorFilter.Override(_newColorFilter);
        }
#endif
    }

    private void RevertFilter()
    {
#if MM_URP
        if (_colorAdjustments != null && _hasOriginalColor)
        {
            _colorAdjustments.colorFilter.Override(_originalColor);
        }
#endif
    }

    private void OnSceneUnloaded(Scene scene)
    {
        // Explicitly revert when scene is unloaded to ensure it doesn't persist if it's a shared profile asset
        RevertFilter();
    }

    private void OnDestroy()
    {
        RevertFilter();
    }
}
