using System;
using UnityEngine;

namespace CharonsCorner.Runtime
{ 
    public class ParticleCloudPlayer : MonoBehaviour
    {
        [SerializeField] private Transform _particleTransform;
        [SerializeField] private Quaternion _startRotation;
        [SerializeField] private Quaternion _upToNormal;
        [SerializeField] private ParticleSystem _dustTrailParticles;
        [SerializeField] private ParticleSystem _cloudLandingParticles;
        [SerializeField] private float _maxAirTime = 0.5f;
    
        private bool _canParticleTrigger;
    
        private float _timer;
    
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            _startRotation = _particleTransform.rotation;
        }
        
        void Update()
        {
            if (_canParticleTrigger)
            {
                _timer += Time.deltaTime;
            }
            if (_timer >= _maxAirTime)
            {
                // Debug.Log("playing dust trail");
                _dustTrailParticles.Play();
            }
        }
        
        public void OnCollisionEnter(Collision collision)
        {
            ContactPoint contact = collision.contacts[0];
            Vector3 normal = contact.normal;
    
            _upToNormal = Quaternion.FromToRotation(Vector3.up, normal);
    
            _particleTransform.rotation = _upToNormal * _startRotation;
            _particleTransform.position = contact.point;
    
            _dustTrailParticles.Play();
    
            if (_timer >= _maxAirTime)
            {
                _cloudLandingParticles.Play();  
            }
            
            // resets after hitting the ground
            EnableParticleTrigger(false);
            _timer = 0;
        }
        public void OnCollisionExit(Collision collision)
        {
            _dustTrailParticles.Stop();
        }
            
        public void EnableParticleTrigger(bool canParticleTrigger) => _canParticleTrigger = canParticleTrigger;
    }
}
