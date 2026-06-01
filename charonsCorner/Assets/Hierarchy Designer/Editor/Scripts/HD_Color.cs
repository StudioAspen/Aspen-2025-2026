#if UNITY_EDITOR
using System;
using System.Globalization;
using UnityEngine;

namespace HierarchyDesigner
{
    internal static class HD_Color
    {
        #region Properties
        #region Hierarchy
        public static readonly Color DefaultDarkThemeSkinColor = HexToColor("#383838");
        public static readonly Color DefaultLightThemeSkinColor = HexToColor("#C8C8C8");
        public static readonly Color DefaultDarkThemeSkinRowColor = HexToColor("#333333");
        public static readonly Color DefaultLightThemeSkinRowColor = HexToColor("#CFCFCF");
        public static readonly Color HighlightedDarkThemeSkinColor = HexToColor("#464646");
        public static readonly Color HighlightedLightThemeSkinColor = HexToColor("#B5B5B5");
        public static readonly Color HighlightedDarkThemeSkinRowColor = HexToColor("#414141");
        public static readonly Color HighlightedLightThemeSkinRowColor = HexToColor("#BCBCBC");
        public static readonly Color HighlightedFocusedDarkThemeSkinColor = HexToColor("#4D4D4D");
        public static readonly Color HighlightedFocusedLightThemeSkinColor = HexToColor("#AEAEAE");
        public static readonly Color HighlightedFocusedDarkThemeSkinRowColor = HexToColor("#484848");
        public static readonly Color HighlightedFocusedLightThemeSkinRowColor = HexToColor("#B5B5B5");
        public static readonly Color SelectedDarkThemeSkinColor = HexToColor("#2D5C8E");
        public static readonly Color SelectedLightThemeSkinColor = HexToColor("#3372B7");
        public static readonly Color SelectedDarkThemeSkinRowColor = HexToColor("#225585");
        public static readonly Color SelectedLightThemeSkinRowColor = HexToColor("#4A82C1");
        public static readonly Color RowColorDarkThemeSkinColor = HexToColor("00000015");
        public static readonly Color RowColorLightThemeSkinColor = HexToColor("FFFFFF20");
        #endregion

        #region Hierarchy Designer
        public static readonly Color PrimaryFontColorDark = HexToColor("#FFFFFF");
        public static readonly Color PrimaryFontColorLight = HexToColor("#000000");
        public static readonly Color PrimaryFontColorFadedDark = HexToColor("#F2F2F280");
        public static readonly Color PrimaryFontColorFadedLight = HexToColor("#0D0D0D80");
        public static readonly Color SecondaryFontColorDark = HexToColor("#6AFF5A");
        public static readonly Color SecondaryFontColorLight = HexToColor("#50C044");
        public static readonly Color SecondaryFontColorFadedDark = HexToColor("#6AFF5A80");
        public static readonly Color SecondaryFontColorFadedLight = HexToColor("#50C04480");
        public static readonly Color TertiaryFontColorDark = HexToColor("#FFF35A");
        public static readonly Color TertiaryFontColorLight = HexToColor("#C0B744");
        public static readonly Color PrimaryPanelColorTopDark = HexToColor("#181818"); 
        public static readonly Color PrimaryPanelColorTopLight = HexToColor("#E7E7E7");
        public static readonly Color PrimaryPanelColorMiddleDark = HexToColor("#141414");
        public static readonly Color PrimaryPanelColorMiddleLight = HexToColor("#EBEBEB");
        public static readonly Color PrimaryPanelColorBottomDark = HexToColor("#121212");
        public static readonly Color PrimaryPanelColorBottomLight = HexToColor("#EDEDED");
        public static readonly Color SecondaryPanelColorTopDark = HexToColor("#282828");
        public static readonly Color SecondaryPanelColorTopLight = HexToColor("#D7D7D7");
        public static readonly Color SecondaryPanelColorBottomDark = HexToColor("#232323");
        public static readonly Color SecondaryPanelColorBottomLight = HexToColor("#DCDCDC");
        public static readonly Color TertiaryPanelColorTopDark = HexToColor("#1E1E1EC0");
        public static readonly Color TertiaryPanelColorTopLight = HexToColor("#E1E1E1C0");
        public static readonly Color TertiaryPanelColorBottomDark = HexToColor("#00000080");
        public static readonly Color TertiaryPanelColorBottomLight = HexToColor("#FFFFFF80");
        public static readonly Color PopupPanelColorDark = HexToColor("#323232");
        public static readonly Color PopupPanelColorLight = HexToColor("#CDCDCD");
        #endregion
        #endregion

        #region Getters
        #region Hierarchy
        public static Color GetDefaultEditorBackgroundColor()
        {
            return HD_Editor.IsProSkin ? DefaultDarkThemeSkinColor : DefaultLightThemeSkinColor;
        }

        public static Color GetDefaultEditorRowBackgroundColor()
        {
            return HD_Editor.IsProSkin ? DefaultDarkThemeSkinRowColor : DefaultLightThemeSkinRowColor;
        }

        public static Color GetHighlightedEditorColor()
        {
            return HD_Editor.IsProSkin ? HighlightedDarkThemeSkinColor : HighlightedLightThemeSkinColor;
        }

        public static Color GetHighlightedEditorRowColor()
        {
            return HD_Editor.IsProSkin ? HighlightedDarkThemeSkinRowColor : HighlightedLightThemeSkinRowColor;
        }

        public static Color GetHighlightedFocusedEditorColor()
        {
            return HD_Editor.IsProSkin ? HighlightedFocusedDarkThemeSkinColor : HighlightedFocusedLightThemeSkinColor;
        }

