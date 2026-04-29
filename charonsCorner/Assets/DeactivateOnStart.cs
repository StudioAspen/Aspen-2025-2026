using UnityEngine;

public class DeactivateOnStart : MonoBehaviour
{
    private void Awake()
    {
        gameObject.SetActive(false);
    }
}
