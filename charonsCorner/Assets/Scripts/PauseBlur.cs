using UnityEngine;
using UnityEngine.Rendering;

public class PauseBlur : MonoBehaviour
{
    public Volume volume;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            volume.enabled = !volume.enabled;
        }
    }
}