using UnityEngine;

public class LightPointOnPlayer : MonoBehaviour
{
    [SerializeField] private Light _light;
    [SerializeField] private Transform _playerTransform;
    [SerializeField] private float _radius = 5f;
    [SerializeField] private float _minIntensity = 0f;
    [SerializeField] private float _maxIntensity = 1f;

    void Update()
    {
        if (_playerTransform == null || _light == null)
            return;

        float distanceX = Mathf.Abs(_playerTransform.position.x - transform.position.x);
        
        // Calculate normalized distance (0 at center, 1 at radius)
        float t = Mathf.Clamp01(distanceX / _radius);
        
        // Invert t so it's 1 at center and 0 at radius for fading in
        float intensity = Mathf.Lerp(_maxIntensity, _minIntensity, t);
        
        _light.intensity = intensity;
        
        // Optional: toggle GameObject state if intensity is 0 to save performance, 
        // but the prompt asked to go up/down towards min/max.
    }
}
