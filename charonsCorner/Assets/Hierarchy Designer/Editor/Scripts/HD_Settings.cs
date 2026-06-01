#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.ShortcutManagement;
using UnityEngine;

namespace HierarchyDesigner
{
    internal static class HD_Settings
    {
        #region Enums
        public enum HierarchyLayoutMode { Consecutive, Docked, Split }
        public enum HierarchyTreeMode { Default, Minimal}
        public enum TreeBranchImageType { Default, Curved, Dotted, Segmented }
        public enum HierarchyDesignerLocation { Author, Plugins, Tools, TopBar, Window }
        public enum UpdateMode { Dynamic, Smart }
        #endregion

        #region Classes
        #region General
        [System.Serializable]
        private class HD_GeneralSettings
        {
            public HierarchyLayoutMode LayoutMode = HierarchyLayoutMode.Split;
            public HierarchyTreeMode TreeMode = HierarchyTreeMode.Default;

            public bool EnableGameObjectMainIcon = true;
            public bool EnableGameObjectComponentIcons = true;
            public bool EnableHierarchyTree = true;
            public bool EnableGameObjectTag = true;
            public bool EnableGameObjectLayer = true;
            public bool EnableHierarchyRows = true;
            public bool EnableHierarchyLines = true;
            public bool EnableHierarchyButtons = true;
            public bool EnableHeaderUtilities = true;
            public bool EnableMajorShortcuts = true;
            public bool DisableHierarchyDesignerDuringPlayMode = true;

            public bool ExcludeFolderProperties = true;
            public List<string> ExcludedComponents = new() { "Transform", "RectTransform", "CanvasRenderer" };
            public int MaximumComponentIconsAmount = 10;
            public List<string> ExcludedTags = new() { "Untagged" };
            public List<string> ExcludedLayers = new() { "Default" };
        }
        private static HD_GeneralSettings generalSettings = new();
        #endregion

        #region Design
        [System.Serializable]
        private class HD_DesignSettings
        {
            public float ComponentIconsSize = 1f;
            public int ComponentIconsOffset = 21;
            public float ComponentIconsSpacing = 2f;
            public Color HierarchyTreeColor = Color.white;
            public TreeBranchImageType TreeBranchImageType_I = TreeBranchImageType.Default;
            public TreeBranchImageType TreeBranchImageType_L = TreeBranchImageType.Default;
            public TreeBranchImageType TreeBranchImageType_T = TreeBranchImageType.Default;
            public TreeBranchImageType TreeBranchImageType_TerminalBud = TreeBranchImageType.Default;
            public Color TagColor = Color.gray;
            public TextAnchor TagTextAnchor = TextAnchor.MiddleRight;
            public FontStyle TagFontStyle = FontStyle.BoldAndItalic;
            public int TagFontSize = 10;
            public Color LayerColor = Color.gray;
            public TextAnchor LayerTextAnchor = TextAnchor.MiddleLeft;
            public FontStyle LayerFontStyle = FontStyle.BoldAndItalic;
            public int LayerFontSize = 10;
            public int TagLayerOffset = 5;
            public int TagLayerSpacing = 5;
            public Color HierarchyLineColor = HD_Color.HexToColor("00000080");
            public int HierarchyLineThickness = 1;
            public Color HierarchyButtonLockColor = HD_Color.HexToColor("404040");
            public Color HierarchyButtonVisibilityColor = HD_Color.HexToColor("404040");
            public Color FolderDefaultTextColor = Color.white;
            public int FolderDefaultFontSize = 12;
            public FontStyle FolderDefaultFontStyle = FontStyle.Normal;
            public Color FolderDefaultImageColor = Color.white;
            public HD_Folders.FolderImageType FolderDefaultImageType = HD_Folders.FolderImageType.Default;
            public Color SeparatorDefaultTextColor = Color.white;
            public bool SeparatorDefaultIsGradientBackground = false;
            public Color SeparatorDefaultBackgroundColor = Color.gray;
            public Gradient SeparatorDefaultBackgroundGradient = new();
            public int SeparatorDefaultFontSize = 12;
            public FontStyle SeparatorDefaultFontStyle = FontStyle.Normal;
            public TextAnchor SeparatorDefaultTextAnchor = TextAnchor.MiddleCenter;
            public HD_Separators.SeparatorImageType SeparatorDefaultImageType = HD_Separators.SeparatorImageType.Default;
            public int SeparatorLeftSideTextAnchorOffset = 3;
            public int SeparatorCenterTextAnchorOffset = -15;
            public int SeparatorRightSideTextAnchorOffset = 36;
            public Color LockColor = Color.white;
            public TextAnchor LockTextAnchor = TextAnchor.MiddleCenter;
            public FontStyle LockFontStyle = FontStyle.BoldAndItalic;
            public int LockFontSize = 11;
        }
        private static HD_DesignSettings designSettings = new();
        #endregion

        #region Shortcuts
        [System.Serializable]
        private class HD_ShortcutsSettings
        {
            public KeyCode OpenIconPickerKeyCode = KeyCode.Mouse0;
            public KeyCode ToggleGameObjectActiveStateKeyCode = KeyCode.Mouse2;
            public KeyCode ToggleLockStateKeyCode = KeyCode.F1;
            public KeyCode ChangeTagLayerKeyCode = KeyCode.Mouse0;
            public KeyCode RenameSelectedGameObjectsKeyCode = KeyCode.F3;
        }
        private static HD_ShortcutsSettings shortcutsSettings = new();
        #endregion

        #region Advanced
        [System.Serializable]
        private class HD_AdvancedSettings
        {
            public HierarchyDesignerLocation HierarchyLocation = HierarchyDesignerLocation.Tools;
            public UpdateMode MainIconUpdateMode = UpdateMode.Dynamic;
            public UpdateMode ComponentsIconsUpdateMode = UpdateMode.Dynamic;
            public UpdateMode HierarchyTreeUpdateMode = UpdateMode.Dynamic;
            public UpdateMode TagUpdateMode = UpdateMode.Dynamic;
            public UpdateMode LayerUpdateMode = UpdateMode.Dynamic;
            public bool EnableDynamicBackgroundForGameObjectMainIcon = true;
            public bool EnablePreciseRectForDynamicBackgroundForGameObjectMainIcon = true;
            public bool EnableProjectTexturesInMainIconOverrideWindow = false;
            public bool EnableCustomizationForGameObjectComponentIcons = true;
            public bool EnableTooltipOnComponentIconHovered = true;
            public bool EnableActiveStateEffectForComponentIcons = true;
            public bool DisableComponentIconsForInactiveGameObjects = true;
            public bool UseHierarchyTreeColorForInactiveGameObjects = false;
            public bool EnableCustomInspectorUI = true;
            public bool EnableEditorUtilities = true;
            public bool IncludeBackgroundImageForGradientBackground = true;
            public bool ExpandHierarchyOnStartup = false;
            public bool ExcludeFoldersFromCountSelectToolCalculations = true;
            public bool ExcludeSeparatorsFromCountSelectToolCalculations = true;
        }
        private static HD_AdvancedSettings advancedSettings = new();
        #endregion
        #endregion

        #region Initialization
        public static void Initialize()
        {
            LoadSettings();
            LoadSettingsIntoCaches();

            InitializeAdvancedSettings();
        }

        private static void LoadSettings()
        {
            LoadGeneralSettings();
            LoadDesignSettings();
            LoadShortcutSettings();
            LoadAdvancedSettings();
        }

