using UnityEngine;

public class Particlecloudrotator : MonoBehaviour
{
    public Transform particleTransform;
    public Quaternion startRotation;
    public Quaternion upToNormal;
    public ParticleSystem PS;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        startRotation = particleTransform.rotation;
    }

    public void OnCollisionEnter(Collision collision)
    {
        ContactPoint contact = collision.contacts[0];
        Vector3 normal = contact.normal;


        upToNormal = Quaternion.FromToRotation(Vector3.up, normal);

        particleTransform.rotation = upToNormal * startRotation;

        PS.Play();
    }
}
