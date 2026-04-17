using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class PauseBlur : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        { 
            PostProcessVolume ppVolume = Camera.main.GetComponent<PostProcessVolume>(); 
            ppVolume.enabled = !ppVolume.enabled; 
        }
    }
}