        private static void LoadSettingsIntoCaches()
        {
            // General Settings
            HD_Manager.LayoutModeCache = LayoutMode;
            HD_Manager.TreeModeCache = TreeMode;
            HD_Manager.EnableGameObjectMainIconCache = EnableGameObjectMainIcon;
            HD_Manager.EnableGameObjectComponentIconsCache = EnableGameObjectComponentIcons;
            HD_Manager.EnableHierarchyTreeCache = EnableHierarchyTree;
            HD_Manager.EnableGameObjectTagCache = EnableGameObjectTag;
            HD_Manager.EnableGameObjectLayerCache = EnableGameObjectLayer;
            HD_Manager.EnableHierarchyRowsCache = EnableHierarchyRows;
            HD_Manager.EnableHierarchyLinesCache = EnableHierarchyLines;
            HD_Manager.EnableHierarchyButtonsCache = EnableHierarchyButtons;
            HD_Manager.EnableHeaderUtilitiesrCache = EnableHeaderUtilities;
            HD_Manager.EnableMajorShortcutsCache = EnableMajorShortcuts;
            HD_Manager.DisableHierarchyDesignerDuringPlayModeCache = DisableHierarchyDesignerDuringPlayMode;
            HD_Manager.ExcludeFolderProperties = ExcludeFolderProperties;
            HD_Manager.ExcludedComponentsCache = ExcludedComponents;
            HD_Manager.MaximumComponentIconsAmountCache = MaximumComponentIconsAmount;
            HD_Manager.ExcludedTagsCache = ExcludedTags;
            HD_Manager.ExcludedLayersCache = ExcludedLayers;

            // Design Settings
            HD_Manager.ComponentIconsSizeCache = ComponentIconsSize;
            HD_Manager.ComponentIconsOffsetCache = ComponentIconsOffset;
            HD_Manager.ComponentIconsSpacingCache = ComponentIconsSpacing;
            HD_Manager.HierarchyTreeColorCache = HierarchyTreeColor;
            HD_Manager.TreeBranchImageType_ICache = TreeBranchImageType_I;
            HD_Manager.TreeBranchImageType_LCache = TreeBranchImageType_L;
            HD_Manager.TreeBranchImageType_TCache = TreeBranchImageType_T;
            HD_Manager.TreeBranchImageType_TerminalBudCache = TreeBranchImageType_TerminalBud;
            HD_Manager.TagColorCache = TagColor;
            HD_Manager.TagTextAnchorCache = TagTextAnchor;
            HD_Manager.TagFontStyleCache = TagFontStyle;
            HD_Manager.TagFontSizeCache = TagFontSize;
            HD_Manager.LayerColorCache = LayerColor;
            HD_Manager.LayerTextAnchorCache = LayerTextAnchor;
            HD_Manager.LayerFontStyleCache = LayerFontStyle;
            HD_Manager.LayerFontSizeCache = LayerFontSize;
            HD_Manager.TagLayerOffsetCache = TagLayerOffset;
            HD_Manager.TagLayerSpacingCache = TagLayerSpacing;
            HD_Manager.HierarchyLineColorCache = HierarchyLineColor;
            HD_Manager.HierarchyLineThicknessCache = HierarchyLineThickness;
            HD_Manager.SeparatorCenterTextAnchorOffsetCache = SeparatorCenterTextAnchorOffset;
            HD_Manager.SeparatorLeftSideTextAnchorOffsetCache = SeparatorLeftSideTextAnchorOffset;
            HD_Manager.SeparatorRightSideTextAnchorOffsetCache = SeparatorRightSideTextAnchorOffset;
            HD_Manager.LockColorCache = LockColor;
            HD_Manager.LockTextAnchorCache = LockTextAnchor;
            HD_Manager.LockFontStyleCache = LockFontStyle;
            HD_Manager.LockFontSizeCache = LockFontSize;
            HD_GUI.RefreshHierarchyButtonLockStyle();
            HD_GUI.RefreshHierarchyButtonVisibilityStyle();

            // Shortcut Settings
            HD_Manager.ToggleGameObjectActiveStateKeyCodeCache = ToggleGameObjectActiveStateKeyCode;
            HD_Manager.ToggleLockStateKeyCodeCache = ToggleLockStateKeyCode;
            HD_Manager.ChangeTagLayerKeyCodeCache = ChangeTagLayerKeyCode;
            HD_Manager.RenameSelectedGameObjectsKeyCodeCache = RenameSelectedGameObjectsKeyCode;
            HD_Manager.OpenIconPickerKeyCodeCache = OpenIconPickerKeyCode;

            // Advanced Settings
            HD_Manager.MainIconUpdateModeCache = MainIconUpdateMode;
            HD_Manager.ComponentsIconsUpdateModeCache = ComponentsIconsUpdateMode;
            HD_Manager.HierarchyTreeUpdateModeCache = HierarchyTreeUpdateMode;
            HD_Manager.TagUpdateModeCache = TagUpdateMode;
            HD_Manager.LayerUpdateModeCache = LayerUpdateMode;
            HD_Manager.EnableDynamicBackgroundForGameObjectMainIconCache = EnableDynamicBackgroundForGameObjectMainIcon;
            HD_Manager.EnablePreciseRectForDynamicBackgroundForGameObjectMainIconCache = EnablePreciseRectForDynamicBackgroundForGameObjectMainIcon;
            HD_Manager.DisableComponentIconsForInactiveGameObjectsCache = DisableComponentIconsForInactiveGameObjects;
            HD_Manager.UseHierarchyTreeColorForInactiveGameObjectsCache = UseHierarchyTreeColorForInactiveGameObjects;
            HD_Manager.EnableCustomizationForGameObjectComponentIconsCache = EnableCustomizationForGameObjectComponentIcons;
            HD_Manager.EnableTooltipOnComponentIconHoveredCache = EnableTooltipOnComponentIconHovered;
            HD_Manager.EnableActiveStateEffectForComponentIconsCache = EnableActiveStateEffectForComponentIcons;
            HD_Manager.IncludeBackgroundImageForGradientBackgroundCache = IncludeBackgroundImageForGradientBackground;
        }

        private static void InitializeAdvancedSettings()
        {
            string currentBase = ReadBaseHierarchyDesigner();
            string expectedBase = GetBaseHierarchyDesigner(advancedSettings.HierarchyLocation);
            if (currentBase != expectedBase) GenerateConstantsFile(HierarchyLocation);
            if (ExpandHierarchyOnStartup)
            {
                if (HD_Session.instance.ExpandHierarchyOnStartupOnce) return;
                HD_Session.instance.ExpandHierarchyOnStartupOnce = true;
                EditorApplication.delayCall += () => HD_Operations.ExpandAllGameObjects();
            }
        }
        #endregion

        #region Helpers
        private static string ReadBaseHierarchyDesigner()
        {
            string filePath = HD_File.GetScriptsFilePath(HD_Constants.ConstantClassTextFileName);
            if (File.Exists(filePath))
            {
                string[] lines = File.ReadAllLines(filePath);
                foreach (string line in lines)
                {
                    if (line.Contains("public const string AssetLocation ="))
                    {
                        int startIndex = line.IndexOf("\"") + 1;
                        int endIndex = line.LastIndexOf("\"");
                        return line[startIndex..endIndex];
                    }
                }
            }
            return null;
        }

        private static string GetBaseHierarchyDesigner(HierarchyDesignerLocation hierarchyLocation)
        {
            return hierarchyLocation switch
            {
                HierarchyDesignerLocation.Author => "Verpha/Hierarchy Designer",
                HierarchyDesignerLocation.Plugins => "Plugins/Hierarchy Designer",
                HierarchyDesignerLocation.Tools => "Tools/Hierarchy Designer",
                HierarchyDesignerLocation.TopBar => "Hierarchy Designer/Open Window",
                HierarchyDesignerLocation.Window => "Window/Hierarchy Designer",
                _ => "Tools/Hierarchy Designer"
            };
        }

        public static void GenerateConstantsFile(HierarchyDesignerLocation tempHierarchyLocation)
        {
            string filePath = HD_File.GetScriptsFilePath(HD_Constants.ConstantClassTextFileName);
            if (File.Exists(filePath))
            {
                string[] lines = File.ReadAllLines(filePath);
                for (int i = 0; i < lines.Length; i++)
                {
                    if (lines[i].TrimStart().StartsWith("public const string AssetLocation"))
                    {
                        lines[i] = $"        public const string AssetLocation = \"{GetBaseHierarchyDesigner(tempHierarchyLocation)}\";";
                        break;
                    }
                }
                File.WriteAllLines(filePath, lines, Encoding.UTF8);
            }
            AssetDatabase.Refresh();
        }
        #endregion

