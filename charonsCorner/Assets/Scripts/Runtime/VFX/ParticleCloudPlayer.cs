using System;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    public class ParticleCloudPlayer : MonoBehaviour
{
    [SerializeField] private Transform _particleTransform;
    [SerializeField] private Quaternion _startRotation;
    [SerializeField] private Quaternion _upToNormal;
    [SerializeField] private ParticleSystem _DustTrailParticles;
    [SerializeField] private ParticleSystem _CloudLandingParticles;
    [SerializeField] private float _MaxAirTime;

    public bool _canParticleTrigger;

    public float timer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _startRotation = _particleTransform.rotation;
    }
    void Update()
    {
        if (_canParticleTrigger)
        {
            timer += Time.deltaTime;
        }
        if (timer >= _MaxAirTime)
        {
            Debug.Log("playing dust trail");
            _DustTrailParticles.Play();
        }
        

    }

    public void OnCollisionEnter(Collision collision)
    {
        ContactPoint contact = collision.contacts[0];
        Vector3 normal = contact.normal;
        

        // resets after hitting the ground
        _canParticleTrigger = false;
        timer = 0;

        _upToNormal = Quaternion.FromToRotation(Vector3.up, normal);

        _particleTransform.rotation = _upToNormal * _startRotation;

        _DustTrailParticles.Play();

        // if (timer >= _MaxAirTime)
        // {
        //     _CloudLandingParticles.Play();  
        //     timer = 0;
        // }
        
    }
        public void OnCollisionExit(Collision collision)
        {
            _DustTrailParticles.Stop();
        }
    }
}
