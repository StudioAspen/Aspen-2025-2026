using UnityEngine;
using UnityEngine.UI;

public class UnscaledShaderTime : MonoBehaviour
{
    [SerializeField] private Image targetImage;
    private Material material;

    void Start()
    {
        material = targetImage.materialForRendering;
        material.SetFloat("_UnscaledTime", 0f);
    }
    void Update()
    {
        if (material != null && material.GetFloat("_UseUnscaledTime") > 0.5f)
        {
            material.SetFloat("_UnscaledTime", Time.unscaledTime * 0.05f);
        }
    }
}