        #region Accessors
        #region General
        public static HierarchyLayoutMode LayoutMode
        {
            get => generalSettings.LayoutMode;
            set
            {
                if (generalSettings.LayoutMode != value)
                {
                    generalSettings.LayoutMode = value;
                    HD_Manager.LayoutModeCache = value;
                }
            }
        }

        public static HierarchyTreeMode TreeMode
        {
            get => generalSettings.TreeMode;
            set
            {
                if (generalSettings.TreeMode != value)
                {
                    generalSettings.TreeMode = value;
                    HD_Manager.TreeModeCache = value;
                }
            }
        }

        public static bool EnableGameObjectMainIcon
        {
            get => generalSettings.EnableGameObjectMainIcon;
            set
            {
                if (generalSettings.EnableGameObjectMainIcon != value)
                {
                    generalSettings.EnableGameObjectMainIcon = value;
                    HD_Manager.EnableGameObjectMainIconCache = value;
                }
            }
        }

        public static bool EnableGameObjectComponentIcons
        {
            get => generalSettings.EnableGameObjectComponentIcons;
            set
            {
                if (generalSettings.EnableGameObjectComponentIcons != value)
                {
                    generalSettings.EnableGameObjectComponentIcons = value;
                    HD_Manager.EnableGameObjectComponentIconsCache = value;
                }
            }
        }

        public static bool EnableHierarchyTree
        {
            get => generalSettings.EnableHierarchyTree;
            set
            {
                if (generalSettings.EnableHierarchyTree != value)
                {
                    generalSettings.EnableHierarchyTree = value;
                    HD_Manager.EnableHierarchyTreeCache = value;
                }
            }
        }

        public static bool EnableGameObjectTag
        {
            get => generalSettings.EnableGameObjectTag;
            set
            {
                if (generalSettings.EnableGameObjectTag != value)
                {
                    generalSettings.EnableGameObjectTag = value;
                    HD_Manager.EnableGameObjectTagCache = value;
                }
            }
        }

        public static bool EnableGameObjectLayer
        {
            get => generalSettings.EnableGameObjectLayer;
            set
            {
                if (generalSettings.EnableGameObjectLayer != value)
                {
                    generalSettings.EnableGameObjectLayer = value;
                    HD_Manager.EnableGameObjectLayerCache = value;
                }
            }
        }

        public static bool EnableHierarchyRows
        {
            get => generalSettings.EnableHierarchyRows;
            set
            {
                if (generalSettings.EnableHierarchyRows != value)
                {
                    generalSettings.EnableHierarchyRows = value;
                    HD_Manager.EnableHierarchyRowsCache = value;
                }
            }
        }

        public static bool EnableHierarchyLines
        {
            get => generalSettings.EnableHierarchyLines;
            set
            {
                if (generalSettings.EnableHierarchyLines != value)
                {
                    generalSettings.EnableHierarchyLines = value;
                    HD_Manager.EnableHierarchyLinesCache = value;
                }
            }
        }

        public static bool EnableHierarchyButtons
        {
            get => generalSettings.EnableHierarchyButtons;
            set
            {
                if (generalSettings.EnableHierarchyButtons != value)
                {
                    generalSettings.EnableHierarchyButtons = value;
                    HD_Manager.EnableHierarchyButtonsCache = value;
                }
            }
        }

        public static bool EnableHeaderUtilities
        {
            get => generalSettings.EnableHeaderUtilities;
            set
            {
                if (generalSettings.EnableHeaderUtilities != value)
                {
                    generalSettings.EnableHeaderUtilities = value;
                    HD_Manager.EnableHeaderUtilitiesrCache = value;
                }
            }
        }

        public static bool EnableMajorShortcuts
        {
            get => generalSettings.EnableMajorShortcuts;
            set
            {
                if (generalSettings.EnableMajorShortcuts != value)
                {
                    generalSettings.EnableMajorShortcuts = value;
                    HD_Manager.EnableMajorShortcutsCache = value;
                }
            }
        }

        public static bool DisableHierarchyDesignerDuringPlayMode
        {
            get => generalSettings.DisableHierarchyDesignerDuringPlayMode;
            set
            {
                if (generalSettings.DisableHierarchyDesignerDuringPlayMode != value)
                {
                    generalSettings.DisableHierarchyDesignerDuringPlayMode = value;
                    HD_Manager.DisableHierarchyDesignerDuringPlayModeCache = value;
                }
            }
        }

        public static bool ExcludeFolderProperties
        {
            get => generalSettings.ExcludeFolderProperties;
            set
            {
                if (generalSettings.ExcludeFolderProperties != value)
                {
                    generalSettings.ExcludeFolderProperties = value;
                    HD_Manager.ExcludeFolderProperties = value;
                }
            }
        }

        public static List<string> ExcludedComponents
        {
            get => generalSettings.ExcludedComponents;
            set
            {
                if (generalSettings.ExcludedComponents != value)
                {
                    generalSettings.ExcludedComponents = value;
                    HD_Manager.ExcludedComponentsCache = value;
                    HD_Manager.ClearGameObjectDataCache();
                }
            }
        }

        public static int MaximumComponentIconsAmount
        {
            get => generalSettings.MaximumComponentIconsAmount;
            set
            {
                int clampedValue = Mathf.Clamp(value, 1, 20);
                if (generalSettings.MaximumComponentIconsAmount != clampedValue)
                {
                    generalSettings.MaximumComponentIconsAmount = clampedValue;
                    HD_Manager.MaximumComponentIconsAmountCache = clampedValue;
                }
            }
        }

        public static List<string> ExcludedTags
        {
            get => generalSettings.ExcludedTags;
            set
            {
                if (generalSettings.ExcludedTags != value)
                {
                    generalSettings.ExcludedTags = value;
                    HD_Manager.ExcludedTagsCache = value;
                }
            }
        }

        public static List<string> ExcludedLayers
        {
            get => generalSettings.ExcludedLayers;
            set
            {
                if (generalSettings.ExcludedLayers != value)
                {
                    generalSettings.ExcludedLayers = value;
                    HD_Manager.ExcludedLayersCache = value;
                }
            }
        }
        #endregion

        #region Design
        public static float ComponentIconsSize
        {
            get => designSettings.ComponentIconsSize;
            set
            {
                float clamped = Mathf.Clamp(value, 0.5f, 1.0f);
                if (!Mathf.Approximately(designSettings.ComponentIconsSize, clamped))
                {
                    designSettings.ComponentIconsSize = clamped;
                    HD_Manager.ComponentIconsSizeCache = clamped;
                }
            }
        }

        public static int ComponentIconsOffset
        {
            get => designSettings.ComponentIconsOffset;
            set
            {
                int clamped = Mathf.Clamp(value, 15, 30);
                if (designSettings.ComponentIconsOffset != clamped)
                {
                    designSettings.ComponentIconsOffset = clamped;
                    HD_Manager.ComponentIconsOffsetCache = clamped;
                }
            }
        }

        public static float ComponentIconsSpacing
        {
            get => designSettings.ComponentIconsSpacing;
            set
            {
                float clamped = Mathf.Clamp(value, 0.0f, 10.0f);
                if (!Mathf.Approximately(designSettings.ComponentIconsSpacing, clamped))
                {
                    designSettings.ComponentIconsSpacing = clamped;
                    HD_Manager.ComponentIconsSpacingCache = clamped;
                }
            }
        }

        public static Color HierarchyTreeColor
        {
            get => designSettings.HierarchyTreeColor;
            set
            {
                if (designSettings.HierarchyTreeColor != value)
                {
                    designSettings.HierarchyTreeColor = value;
                    HD_Manager.HierarchyTreeColorCache = value;
                }
            }
        }

