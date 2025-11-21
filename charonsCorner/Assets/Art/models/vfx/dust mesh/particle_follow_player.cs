using UnityEngine;

public class particle_follow_player : MonoBehaviour
{
    public Transform Parent;
    public Vector3 Offset;
    void Start()
    {
        Offset = transform.localPosition - Parent.localPosition;
    }

    // Update is called once per frame
    void Update()
    {
        transform.localPosition = Parent.localPosition + Offset;
    }
}