        public static Color GetHighlightedFocusedEditorRowColor()
        {
            return HD_Editor.IsProSkin ? HighlightedFocusedDarkThemeSkinRowColor : HighlightedFocusedLightThemeSkinRowColor;
        }

        public static Color GetSelectedEditorColor()
        {
            return HD_Editor.IsProSkin ? SelectedDarkThemeSkinColor : SelectedLightThemeSkinColor;
        }

        public static Color GetSelectedEditorRowColor()
        {
            return HD_Editor.IsProSkin ? SelectedDarkThemeSkinRowColor : SelectedLightThemeSkinRowColor;
        }

        public static Color GetRowColor()
        {
            return HD_Editor.IsProSkin ? RowColorDarkThemeSkinColor : RowColorLightThemeSkinColor;
        }
        #endregion

        #region Hierarchy Designer
        public static Color GetPrimaryFontColor()
        {
            return HD_Editor.IsProSkin ? PrimaryFontColorDark : PrimaryFontColorLight;
        }

        public static Color GetPrimaryFontColorFaded()
        {
            return HD_Editor.IsProSkin ? PrimaryFontColorFadedDark : PrimaryFontColorFadedLight;
        }

        public static Color GetSecondaryFontColor()
        {
            return HD_Editor.IsProSkin ? SecondaryFontColorDark : SecondaryFontColorLight;
        }

        public static Color GetSecondaryFontColorFaded()
        {
            return HD_Editor.IsProSkin ? SecondaryFontColorFadedDark : SecondaryFontColorFadedLight;
        }

        public static Color GetTertiaryFontColor()
        {
            return HD_Editor.IsProSkin ? TertiaryFontColorDark : TertiaryFontColorLight;
        }

        public static Color GetPrimaryPanelColorTop()
        {
            return HD_Editor.IsProSkin ? PrimaryPanelColorTopDark : PrimaryPanelColorTopLight;
        }

        public static Color GetPrimaryPanelColorMiddle()
        {
            return HD_Editor.IsProSkin ? PrimaryPanelColorMiddleDark : PrimaryPanelColorMiddleLight;
        }

        public static Color GetPrimaryPanelColorBottom()
        {
            return HD_Editor.IsProSkin ? PrimaryPanelColorBottomDark : PrimaryPanelColorBottomLight;
        }

        public static Color GetSecondaryPanelColorTop()
        {
            return HD_Editor.IsProSkin ? SecondaryPanelColorTopDark : SecondaryPanelColorTopLight;
        }

        public static Color GetSecondaryPanelColorBottom()
        {
            return HD_Editor.IsProSkin ? SecondaryPanelColorBottomDark : SecondaryPanelColorBottomLight;
        }

        public static Color GetTertiaryPanelColorTop()
        {
            return HD_Editor.IsProSkin ? TertiaryPanelColorTopDark : TertiaryPanelColorTopLight;
        }

        public static Color GetTertiaryPanelColorBottom()
        {
            return HD_Editor.IsProSkin ? TertiaryPanelColorBottomDark : TertiaryPanelColorBottomLight;
        }

        public static Color GetPopupPanelColor()
        {
            return HD_Editor.IsProSkin ? PopupPanelColorDark : PopupPanelColorLight;
        }
        #endregion
        #endregion

        #region Methods
        public static Color HexToColor(string hex)
        {
            try
            {
                hex = hex.Replace("0x", "").Replace("#", "");
                byte a = 255;
                byte r = byte.Parse(hex.Substring(0, 2), NumberStyles.HexNumber);
                byte g = byte.Parse(hex.Substring(2, 2), NumberStyles.HexNumber);
                byte b = byte.Parse(hex.Substring(4, 2), NumberStyles.HexNumber);
                if (hex.Length == 8)
                {
                    a = byte.Parse(hex.Substring(6, 2), NumberStyles.HexNumber);
                }
                return new Color32(r, g, b, a);
            }
            catch (Exception ex)
            {
                Debug.LogError("Color parsing failed: " + ex.Message);
                return Color.white;
            }
        }

        public static Color GetDarkenedInactiveColor(Color color, float alphaFactor)
        {
            float luminance = color.r * 0.2126f + color.g * 0.7152f + color.b * 0.0722f;
            Color inactiveColor = luminance > 0.25f ? Color.Lerp(color, Color.black, 0.45f) : color;
            inactiveColor.a = color.a * Mathf.Clamp01(alphaFactor);
            return inactiveColor;
        }

        public static Gradient CreateGradient(GradientMode mode, params (string hexColor, int alpha, float locationPercentage)[] colorAlphaPairs)
        {
            int length = colorAlphaPairs.Length;
            Gradient gradient = new();
            GradientColorKey[] colorKeys = new GradientColorKey[length];
            GradientAlphaKey[] alphaKeys = new GradientAlphaKey[length];

            for (int i = 0; i < length; i++)
            {
                float location = colorAlphaPairs[i].locationPercentage / 100f;
                colorKeys[i] = new(HexToColor(colorAlphaPairs[i].hexColor), location);
                alphaKeys[i] = new(colorAlphaPairs[i].alpha / 255f, location);
            }

            gradient.colorKeys = colorKeys;
            gradient.alphaKeys = alphaKeys;
            gradient.mode = mode;

            return gradient;
        }

        public static Gradient CopyGradient(Gradient original)
        {
            if (original == null) return new();
            Gradient newGradient = new();
            newGradient.SetKeys(original.colorKeys, original.alphaKeys);
            return newGradient;
        }
        #endregion
    }
}
#endif