        public static TreeBranchImageType TreeBranchImageType_I
        {
            get => designSettings.TreeBranchImageType_I;
            set
            {
                if (designSettings.TreeBranchImageType_I != value)
                {
                    designSettings.TreeBranchImageType_I = value;
                    HD_Manager.TreeBranchImageType_ICache = value;
                }
            }
        }

        public static TreeBranchImageType TreeBranchImageType_L
        {
            get => designSettings.TreeBranchImageType_L;
            set
            {
                if (designSettings.TreeBranchImageType_L != value)
                {
                    designSettings.TreeBranchImageType_L = value;
                    HD_Manager.TreeBranchImageType_LCache = value;
                }
            }
        }

        public static TreeBranchImageType TreeBranchImageType_T
        {
            get => designSettings.TreeBranchImageType_T;
            set
            {
                if (designSettings.TreeBranchImageType_T != value)
                {
                    designSettings.TreeBranchImageType_T = value;
                    HD_Manager.TreeBranchImageType_TCache = value;
                }
            }
        }

        public static TreeBranchImageType TreeBranchImageType_TerminalBud
        {
            get => designSettings.TreeBranchImageType_TerminalBud;
            set
            {
                if (designSettings.TreeBranchImageType_TerminalBud != value)
                {
                    designSettings.TreeBranchImageType_TerminalBud = value;
                    HD_Manager.TreeBranchImageType_TerminalBudCache = value;
                }
            }
        }

        public static Color TagColor
        {
            get => designSettings.TagColor;
            set
            {
                if (designSettings.TagColor != value)
                {
                    designSettings.TagColor = value;
                    HD_Manager.TagColorCache = value;
                }
            }
        }

        public static TextAnchor TagTextAnchor
        {
            get => designSettings.TagTextAnchor;
            set
            {
                if (designSettings.TagTextAnchor != value)
                {
                    designSettings.TagTextAnchor = value;
                    HD_Manager.TagTextAnchorCache = value;
                }
            }
        }

        public static FontStyle TagFontStyle
        {
            get => designSettings.TagFontStyle;
            set
            {
                if (designSettings.TagFontStyle != value)
                {
                    designSettings.TagFontStyle = value;
                    HD_Manager.TagFontStyleCache = value;
                }
            }
        }

        public static int TagFontSize
        {
            get => designSettings.TagFontSize;
            set
            {
                int clamped = Mathf.Clamp(value, 7, 21);
                if (designSettings.TagFontSize != clamped)
                {
                    designSettings.TagFontSize = clamped;
                    HD_Manager.TagFontSizeCache = clamped;
                }
            }
        }

        public static Color LayerColor
        {
            get => designSettings.LayerColor;
            set
            {
                if (designSettings.LayerColor != value)
                {
                    designSettings.LayerColor = value;
                    HD_Manager.LayerColorCache = value;
                }
            }
        }

        public static TextAnchor LayerTextAnchor
        {
            get => designSettings.LayerTextAnchor;
            set
            {
                if (designSettings.LayerTextAnchor != value)
                {
                    designSettings.LayerTextAnchor = value;
                    HD_Manager.LayerTextAnchorCache = value;
                }
            }
        }

        public static FontStyle LayerFontStyle
        {
            get => designSettings.LayerFontStyle;
            set
            {
                if (designSettings.LayerFontStyle != value)
                {
                    designSettings.LayerFontStyle = value;
                    HD_Manager.LayerFontStyleCache = value;
                }
            }
        }

        public static int LayerFontSize
        {
            get => designSettings.LayerFontSize;
            set
            {
                int clamped = Mathf.Clamp(value, 7, 21);
                if (designSettings.LayerFontSize != clamped)
                {
                    designSettings.LayerFontSize = clamped;
                    HD_Manager.LayerFontSizeCache = clamped;
                }
            }
        }

        public static int TagLayerOffset
        {
            get => designSettings.TagLayerOffset;
            set
            {
                int clamped = Mathf.Clamp(value, 0, 20);
                if (designSettings.TagLayerOffset != clamped)
                {
                    designSettings.TagLayerOffset = clamped;
                    HD_Manager.TagLayerOffsetCache = clamped;
                }
            }
        }

        public static int TagLayerSpacing
        {
            get => designSettings.TagLayerSpacing;
            set
            {
                int clamped = Mathf.Clamp(value, 0, 20);
                if (designSettings.TagLayerSpacing != clamped)
                {
                    designSettings.TagLayerSpacing = clamped;
                    HD_Manager.TagLayerSpacingCache = clamped;
                }
            }
        }

        public static Color HierarchyLineColor
        {
            get => designSettings.HierarchyLineColor;
            set
            {
                if (designSettings.HierarchyLineColor != value)
                {
                    designSettings.HierarchyLineColor = value;
                    HD_Manager.HierarchyLineColorCache = value;
                }
            }
        }

        public static int HierarchyLineThickness
        {
            get => designSettings.HierarchyLineThickness;
            set
            {
                int clamped = Mathf.Clamp(value, 1, 3);
                if (designSettings.HierarchyLineThickness != clamped)
                {
                    designSettings.HierarchyLineThickness = clamped;
                    HD_Manager.HierarchyLineThicknessCache = clamped;
                }
            }
        }

        public static Color HierarchyButtonLockColor
        {
            get => designSettings.HierarchyButtonLockColor;
            set
            {
                if (designSettings.HierarchyButtonLockColor != value)
                {
                    designSettings.HierarchyButtonLockColor = value;
                    HD_GUI.RefreshHierarchyButtonLockStyle();
                }
            }
        }

        public static Color HierarchyButtonVisibilityColor
        {
            get => designSettings.HierarchyButtonVisibilityColor;
            set
            {
                if (designSettings.HierarchyButtonVisibilityColor != value)
                {
                    designSettings.HierarchyButtonVisibilityColor = value;
                    HD_GUI.RefreshHierarchyButtonVisibilityStyle();
                }
            }
        }

        public static Color FolderDefaultTextColor
        {
            get => designSettings.FolderDefaultTextColor;
            set
            {
                if (designSettings.FolderDefaultTextColor != value)
                {
                    designSettings.FolderDefaultTextColor = value;
                }
            }
        }

        public static int FolderDefaultFontSize
        {
            get => designSettings.FolderDefaultFontSize;
            set
            {
                int clamped = Mathf.Clamp(value, 7, 21);
                if (designSettings.FolderDefaultFontSize != clamped)
                {
                    designSettings.FolderDefaultFontSize = clamped;
                }
            }
        }

        public static FontStyle FolderDefaultFontStyle
        {
            get => designSettings.FolderDefaultFontStyle;
            set
            {
                if (designSettings.FolderDefaultFontStyle != value)
                {
                    designSettings.FolderDefaultFontStyle = value;
                }
            }
        }

        public static Color FolderDefaultImageColor
        {
            get => designSettings.FolderDefaultImageColor;
            set
            {
                if (designSettings.FolderDefaultImageColor != value)
                {
                    designSettings.FolderDefaultImageColor = value;
                }
            }
        }

        public static HD_Folders.FolderImageType FolderDefaultImageType
        {
            get => designSettings.FolderDefaultImageType;
            set
            {
                if (designSettings.FolderDefaultImageType != value)
                {
                    designSettings.FolderDefaultImageType = value;
                }
            }
        }

        public static Color SeparatorDefaultTextColor
        {
            get => designSettings.SeparatorDefaultTextColor;
            set
            {
                if (designSettings.SeparatorDefaultTextColor != value)
                {
                    designSettings.SeparatorDefaultTextColor = value;
                }
            }
        }

        public static bool SeparatorDefaultIsGradientBackground
        {
            get => designSettings.SeparatorDefaultIsGradientBackground;
            set
            {
                if (designSettings.SeparatorDefaultIsGradientBackground != value)
                {
                    designSettings.SeparatorDefaultIsGradientBackground = value;
                }
            }
        }

