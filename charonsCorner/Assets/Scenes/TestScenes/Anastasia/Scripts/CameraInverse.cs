using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.Rendering.Universal;
[ExecuteInEditMode]

public class CameraInverse : MonoBehaviour
{
    //public Material Mat;
    public bool Inv;
    public GameObject PostProcess;
    private ColorAdjustments ColorAdj;

    void Start()
    {
        var volume = PostProcess.GetComponent<Volume>();
        volume.profile.TryGet(out ColorAdj);
    }

    void Update()
    {
        if (PostProcess == null)
        {
             Debug.LogError("PostProcess is NOT assigned!");
        } else {
            
            if (Inv) {
                ColorAdj.hueShift.value = 180f;
                //Debug.Log("Inverse True");
            }

            else {
            ColorAdj.hueShift.value = 0f;
            //Debug.Log("Inverse False");
            }   

        }
        
    }    
}
