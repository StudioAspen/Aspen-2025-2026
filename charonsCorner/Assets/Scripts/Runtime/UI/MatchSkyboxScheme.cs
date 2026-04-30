using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MoreMountains.Tools;

[ExecuteInEditMode]
public class MatchSkyboxScheme : MonoBehaviour, MMEventListener<MMGameEvent>
{
    [SerializeField] private Image targetImage;
    [SerializeField] private List<Image> borderImages;
    [SerializeField] private List<Button> targetButtons;
    [SerializeField] private float borderBrightnessMultiplier = 1.2f;
    [SerializeField] private float targetImageBorderMultiplier = 1.0f;
    [SerializeField] private float targetImageBandMultiplier = 1.0f;
    [SerializeField] private float buttonNormalBrightness = 1.0f;
    [SerializeField] private float buttonHighlightedBrightness = 1.2f;
    [SerializeField] private float buttonPressedBrightness = 0.8f;
    [SerializeField] private float buttonSelectedBrightness = 1.1f;
    [SerializeField] private float blackThreshold = 0.1f;
    [SerializeField] private Color fallbackBorderColor = Color.white;
    [SerializeField] private Color fallbackTargetImageBorderColor = Color.white;
    [SerializeField] private Color fallbackTargetImageBandColor = Color.white;
    [SerializeField] private Color fallbackNormalColor = Color.white;
    [SerializeField] private Color fallbackHighlightedColor = Color.white;
    [SerializeField] private Color fallbackPressedColor = Color.gray;
    [SerializeField] private Color fallbackSelectedColor = Color.white;
    
    private static readonly int BorderColorId = Shader.PropertyToID("_BorderColor");
    private static readonly int BandColorId = Shader.PropertyToID("_BandColor");

    private void Start()
    {
        UpdateScheme();
    }

    private void OnEnable()
    {
        this.MMEventStartListening<MMGameEvent>();
    }

    private void OnDisable()
    {
        this.MMEventStopListening<MMGameEvent>();
    }

    public void OnMMEvent(MMGameEvent gameEvent)
    {
        if (gameEvent.EventName == "UpdateUIBasedOnCurrentSkybox")
        {
            UpdateScheme();
        }
    }

#if UNITY_EDITOR
    private void Update()
    {
        if (!Application.isPlaying)
        {
            UpdateScheme();
        }
    }
#endif

    [ContextMenu("Update Scheme")]
    public void UpdateScheme()
    {
        Material skyboxMaterial = RenderSettings.skybox;
        if (skyboxMaterial == null) return;

        if (skyboxMaterial.HasProperty(BorderColorId))
        {
            Color borderColor = skyboxMaterial.GetColor(BorderColorId);
            
            if (targetImage != null)
            {
                Material targetMat = targetImage.materialForRendering;
                if (targetMat != null)
                {
                    targetMat.SetColor(BorderColorId, borderColor);
                }
            }

            if (borderImages != null && borderImages.Count > 0)
            {
                Color bumpedColor = CalculateBumpedColor(borderColor, borderBrightnessMultiplier, fallbackBorderColor);

                foreach (var img in borderImages)
                {
                    if (img != null)
                    {
                        img.color = bumpedColor;
                    }
                }
            }

            if (targetButtons != null && targetButtons.Count > 0)
            {
                Color normalColor = CalculateBumpedColor(borderColor, buttonNormalBrightness, fallbackNormalColor);
                Color highlightedColor = CalculateBumpedColor(borderColor, buttonHighlightedBrightness, fallbackHighlightedColor);
                Color pressedColor = CalculateBumpedColor(borderColor, buttonPressedBrightness, fallbackPressedColor);
                Color selectedColor = CalculateBumpedColor(borderColor, buttonSelectedBrightness, fallbackSelectedColor);

                foreach (var btn in targetButtons)
                {
                    if (btn != null)
                    {
                        ColorBlock cb = btn.colors;
                        cb.normalColor = normalColor;
                        cb.highlightedColor = highlightedColor;
                        cb.pressedColor = pressedColor;
                        cb.selectedColor = selectedColor;
                        btn.colors = cb;
                    }
                }
            }
        }

        if (skyboxMaterial.HasProperty(BandColorId))
        {
            if (targetImage != null)
            {
                Material targetMat = targetImage.materialForRendering;
                if (targetMat != null)
                {
                    targetMat.SetColor(BandColorId, skyboxMaterial.GetColor(BandColorId));
                }
            }
        }
    }

    private Color CalculateBumpedColor(Color sourceColor, float multiplier, Color fallbackColor)
    {
        Color bumpedColor = sourceColor;
        
        // If the color is too dark, multiplication won't help, so use fallback
        if (bumpedColor.r < blackThreshold && bumpedColor.g < blackThreshold && bumpedColor.b < blackThreshold)
        {
            bumpedColor = fallbackColor;
        }
        else
        {
            bumpedColor.r *= multiplier;
            bumpedColor.g *= multiplier;
            bumpedColor.b *= multiplier;
        }

        return bumpedColor;
    }
}