        public static Color SeparatorDefaultBackgroundColor
        {
            get => designSettings.SeparatorDefaultBackgroundColor;
            set
            {
                if (designSettings.SeparatorDefaultBackgroundColor != value)
                {
                    designSettings.SeparatorDefaultBackgroundColor = value;
                }
            }
        }

        public static Gradient SeparatorDefaultBackgroundGradient
        {
            get => designSettings.SeparatorDefaultBackgroundGradient;
            set
            {
                if (designSettings.SeparatorDefaultBackgroundGradient != value)
                {
                    designSettings.SeparatorDefaultBackgroundGradient = value;
                }
            }
        }

        public static int SeparatorDefaultFontSize
        {
            get => designSettings.SeparatorDefaultFontSize;
            set
            {
                int clamped = Mathf.Clamp(value, 7, 21);
                if (designSettings.SeparatorDefaultFontSize != clamped)
                {
                    designSettings.SeparatorDefaultFontSize = clamped;
                }
            }
        }

        public static FontStyle SeparatorDefaultFontStyle
        {
            get => designSettings.SeparatorDefaultFontStyle;
            set
            {
                if (designSettings.SeparatorDefaultFontStyle != value)
                {
                    designSettings.SeparatorDefaultFontStyle = value;
                }
            }
        }

        public static TextAnchor SeparatorDefaultTextAnchor
        {
            get => designSettings.SeparatorDefaultTextAnchor;
            set
            {
                if (designSettings.SeparatorDefaultTextAnchor != value)
                {
                    designSettings.SeparatorDefaultTextAnchor = value;
                }
            }
        }

        public static HD_Separators.SeparatorImageType SeparatorDefaultImageType
        {
            get => designSettings.SeparatorDefaultImageType;
            set
            {
                if (designSettings.SeparatorDefaultImageType != value)
                {
                    designSettings.SeparatorDefaultImageType = value;
                }
            }
        }

        public static int SeparatorLeftSideTextAnchorOffset
        {
            get => designSettings.SeparatorLeftSideTextAnchorOffset;
            set
            {
                int clamped = Mathf.Clamp(value, 0, 33);
                if (designSettings.SeparatorLeftSideTextAnchorOffset != clamped)
                {
                    designSettings.SeparatorLeftSideTextAnchorOffset = clamped;
                    HD_Manager.SeparatorLeftSideTextAnchorOffsetCache = clamped;
                }
            }
        }

        public static int SeparatorCenterTextAnchorOffset
        {
            get => designSettings.SeparatorCenterTextAnchorOffset;
            set
            {
                int clamped = Mathf.Clamp(value, -66, 66);
                if (designSettings.SeparatorCenterTextAnchorOffset != clamped)
                {
                    designSettings.SeparatorCenterTextAnchorOffset = clamped;
                    HD_Manager.SeparatorCenterTextAnchorOffsetCache = clamped;
                }
            }
        }

        public static int SeparatorRightSideTextAnchorOffset
        {
            get => designSettings.SeparatorRightSideTextAnchorOffset;
            set
            {
                int clamped = Mathf.Clamp(value, 33, 66);
                if (designSettings.SeparatorRightSideTextAnchorOffset != clamped)
                {
                    designSettings.SeparatorRightSideTextAnchorOffset = clamped;
                    HD_Manager.SeparatorRightSideTextAnchorOffsetCache = clamped;
                }
            }
        }

        public static Color LockColor
        {
            get => designSettings.LockColor;
            set
            {
                if (designSettings.LockColor != value)
                {
                    designSettings.LockColor = value;
                    HD_Manager.LockColorCache = value;
                }
            }
        }

        public static TextAnchor LockTextAnchor
        {
            get => designSettings.LockTextAnchor;
            set
            {
                if (designSettings.LockTextAnchor != value)
                {
                    designSettings.LockTextAnchor = value;
                    HD_Manager.LockTextAnchorCache = value;
                }
            }
        }

        public static FontStyle LockFontStyle
        {
            get => designSettings.LockFontStyle;
            set
            {
                if (designSettings.LockFontStyle != value)
                {
                    designSettings.LockFontStyle = value;
                    HD_Manager.LockFontStyleCache = value;
                }
            }
        }

        public static int LockFontSize
        {
            get => designSettings.LockFontSize;
            set
            {
                int clamped = Mathf.Clamp(value, 7, 21);
                if (designSettings.LockFontSize != clamped)
                {
                    designSettings.LockFontSize = clamped;
                    HD_Manager.LockFontSizeCache = clamped;
                }
            }
        }
        #endregion

        #region Shortcuts
        #region Major Shortcuts
        public static KeyCode OpenIconPickerKeyCode
        {
            get => shortcutsSettings.OpenIconPickerKeyCode;
            set
            {
                if (shortcutsSettings.OpenIconPickerKeyCode != value)
                {
                    shortcutsSettings.OpenIconPickerKeyCode = value;
                    HD_Manager.OpenIconPickerKeyCodeCache = value;
                }
            }
        }

        public static KeyCode ToggleGameObjectActiveStateKeyCode
        {
            get => shortcutsSettings.ToggleGameObjectActiveStateKeyCode;
            set
            {
                if (shortcutsSettings.ToggleGameObjectActiveStateKeyCode != value)
                {
                    shortcutsSettings.ToggleGameObjectActiveStateKeyCode = value;
                    HD_Manager.ToggleGameObjectActiveStateKeyCodeCache = value;
                }
            }
        }

        public static KeyCode ToggleLockStateKeyCode
        {
            get => shortcutsSettings.ToggleLockStateKeyCode;
            set
            {
                if (shortcutsSettings.ToggleLockStateKeyCode != value)
                {
                    shortcutsSettings.ToggleLockStateKeyCode = value;
                    HD_Manager.ToggleLockStateKeyCodeCache = value;
                }
            }
        }

        public static KeyCode ChangeTagLayerKeyCode
        {
            get => shortcutsSettings.ChangeTagLayerKeyCode;
            set
            {
                if (shortcutsSettings.ChangeTagLayerKeyCode != value)
                {
                    shortcutsSettings.ChangeTagLayerKeyCode = value;
                    HD_Manager.ChangeTagLayerKeyCodeCache = value;
                }
            }
        }

        public static KeyCode RenameSelectedGameObjectsKeyCode
        {
            get => shortcutsSettings.RenameSelectedGameObjectsKeyCode;
            set
            {
                if (shortcutsSettings.RenameSelectedGameObjectsKeyCode != value)
                {
                    shortcutsSettings.RenameSelectedGameObjectsKeyCode = value;
                    HD_Manager.RenameSelectedGameObjectsKeyCodeCache = value;
                }
            }
        }
        #endregion

        #region Minor Shortcuts
        #region Windows
        [Shortcut("Hierarchy Designer/Open Hierarchy Designer Window", KeyCode.Alpha1, ShortcutModifiers.Alt)]
        private static void OpenHierarchyDesignerWindow()
        {
            HD_Main.OpenWindow();
        }

        [Shortcut("Hierarchy Designer/Open Folder Panel", KeyCode.Alpha2, ShortcutModifiers.Alt)]
        private static void OpenFolderManagerPanel()
        {
            HD_Main.SwitchWindow(HD_Main.CurrentWindow.Folders);
            HD_Main.OpenWindow();
        }

        [Shortcut("Hierarchy Designer/Open Separator Panel", KeyCode.Alpha3, ShortcutModifiers.Alt)]
        private static void OpenSeparatorManagerPanel()
        {
            HD_Main.SwitchWindow(HD_Main.CurrentWindow.Separators);
            HD_Main.OpenWindow();
        }

        [Shortcut("Hierarchy Designer/Open Tools Panel")]
        private static void OpenToolsPanel()
        {
            HD_Main.SwitchWindow(HD_Main.CurrentWindow.Tools);
            HD_Main.OpenWindow();
        }

