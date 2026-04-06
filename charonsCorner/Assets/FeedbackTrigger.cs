using System.Collections.Generic;
using MoreMountains.Feedbacks;
using UnityEngine;
using CharonsCorner.Runtime;

public class FeedbackTrigger : MonoBehaviour
{
    [SerializeField] private List<MMF_Player> _feedbacks;

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out GameplayPlayerController _))
        {
            foreach (var feedback in _feedbacks)
            {
                if (feedback != null)
                {
                    feedback.PlayFeedbacks();
                }
            }
        }
    }

    private void OnDrawGizmos()
    {
        BoxCollider boxCollider = GetComponent<BoxCollider>();
        if (boxCollider == null) return;

        Gizmos.color = Color.cyan;
        Matrix4x4 oldMatrix = Gizmos.matrix;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireCube(boxCollider.center, boxCollider.size);
        Gizmos.matrix = oldMatrix;
    }
}