        [Shortcut("Hierarchy Designer/Open Presets Panel")]
        private static void OpenPresetsPanel()
        {
            HD_Main.SwitchWindow(HD_Main.CurrentWindow.Presets);
            HD_Main.OpenWindow();
        }

        [Shortcut("Hierarchy Designer/Open Preset Creator Panel")]
        private static void OpenPresetCreatorPanel()
        {
            HD_Main.SwitchWindow(HD_Main.CurrentWindow.PresetCreator);
            HD_Main.OpenWindow();
        }

        [Shortcut("Hierarchy Designer/Open General Settings Panel")]
        private static void OpenGeneralSettingsPanel()
        {
            HD_Main.SwitchWindow(HD_Main.CurrentWindow.GeneralSettings);
            HD_Main.OpenWindow();
        }

        [Shortcut("Hierarchy Designer/Open Design Settings Panel")]
        private static void OpenDesignSettingsPanel()
        {
            HD_Main.SwitchWindow(HD_Main.CurrentWindow.DesignSettings);
            HD_Main.OpenWindow();
        }

        [Shortcut("Hierarchy Designer/Open Shortcut Settings Panel")]
        private static void OpenShortcutSettingsPanel()
        {
            HD_Main.SwitchWindow(HD_Main.CurrentWindow.ShortcutSettings);
            HD_Main.OpenWindow();
        }

        [Shortcut("Hierarchy Designer/Open Advanced Settings Panel")]
        private static void OpenAdvancedSettingsPanel()
        {
            HD_Main.SwitchWindow(HD_Main.CurrentWindow.AdvancedSettings);
            HD_Main.OpenWindow();
        }

        [Shortcut("Hierarchy Designer/Open Rename Tool Window")]
        private static void OpenRenameToolWindow()
        {
            HD_Rename.OpenWindow(null, true, 0);
        }
        #endregion

        #region Create
        [Shortcut("Hierarchy Designer/Create All Folders")]
        private static void CreateAllHierarchyFolders() => HD_Menu.CreateAllFolders();

        [Shortcut("Hierarchy Designer/Create Default Folder")]
        private static void CreateDefaultHierarchyFolder() => HD_Menu.CreateDefaultFolder();

        [Shortcut("Hierarchy Designer/Create Missing Folders")]
        private static void CreateMissingHierarchyFolders() => HD_Menu.CreateMissingFolders();

        [Shortcut("Hierarchy Designer/Create All Separators")]
        private static void CreateAllHierarchySeparators() => HD_Menu.CreateAllSeparators();

        [Shortcut("Hierarchy Designer/Create Default Separator")]
        private static void CreateDefaultHierarchySeparator() => HD_Menu.CreateDefaultSeparator();

        [Shortcut("Hierarchy Designer/Create Missing Separators")]
        private static void CreateMissingHierarchySeparators() => HD_Menu.CreateMissingSeparators();
        #endregion

        #region Refresh
        [Shortcut("Hierarchy Designer/Refresh All GameObjects' Data", KeyCode.R, ShortcutModifiers.Shift)]
        private static void RefreshAllGameObjectsData() => HD_Menu.RefreshAllGameObjectsData();

        [Shortcut("Hierarchy Designer/Refresh Selected GameObject's Data", KeyCode.R, ShortcutModifiers.Alt)]
        private static void RefreshSelectedGameObjectsData() => HD_Menu.RefreshSelectedGameObjectsData();

        [Shortcut("Hierarchy Designer/Refresh Selected Main Icon")]
        private static void RefreshMainIconForSelectedGameObject() => HD_Menu.RefreshSelectedMainIcon();

        [Shortcut("Hierarchy Designer/Refresh Selected Component Icons")]
        private static void RefreshComponentIconsForSelectedGameObjects() => HD_Menu.RefreshSelectedComponentIcons();

        [Shortcut("Hierarchy Designer/Refresh Selected Hierarchy Tree Icon")]
        private static void RefreshHierarchyTreeIconForSelectedGameObjects() => HD_Menu.RefreshSelectedHierarchyTreeIcon();

        [Shortcut("Hierarchy Designer/Refresh Selected Tag")]
        private static void RefreshTagForSelectedGameObjects() => HD_Menu.RefreshSelectedTag();

        [Shortcut("Hierarchy Designer/Refresh Selected Layer")]
        private static void RefreshLayerForSelectedGameObjects() => HD_Menu.RefreshSelectedLayer();
        #endregion

        #region Transform
        [Shortcut("Hierarchy Designer/Transform GameObject into a Folder")]
        private static void TransformGameObjectIntoAFolder() => HD_Menu.TransformGameObjectIntoAFolder();

        [Shortcut("Hierarchy Designer/Transform Folder into a GameObject")]
        private static void TransformFolderIntoAGameObject() => HD_Menu.TransformFolderIntoAGameObject();

        [Shortcut("Hierarchy Designer/Transform GameObject into a Separator")]
        private static void TransformGameObjectIntoASeparator() => HD_Menu.TransformGameObjectIntoASeparator();

        [Shortcut("Hierarchy Designer/Transform Separator into a GameObject")]
        private static void TransformSeparatorIntoAGameObject() => HD_Menu.TransformSeparatorIntoAGameObject();
        #endregion

        #region General
        [Shortcut("Hierarchy Designer/Expand All GameObjects", KeyCode.E, ShortcutModifiers.Shift | ShortcutModifiers.Alt)]
        private static void ExpandAllGameObjects() => HD_Menu.GeneralExpandAll();

        [Shortcut("Hierarchy Designer/Collapse All GameObjects", KeyCode.C, ShortcutModifiers.Shift | ShortcutModifiers.Alt)]
        private static void CollapseAllGameObjects() => HD_Menu.GeneralCollapseAll();
        #endregion
        #endregion
        #endregion

        #region Advanced
        public static HierarchyDesignerLocation HierarchyLocation
        {
            get => advancedSettings.HierarchyLocation;
            set
            {
                if (advancedSettings.HierarchyLocation != value)
                {
                    advancedSettings.HierarchyLocation = value;
                }
            }
        }

        public static UpdateMode MainIconUpdateMode
        {
            get => advancedSettings.MainIconUpdateMode;
            set
            {
                if (advancedSettings.MainIconUpdateMode != value)
                {
                    advancedSettings.MainIconUpdateMode = value;
                    HD_Manager.MainIconUpdateModeCache = value;
                }
            }
        }

        public static UpdateMode ComponentsIconsUpdateMode
        {
            get => advancedSettings.ComponentsIconsUpdateMode;
            set
            {
                if (advancedSettings.ComponentsIconsUpdateMode != value)
                {
                    advancedSettings.ComponentsIconsUpdateMode = value;
                    HD_Manager.ComponentsIconsUpdateModeCache = value;
                }
            }
        }

        public static UpdateMode HierarchyTreeUpdateMode
        {
            get => advancedSettings.HierarchyTreeUpdateMode;
            set
            {
                if (advancedSettings.HierarchyTreeUpdateMode != value)
                {
                    advancedSettings.HierarchyTreeUpdateMode = value;
                    HD_Manager.HierarchyTreeUpdateModeCache = value;
                }
            }
        }

        public static UpdateMode TagUpdateMode
        {
            get => advancedSettings.TagUpdateMode;
            set
            {
                if (advancedSettings.TagUpdateMode != value)
                {
                    advancedSettings.TagUpdateMode = value;
                    HD_Manager.TagUpdateModeCache = value;
                }
            }
        }

        public static UpdateMode LayerUpdateMode
        {
            get => advancedSettings.LayerUpdateMode;
            set
            {
                if (advancedSettings.LayerUpdateMode != value)
                {
                    advancedSettings.LayerUpdateMode = value;
                    HD_Manager.LayerUpdateModeCache = value;
                }
            }
        }

        public static bool EnableDynamicBackgroundForGameObjectMainIcon
        {
            get => advancedSettings.EnableDynamicBackgroundForGameObjectMainIcon;
            set
            {
                if (advancedSettings.EnableDynamicBackgroundForGameObjectMainIcon != value)
                {
                    advancedSettings.EnableDynamicBackgroundForGameObjectMainIcon = value;
                    HD_Manager.EnableDynamicBackgroundForGameObjectMainIconCache = value;
                }
            }
        }

        public static bool EnablePreciseRectForDynamicBackgroundForGameObjectMainIcon
        {
            get => advancedSettings.EnablePreciseRectForDynamicBackgroundForGameObjectMainIcon;
            set
            {
                if (advancedSettings.EnablePreciseRectForDynamicBackgroundForGameObjectMainIcon != value)
                {
                    advancedSettings.EnablePreciseRectForDynamicBackgroundForGameObjectMainIcon = value;
                    HD_Manager.EnablePreciseRectForDynamicBackgroundForGameObjectMainIconCache = value;
                }
            }
        }

        public static bool EnableProjectTexturesInMainIconOverrideWindow
        {
            get => advancedSettings.EnableProjectTexturesInMainIconOverrideWindow;
            set
            {
                if (advancedSettings.EnableProjectTexturesInMainIconOverrideWindow != value)
                {
                    advancedSettings.EnableProjectTexturesInMainIconOverrideWindow = value;
                }
            }
        }

        public static bool EnableCustomizationForGameObjectComponentIcons
        {
            get => advancedSettings.EnableCustomizationForGameObjectComponentIcons;
            set
            {
                if (advancedSettings.EnableCustomizationForGameObjectComponentIcons != value)
                {
                    advancedSettings.EnableCustomizationForGameObjectComponentIcons = value;
                    HD_Manager.EnableCustomizationForGameObjectComponentIconsCache = value;
                }
            }
        }

        public static bool EnableTooltipOnComponentIconHovered
        {
            get => advancedSettings.EnableTooltipOnComponentIconHovered;
            set
            {
                if (advancedSettings.EnableTooltipOnComponentIconHovered != value)
                {
                    advancedSettings.EnableTooltipOnComponentIconHovered = value;
                    HD_Manager.EnableTooltipOnComponentIconHoveredCache = value;
                }
            }
        }

        public static bool EnableActiveStateEffectForComponentIcons
        {
            get => advancedSettings.EnableActiveStateEffectForComponentIcons;
            set
            {
                if (advancedSettings.EnableActiveStateEffectForComponentIcons != value)
                {
                    advancedSettings.EnableActiveStateEffectForComponentIcons = value;
                    HD_Manager.EnableActiveStateEffectForComponentIconsCache = value;
                }
            }
        }

        public static bool DisableComponentIconsForInactiveGameObjects
        {
            get => advancedSettings.DisableComponentIconsForInactiveGameObjects;
            set
            {
                if (advancedSettings.DisableComponentIconsForInactiveGameObjects != value)
                {
                    advancedSettings.DisableComponentIconsForInactiveGameObjects = value;
                    HD_Manager.DisableComponentIconsForInactiveGameObjectsCache = value;
                }
            }
        }

        public static bool UseHierarchyTreeColorForInactiveGameObjects
        {
            get => advancedSettings.UseHierarchyTreeColorForInactiveGameObjects;
            set
            {
                if (advancedSettings.UseHierarchyTreeColorForInactiveGameObjects != value)
                {
                    advancedSettings.UseHierarchyTreeColorForInactiveGameObjects = value;
                    HD_Manager.UseHierarchyTreeColorForInactiveGameObjectsCache = value;
                }
            }
        }

        public static bool EnableCustomInspectorGUI
        {
            get => advancedSettings.EnableCustomInspectorUI;
            set
            {
                if (advancedSettings.EnableCustomInspectorUI != value)
                {
                    advancedSettings.EnableCustomInspectorUI = value;
                }
            }
        }

        public static bool IncludeEditorUtilitiesForHierarchyDesignerRuntimeFolder
        {
            get => advancedSettings.EnableEditorUtilities;
            set
            {
                if (advancedSettings.EnableEditorUtilities != value)
                {
                    advancedSettings.EnableEditorUtilities = value;
                }
            }
        }

        public static bool IncludeBackgroundImageForGradientBackground
        {
            get => advancedSettings.IncludeBackgroundImageForGradientBackground;
            set
            {
                if (advancedSettings.IncludeBackgroundImageForGradientBackground != value)
                {
                    advancedSettings.IncludeBackgroundImageForGradientBackground = value;
                    HD_Manager.IncludeBackgroundImageForGradientBackgroundCache = value;
                }
            }
        }

        public static bool ExpandHierarchyOnStartup
        {
            get => advancedSettings.ExpandHierarchyOnStartup;
            set
            {
                if (advancedSettings.ExpandHierarchyOnStartup != value)
                {
                    advancedSettings.ExpandHierarchyOnStartup = value;
                }
            }
        }

        public static bool ExcludeFoldersFromCountSelectToolCalculations
        {
            get => advancedSettings.ExcludeFoldersFromCountSelectToolCalculations;
            set
            {
                if (advancedSettings.ExcludeFoldersFromCountSelectToolCalculations != value)
                {
                    advancedSettings.ExcludeFoldersFromCountSelectToolCalculations = value;
                }
            }
        }

        public static bool ExcludeSeparatorsFromCountSelectToolCalculations
        {
            get => advancedSettings.ExcludeSeparatorsFromCountSelectToolCalculations;
            set
            {
                if (advancedSettings.ExcludeSeparatorsFromCountSelectToolCalculations != value)
                {
                    advancedSettings.ExcludeSeparatorsFromCountSelectToolCalculations = value;
                }
            }
        }
        #endregion
        #endregion

        #region Save and Load
        #region General
        public static void SaveGeneralSettings()
        {
            string dataFilePath = HD_File.GetSavedDataFilePath(HD_Constants.GeneralSettingsTextFileName);
            string json = JsonUtility.ToJson(generalSettings, true);
            File.WriteAllText(dataFilePath, json);
            AssetDatabase.Refresh();
        }

        public static void LoadGeneralSettings()
        {
            string dataFilePath = HD_File.GetSavedDataFilePath(HD_Constants.GeneralSettingsTextFileName);
            if (File.Exists(dataFilePath))
            {
                string json = File.ReadAllText(dataFilePath);
                HD_GeneralSettings loadedSettings = JsonUtility.FromJson<HD_GeneralSettings>(json);
                loadedSettings.LayoutMode = HD_Validation.ParseEnum(loadedSettings.LayoutMode.ToString(), HierarchyLayoutMode.Docked);
                loadedSettings.TreeMode = HD_Validation.ParseEnum(loadedSettings.TreeMode.ToString(), HierarchyTreeMode.Default);
                generalSettings = loadedSettings;
            }
            else
            {
                SetDefaultGeneralSettings();
            }
        }

        private static void SetDefaultGeneralSettings()
        {
            generalSettings = new()
            {
                LayoutMode = HierarchyLayoutMode.Split,
                TreeMode = HierarchyTreeMode.Default,
                EnableGameObjectMainIcon = true,
                EnableGameObjectComponentIcons = true,
                EnableHierarchyTree = true,
                EnableGameObjectTag = true,
                EnableGameObjectLayer = true,
                EnableHierarchyRows = true,
                EnableHierarchyLines = true,
                EnableHierarchyButtons = true,
                EnableHeaderUtilities = true,
                EnableMajorShortcuts = true,
                DisableHierarchyDesignerDuringPlayMode = true,
                ExcludeFolderProperties = true,
                ExcludedComponents = new() { "Transform", "RectTransform", "CanvasRenderer" },
                MaximumComponentIconsAmount = 10,
                ExcludedTags = new() { "Untagged" },
                ExcludedLayers = new() { "Default" }
            };
        }
        #endregion

        #region Design
        public static void SaveDesignSettings()
        {
            string dataFilePath = HD_File.GetSavedDataFilePath(HD_Constants.DesignSettingsTextFileName);
            string json = JsonUtility.ToJson(designSettings, true);
            File.WriteAllText(dataFilePath, json);
            AssetDatabase.Refresh();
        }

        public static void LoadDesignSettings()
        {
            string dataFilePath = HD_File.GetSavedDataFilePath(HD_Constants.DesignSettingsTextFileName);
            if (File.Exists(dataFilePath))
            {
                string json = File.ReadAllText(dataFilePath);
                HD_DesignSettings loadedSettings = JsonUtility.FromJson<HD_DesignSettings>(json);
                designSettings = loadedSettings;
            }
            else
            {
                SetDefaultDesignSettings();
            }
        }

        private static void SetDefaultDesignSettings()
        {
            designSettings = new()
            {
                ComponentIconsSize = 1f,
                ComponentIconsOffset = 21,
                ComponentIconsSpacing = 2f,
                HierarchyTreeColor = Color.white,
                TreeBranchImageType_I = TreeBranchImageType.Default,
                TreeBranchImageType_L = TreeBranchImageType.Default,
                TreeBranchImageType_T = TreeBranchImageType.Default,
                TreeBranchImageType_TerminalBud = TreeBranchImageType.Default,
                TagColor = Color.gray,
                TagTextAnchor = TextAnchor.MiddleRight,
                TagFontStyle = FontStyle.BoldAndItalic,
                TagFontSize = 10,
                LayerColor = Color.gray,
                LayerTextAnchor = TextAnchor.MiddleLeft,
                LayerFontStyle = FontStyle.BoldAndItalic,
                LayerFontSize = 10,
                TagLayerOffset = 5,
                TagLayerSpacing = 5,
                HierarchyLineColor = HD_Color.HexToColor("00000080"),
                HierarchyLineThickness = 1,
                HierarchyButtonLockColor = HD_Color.HexToColor("404040"),
                HierarchyButtonVisibilityColor = HD_Color.HexToColor("404040"),
                FolderDefaultTextColor = Color.white,
                FolderDefaultFontSize = 12,
                FolderDefaultFontStyle = FontStyle.Normal,
                FolderDefaultImageColor = Color.white,
                FolderDefaultImageType = HD_Folders.FolderImageType.Default,
                SeparatorDefaultTextColor = Color.white,
                SeparatorDefaultIsGradientBackground = false,
                SeparatorDefaultBackgroundColor = Color.gray,
                SeparatorDefaultBackgroundGradient = new(),
                SeparatorDefaultFontSize = 12,
                SeparatorDefaultFontStyle = FontStyle.Normal,
                SeparatorDefaultTextAnchor = TextAnchor.MiddleCenter,
                SeparatorDefaultImageType = HD_Separators.SeparatorImageType.Default,
                SeparatorLeftSideTextAnchorOffset = 3,
                SeparatorCenterTextAnchorOffset = -15,
                SeparatorRightSideTextAnchorOffset = 36,
                LockColor = Color.white,
                LockTextAnchor = TextAnchor.MiddleCenter,
                LockFontStyle = FontStyle.BoldAndItalic,
                LockFontSize = 11
            };
        }
        #endregion

        #region Shortcuts
        public static void SaveShortcutSettings()
        {
            string dataFilePath = HD_File.GetSavedDataFilePath(HD_Constants.ShortcutSettingsTextFileName);
            string json = JsonUtility.ToJson(shortcutsSettings, true);
            File.WriteAllText(dataFilePath, json);
            AssetDatabase.Refresh();
        }

        public static void LoadShortcutSettings()
        {
            string dataFilePath = HD_File.GetSavedDataFilePath(HD_Constants.ShortcutSettingsTextFileName);
            if (File.Exists(dataFilePath))
            {
                string json = File.ReadAllText(dataFilePath);
                HD_ShortcutsSettings loadedSettings = JsonUtility.FromJson<HD_ShortcutsSettings>(json);
                shortcutsSettings = loadedSettings;
            }
            else
            {
                SetDefaultShortcutSettings();
            }
        }

        private static void SetDefaultShortcutSettings()
        {
            shortcutsSettings = new()
            {
                OpenIconPickerKeyCode = KeyCode.Mouse0,
                ToggleGameObjectActiveStateKeyCode = KeyCode.Mouse2,
                ToggleLockStateKeyCode = KeyCode.F1,
                ChangeTagLayerKeyCode = KeyCode.Mouse0,
                RenameSelectedGameObjectsKeyCode = KeyCode.F3,
            };
        }
        #endregion

        #region Advanced
        public static void SaveAdvancedSettings()
        {
            string dataFilePath = HD_File.GetSavedDataFilePath(HD_Constants.AdvancedSettingsTextFileName);
            string json = JsonUtility.ToJson(advancedSettings, true);
            File.WriteAllText(dataFilePath, json);
            AssetDatabase.Refresh();
        }

        public static void LoadAdvancedSettings()
        {
            string dataFilePath = HD_File.GetSavedDataFilePath(HD_Constants.AdvancedSettingsTextFileName);
            if (File.Exists(dataFilePath))
            {
                string json = File.ReadAllText(dataFilePath);
                HD_AdvancedSettings loadedSettings = JsonUtility.FromJson<HD_AdvancedSettings>(json);
                loadedSettings.HierarchyLocation = HD_Validation.ParseEnum(loadedSettings.HierarchyLocation.ToString(), HierarchyDesignerLocation.Tools);
                loadedSettings.MainIconUpdateMode = HD_Validation.ParseEnum(loadedSettings.MainIconUpdateMode.ToString(), UpdateMode.Dynamic);
                loadedSettings.ComponentsIconsUpdateMode = HD_Validation.ParseEnum(loadedSettings.ComponentsIconsUpdateMode.ToString(), UpdateMode.Dynamic);
                loadedSettings.HierarchyTreeUpdateMode = HD_Validation.ParseEnum(loadedSettings.HierarchyTreeUpdateMode.ToString(), UpdateMode.Dynamic);
                loadedSettings.TagUpdateMode = HD_Validation.ParseEnum(loadedSettings.TagUpdateMode.ToString(), UpdateMode.Dynamic);
                loadedSettings.LayerUpdateMode = HD_Validation.ParseEnum(loadedSettings.LayerUpdateMode.ToString(), UpdateMode.Dynamic);
                advancedSettings = loadedSettings;
            }
            else
            {
                SetDefaultAdvancedSettings();
            }
        }

        private static void SetDefaultAdvancedSettings()
        {
            advancedSettings = new()
            {
                HierarchyLocation = HierarchyDesignerLocation.Tools,
                MainIconUpdateMode = UpdateMode.Dynamic,
                ComponentsIconsUpdateMode = UpdateMode.Dynamic,
                HierarchyTreeUpdateMode = UpdateMode.Dynamic,
                TagUpdateMode = UpdateMode.Dynamic,
                LayerUpdateMode = UpdateMode.Dynamic,
                EnableDynamicBackgroundForGameObjectMainIcon = true,
                EnablePreciseRectForDynamicBackgroundForGameObjectMainIcon = true,
                EnableProjectTexturesInMainIconOverrideWindow = false,
                EnableCustomizationForGameObjectComponentIcons = true,
                EnableTooltipOnComponentIconHovered = true,
                EnableActiveStateEffectForComponentIcons = true,
                DisableComponentIconsForInactiveGameObjects = true,
                UseHierarchyTreeColorForInactiveGameObjects = false,
                EnableCustomInspectorUI = true,
                EnableEditorUtilities = true,
                IncludeBackgroundImageForGradientBackground = true,
                ExpandHierarchyOnStartup = false,
                ExcludeFoldersFromCountSelectToolCalculations = true,
                ExcludeSeparatorsFromCountSelectToolCalculations = true,
            };
        }
        #endregion
        #endregion
    }
}
#endif