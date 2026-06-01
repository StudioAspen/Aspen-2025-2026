#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.ShortcutManagement;
using UnityEditorInternal;
using UnityEngine;

namespace HierarchyDesigner
{
    #region Main
    internal class HD_Main : EditorWindow
    {
        #region Properties
        #region General
        public enum CurrentWindow { Home, About, Folders, Separators, Tools, Presets, PresetCreator, GeneralSettings, DesignSettings, ShortcutSettings, AdvancedSettings }
        private static CurrentWindow currentWindow;
        private static string cachedCurrentWindowLabel;
        private Vector2 headerButtonsScroll;
        private Dictionary<string, Action> utilitiesMenuItems;
        private Dictionary<string, Action> settingsMenuItems;
        private const int primaryButtonsHeight = 30;
        private const int secondaryButtonsHeight = 25;
        private const int defaultMarginSpacing = 5;
        private const float moveItemInListButtonWidth = 25;
        private const float createButtonWidth = 52;
        private const float removeButtonWidth = 60;
        private readonly int[] fontSizeOptions = new int[15];
        #endregion

        #region Home
        private Vector2 homeScroll;
        private string patchNotes = string.Empty;
        private const float titleAspectRatio = 1f;
        private const float titleWidthPercentage = 1f;
        private const float titleMinWidth = 256f;
        private const float titleMaxWidth = 512f;
        private const float titleMinHeight = 128f;
        private const float titleMaxHeight = 400f;
        #endregion

        #region About
        private Vector2 aboutSummaryScroll;
        private Vector2 aboutPatchNotesScroll;
        private Vector2 aboutMyOtherAssetsScroll;

        private const string folderText =
            "Folders are great for organizing multiple GameObjects of the same or similar type (e.g., static environment objects, reflection probes, and so on).\n\n" +
            "Folders have a script called 'Hierarchy Designer Folder' with a main variable, 'Flatten Folder.'" +
            "If 'Flatten Folder' is true, in the Flatten Event (Awake or Start method), the folder will FREE all GameObject children, and once that is complete, it will destroy the folder.\n";

        private const string separatorText =
            "Separators are visual dividers; they are meant to organize your scenes and provide clarity.\n\n" +
            "Separators are editor-only and will NOT be included in your game's build. Therefore, do not use them as GameObject parents; instead, use folders.\n";

        private const string savedDataText =
            "Settings and Custom Presets are saved in the 'Saved Data' folder (located at: Assets/.../Hierarchy Designer/Editor/Saved Data) as .json files.\n\n" +
            "To export Hierarchy Designer's data to another project, simply copy and paste the .json files into the other project's saved data folder, and then restart the editor.\n";

        private const string additionalNotesText =
            "Hierarchy Designer is currently in development, and more features and improvements are coming soon.\n\n" +
            "Hierarchy Designer is an Editor-Only tool (with the exception of the Hierarchy Designer Folder script) and will not affect your build or game.\n\n" +
            "Like most editor tool, it will slightly affect performance (EDITOR ONLY). Disabling features you don't use or setting their update values to 'smart' will greatly improve performance, especially in larger scenes.\n\n" +
            "If you have any questions or would like to report a bug, you may email me at: VerphaSuporte@outlook.com.\n\nIf you like Hierarchy Designer, please rate it on the Store.";
        #endregion

        #region Folders
        private Vector2 folderMainScroll;
        private Vector2 foldersListScroll;
        private const float folderCreationLabelWidth = 110;
        private Dictionary<string, HD_Folders.HD_FolderData> tempFolders;
        private List<string> foldersOrder;
        private string newFolderName = "";
        private Color newFolderTextColor = HD_Settings.FolderDefaultTextColor;
        private int newFolderFontSize = HD_Settings.FolderDefaultFontSize;
        private FontStyle newFolderFontStyle = HD_Settings.FolderDefaultFontStyle;
        private Color newFolderIconColor = HD_Settings.FolderDefaultImageColor;
        private HD_Folders.FolderImageType newFolderImageType = HD_Settings.FolderDefaultImageType;
        private bool folderHasModifiedChanges = false;
        private Color tempFolderGlobalTextColor = Color.white;
        private int tempFolderGlobalFontSize = 12;
        private FontStyle tempFolderGlobalFontStyle = FontStyle.Normal;
        private Color tempFolderGlobalIconColor = Color.white;
        private HD_Folders.FolderImageType tempGlobalFolderImageType = HD_Folders.FolderImageType.Default;
        #endregion

        #region Separators
        private Vector2 separatorMainScroll;
        private Vector2 separatorsListScroll;
        private const float separatorCreationLabelWidth = 160;
        private Dictionary<string, HD_Separators.HD_SeparatorData> tempSeparators;
        private List<string> separatorsOrder;
        private bool separatorHasModifiedChanges = false;
        private string newSeparatorName = "";
        private Color newSeparatorTextColor = HD_Settings.SeparatorDefaultTextColor;
        private bool newSeparatorIsGradient = HD_Settings.SeparatorDefaultIsGradientBackground;
        private Color newSeparatorBackgroundColor = HD_Settings.SeparatorDefaultBackgroundColor;
        private Gradient newSeparatorBackgroundGradient = HD_Color.CopyGradient(HD_Settings.SeparatorDefaultBackgroundGradient);
        private int newSeparatorFontSize = HD_Settings.SeparatorDefaultFontSize;
        private FontStyle newSeparatorFontStyle = HD_Settings.SeparatorDefaultFontStyle;
        private TextAnchor newSeparatorTextAnchor = HD_Settings.SeparatorDefaultTextAnchor;
        private HD_Separators.SeparatorImageType newSeparatorImageType = HD_Settings.SeparatorDefaultImageType;
        private Color tempSeparatorGlobalTextColor = Color.white;
        private bool tempSeparatorGlobalIsGradient = false;
        private Color tempSeparatorGlobalBackgroundColor = Color.gray;
        private Gradient tempSeparatorGlobalBackgroundGradient = new();
        private int tempSeparatorGlobalFontSize = 12;
        private FontStyle tempSeparatorGlobalFontStyle = FontStyle.Normal;
        private TextAnchor tempSeparatorGlobalTextAnchor = TextAnchor.MiddleCenter;
        private HD_Separators.SeparatorImageType tempSeparatorGlobalImageType = HD_Separators.SeparatorImageType.Default;
        #endregion

        #region Tools
        private Vector2 toolsMainScroll;
        private const float labelWidth = 80;
        private HierarchyDesigner_Attribute_Tools selectedCategory = HierarchyDesigner_Attribute_Tools.Activate;
        private int selectedActionIndex = 0;
        private readonly List<string> availableActionNames = new();
        private readonly List<MethodInfo> availableActionMethods = new();
        private static readonly Dictionary<HierarchyDesigner_Attribute_Tools, List<(string Name, MethodInfo Method)>> cachedActions = new();
        private static bool cacheInitialized = false;
        #endregion

        #region Presets
        private Vector2 presetsMainScroll;
        private const float presetslabelWidth = 130;
        private const float presetsToggleLabelWidth = 205;
        private int selectedPresetIndex = 0;
        private string[] presetNames;
        private bool applyToFolders = true;
        private bool applyToSeparators = true;
        private bool applyToTag = true;
        private bool applyToLayer = true;
        private bool applyToTree = true;
        private bool applyToLines = true;
        private bool applyToHierarchyButtons = true;
        private bool applyToFolderDefaultValues = true;
        private bool applyToSeparatorDefaultValues = true;
        private bool applyToLock = true;
        #endregion

        #region Preset Creator
        private Vector2 presetCreatorMainScroll;
        private Vector2 presetCreatorListScroll;
        private const int customPresetsSpacing = 10;
        private const float customPresetsLabelWidth = 185;
        private string customPresetName = string.Empty;
        private Color customPresetFolderTextColor = Color.white;
        private int customPresetFolderFontSize = 12;
        private FontStyle customPresetFolderFontStyle = FontStyle.Normal;
        private Color customPresetFolderColor = Color.white;
        private HD_Folders.FolderImageType customPresetFolderImageType = HD_Folders.FolderImageType.Default;
        private Color customPresetSeparatorTextColor = Color.white;
        private bool customPresetSeparatorIsGradientBackground = false;
        private Color customPresetSeparatorBackgroundColor = Color.gray;
        private Gradient customPresetSeparatorBackgroundGradient = new();
        private int customPresetSeparatorFontSize = 12;
        private FontStyle customPresetSeparatorFontStyle = FontStyle.Normal;
        private TextAnchor customPresetSeparatorTextAlignment = TextAnchor.MiddleCenter;
        private HD_Separators.SeparatorImageType customPresetSeparatorBackgroundImageType = HD_Separators.SeparatorImageType.Default;
        private Color customPresetTagTextColor = Color.gray;
        private FontStyle customPresetTagFontStyle = FontStyle.BoldAndItalic;
        private int customPresetTagFontSize = 10;
        private TextAnchor customPresetTagTextAnchor = TextAnchor.MiddleRight;
        private Color customPresetLayerTextColor = Color.gray;
        private FontStyle customPresetLayerFontStyle = FontStyle.Bold;
        private int customPresetLayerFontSize = 10;
        private TextAnchor customPresetLayerTextAnchor = TextAnchor.MiddleLeft;
        private Color customPresetTreeColor = Color.white;
        private Color customPresetHierarchyLineColor = HD_Color.HexToColor("00000080");
        private Color customPresetHierarchyButtonLockColor = HD_Color.HexToColor("404040");
        private Color customPresetHierarchyButtonVisibilityColor = HD_Color.HexToColor("404040");
        private Color customPresetLockColor = Color.white;
        private int customPresetLockFontSize = 11;
        private FontStyle customPresetLockFontStyle = FontStyle.BoldAndItalic;
        private TextAnchor customPresetLockTextAnchor = TextAnchor.MiddleCenter;
        private List<HD_Presets.HD_Preset> customPresets;
        #endregion

        #region General Settings
        private Vector2 generalSettingsMainScroll;
        private const float enumPopupLabelWidth = 190;
        private const float generalSettingsMainToggleLabelWidth = 360;
        private const float generalSettingsFilterToggleLabelWidth = 300;
        private const float maskFieldLabelWidth = 145;
        private HD_Settings.HierarchyLayoutMode tempLayoutMode;
        private HD_Settings.HierarchyTreeMode tempTreeMode;
        private bool tempEnableGameObjectMainIcon;
        private bool tempEnableGameObjectComponentIcons;
        private bool tempEnableHierarchyTree;
        private bool tempEnableGameObjectTag;
        private bool tempEnableGameObjectLayer;
        private bool tempEnableHierarchyRows;
        private bool tempEnableHierarchyLines;
        private bool tempEnableHierarchyButtons;
        private bool tempEnableHeaderUtilities;
        private bool tempEnableMajorShortcuts;
        private bool tempDisableHierarchyDesignerDuringPlayMode;
        private bool tempExcludeFolderProperties;
        private List<string> tempExcludedComponents;
        private int tempMaximumComponentIconsAmount;
        private List<string> tempExcludedTags;
        private List<string> tempExcludedLayers;
        private static bool generalSettingsHasModifiedChanges = false;
        #endregion

        #region Design Settings
        private Vector2 designSettingsMainScroll;
        private const float designSettingslabelWidth = 260;
        private float tempComponentIconsSize;
        private int tempComponentIconsOffset;
        private float tempComponentIconsSpacing;
        private Color tempHierarchyTreeColor;
        private HD_Settings.TreeBranchImageType tempTreeBranchImageType_I;
        private HD_Settings.TreeBranchImageType tempTreeBranchImageType_L;
        private HD_Settings.TreeBranchImageType tempTreeBranchImageType_T;
        private HD_Settings.TreeBranchImageType tempTreeBranchImageType_TerminalBud;
        private Color tempTagColor;
        private TextAnchor tempTagTextAnchor;
        private FontStyle tempTagFontStyle;
        private int tempTagFontSize;
        private Color tempLayerColor;
        private TextAnchor tempLayerTextAnchor;
        private FontStyle tempLayerFontStyle;
        private int tempLayerFontSize;
        private int tempTagLayerOffset;
        private int tempTagLayerSpacing;
        private Color tempHierarchyLineColor;
        private int tempHierarchyLineThickness;
        private Color tempHierarchyButtonLockColor;
        private Color tempHierarchyButtonVisibilityColor;
        private Color tempFolderDefaultTextColor;
        private int tempFolderDefaultFontSize;
        private FontStyle tempFolderDefaultFontStyle;
        private Color tempFolderDefaultImageColor;
        private HD_Folders.FolderImageType tempFolderDefaultImageType;
        private Color tempSeparatorDefaultTextColor;
        private bool tempSeparatorDefaultIsGradientBackground;
        private Color tempSeparatorDefaultBackgroundColor;
        private Gradient tempSeparatorDefaultBackgroundGradient;
        private int tempSeparatorDefaultFontSize;
        private FontStyle tempSeparatorDefaultFontStyle;
        private TextAnchor tempSeparatorDefaultTextAnchor;
        private HD_Separators.SeparatorImageType tempSeparatorDefaultImageType;
        private int tempSeparatorLeftSideTextAnchorOffset;
        private int tempSeparatorCenterTextAnchorOffset;
        private int tempSeparatorRightSideTextAnchorOffset;
        private Color tempLockColor;
        private TextAnchor tempLockTextAnchor;
        private FontStyle tempLockFontStyle;
        private int tempLockFontSize;
        private static bool designSettingsHasModifiedChanges = false;
        #endregion

        #region Shortcut Settings
        private Vector2 shortcutSettingsMainScroll;
        private Vector2 minorShortcutSettingsScroll;
        private const float majorShortcutEnumToggleLabelWidth = 340;
        private const float minorShortcutCommandLabelWidth = 200;
        private const float minorShortcutLabelWidth = 400;
        private readonly List<string> minorShortcutIdentifiers = new()
        {
            "Hierarchy Designer/Open Hierarchy Designer Window",
            "Hierarchy Designer/Open Folder Panel",
            "Hierarchy Designer/Open Separator Panel",
            "Hierarchy Designer/Open Tools Panel",
            "Hierarchy Designer/Open Presets Panel",
            "Hierarchy Designer/Open Preset Creator Panel",
            "Hierarchy Designer/Open General Settings Panel",
            "Hierarchy Designer/Open Design Settings Panel",
            "Hierarchy Designer/Open Shortcut Settings Panel",
            "Hierarchy Designer/Open Advanced Settings Panel",
            "Hierarchy Designer/Open Rename Tool Window",
            "Hierarchy Designer/Create All Folders",
            "Hierarchy Designer/Create Default Folder",
            "Hierarchy Designer/Create Missing Folders",
            "Hierarchy Designer/Create All Separators",
            "Hierarchy Designer/Create Default Separator",
            "Hierarchy Designer/Create Missing Separators",
            "Hierarchy Designer/Refresh All GameObjects' Data",
            "Hierarchy Designer/Refresh Selected GameObject's Data",
            "Hierarchy Designer/Refresh Selected Main Icon",
            "Hierarchy Designer/Refresh Selected Component Icons",
            "Hierarchy Designer/Refresh Selected Hierarchy Tree Icon",
            "Hierarchy Designer/Refresh Selected Tag",
            "Hierarchy Designer/Refresh Selected Layer",
            "Hierarchy Designer/Refresh Selected Layer",
            "Hierarchy Designer/Transform GameObject into a Folder",
            "Hierarchy Designer/Transform Folder into a GameObject",
            "Hierarchy Designer/Transform GameObject into a Separator",
            "Hierarchy Designer/Transform Separator into a GameObject",
            "Hierarchy Designer/Expand All GameObjects",
            "Hierarchy Designer/Collapse All GameObjects",
        };
        private readonly Dictionary<string, string> minorShortcutTooltips = new()
        {
            { "Hierarchy Designer/Open Hierarchy Designer Window", "Opens the Hierarchy Designer window on the last opened panel." },
            { "Hierarchy Designer/Open Folder Panel", "Opens the Hierarchy Designer window on the folder panel." },
            { "Hierarchy Designer/Open Separator Panel", "Opens the Hierarchy Designer window on the separator panel." },
            { "Hierarchy Designer/Open Tools Panel", "Opens the Hierarchy Designer window on the tools panel." },
            { "Hierarchy Designer/Open Presets Panel", "Opens the Hierarchy Designer window on the presets panel." },
            { "Hierarchy Designer/Open Preset Creator Panel", "Opens the Hierarchy Designer window on the preset creator panel." },
            { "Hierarchy Designer/Open General Settings Panel", "Opens the Hierarchy Designer window on the general settings panel." },
            { "Hierarchy Designer/Open Design Settings Panel", "Opens the Hierarchy Designer window on the design settings panel." },
            { "Hierarchy Designer/Open Shortcut Settings Panel", "Opens the Hierarchy Designer window on the shortcut settings panel." },
            { "Hierarchy Designer/Open Advanced Settings Panel", "Opens the Hierarchy Designer window on the advanced settings panel." },
            { "Hierarchy Designer/Open Rename Tool Window", "Opens the Rename Tool Window." },
            { "Hierarchy Designer/Create All Folders", "Creates all folders from your folder list." },
            { "Hierarchy Designer/Create Default Folder", "Creates a default folder." },
            { "Hierarchy Designer/Create Missing Folders", "Creates any folders defined in your folder list that are missing in the scene." },
            { "Hierarchy Designer/Create All Separators", "Creates all separators from your separator list." },
            { "Hierarchy Designer/Create Default Separator", "Creates a default separator." },
            { "Hierarchy Designer/Create Missing Separators", "Creates any separators defined in your separator list that are missing in the scene." },
            { "Hierarchy Designer/Refresh All GameObjects' Data", "Refreshes all GameObjects' data (e.g., main icon, component icon, tag, layer, etc.).\n\nNote: Only applicable if core features are in Smart Mode." },
            { "Hierarchy Designer/Refresh Selected GameObject's Data", "Refreshes all GameObjects' data (e.g., main icon, component icon, tag, layer, etc.) of the selected GameObjects.\n\nNote: Only applicable if core features are in Smart Mode." },
            { "Hierarchy Designer/Refresh Selected Main Icon", "Refreshes the main icon of the selected GameObjects.\n\nNote: Only applicable if the main icon is in Smart Mode." },
            { "Hierarchy Designer/Refresh Selected Component Icons", "Refreshes the component icon of the selected GameObjects.\n\nNote: Only applicable if the component icon is in Smart Mode." },
            { "Hierarchy Designer/Refresh Selected Hierarchy Tree Icon", "Refreshes the hierarchy tree icon of the selected GameObjects.\n\nNote: Only applicable if the hierarchy tree icon is in Smart Mode." },
            { "Hierarchy Designer/Refresh Selected Tag", "Refreshes the tag of the selected GameObjects.\n\nNote: Only applicable if the tag feature is in Smart Mode." },
            { "Hierarchy Designer/Refresh Selected Layer", "Refreshes the layer of the selected GameObjects.\n\nNote: Only applicable if the layer feature is in Smart Mode." },
            { "Hierarchy Designer/Transform GameObject into a Folder", "Transforms the selected GameObject into a folder and adds it to the folders list." },
            { "Hierarchy Designer/Transform Folder into a GameObject", "Transforms the selected folder into a GameObject and removes it from the folders list." },
            { "Hierarchy Designer/Transform GameObject into a Separator", "Transforms the selected GameObject into a separator and adds it to the separators list." },
            { "Hierarchy Designer/Transform Separator into a GameObject", "Transforms the selected separator into a GameObject and removes it from the separators list." },
            { "Hierarchy Designer/Expand All GameObjects", "Expands all GameObjects in the Hierarchy." },
            { "Hierarchy Designer/Collapse All GameObjects", "Collapses all GameObjects in the Hierarchy." },
        };
        private KeyCode tempToggleGameObjectActiveStateKeyCode;
        private KeyCode tempToggleLockStateKeyCode;
        private KeyCode tempChangeTagLayerKeyCode;
        private KeyCode tempRenameSelectedGameObjectsKeyCode;
        private KeyCode tempOpenIconPickerKeyCode;
        private static bool shortcutSettingsHasModifiedChanges = false;
        #endregion

        #region Advanced Settings
        private Vector2 advancedSettingsMainScroll;
        private const float advancedSettingsEnumPopupLabelWidth = 250;
        private const float advancedSettingsToggleLabelWidth = 460;
        private HD_Settings.HierarchyDesignerLocation tempHierarchyLocation;
        private HD_Settings.UpdateMode tempMainIconUpdateMode;
        private HD_Settings.UpdateMode tempComponentsIconsUpdateMode;
        private HD_Settings.UpdateMode tempHierarchyTreeUpdateMode;
        private HD_Settings.UpdateMode tempTagUpdateMode;
        private HD_Settings.UpdateMode tempLayerUpdateMode;
        private bool tempEnableDynamicBackgroundForGameObjectMainIcon;
        private bool tempEnablePreciseRectForDynamicBackgroundForGameObjectMainIcon;
        private bool tempEnableProjectTexturesInMainIconOverrideWindow;
        private bool tempEnableCustomizationForGameObjectComponentIcons;
        private bool tempEnableTooltipOnComponentIconHovered;
        private bool tempEnableActiveStateEffectForComponentIcons;
        private bool tempDisableComponentIconsForInactiveGameObjects;
        private bool tempUseHierarchyTreeColorForInactiveGameObjects;
        private bool tempEnableCustomInspectorUI;
        private bool tempEnableEditorUtilities;
        private bool tempIncludeBackgroundImageForGradientBackground;
        private bool tempExpandHierarchyOnStartup;
        private bool tempExcludeFoldersFromCountSelectToolCalculations;
        private bool tempExcludeSeparatorsFromCountSelectToolCalculations;
        private static bool advancedSettingsHasModifiedChanges = false;
        #endregion
        #endregion

        #region Initialization
        public static void OpenWindow()
        {
            HD_Main editorWindow = GetWindow<HD_Main>(HD_Constants.AssetName);
            editorWindow.minSize = new(500, 400);
            UpdateCurrentWindowLabel();
        }

        private void OnEnable()
        {
            InitializeMenus();
            InitializeFontSizeOptions();
            LoadSessionData();
            LoadFolderData();
            LoadSeparatorData();
            LoadTools();
            LoadPresets();
            LoadGeneralSettingsData();
            LoadDesignSettingsData();
            LoadShortcutSettingsData();
            LoadAdvancedSettingsData();
        }

        private void InitializeMenus()
        {
            if (utilitiesMenuItems == null)
            {
                utilitiesMenuItems = new()
                {
                    { "Tools", () => { SelectToolsWindow(); } },
                    { "Presets", () => { SelectPresetsWindow(); } },
                    { "Preset Creator", () => { SelectPresetCreatorWindow(); } }
                };
            }

            if (settingsMenuItems == null)
            {
                settingsMenuItems = new()
                {
                    { "General Settings", () => { SelectGeneralSettingsWindow(); } },
                    { "Design Settings", () => { SelectDesignSettingsWindow(); } },
                    { "Shortcut Settings", () => { SelectShortcutSettingsWindow(); } },
                    { "Advanced Settings", () => { SelectAdvancedSettingsWindow(); } }
                };
            }
        }

        private void InitializeFontSizeOptions()
        {
            for (int i = 0; i < fontSizeOptions.Length; i++)
            {
                fontSizeOptions[i] = 7 + i;
            }
        }

        private void LoadSessionData()
        {
            if (!HD_Session.instance.IsPatchNotesLoaded)
            {
                patchNotes = HD_File.GetPatchNotesData();
                HD_Session.instance.PatchNotesContent = patchNotes;
                HD_Session.instance.IsPatchNotesLoaded = true;
            }
            else
            {
                patchNotes = HD_Session.instance.PatchNotesContent;
            }

            currentWindow = HD_Session.instance.currentWindow;
        }
        #endregion

        private void OnGUI()
        {
            EditorGUILayout.BeginVertical(HD_GUI.PrimaryPanelStyle);

            if (currentWindow != CurrentWindow.Home)
            {
                EditorGUILayout.BeginVertical(HD_GUI.HeaderPanelStyle);
                headerButtonsScroll = EditorGUILayout.BeginScrollView(headerButtonsScroll, GUI.skin.horizontalScrollbar, GUIStyle.none, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(false));
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"Hierarchy <color=#{(HD_Editor.IsProSkin ? "67F758" : "50C044")}>Designer</color>", HD_GUI.HeaderLabelLeftStyle, GUILayout.Width(220));
                GUILayout.FlexibleSpace();
                DrawHeaderButtons();
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndScrollView();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"▷ {cachedCurrentWindowLabel}", HD_GUI.TabLabelStyle);
                GUILayout.FlexibleSpace();
                GUILayout.Label($"{HD_Constants.AssetVersion}", HD_GUI.VersionLabelHeaderStyle, GUILayout.Height(20));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.EndVertical();
            }

            #region Body
            switch (currentWindow)
            {
                case CurrentWindow.Home:
                    DrawHomeTab();
                    break;
                case CurrentWindow.About:
                    DrawAboutPanel();
                    break;
                case CurrentWindow.Folders:
                    DrawFoldersTab();
                    break;
                case CurrentWindow.Separators:
                    DrawSeparatorsTab();
                    break;
                case CurrentWindow.Tools:
                    DrawToolsTab();
                    break;
                case CurrentWindow.Presets:
                    DrawPresetsTab();
                    break;
                case CurrentWindow.PresetCreator:
                    DrawPresetCreatorTab();
                    break;
                case CurrentWindow.GeneralSettings:
                    DrawGeneralSettingsTab();
                    break;
                case CurrentWindow.DesignSettings:
                    DrawDesignSettingsTab();
                    break;
                case CurrentWindow.ShortcutSettings:
                    DrawShortcutSettingsTab();
                    break;
                case CurrentWindow.AdvancedSettings:
                    DrawAdvancedSettingsTab();
                    break;
            }
            #endregion

            EditorGUILayout.EndVertical();
        }

        #region Methods
        #region General
        private void DrawHeaderButtons()
        {
            if (GUILayout.Button("FOLDERS", HD_GUI.HeaderButtonStyle, GUILayout.Height(primaryButtonsHeight)))
            {
                SelectFoldersWindow();
            }

            GUILayout.Space(10);

            if (GUILayout.Button("SEPARATORS", HD_GUI.HeaderButtonStyle, GUILayout.Height(primaryButtonsHeight)))
            {
                SelectSeparatorsWindow();
            }

            GUILayout.Label("│", HD_GUI.DivisorLabelStyle, GUILayout.Width(15), GUILayout.Height(primaryButtonsHeight));

            if (GUILayout.Button("HOME", HD_GUI.HeaderButtonStyle, GUILayout.Height(primaryButtonsHeight)))
            {
                SelectHomeWindow();
            }

            GUILayout.Space(10);

            if (GUILayout.Button("ABOUT", HD_GUI.HeaderButtonStyle, GUILayout.Height(primaryButtonsHeight)))
            {
                SelectAboutWindow();
            }

            GUILayout.Label("│", HD_GUI.DivisorLabelStyle, GUILayout.Width(15), GUILayout.Height(primaryButtonsHeight));

            HD_GUI.DrawPopupButton("UTILITIES ▾", HD_GUI.HeaderButtonStyle, primaryButtonsHeight, utilitiesMenuItems);
            
            GUILayout.Space(8);

            HD_GUI.DrawPopupButton("SETTINGS ▾", HD_GUI.HeaderButtonStyle, primaryButtonsHeight, settingsMenuItems);
        }

        private void SelectFoldersWindow()
        {
            SwitchWindow(CurrentWindow.Folders);
        }

        private void SelectSeparatorsWindow()
        {
            SwitchWindow(CurrentWindow.Separators);
        }

        private void SelectHomeWindow()
        {
            SwitchWindow(CurrentWindow.Home);
        }

        private void SelectAboutWindow()
        {
            SwitchWindow(CurrentWindow.About);
        }

        private void SelectToolsWindow()
        {
            SwitchWindow(CurrentWindow.Tools);
        }

        private void SelectPresetsWindow()
        {
            SwitchWindow(CurrentWindow.Presets);
        }

        private void SelectPresetCreatorWindow()
        {
            SwitchWindow(CurrentWindow.PresetCreator);
        }

        private void SelectGeneralSettingsWindow()
        {
            SwitchWindow(CurrentWindow.GeneralSettings);
        }

        private void SelectDesignSettingsWindow()
        {
            SwitchWindow(CurrentWindow.DesignSettings);
        }

        private void SelectShortcutSettingsWindow()
        {
            SwitchWindow(CurrentWindow.ShortcutSettings);
        }

        private void SelectAdvancedSettingsWindow()
        {
            SwitchWindow(CurrentWindow.AdvancedSettings);
        }

        public static void SwitchWindow(CurrentWindow newWindow, Action extraAction = null)
        {
            if (currentWindow == newWindow) return;

            extraAction?.Invoke();
            currentWindow = newWindow;
            HD_Session.instance.currentWindow = currentWindow;
            UpdateCurrentWindowLabel();
        }

        private static void UpdateCurrentWindowLabel()
        {
            string name = currentWindow.ToString();
            string correctedName = System.Text.RegularExpressions.Regex.Replace(name, "([a-z])([A-Z])", "$1 $2");
            cachedCurrentWindowLabel = correctedName.ToUpper();
        }
        #endregion

        #region Home
        private void DrawHomeTab()
        {
            homeScroll = EditorGUILayout.BeginScrollView(homeScroll, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            GUILayout.FlexibleSpace();

            DrawTitle();
            GUILayout.Space(5);
            EditorGUILayout.BeginHorizontal();
            DrawButtons();
            EditorGUILayout.EndHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Space(40);
            DrawFooter();

            EditorGUILayout.EndScrollView();
        }

        private void DrawTitle()
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            float labelWidth = position.width * titleWidthPercentage;
            float labelHeight = labelWidth * titleAspectRatio;

            labelWidth = Mathf.Clamp(labelWidth, titleMinWidth, titleMaxWidth);
            labelHeight = Mathf.Clamp(labelHeight, titleMinHeight, titleMaxHeight);

            GUILayout.Label(HD_Editor.IsProSkin ? HD_Resources.Graphics.TitleDark : HD_Resources.Graphics.TitleLight, HD_GUI.TitleLabelStyle, GUILayout.Width(labelWidth), GUILayout.Height(labelHeight));

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }

        private void DrawButtons()
        {
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("FOLDERS", HD_GUI.PrimaryButtonStyle, GUILayout.Height(primaryButtonsHeight)))
            {
                SwitchWindow(CurrentWindow.Folders);
            }

            GUILayout.Space(15);

            if (GUILayout.Button("SEPARATORS", HD_GUI.PrimaryButtonStyle, GUILayout.Height(primaryButtonsHeight)))
            {
                SwitchWindow(CurrentWindow.Separators);
            }

            GUILayout.Space(15);

            if (GUILayout.Button("HOME", HD_GUI.PrimaryButtonStyle, GUILayout.Height(primaryButtonsHeight)))
            {
                SwitchWindow(CurrentWindow.Home);
            }

            GUILayout.Space(15);

            if (GUILayout.Button("ABOUT", HD_GUI.PrimaryButtonStyle, GUILayout.Height(primaryButtonsHeight)))
            {
                SwitchWindow(CurrentWindow.About);
            }

            GUILayout.Space(13);

            HD_GUI.DrawPopupButton("UTILITIES ▾", HD_GUI.PrimaryButtonStyle, primaryButtonsHeight, utilitiesMenuItems);

            GUILayout.Space(9);

            HD_GUI.DrawPopupButton("SETTINGS ▾", HD_GUI.PrimaryButtonStyle, primaryButtonsHeight, settingsMenuItems);

            GUILayout.FlexibleSpace();
        }

        private void DrawFooter()
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(5);

            #region Review Link
            const string linkText = "REVIEW HD";
            const string linkHex = "3EA6FF";

            GUIStyle linkStyle = new(HD_GUI.FooterLabelStyle);
            linkStyle.normal.textColor = HD_Color.HexToColor(linkHex);
            linkStyle.hover.textColor = HD_Color.HexToColor(linkHex);
            linkStyle.active.textColor = linkStyle.hover.textColor;

            Rect r = GUILayoutUtility.GetRect(new GUIContent(linkText), linkStyle, GUILayout.Height(20));
            EditorGUIUtility.AddCursorRect(r, MouseCursor.Link);

            if (GUI.Button(r, linkText, linkStyle))
            {
                Application.OpenURL("https://assetstore.unity.com/packages/tools/utilities/hierarchy-designer-273928");
            }
            #endregion

            GUILayout.FlexibleSpace();

            GUILayout.Label($"{HD_Constants.AssetVersion}", HD_GUI.FooterLabelStyle, GUILayout.Height(20));

            GUILayout.Space(5);
            EditorGUILayout.EndHorizontal();
        }
        #endregion

        #region About
        private void DrawAboutPanel()
        {
            EditorGUILayout.BeginVertical();
            EditorGUILayout.BeginHorizontal();
            DrawSummary();
            EditorGUILayout.BeginVertical();
            DrawPatchNotes();
            DrawMyOtherAssets();
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        private void DrawSummary()
        {
            EditorGUILayout.BeginHorizontal(HD_GUI.SecondaryPanelStyle);
            aboutSummaryScroll = EditorGUILayout.BeginScrollView(aboutSummaryScroll, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

            GUILayout.Label("Features Breakdown", HD_GUI.FieldsCategoryLabelStyle);

            GUILayout.Label("Folders", HD_GUI.MiniBoldLabelStyle);
            GUILayout.Label(folderText, HD_GUI.RegularLabelStyle);

            GUILayout.Label("Separators", HD_GUI.MiniBoldLabelStyle);
            GUILayout.Label(separatorText, HD_GUI.RegularLabelStyle);

            GUILayout.Label("Saved Data", HD_GUI.MiniBoldLabelStyle);
            GUILayout.Label(savedDataText, HD_GUI.RegularLabelStyle);

            GUILayout.Label("Additional Notes", HD_GUI.FieldsCategoryLabelStyle);
            GUILayout.Label(additionalNotesText, HD_GUI.RegularLabelStyle);

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndHorizontal();
        }

        private void DrawPatchNotes()
        {
            EditorGUILayout.BeginHorizontal(HD_GUI.SecondaryPanelStyle);
            aboutPatchNotesScroll = EditorGUILayout.BeginScrollView(aboutPatchNotesScroll, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            GUILayout.Label("Patch Notes", HD_GUI.FieldsCategoryLabelStyle);
            GUILayout.Label(patchNotes, HD_GUI.RegularLabelStyle);
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndHorizontal();
        }

        private void DrawMyOtherAssets()
        {
            EditorGUILayout.BeginVertical(HD_GUI.SecondaryPanelStyle, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            aboutMyOtherAssetsScroll = EditorGUILayout.BeginScrollView(aboutMyOtherAssetsScroll, GUILayout.MinHeight(200), GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

            GUILayout.Label("My Other Assets", HD_GUI.FieldsCategoryLeftLabelStyle);
            GUILayout.Space(5);

            #region Project Designer
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(string.Empty, HD_GUI.PromotionalProjectDesignerStyle))
            {
                Application.OpenURL("https://assetstore.unity.com/packages/tools/utilities/project-designer-330601");
            }
            GUILayout.Space(5);
            EditorGUILayout.BeginVertical();
            GUILayout.Label("Project Designer", HD_GUI.MiniBoldLabelLeftStyle);
            GUILayout.Space(4);
            GUILayout.Label("An editor tool designed to enhance your project window and improve your workflow.", HD_GUI.RegularLabelLeftStyle);
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            #endregion

            GUILayout.Space(10);

            #region PicEase
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(string.Empty, HD_GUI.PromotionalPicEaseStyle))
            {
                Application.OpenURL("https://assetstore.unity.com/packages/tools/utilities/picease-297051");
            }
            GUILayout.Space(5);
            EditorGUILayout.BeginVertical();
            GUILayout.Label("PicEase", HD_GUI.MiniBoldLabelLeftStyle);
            GUILayout.Space(4);
            GUILayout.Label("An image editor, map generator and screenshot tool.", HD_GUI.RegularLabelLeftStyle);
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            #endregion

            GUILayout.Space(10);

            #region PicEase Mini
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(string.Empty, HD_GUI.PromotionalPicEaseMiniStyle))
            {
                Application.OpenURL("https://assetstore.unity.com/packages/tools/utilities/picease-mini-314430");
            }
            GUILayout.Space(5);
            EditorGUILayout.BeginVertical();
            GUILayout.Label("PicEase Mini", HD_GUI.MiniBoldLabelLeftStyle);
            GUILayout.Space(4);
            GUILayout.Label("An image editor and screenshot tool.", HD_GUI.RegularLabelLeftStyle);
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            #endregion

            GUILayout.Space(10);

            #region PicEase Lite
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(string.Empty, HD_GUI.PromotionalPicEaseLiteStyle))
            {
                Application.OpenURL("https://assetstore.unity.com/packages/tools/utilities/picease-lite-302896");
            }
            GUILayout.Space(5);
            EditorGUILayout.BeginVertical();
            GUILayout.Label("PicEase Lite", HD_GUI.MiniBoldLabelLeftStyle);
            GUILayout.Space(4);
            GUILayout.Label("The Lite, free version of PicEase.", HD_GUI.RegularLabelLeftStyle);
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            #endregion

            GUILayout.Space(10);

            #region PicEase Post Processing (Built-In)
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(string.Empty, HD_GUI.PromotionalPicEasePostProcessingBuiltIn))
            {
                Application.OpenURL("https://assetstore.unity.com/packages/vfx/shaders/fullscreen-camera-effects/picease-post-processing-built-in-328900");
            }
            GUILayout.Space(5);
            EditorGUILayout.BeginVertical();
            GUILayout.Label("PicEase Post Processing (Built-In)", HD_GUI.MiniBoldLabelLeftStyle);
            GUILayout.Space(4);
            GUILayout.Label("Quick and easy post-processing effects to use in your games.", HD_GUI.RegularLabelLeftStyle);
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            #endregion

            GUILayout.Space(10);

            #region PicEase Post Processing (URP)
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(string.Empty, HD_GUI.PromotionalPicEasePostProcessingURP))
            {
                Application.OpenURL("https://assetstore.unity.com/packages/vfx/shaders/fullscreen-camera-effects/picease-post-processing-urp-329189");
            }
            GUILayout.Space(5);
            EditorGUILayout.BeginVertical();
            GUILayout.Label("PicEase Post Processing (URP)", HD_GUI.MiniBoldLabelLeftStyle);
            GUILayout.Space(4);
            GUILayout.Label("Quick and easy post-processing effects to use in your games.", HD_GUI.RegularLabelLeftStyle);
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            #endregion

            GUILayout.Space(10);

            #region Editor Favorites
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(string.Empty, HD_GUI.PromotionalEditorFavorites))
            {
                Application.OpenURL("https://assetstore.unity.com/packages/vfx/shaders/fullscreen-camera-effects/picease-post-processing-urp-329189");
            }
            GUILayout.Space(5);
            EditorGUILayout.BeginVertical();
            GUILayout.Label("Editor Favorites", HD_GUI.MiniBoldLabelLeftStyle);
            GUILayout.Space(4);
            GUILayout.Label("An editor extension that lets you create customizable favorites tabs to quickly store, organize, and access your most-used assets.", HD_GUI.RegularLabelLeftStyle);
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            #endregion

            GUILayout.Space(10);

            #region Music Player Manager
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(string.Empty, HD_GUI.PromotionalMusicPlayerManager))
            {
                Application.OpenURL("https://assetstore.unity.com/packages/tools/audio/music-player-manager-330161");
            }
            GUILayout.Space(5);
            EditorGUILayout.BeginVertical();
            GUILayout.Label("Music Player Manager", HD_GUI.MiniBoldLabelLeftStyle);
            GUILayout.Space(4);
            GUILayout.Label("A lightweight, persistent music manager for Unity with playlist support, random/loop modes, and crossfade playback — all controllable via inspector or script.", HD_GUI.RegularLabelLeftStyle);
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            #endregion

            GUILayout.Space(10);

            #region Procedural Skybox 2.0
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(string.Empty, HD_GUI.PromotionalProceduralSkyboxStyle))
            {
                Application.OpenURL("https://assetstore.unity.com/packages/2d/textures-materials/sky/procedural-skybox-2-0-328930");
            }
            GUILayout.Space(5);
            EditorGUILayout.BeginVertical();
            GUILayout.Label("Procedural Skybox 2.0", HD_GUI.MiniBoldLabelLeftStyle);
            GUILayout.Space(4);
            GUILayout.Label("An extension of Unity’s default procedural skybox.", HD_GUI.RegularLabelLeftStyle);
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            #endregion

            GUILayout.Space(10);

            #region Procedural Skybox 2.0 Lite
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(string.Empty, HD_GUI.PromotionalProceduralSkyboxLiteStyle))
            {
                Application.OpenURL("https://assetstore.unity.com/packages/2d/textures-materials/sky/procedural-skybox-2-0-lite-332554");
            }
            GUILayout.Space(5);
            EditorGUILayout.BeginVertical();
            GUILayout.Label("Procedural Skybox 2.0 Lite", HD_GUI.MiniBoldLabelLeftStyle);
            GUILayout.Space(4);
            GUILayout.Label("The Lite, free version of Procedural Skybox 2.0.", HD_GUI.RegularLabelLeftStyle);
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            #endregion

            GUILayout.Space(10);

            #region Seamless Grass Textures
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(string.Empty, HD_GUI.PromotionalSeamlessGrassTexturesMiniStyle))
            {
                Application.OpenURL("https://assetstore.unity.com/packages/2d/textures-materials/seamless-grass-textures-328603");
            }
            GUILayout.Space(5);
            EditorGUILayout.BeginVertical();
            GUILayout.Label("Seamless Grass Textures", HD_GUI.MiniBoldLabelLeftStyle);
            GUILayout.Space(4);
            GUILayout.Label("Seamless grass textures, optimized for terrain use.", HD_GUI.RegularLabelLeftStyle);
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            #endregion

            GUILayout.Space(10);

            #region Seamless Mud Textures
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(string.Empty, HD_GUI.PromotionalSeamlessMudTexturesMiniStyle))
            {
                Application.OpenURL("https://assetstore.unity.com/packages/2d/textures-materials/seamless-mud-textures-327770");
            }
            GUILayout.Space(5);
            EditorGUILayout.BeginVertical();
            GUILayout.Label("Seamless Mud Textures", HD_GUI.MiniBoldLabelLeftStyle);
            GUILayout.Space(4);
            GUILayout.Label("Seamless mud textures, optimized for terrain use.", HD_GUI.RegularLabelLeftStyle);
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            #endregion

            GUILayout.Space(10);

            #region Seamless Sand Textures
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(string.Empty, HD_GUI.PromotionalSeamlessSandTexturesMiniStyle))
            {
                Application.OpenURL("https://assetstore.unity.com/packages/2d/textures-materials/seamless-sand-textures-326364");
            }
            GUILayout.Space(5);
            EditorGUILayout.BeginVertical();
            GUILayout.Label("Seamless Sand Textures", HD_GUI.MiniBoldLabelLeftStyle);
            GUILayout.Space(4);
            GUILayout.Label("Seamless sand textures, optimized for terrain use.", HD_GUI.RegularLabelLeftStyle);
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            #endregion

            GUILayout.Space(10);

            #region Seamless Snow Textures
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(string.Empty, HD_GUI.PromotionalSeamlessSnowTexturesMiniStyle))
            {
                Application.OpenURL("https://assetstore.unity.com/packages/2d/textures-materials/seamless-snow-textures-326153");
            }
            GUILayout.Space(5);
            EditorGUILayout.BeginVertical();
            GUILayout.Label("Seamless Snow Textures", HD_GUI.MiniBoldLabelLeftStyle);
            GUILayout.Space(4);
            GUILayout.Label("Seamless snow textures, optimized for terrain use.", HD_GUI.RegularLabelLeftStyle);
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            #endregion

            GUILayout.Space(10);

            #region Seamless Grass Pattern Textures
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(string.Empty, HD_GUI.PromotionalSeamlessGrassPatternTexturesMiniStyle))
            {
                Application.OpenURL("https://assetstore.unity.com/packages/2d/textures-materials/seamless-grass-pattern-textures-332098");
            }
            GUILayout.Space(5);
            EditorGUILayout.BeginVertical();
            GUILayout.Label("Seamless Grass Pattern Textures", HD_GUI.MiniBoldLabelLeftStyle);
            GUILayout.Space(4);
            GUILayout.Label("Seamless grass pattern textures, optimized for terrain use.", HD_GUI.RegularLabelLeftStyle);
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            #endregion

            GUILayout.Space(5);
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }
        #endregion

        #region Folders
        private void DrawFoldersTab()
        {
            #region Body
            folderMainScroll = EditorGUILayout.BeginScrollView(folderMainScroll, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            DrawFoldersCreationFields();
            if (tempFolders.Count > 0)
            {
                DrawFoldersGlobalFields();
                DrawFoldersList();
            }
            else
            {
                EditorGUILayout.LabelField("No folders found. Please create a new folder.", HD_GUI.UnassignedLabelStyle);
            }
            EditorGUILayout.EndScrollView();
            #endregion

            #region Footer
            if (GUILayout.Button("Update and Save Folders", GUILayout.Height(primaryButtonsHeight)))
            {
                UpdateAndSaveFoldersData();
            }
            #endregion
        }

        private void DrawFoldersCreationFields()
        {
            EditorGUILayout.BeginVertical(HD_GUI.SecondaryPanelStyle);
            EditorGUILayout.LabelField("Folder Creation", HD_GUI.FieldsCategoryLabelStyle);
            GUILayout.Space(defaultMarginSpacing);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Name", HD_GUI.LayoutLabelStyle, GUILayout.Width(folderCreationLabelWidth));
            newFolderName = EditorGUILayout.TextField(newFolderName);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Text Color", HD_GUI.LayoutLabelStyle, GUILayout.Width(folderCreationLabelWidth));
            newFolderTextColor = EditorGUILayout.ColorField(newFolderTextColor);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            string[] newFontSizeOptionsStrings = Array.ConvertAll(fontSizeOptions, x => x.ToString());
            int newFontSizeIndex = Array.IndexOf(fontSizeOptions, newFolderFontSize);
            EditorGUILayout.LabelField("Font Size", HD_GUI.LayoutLabelStyle, GUILayout.Width(folderCreationLabelWidth));
            newFolderFontSize = fontSizeOptions[EditorGUILayout.Popup(newFontSizeIndex, newFontSizeOptionsStrings)];
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Font Style", HD_GUI.LayoutLabelStyle, GUILayout.Width(folderCreationLabelWidth));
            newFolderFontStyle = (FontStyle)EditorGUILayout.EnumPopup(newFolderFontStyle);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Image Color", HD_GUI.LayoutLabelStyle, GUILayout.Width(folderCreationLabelWidth));
            newFolderIconColor = EditorGUILayout.ColorField(newFolderIconColor);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Image Type", HD_GUI.LayoutLabelStyle, GUILayout.Width(folderCreationLabelWidth));
            if (GUILayout.Button(HD_Folders.GetFolderImageTypeDisplayName(newFolderImageType), EditorStyles.popup))
            {
                ShowFolderImageTypePopup();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(4);
            if (GUILayout.Button("Create Folder", GUILayout.Height(secondaryButtonsHeight)))
            {
                if (IsFolderNameValid(newFolderName))
                {
                    CreateFolder(newFolderName, newFolderTextColor, newFolderFontSize, newFolderFontStyle, newFolderIconColor, newFolderImageType);
                }
                else
                {
                    EditorUtility.DisplayDialog("Invalid Folder Name", "Folder name is either duplicate or invalid.", "OK");
                }
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawFoldersGlobalFields()
        {
            EditorGUILayout.BeginVertical(HD_GUI.SecondaryPanelStyle);
            EditorGUILayout.LabelField("Folders' Global Fields", HD_GUI.FieldsCategoryLabelStyle);
            GUILayout.Space(defaultMarginSpacing);

            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginChangeCheck();
            tempFolderGlobalTextColor = EditorGUILayout.ColorField(tempFolderGlobalTextColor, GUILayout.MinWidth(100), GUILayout.ExpandWidth(true));
            if (EditorGUI.EndChangeCheck()) { UpdateGlobalFolderTextColor(tempFolderGlobalTextColor); }
            EditorGUI.BeginChangeCheck();
            string[] tempFontSizeOptionsStrings = Array.ConvertAll(fontSizeOptions, x => x.ToString());
            int tempFontSizeIndex = Array.IndexOf(fontSizeOptions, tempFolderGlobalFontSize);
            tempFolderGlobalFontSize = fontSizeOptions[EditorGUILayout.Popup(tempFontSizeIndex, tempFontSizeOptionsStrings, GUILayout.Width(50))];
            if (EditorGUI.EndChangeCheck()) { UpdateGlobalFolderFontSize(tempFolderGlobalFontSize); }
            EditorGUI.BeginChangeCheck();
            tempFolderGlobalFontStyle = (FontStyle)EditorGUILayout.EnumPopup(tempFolderGlobalFontStyle, GUILayout.MinWidth(100), GUILayout.ExpandWidth(true));
            if (EditorGUI.EndChangeCheck()) { UpdateGlobalFolderFontStyle(tempFolderGlobalFontStyle); }
            EditorGUI.BeginChangeCheck();
            tempFolderGlobalIconColor = EditorGUILayout.ColorField(tempFolderGlobalIconColor, GUILayout.MinWidth(100), GUILayout.ExpandWidth(true));
            if (EditorGUI.EndChangeCheck()) { UpdateGlobalFolderIconColor(tempFolderGlobalIconColor); }
            if (GUILayout.Button(HD_Folders.GetFolderImageTypeDisplayName(tempGlobalFolderImageType), EditorStyles.popup, GUILayout.MinWidth(125), GUILayout.ExpandWidth(true))) { ShowFolderImageTypePopupGlobal(); }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        private void DrawFoldersList()
        {
            EditorGUILayout.BeginVertical(HD_GUI.SecondaryPanelStyle);
            EditorGUILayout.LabelField("Folders' List", HD_GUI.FieldsCategoryLabelStyle);
            GUILayout.Space(defaultMarginSpacing);

            foldersListScroll = EditorGUILayout.BeginScrollView(foldersListScroll, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            int index = 1;
            for (int i = 0; i < foldersOrder.Count; i++)
            {
                string key = foldersOrder[i];
                DrawFolders(index, key, tempFolders[key], i, foldersOrder.Count);
                index++;
            }
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();

        }

        private void UpdateAndSaveFoldersData()
        {
            HD_Folders.ApplyChangesToFolders(tempFolders, foldersOrder);
            HD_Folders.SaveSettings();
            HD_Manager.ClearFolderCache();
            folderHasModifiedChanges = false;
        }

        private void LoadFolderData()
        {
            tempFolders = HD_Folders.GetAllFoldersData(true);
            foldersOrder = new List<string>(tempFolders.Keys);
        }

        private void LoadFolderCreationFields()
        {
            newFolderTextColor = HD_Settings.FolderDefaultTextColor;
            newFolderFontSize = HD_Settings.FolderDefaultFontSize;
            newFolderFontStyle = HD_Settings.FolderDefaultFontStyle;
            newFolderIconColor = HD_Settings.FolderDefaultImageColor;
            _ = HD_Settings.FolderDefaultImageType;
        }

        private bool IsFolderNameValid(string folderName)
        {
            return !string.IsNullOrEmpty(folderName) && !tempFolders.TryGetValue(folderName, out _);
        }

        private void CreateFolder(string folderName, Color textColor, int fontSize, FontStyle fontStyle, Color ImageColor, HD_Folders.FolderImageType imageType)
        {
            HD_Folders.HD_FolderData newFolderData = new HD_Folders.HD_FolderData
            {
                Name = folderName,
                TextColor = textColor,
                FontSize = fontSize,
                FontStyle = fontStyle,
                ImageColor = ImageColor,
                ImageType = imageType
            };
            tempFolders[folderName] = newFolderData;
            foldersOrder.Add(folderName);
            newFolderName = "";
            newFolderTextColor = HD_Settings.FolderDefaultTextColor;
            newFolderFontSize = HD_Settings.FolderDefaultFontSize;
            newFolderFontStyle = HD_Settings.FolderDefaultFontStyle;
            newFolderIconColor = HD_Settings.FolderDefaultImageColor;
            newFolderImageType = HD_Settings.FolderDefaultImageType;
            folderHasModifiedChanges = true;
            GUI.FocusControl(null);
        }

        private void DrawFolders(int index, string key, HD_Folders.HD_FolderData folderData, int position, int totalItems)
        {
            float folderLabelWidth = HD_GUI.CalculateMaxLabelWidth(tempFolders.Keys);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"{index}) {folderData.Name}", HD_GUI.LayoutLabelStyle, GUILayout.Width(folderLabelWidth));
            EditorGUI.BeginChangeCheck();
            folderData.TextColor = EditorGUILayout.ColorField(folderData.TextColor, GUILayout.MinWidth(100), GUILayout.ExpandWidth(true));
            string[] fontSizeOptionsStrings = Array.ConvertAll(fontSizeOptions, x => x.ToString());
            int fontSizeIndex = Array.IndexOf(fontSizeOptions, folderData.FontSize);
            if (fontSizeIndex == -1) { fontSizeIndex = 5; }
            folderData.FontSize = fontSizeOptions[EditorGUILayout.Popup(fontSizeIndex, fontSizeOptionsStrings, GUILayout.Width(50))];
            folderData.FontStyle = (FontStyle)EditorGUILayout.EnumPopup(folderData.FontStyle, GUILayout.Width(110));
            folderData.ImageColor = EditorGUILayout.ColorField(folderData.ImageColor, GUILayout.MinWidth(100), GUILayout.ExpandWidth(true));
            if (GUILayout.Button(HD_Folders.GetFolderImageTypeDisplayName(folderData.ImageType), EditorStyles.popup, GUILayout.MinWidth(125), GUILayout.ExpandWidth(true))) { ShowFolderImageTypePopupForFolder(folderData); }
            if (EditorGUI.EndChangeCheck()) { folderHasModifiedChanges = true; }

            if (GUILayout.Button("↑", GUILayout.Width(moveItemInListButtonWidth)) && position > 0)
            {
                MoveFolder(position, position - 1);
            }
            if (GUILayout.Button("↓", GUILayout.Width(moveItemInListButtonWidth)) && position < totalItems - 1)
            {
                MoveFolder(position, position + 1);
            }
            if (GUILayout.Button("Create", GUILayout.Width(createButtonWidth)))
            {
                CreateFolderGameObject(folderData);
            }
            if (GUILayout.Button("Remove", GUILayout.Width(removeButtonWidth)))
            {
                RemoveFolder(key);
            }
            EditorGUILayout.EndHorizontal();
        }

        private void MoveFolder(int indexA, int indexB)
        {
            string keyA = foldersOrder[indexA];
            string keyB = foldersOrder[indexB];

            foldersOrder[indexA] = keyB;
            foldersOrder[indexB] = keyA;
            folderHasModifiedChanges = true;
        }

        private void CreateFolderGameObject(HD_Folders.HD_FolderData folderData)
        {
            GameObject folder = new GameObject(folderData.Name);
            folder.AddComponent<HierarchyDesignerFolder>();
            Undo.RegisterCreatedObjectUndo(folder, $"Create {folderData.Name}");

            Texture2D inspectorIcon = HD_Resources.Textures.FolderScene;
            if (inspectorIcon != null)
            {
                EditorGUIUtility.SetIconForObject(folder, inspectorIcon);
            }
        }

        private void RemoveFolder(string folderName)
        {
            if (tempFolders.TryGetValue(folderName, out _))
            {
                tempFolders.Remove(folderName);
                foldersOrder.Remove(folderName);
                folderHasModifiedChanges = true;
                GUIUtility.ExitGUI();
            }
        }

        private void ShowFolderImageTypePopup()
        {
            GenericMenu menu = new GenericMenu();
            Dictionary<string, List<string>> groupedTypes = HD_Folders.GetFolderImageTypesGrouped();
            foreach (KeyValuePair<string, List<string>> group in groupedTypes)
            {
                foreach (string typeName in group.Value)
                {
                    menu.AddItem(new GUIContent($"{group.Key}/{typeName}"), typeName == HD_Folders.GetFolderImageTypeDisplayName(newFolderImageType), OnFolderImageTypeSelected, typeName);
                }
            }
            menu.ShowAsContext();
        }

        private void ShowFolderImageTypePopupGlobal()
        {
            GenericMenu menu = new GenericMenu();
            Dictionary<string, List<string>> groupedTypes = HD_Folders.GetFolderImageTypesGrouped();
            foreach (KeyValuePair<string, List<string>> group in groupedTypes)
            {
                foreach (string typeName in group.Value)
                {
                    menu.AddItem(new GUIContent($"{group.Key}/{typeName}"), typeName == HD_Folders.GetFolderImageTypeDisplayName(tempGlobalFolderImageType), OnFolderImageTypeGlobalSelected, typeName);
                }
            }
            menu.ShowAsContext();
        }

        private void ShowFolderImageTypePopupForFolder(HD_Folders.HD_FolderData folderData)
        {
            GenericMenu menu = new GenericMenu();
            Dictionary<string, List<string>> groupedTypes = HD_Folders.GetFolderImageTypesGrouped();
            foreach (KeyValuePair<string, List<string>> group in groupedTypes)
            {
                foreach (string typeName in group.Value)
                {
                    menu.AddItem(new GUIContent($"{group.Key}/{typeName}"), typeName == HD_Folders.GetFolderImageTypeDisplayName(folderData.ImageType), OnFolderImageTypeForFolderSelected, new KeyValuePair<HD_Folders.HD_FolderData, string>(folderData, typeName));
                }
            }
            menu.ShowAsContext();
        }

        private void OnFolderImageTypeSelected(object imageTypeObj)
        {
            string typeName = (string)imageTypeObj;
            newFolderImageType = HD_Folders.ParseFolderImageType(typeName);
        }

        private void OnFolderImageTypeGlobalSelected(object imageTypeObj)
        {
            string typeName = (string)imageTypeObj;
            tempGlobalFolderImageType = HD_Folders.ParseFolderImageType(typeName);
            UpdateGlobalFolderImageType(tempGlobalFolderImageType);
        }

        private void OnFolderImageTypeForFolderSelected(object folderDataAndTypeObj)
        {
            KeyValuePair<HD_Folders.HD_FolderData, string> folderDataAndType = (KeyValuePair<HD_Folders.HD_FolderData, string>)folderDataAndTypeObj;
            folderDataAndType.Key.ImageType = HD_Folders.ParseFolderImageType(folderDataAndType.Value);
        }

        private void UpdateGlobalFolderTextColor(Color color)
        {
            foreach (HD_Folders.HD_FolderData folder in tempFolders.Values)
            {
                folder.TextColor = color;
            }
            folderHasModifiedChanges = true;
        }

        private void UpdateGlobalFolderFontSize(int size)
        {
            foreach (HD_Folders.HD_FolderData folder in tempFolders.Values)
            {
                folder.FontSize = size;
            }
            folderHasModifiedChanges = true;
        }

        private void UpdateGlobalFolderFontStyle(FontStyle style)
        {
            foreach (HD_Folders.HD_FolderData folder in tempFolders.Values)
            {
                folder.FontStyle = style;
            }
            folderHasModifiedChanges = true;
        }

        private void UpdateGlobalFolderIconColor(Color color)
        {
            foreach (HD_Folders.HD_FolderData folder in tempFolders.Values)
            {
                folder.ImageColor = color;
            }
            folderHasModifiedChanges = true;
        }

        private void UpdateGlobalFolderImageType(HD_Folders.FolderImageType imageType)
        {
            foreach (HD_Folders.HD_FolderData folder in tempFolders.Values)
            {
                folder.ImageType = imageType;
            }
            folderHasModifiedChanges = true;
        }
        #endregion

        #region Separators
        private void DrawSeparatorsTab()
        {
            #region Body
            separatorMainScroll = EditorGUILayout.BeginScrollView(separatorMainScroll, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            DrawSeparatorsCreationFields();

            if (tempSeparators.Count > 0)
            {
                DrawSeparatorsGlobalFields();
                DrawSeparatorsList();
            }
            else
            {
                EditorGUILayout.LabelField("No separators found. Please create a new separator.", HD_GUI.UnassignedLabelStyle);
            }
            EditorGUILayout.EndScrollView();
            #endregion

            #region Footer
            if (GUILayout.Button("Update and Save Separators", GUILayout.Height(primaryButtonsHeight)))
            {
                UpdateAndSaveSeparatorsData();
            }
            #endregion
        }

        private void DrawSeparatorsCreationFields()
        {
            EditorGUILayout.BeginVertical(HD_GUI.SecondaryPanelStyle);
            EditorGUILayout.LabelField("Separator Creation", HD_GUI.FieldsCategoryLabelStyle);
            GUILayout.Space(defaultMarginSpacing);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Name", HD_GUI.LayoutLabelStyle, GUILayout.Width(separatorCreationLabelWidth));
            newSeparatorName = EditorGUILayout.TextField(newSeparatorName);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Text Color", HD_GUI.LayoutLabelStyle, GUILayout.Width(separatorCreationLabelWidth));
            newSeparatorTextColor = EditorGUILayout.ColorField(newSeparatorTextColor);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Is Gradient", HD_GUI.LayoutLabelStyle, GUILayout.Width(separatorCreationLabelWidth));
            newSeparatorIsGradient = EditorGUILayout.Toggle(newSeparatorIsGradient);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (newSeparatorIsGradient)
            {
                EditorGUILayout.LabelField("Background Gradient", HD_GUI.LayoutLabelStyle, GUILayout.Width(separatorCreationLabelWidth));
                newSeparatorBackgroundGradient = EditorGUILayout.GradientField(newSeparatorBackgroundGradient) != null ? newSeparatorBackgroundGradient : new Gradient();
            }
            else
            {
                EditorGUILayout.LabelField("Background Color", HD_GUI.LayoutLabelStyle, GUILayout.Width(separatorCreationLabelWidth));
                newSeparatorBackgroundColor = EditorGUILayout.ColorField(newSeparatorBackgroundColor);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            string[] newFontSizeOptionsStrings = Array.ConvertAll(fontSizeOptions, x => x.ToString());
            int newFontSizeIndex = Array.IndexOf(fontSizeOptions, newSeparatorFontSize);
            EditorGUILayout.LabelField("Font Size", HD_GUI.LayoutLabelStyle, GUILayout.Width(separatorCreationLabelWidth));
            newSeparatorFontSize = fontSizeOptions[EditorGUILayout.Popup(newFontSizeIndex, newFontSizeOptionsStrings)];
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Font Style", HD_GUI.LayoutLabelStyle, GUILayout.Width(separatorCreationLabelWidth));
            newSeparatorFontStyle = (FontStyle)EditorGUILayout.EnumPopup(newSeparatorFontStyle);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Text Anchor", HD_GUI.LayoutLabelStyle, GUILayout.Width(separatorCreationLabelWidth));
            newSeparatorTextAnchor = (TextAnchor)EditorGUILayout.EnumPopup(newSeparatorTextAnchor);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Background Type", HD_GUI.LayoutLabelStyle, GUILayout.Width(separatorCreationLabelWidth));
            if (GUILayout.Button(HD_Separators.GetSeparatorImageTypeDisplayName(newSeparatorImageType), EditorStyles.popup))
            {
                ShowSeparatorImageTypePopup();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(4);
            if (GUILayout.Button("Create Separator", GUILayout.Height(secondaryButtonsHeight)))
            {
                if (IsSeparatorNameValid(newSeparatorName))
                {
                    CreateSeparator(newSeparatorName, newSeparatorTextColor, newSeparatorIsGradient, newSeparatorBackgroundColor, newSeparatorBackgroundGradient, newSeparatorFontSize, newSeparatorFontStyle, newSeparatorTextAnchor, newSeparatorImageType);
                }
                else
                {
                    EditorUtility.DisplayDialog("Invalid Separator Name", "Separator name is either duplicate or invalid.", "OK");
                }
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawSeparatorsGlobalFields()
        {
            EditorGUILayout.BeginVertical(HD_GUI.SecondaryPanelStyle);
            EditorGUILayout.LabelField("Separators' Global Fields", HD_GUI.FieldsCategoryLabelStyle);
            GUILayout.Space(defaultMarginSpacing);

            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginChangeCheck();
            tempSeparatorGlobalTextColor = EditorGUILayout.ColorField(tempSeparatorGlobalTextColor, GUILayout.MinWidth(100), GUILayout.ExpandWidth(true));
            if (EditorGUI.EndChangeCheck()) { UpdateGlobalSeparatorTextColor(tempSeparatorGlobalTextColor); }
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.Space(defaultMarginSpacing);
            tempSeparatorGlobalIsGradient = EditorGUILayout.Toggle(tempSeparatorGlobalIsGradient, GUILayout.Width(18));
            if (EditorGUI.EndChangeCheck()) { UpdateGlobalSeparatorIsGradientBackground(tempSeparatorGlobalIsGradient); }
            EditorGUI.BeginChangeCheck();
            tempSeparatorGlobalBackgroundColor = EditorGUILayout.ColorField(tempSeparatorGlobalBackgroundColor, GUILayout.MinWidth(100), GUILayout.ExpandWidth(true));
            if (EditorGUI.EndChangeCheck()) { UpdateGlobalSeparatorBackgroundColor(tempSeparatorGlobalBackgroundColor); }
            EditorGUI.BeginChangeCheck();
            tempSeparatorGlobalBackgroundGradient = EditorGUILayout.GradientField(tempSeparatorGlobalBackgroundGradient, GUILayout.MinWidth(100), GUILayout.ExpandWidth(true));
            if (EditorGUI.EndChangeCheck()) { UpdateGlobalSeparatorGradientBackground(tempSeparatorGlobalBackgroundGradient); }
            EditorGUI.BeginChangeCheck();
            string[] tempFontSizeOptionsStrings = Array.ConvertAll(fontSizeOptions, x => x.ToString());
            int tempFontSizeIndex = Array.IndexOf(fontSizeOptions, tempSeparatorGlobalFontSize);
            tempSeparatorGlobalFontSize = fontSizeOptions[EditorGUILayout.Popup(tempFontSizeIndex, tempFontSizeOptionsStrings, GUILayout.Width(50))];
            if (EditorGUI.EndChangeCheck()) { UpdateGlobalSeparatorFontSize(tempSeparatorGlobalFontSize); }
            EditorGUI.BeginChangeCheck();
            tempSeparatorGlobalFontStyle = (FontStyle)EditorGUILayout.EnumPopup(tempSeparatorGlobalFontStyle, GUILayout.MinWidth(100), GUILayout.ExpandWidth(true));
            if (EditorGUI.EndChangeCheck()) { UpdateGlobalSeparatorFontStyle(tempSeparatorGlobalFontStyle); }
            EditorGUI.BeginChangeCheck();
            tempSeparatorGlobalTextAnchor = (TextAnchor)EditorGUILayout.EnumPopup(tempSeparatorGlobalTextAnchor, GUILayout.MinWidth(100), GUILayout.ExpandWidth(true));
            if (EditorGUI.EndChangeCheck()) { UpdateGlobalSeparatorTextAnchor(tempSeparatorGlobalTextAnchor); }
            if (GUILayout.Button(HD_Separators.GetSeparatorImageTypeDisplayName(tempSeparatorGlobalImageType), EditorStyles.popup, GUILayout.MinWidth(150), GUILayout.ExpandWidth(true))) { ShowSeparatorImageTypePopupGlobal(); }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        private void DrawSeparatorsList()
        {
            EditorGUILayout.BeginVertical(HD_GUI.SecondaryPanelStyle);
            EditorGUILayout.LabelField("Separators' List", HD_GUI.FieldsCategoryLabelStyle);
            GUILayout.Space(defaultMarginSpacing);

            separatorsListScroll = EditorGUILayout.BeginScrollView(separatorsListScroll, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            int index = 1;
            for (int i = 0; i < separatorsOrder.Count; i++)
            {
                string key = separatorsOrder[i];
                DrawSeparators(index, key, tempSeparators[key], i, separatorsOrder.Count);
                index++;
            }
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private void UpdateAndSaveSeparatorsData()
        {
            HD_Separators.ApplyChangesToSeparators(tempSeparators, separatorsOrder);
            HD_Separators.SaveSettings();
            HD_Manager.ClearSeparatorCache();
            separatorHasModifiedChanges = false;
        }

        private void LoadSeparatorData()
        {
            tempSeparators = HD_Separators.GetAllSeparatorsData(true);
            separatorsOrder = new List<string>(tempSeparators.Keys);
        }

        private void LoadSeparatorsCreationFields()
        {
            newSeparatorName = "";
            newSeparatorTextColor = HD_Settings.SeparatorDefaultTextColor;
            newSeparatorIsGradient = HD_Settings.SeparatorDefaultIsGradientBackground;
            newSeparatorBackgroundColor = HD_Settings.SeparatorDefaultBackgroundColor;
            newSeparatorBackgroundGradient = HD_Color.CopyGradient(HD_Settings.SeparatorDefaultBackgroundGradient);
            newSeparatorFontSize = HD_Settings.SeparatorDefaultFontSize;
            newSeparatorFontStyle = HD_Settings.SeparatorDefaultFontStyle;
            newSeparatorTextAnchor = HD_Settings.SeparatorDefaultTextAnchor;
            _ = HD_Settings.SeparatorDefaultImageType;
        }

        private bool IsSeparatorNameValid(string separatorName)
        {
            return !string.IsNullOrEmpty(separatorName) && !tempSeparators.TryGetValue(separatorName, out _);
        }

        private void CreateSeparator(string separatorName, Color textColor, bool isGradient, Color backgroundColor, Gradient backgroundGradient, int fontSize, FontStyle fontStyle, TextAnchor textAnchor, HD_Separators.SeparatorImageType imageType)
        {
            HD_Separators.HD_SeparatorData newSeparatorData = new()
            {
                Name = separatorName,
                TextColor = textColor,
                IsGradientBackground = isGradient,
                BackgroundColor = backgroundColor,
                BackgroundGradient = HD_Color.CopyGradient(backgroundGradient),
                FontSize = fontSize,
                FontStyle = fontStyle,
                TextAnchor = textAnchor,
                ImageType = imageType,

            };
            tempSeparators[separatorName] = newSeparatorData;
            separatorsOrder.Add(separatorName);
            LoadSeparatorsCreationFields();
            separatorHasModifiedChanges = true;
            GUI.FocusControl(null);
        }

        private void DrawSeparators(int index, string key, HD_Separators.HD_SeparatorData separatorData, int position, int totalItems)
        {
            float separatorLabelWidth = HD_GUI.CalculateMaxLabelWidth(tempSeparators.Keys);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"{index}) {separatorData.Name}", HD_GUI.LayoutLabelStyle, GUILayout.Width(separatorLabelWidth));
            EditorGUI.BeginChangeCheck();
            separatorData.TextColor = EditorGUILayout.ColorField(separatorData.TextColor, GUILayout.MinWidth(100), GUILayout.ExpandWidth(true));
            GUILayout.Space(defaultMarginSpacing);
            separatorData.IsGradientBackground = EditorGUILayout.Toggle(separatorData.IsGradientBackground, GUILayout.Width(18));
            if (separatorData.IsGradientBackground) { separatorData.BackgroundGradient = EditorGUILayout.GradientField(separatorData.BackgroundGradient, GUILayout.MinWidth(100), GUILayout.ExpandWidth(true)) ?? new Gradient(); }
            else { separatorData.BackgroundColor = EditorGUILayout.ColorField(separatorData.BackgroundColor, GUILayout.MinWidth(100), GUILayout.ExpandWidth(true)); }
            string[] fontSizeOptionsStrings = Array.ConvertAll(fontSizeOptions, x => x.ToString());
            int fontSizeIndex = Array.IndexOf(fontSizeOptions, separatorData.FontSize);
            if (fontSizeIndex == -1) { fontSizeIndex = 5; }
            separatorData.FontSize = fontSizeOptions[EditorGUILayout.Popup(fontSizeIndex, fontSizeOptionsStrings, GUILayout.Width(50))];
            separatorData.FontStyle = (FontStyle)EditorGUILayout.EnumPopup(separatorData.FontStyle, GUILayout.Width(100));
            separatorData.TextAnchor = (TextAnchor)EditorGUILayout.EnumPopup(separatorData.TextAnchor, GUILayout.Width(115));
            if (GUILayout.Button(HD_Separators.GetSeparatorImageTypeDisplayName(separatorData.ImageType), EditorStyles.popup, GUILayout.Width(150))) { ShowSeparatorImageTypePopupForSeparator(separatorData); }
            if (EditorGUI.EndChangeCheck()) { separatorHasModifiedChanges = true; }

            if (GUILayout.Button("↑", GUILayout.Width(moveItemInListButtonWidth)) && position > 0)
            {
                MoveSeparator(position, position - 1);
            }
            if (GUILayout.Button("↓", GUILayout.Width(moveItemInListButtonWidth)) && position < totalItems - 1)
            {
                MoveSeparator(position, position + 1);
            }
            if (GUILayout.Button("Create", GUILayout.Width(createButtonWidth)))
            {
                CreateSeparatorGameObject(separatorData);
            }
            if (GUILayout.Button("Remove", GUILayout.Width(removeButtonWidth)))
            {
                RemoveSeparator(key);
            }
            EditorGUILayout.EndHorizontal();
        }

        private void MoveSeparator(int indexA, int indexB)
        {
            string keyA = separatorsOrder[indexA];
            string keyB = separatorsOrder[indexB];

            separatorsOrder[indexA] = keyB;
            separatorsOrder[indexB] = keyA;
            separatorHasModifiedChanges = true;
        }

        private void CreateSeparatorGameObject(HD_Separators.HD_SeparatorData separatorData)
        {
            GameObject separator = new($"{HD_Constants.SeparatorPrefix}{separatorData.Name}");
            separator.tag = HD_Constants.SeparatorTag;
            HD_Operations.SetSeparatorState(separator, false);
            separator.SetActive(false);
            Undo.RegisterCreatedObjectUndo(separator, $"Create {separatorData.Name}");

            Texture2D inspectorIcon = HD_Resources.Textures.SeparatorInspectorIcon;
            if (inspectorIcon != null)
            {
                EditorGUIUtility.SetIconForObject(separator, inspectorIcon);
            }
        }

        private void RemoveSeparator(string separatorName)
        {
            if (tempSeparators.TryGetValue(separatorName, out _))
            {
                tempSeparators.Remove(separatorName);
                separatorsOrder.Remove(separatorName);
                separatorHasModifiedChanges = true;
                GUIUtility.ExitGUI();
            }
        }

        private void ShowSeparatorImageTypePopup()
        {
            GenericMenu menu = new();
            Dictionary<string, List<string>> groupedTypes = HD_Separators.GetSeparatorImageTypesGrouped();
            foreach (KeyValuePair<string, List<string>> group in groupedTypes)
            {
                foreach (string typeName in group.Value)
                {
                    menu.AddItem(new GUIContent($"{group.Key}/{typeName}"), typeName == HD_Separators.GetSeparatorImageTypeDisplayName(newSeparatorImageType), OnSeparatorImageTypeSelected, typeName);
                }
            }
            menu.ShowAsContext();
        }

        private void ShowSeparatorImageTypePopupGlobal()
        {
            GenericMenu menu = new();
            Dictionary<string, List<string>> groupedTypes = HD_Separators.GetSeparatorImageTypesGrouped();
            foreach (KeyValuePair<string, List<string>> group in groupedTypes)
            {
                foreach (string typeName in group.Value)
                {
                    menu.AddItem(new GUIContent($"{group.Key}/{typeName}"), typeName == HD_Separators.GetSeparatorImageTypeDisplayName(tempSeparatorGlobalImageType), OnSeparatorImageTypeGlobalSelected, typeName);
                }
            }
            menu.ShowAsContext();
        }

        private void ShowSeparatorImageTypePopupForSeparator(HD_Separators.HD_SeparatorData separatorData)
        {
            GenericMenu menu = new();
            Dictionary<string, List<string>> groupedTypes = HD_Separators.GetSeparatorImageTypesGrouped();
            foreach (KeyValuePair<string, List<string>> group in groupedTypes)
            {
                foreach (string typeName in group.Value)
                {
                    menu.AddItem(new GUIContent($"{group.Key}/{typeName}"), typeName == HD_Separators.GetSeparatorImageTypeDisplayName(separatorData.ImageType), OnSeparatorImageTypeForSeparatorSelected, new KeyValuePair<HD_Separators.HD_SeparatorData, string>(separatorData, typeName));
                }
            }
            menu.ShowAsContext();
        }

        private void OnSeparatorImageTypeSelected(object imageTypeObj)
        {
            string typeName = (string)imageTypeObj;
            newSeparatorImageType = HD_Separators.ParseSeparatorImageType(typeName);
        }

        private void OnSeparatorImageTypeGlobalSelected(object imageTypeObj)
        {
            string typeName = (string)imageTypeObj;
            tempSeparatorGlobalImageType = HD_Separators.ParseSeparatorImageType(typeName);
            UpdateGlobalSeparatorImageType(tempSeparatorGlobalImageType);
        }

        private void OnSeparatorImageTypeForSeparatorSelected(object separatorDataAndTypeObj)
        {
            KeyValuePair<HD_Separators.HD_SeparatorData, string> separatorDataAndType = (KeyValuePair<HD_Separators.HD_SeparatorData, string>)separatorDataAndTypeObj;
            separatorDataAndType.Key.ImageType = HD_Separators.ParseSeparatorImageType(separatorDataAndType.Value);
        }

        private void UpdateGlobalSeparatorTextColor(Color color)
        {
            foreach (HD_Separators.HD_SeparatorData separator in tempSeparators.Values)
            {
                separator.TextColor = color;
            }
            separatorHasModifiedChanges = true;
        }

        private void UpdateGlobalSeparatorIsGradientBackground(bool isGradientBackground)
        {
            foreach (HD_Separators.HD_SeparatorData separator in tempSeparators.Values)
            {
                separator.IsGradientBackground = isGradientBackground;
            }
            separatorHasModifiedChanges = true;
        }

        private void UpdateGlobalSeparatorBackgroundColor(Color color)
        {
            foreach (HD_Separators.HD_SeparatorData separator in tempSeparators.Values)
            {
                separator.BackgroundColor = color;
            }
            separatorHasModifiedChanges = true;
        }

        private void UpdateGlobalSeparatorGradientBackground(Gradient gradientBackground)
        {
            foreach (HD_Separators.HD_SeparatorData separator in tempSeparators.Values)
            {
                separator.BackgroundGradient = HD_Color.CopyGradient(gradientBackground);
            }
            separatorHasModifiedChanges = true;
        }

        private void UpdateGlobalSeparatorFontSize(int size)
        {
            foreach (HD_Separators.HD_SeparatorData separator in tempSeparators.Values)
            {
                separator.FontSize = size;
            }
            separatorHasModifiedChanges = true;
        }

        private void UpdateGlobalSeparatorFontStyle(FontStyle style)
        {
            foreach (HD_Separators.HD_SeparatorData separator in tempSeparators.Values)
            {
                separator.FontStyle = style;
            }
            separatorHasModifiedChanges = true;
        }

        private void UpdateGlobalSeparatorTextAnchor(TextAnchor anchor)
        {
            foreach (HD_Separators.HD_SeparatorData separator in tempSeparators.Values)
            {
                separator.TextAnchor = anchor;
            }
            separatorHasModifiedChanges = true;
        }

        private void UpdateGlobalSeparatorImageType(HD_Separators.SeparatorImageType imageType)
        {
            foreach (HD_Separators.HD_SeparatorData separator in tempSeparators.Values)
            {
                separator.ImageType = imageType;
            }
            separatorHasModifiedChanges = true;
        }
        #endregion

        #region Tools
        private void DrawToolsTab()
        {
            toolsMainScroll = EditorGUILayout.BeginScrollView(toolsMainScroll, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

            DrawToolsCategory();

            EditorGUILayout.BeginVertical(HD_GUI.SecondaryPanelStyle);
            DrawToolsActions();
            EditorGUILayout.Space(defaultMarginSpacing);

            if (GUILayout.Button("Apply Action", GUILayout.Height(primaryButtonsHeight)))
            {
                if (availableActionMethods.Count > selectedActionIndex && selectedActionIndex >= 0)
                {
                    MethodInfo methodToInvoke = availableActionMethods[selectedActionIndex];
                    methodToInvoke?.Invoke(null, null);
                }
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndScrollView();
        }

        private void DrawToolsCategory()
        {
            HierarchyDesigner_Attribute_Tools previousCategory = selectedCategory;

            EditorGUILayout.BeginVertical(HD_GUI.SecondaryPanelStyle);
            EditorGUILayout.LabelField("Category Selection", HD_GUI.FieldsCategoryLabelStyle, GUILayout.Width(labelWidth));
            GUILayout.Space(2);
            selectedCategory = HD_GUI.DrawEnumPopup("Selected Category", 145, selectedCategory, HierarchyDesigner_Attribute_Tools.Activate);
            if (previousCategory != selectedCategory) UpdateAvailableActions();
            EditorGUILayout.EndVertical();
        }

        private void DrawToolsActions()
        {
           
            if (availableActionNames.Count == 0) 
            {
                GUILayout.Label("No tools available for this category.", HD_GUI.UnassignedLabelStyle); 
            }
            else
            {
                EditorGUILayout.LabelField("Action Selection", HD_GUI.FieldsCategoryLabelStyle, GUILayout.Width(labelWidth));
                GUILayout.Space(defaultMarginSpacing);
                selectedActionIndex = EditorGUILayout.Popup(selectedActionIndex, availableActionNames.ToArray());
            }
        }

        private void LoadTools()
        {
            if (!cacheInitialized)
            {
                InitializeActionCache();
                cacheInitialized = true;
            }
            UpdateAvailableActions();
        }

        private void InitializeActionCache()
        {
            MethodInfo[] methods = typeof(HD_Menu).GetMethods(BindingFlags.Public | BindingFlags.Static);
            foreach (MethodInfo method in methods)
            {
                object[] toolAttributes = method.GetCustomAttributes(typeof(HD_Attributes), false);
                for (int i = 0; i < toolAttributes.Length; i++)
                {
                    if (toolAttributes[i] is HD_Attributes toolAttribute)
                    {
                        object[] menuItemAttributes = method.GetCustomAttributes(typeof(MenuItem), true);
                        for (int j = 0; j < menuItemAttributes.Length; j++)
                        {
                            MenuItem menuItemAttribute = menuItemAttributes[j] as MenuItem;
                            if (menuItemAttribute != null)
                            {
                                string rawActionName = menuItemAttribute.menuItem;
                                string actionName = ExtractActionsFromCategories(rawActionName, toolAttribute.Category);
                                if (!string.IsNullOrEmpty(actionName))
                                {
                                    if (!cachedActions.TryGetValue(toolAttribute.Category, out List<(string Name, MethodInfo Method)> actionsList))
                                    {
                                        actionsList = new();
                                        cachedActions[toolAttribute.Category] = actionsList;
                                    }
                                    actionsList.Add((actionName, method));
                                }
                            }
                        }
                    }
                }
            }
        }

        private string ExtractActionsFromCategories(string menuItemPath, HierarchyDesigner_Attribute_Tools category)
        {
            if (string.IsNullOrEmpty(menuItemPath)) return null;

            string[] parts = menuItemPath.Split('/');
            if (parts.Length < 2) return null;

            return category switch
            {
                HierarchyDesigner_Attribute_Tools.Rename or HierarchyDesigner_Attribute_Tools.Sort => parts[^1],
                HierarchyDesigner_Attribute_Tools.Activate or HierarchyDesigner_Attribute_Tools.Deactivate or HierarchyDesigner_Attribute_Tools.Count or HierarchyDesigner_Attribute_Tools.Lock or HierarchyDesigner_Attribute_Tools.Unlock or HierarchyDesigner_Attribute_Tools.Select => (parts.Length > 4) ? string.Join("/", parts, 4, parts.Length - 4) : null,
                _ => (parts.Length >= 2) ? $"{parts[^2]}/{parts[^1]}" : null,
            };
        }

        private void UpdateAvailableActions()
        {
            availableActionNames.Clear();
            availableActionMethods.Clear();
            if (cachedActions.TryGetValue(selectedCategory, out List<(string Name, MethodInfo Method)> actions))
            {
                foreach ((string Name, MethodInfo Method) in actions)
                {
                    availableActionNames.Add(Name);
                    availableActionMethods.Add(Method);
                }
            }
            selectedActionIndex = 0;
        }
        #endregion

        #region Presets
        private void DrawPresetsTab()
        {
            #region Body
            presetsMainScroll = EditorGUILayout.BeginScrollView(presetsMainScroll, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            DrawPresetsList();
            DrawPresetsFeaturesFields();
            EditorGUILayout.EndScrollView();
            #endregion

            #region Footer
            EditorGUILayout.BeginVertical();
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Enable All Features", GUILayout.Height(secondaryButtonsHeight)))
            {
                EnableAllFeatures(true);
            }
            if (GUILayout.Button("Disable All Features", GUILayout.Height(secondaryButtonsHeight)))
            {
                EnableAllFeatures(false);
            }
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Confirm and Apply Preset", GUILayout.Height(primaryButtonsHeight)))
            {
                ApplySelectedPreset();
            }
            EditorGUILayout.EndVertical();
            #endregion
        }

        private void DrawPresetsList()
        {
            EditorGUILayout.BeginVertical(HD_GUI.SecondaryPanelStyle);
            EditorGUILayout.LabelField("Preset Selection", HD_GUI.FieldsCategoryLabelStyle);
            GUILayout.Space(defaultMarginSpacing);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Selected Preset", HD_GUI.LayoutLabelStyle, GUILayout.Width(presetslabelWidth));
            if (GUILayout.Button(presetNames[selectedPresetIndex], EditorStyles.popup))
            {
                ShowPresetPopup();
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        private void DrawPresetsFeaturesFields()
        {
            EditorGUILayout.BeginVertical(HD_GUI.SecondaryPanelStyle);
            EditorGUILayout.LabelField("Apply Preset To:", HD_GUI.FieldsCategoryLabelStyle, GUILayout.Width(presetslabelWidth));
            GUILayout.Space(defaultMarginSpacing);

            applyToFolders = HD_GUI.DrawToggle("Folders", presetsToggleLabelWidth, applyToFolders, true, true, "Applies the folder values from the currently selected preset (i.e., Text Color, Font Size, Font Style, Image Color, and Image Type) to all folders in your folder list.");
            applyToSeparators = HD_GUI.DrawToggle("Separators", presetsToggleLabelWidth, applyToSeparators, true, true, "Applies the separator values from the currently selected preset (i.e., Text Color, Is Gradient Background, Background Color, Background Gradient, Font Size, Font Style, Text Alignment and Background Image Type) to all separators in your separators list.");
            applyToTag = HD_GUI.DrawToggle("GameObject's Tag", presetsToggleLabelWidth, applyToTag, true, true, "Applies the tag values from the currently selected preset (i.e., Color, Font Style, Font Size, and Text Anchor) to the GameObject's Tag.");
            applyToLayer = HD_GUI.DrawToggle("GameObject's Layer", presetsToggleLabelWidth, applyToLayer, true, true, "Applies the layer values from the currently selected preset (i.e., Color, Font Style, Font Size, and Text Anchor) to the GameObject's Layer.");
            applyToTree = HD_GUI.DrawToggle("Hierarchy Tree", presetsToggleLabelWidth, applyToTree, true, true, "Applies the tree values from the currently selected preset (i.e., Color) to the Hierarchy Tree.");
            applyToLines = HD_GUI.DrawToggle("Hierarchy Lines", presetsToggleLabelWidth, applyToLines, true, true, "Applies the line values from the currently selected preset (i.e., Color) to the Hierarchy Lines.");
            applyToHierarchyButtons = HD_GUI.DrawToggle("Hierarchy Buttons", presetsToggleLabelWidth, applyToHierarchyButtons, true, true, "Applies the hierarchy buttons values from the currently selected preset (i.e., Color) to the Hierarchy Buttons.");
            applyToFolderDefaultValues = HD_GUI.DrawToggle("Folder Default Values", presetsToggleLabelWidth, applyToFolderDefaultValues, true, true, "Applies the default folder values from the currently selected preset (i.e., Text Color, Font Size, Font Style, Image Color, and Image Type) to folders that are not present in your folders list, as well as to the folder creation fields.");
            applyToSeparatorDefaultValues = HD_GUI.DrawToggle("Separator Default Values", presetsToggleLabelWidth, applyToSeparatorDefaultValues, true, true, "Applies the default separator values from the currently selected preset (i.e., Text Color, Is Gradient Background, Background Color, Background Gradient, Font Size, Font Style, Text Alignment and Background Image Type) to separators that are not present in your separators list, as well as to the separator creation fields.");
            applyToLock = HD_GUI.DrawToggle("Lock Label", presetsToggleLabelWidth, applyToLock, true, true, "Applies the lock label values from the currently selected preset (i.e., Color, Font Size, Font Style and Text Anchor) to the Lock Label.");
            EditorGUILayout.EndVertical();
        }

        private void LoadPresets()
        {
            List<string> combinedPresetNames = new();
            combinedPresetNames.AddRange(HD_Presets.GetPresetNames());
            customPresets = HD_Presets.CustomPresets;
            combinedPresetNames.AddRange(HD_Presets.CustomPresets.ConvertAll(p => p.presetName));
            presetNames = combinedPresetNames.ToArray();

            if (selectedPresetIndex >= presetNames.Length)
            {
                selectedPresetIndex = 0;
            }
        }

        private void ShowPresetPopup()
        {
            GenericMenu menu = new();
            Dictionary<string, List<string>> groupedPresets = HD_Presets.GetPresetNamesGrouped();

            foreach (KeyValuePair<string, List<string>> group in groupedPresets)
            {
                foreach (string presetName in group.Value)
                {
                    menu.AddItem(new($"{group.Key}/{presetName}"), presetName == presetNames[selectedPresetIndex], OnPresetSelected, presetName);
                }
            }

            menu.ShowAsContext();
        }

        private void OnPresetSelected(object presetNameObj)
        {
            string presetName = (string)presetNameObj;
            int newIndex = Array.IndexOf(presetNames, presetName);

            if (newIndex >= 0 && newIndex < presetNames.Length)
            {
                selectedPresetIndex = newIndex;
            }
            else
            {
                selectedPresetIndex = 0;
            }
        }

        private void ApplySelectedPreset()
        {
            if (selectedPresetIndex < 0 || selectedPresetIndex >= presetNames.Length) return;

            HD_Presets.HD_Preset selectedPreset;
            if (selectedPresetIndex < HD_Presets.DefaultPresets.Count)
            {
                selectedPreset = HD_Presets.DefaultPresets[selectedPresetIndex];
            }
            else
            {
                int customIndex = selectedPresetIndex - HD_Presets.DefaultPresets.Count;
                selectedPreset = HD_Presets.CustomPresets[customIndex];
            }

            string message = "Are you sure you want to override your current values for: ";
            List<string> changesList = new();
            if (applyToFolders) changesList.Add("Folders");
            if (applyToSeparators) changesList.Add("Separators");
            if (applyToTag) changesList.Add("GameObject's Tag");
            if (applyToLayer) changesList.Add("GameObject's Layer");
            if (applyToTree) changesList.Add("Hierarchy Tree");
            if (applyToLines) changesList.Add("Hierarchy Lines");
            if (applyToHierarchyButtons) changesList.Add("Hierarchy Buttons");
            if (applyToFolderDefaultValues) changesList.Add("Folder Default Values");
            if (applyToSeparatorDefaultValues) changesList.Add("Separator Default Values");
            if (applyToLock) changesList.Add("Lock Label");
            message += string.Join(", ", changesList) + "?\n\n*If you select 'confirm' all values will be overridden and saved.*";

            if (EditorUtility.DisplayDialog("Confirm Preset Application", message, "Confirm", "Cancel"))
            {
                if (applyToFolders)
                {
                    HD_Operations.ApplyPresetToFolders(selectedPreset);
                    HD_Manager.ClearFolderCache();
                }
                if (applyToSeparators)
                {
                    HD_Operations.ApplyPresetToSeparators(selectedPreset);
                    HD_Manager.ClearSeparatorCache();
                }
                bool shouldSaveDesignSettings = false;
                if (applyToTag)
                {
                    HD_Operations.ApplyPresetToTag(selectedPreset);
                    shouldSaveDesignSettings = true;
                }
                if (applyToLayer)
                {
                    HD_Operations.ApplyPresetToLayer(selectedPreset);
                    shouldSaveDesignSettings = true;
                }
                if (applyToTree)
                {
                    HD_Operations.ApplyPresetToTree(selectedPreset);
                    shouldSaveDesignSettings = true;
                }
                if (applyToLines)
                {
                    HD_Operations.ApplyPresetToLines(selectedPreset);
                    shouldSaveDesignSettings = true;
                }
                if (applyToHierarchyButtons)
                {
                    HD_Operations.ApplyPresetToHierarchyButtons(selectedPreset);
                    shouldSaveDesignSettings = true;
                }
                if (applyToFolderDefaultValues)
                {
                    HD_Operations.ApplyPresetToDefaultFolderValues(selectedPreset);
                    shouldSaveDesignSettings = true;
                }
                if (applyToSeparatorDefaultValues)
                {
                    HD_Operations.ApplyPresetToDefaultSeparatorValues(selectedPreset);
                    shouldSaveDesignSettings = true;
                }
                if (applyToLock)
                {
                    HD_Operations.ApplyPresetToLockLabel(selectedPreset);
                    shouldSaveDesignSettings = true;
                }
                if (shouldSaveDesignSettings)
                {
                    HD_Settings.SaveDesignSettings();
                    LoadDesignSettingsData();
                    LoadFolderCreationFields();
                    LoadSeparatorsCreationFields();
                }
                EditorApplication.RepaintHierarchyWindow();
            }
        }

        private void EnableAllFeatures(bool enable)
        {
            applyToFolders = enable;
            applyToSeparators = enable;
            applyToTag = enable;
            applyToLayer = enable;
            applyToTree = enable;
            applyToLines = enable;
            applyToFolderDefaultValues = enable;
            applyToSeparatorDefaultValues = enable;
            applyToLock = enable;
        }
        #endregion

        #region Preset Creator
        private void DrawPresetCreatorTab()
        {
            CustomPresetList();
            DrawPresetCreatorFields();

            EditorGUILayout.BeginVertical();
            if (GUILayout.Button("Create Custom Preset", GUILayout.Height(primaryButtonsHeight)))
            {
                CreateCustomPreset();
            }
            if (GUILayout.Button("Reset All Fields", GUILayout.Height(primaryButtonsHeight)))
            {
                ResetCustomPresetFields();
            }
            EditorGUILayout.EndVertical();
        }
        
        private void DrawPresetCreatorFields() 
        {
            EditorGUILayout.BeginVertical(HD_GUI.SecondaryPanelStyle);
            EditorGUILayout.LabelField("Custom Preset Creation", HD_GUI.FieldsCategoryLabelStyle);
            GUILayout.Space(defaultMarginSpacing);

            presetCreatorMainScroll = EditorGUILayout.BeginScrollView(presetCreatorMainScroll, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

            #region General
            GUILayout.Label("General", HD_GUI.MiniBoldLabelStyle);
            GUILayout.Space(2);

            customPresetName = HD_GUI.DrawTextField("Custom Preset Name", customPresetsLabelWidth, string.Empty, customPresetName, true, "The name of your custom preset.");
            #endregion

            GUILayout.Space(customPresetsSpacing);

            #region Folder
            GUILayout.Label("Folder", HD_GUI.MiniBoldLabelStyle);
            GUILayout.Space(2);

            customPresetFolderTextColor = HD_GUI.DrawColorField("Text Color", customPresetsLabelWidth, "#FFFFFF", customPresetFolderTextColor, true, "The folder text color value in your custom preset.");
            customPresetFolderFontSize = HD_GUI.DrawIntSlider("Font Size", customPresetsLabelWidth, customPresetFolderFontSize, 12, 7, 21, true, "The folder font size value in your custom preset.");
            customPresetFolderFontStyle = HD_GUI.DrawEnumPopup("Font Style", customPresetsLabelWidth, customPresetFolderFontStyle, FontStyle.Normal, true, "The folder font style value in your custom preset.");
            customPresetFolderColor = HD_GUI.DrawColorField("Color", customPresetsLabelWidth, "#FFFFFF", customPresetFolderColor, true, "The folder color value in your custom preset.");
            customPresetFolderImageType = HD_GUI.DrawEnumPopup("Image Type", customPresetsLabelWidth, customPresetFolderImageType, HD_Folders.FolderImageType.Default, true, "The folder image type value in your custom preset.");
            #endregion

            GUILayout.Space(customPresetsSpacing);

            #region Separator
            GUILayout.Label("Separator", HD_GUI.MiniBoldLabelStyle);
            GUILayout.Space(2);

            customPresetSeparatorTextColor = HD_GUI.DrawColorField("Text Color", customPresetsLabelWidth, "#FFFFFF", customPresetSeparatorTextColor, true, "The separator text color value in your custom preset.");
            customPresetSeparatorIsGradientBackground = HD_GUI.DrawToggle("Is Gradient Background", customPresetsLabelWidth, customPresetSeparatorIsGradientBackground, false, true, "The separator is gradient value in your custom preset.");
            customPresetSeparatorBackgroundColor = HD_GUI.DrawColorField("Background Color", customPresetsLabelWidth, "#808080", customPresetSeparatorBackgroundColor, true, "The separator background color value in your custom preset.");
            customPresetSeparatorBackgroundGradient = HD_GUI.DrawGradientField("Background Gradient", customPresetsLabelWidth, new Gradient(), customPresetSeparatorBackgroundGradient, true, "The separator background gradient color value in your custom preset.");
            customPresetSeparatorFontStyle = HD_GUI.DrawEnumPopup("Font Style", customPresetsLabelWidth, customPresetSeparatorFontStyle, FontStyle.Normal, true, "The separator font style value in your custom preset.");
            customPresetSeparatorFontSize = HD_GUI.DrawIntSlider("Font Size", customPresetsLabelWidth, customPresetSeparatorFontSize, 12, 7, 21, true, "The separator font size value in your custom preset.");
            customPresetSeparatorTextAlignment = HD_GUI.DrawEnumPopup("Text Alignment", customPresetsLabelWidth, customPresetSeparatorTextAlignment, TextAnchor.MiddleCenter, true, "The separator text alignment value in your custom preset.");
            customPresetSeparatorBackgroundImageType = HD_GUI.DrawEnumPopup("Image Type", customPresetsLabelWidth, customPresetSeparatorBackgroundImageType, HD_Separators.SeparatorImageType.Default, true, "The separator background image type value in your custom preset.");
            #endregion

            GUILayout.Space(customPresetsSpacing);

            #region Tag
            GUILayout.Label("Tag", HD_GUI.MiniBoldLabelStyle);
            GUILayout.Space(2);

            customPresetTagTextColor = HD_GUI.DrawColorField("Text Color", customPresetsLabelWidth, "#808080", customPresetTagTextColor, true, "The tag text color value in your custom preset.");
            customPresetTagFontStyle = HD_GUI.DrawEnumPopup("Font Style", customPresetsLabelWidth, customPresetTagFontStyle, FontStyle.Bold, true, "The tag font style value in your custom preset.");
            customPresetTagFontSize = HD_GUI.DrawIntSlider("Font Size", customPresetsLabelWidth, customPresetTagFontSize, 10, 7, 21, true, "The tag font size value in your custom preset.");
            customPresetTagTextAnchor = HD_GUI.DrawEnumPopup("Text Anchor", customPresetsLabelWidth, customPresetTagTextAnchor, TextAnchor.MiddleRight, true, "The tag text anchor value in your custom preset.");
            #endregion

            GUILayout.Space(customPresetsSpacing);

            #region Layer
            GUILayout.Label("Layer", HD_GUI.MiniBoldLabelStyle);
            GUILayout.Space(2);

            customPresetLayerTextColor = HD_GUI.DrawColorField("Text Color", customPresetsLabelWidth, "#808080", customPresetLayerTextColor, true, "The layer text color value in your custom preset.");
            customPresetLayerFontStyle = HD_GUI.DrawEnumPopup("Font Style", customPresetsLabelWidth, customPresetLayerFontStyle, FontStyle.Bold, true, "The layer font style value in your custom preset.");
            customPresetLayerFontSize = HD_GUI.DrawIntSlider("Font Size", customPresetsLabelWidth, customPresetLayerFontSize, 10, 7, 21, true, "The layer font size value in your custom preset.");
            customPresetLayerTextAnchor = HD_GUI.DrawEnumPopup("Text Anchor", customPresetsLabelWidth, customPresetLayerTextAnchor, TextAnchor.MiddleLeft, true, "The layer text anchor value in your custom preset.");
            #endregion

            GUILayout.Space(customPresetsSpacing);

            #region Tree
            GUILayout.Label("Tree", HD_GUI.MiniBoldLabelStyle);
            GUILayout.Space(2);

            customPresetTreeColor = HD_GUI.DrawColorField("Tree Color", customPresetsLabelWidth, "#FFFFFF", customPresetTreeColor, true, "The hierarchy tree color value in your custom preset.");
            #endregion

            GUILayout.Space(customPresetsSpacing);

            #region Lines
            GUILayout.Label("Lines", HD_GUI.MiniBoldLabelStyle);
            GUILayout.Space(2);

            customPresetHierarchyLineColor = HD_GUI.DrawColorField("Hierarchy Line Color", customPresetsLabelWidth, "#808080", customPresetHierarchyLineColor, true, "The hierarchy lines color value in your custom preset.");
            #endregion

            GUILayout.Space(customPresetsSpacing);

            #region Buttons
            GUILayout.Label("Buttons", HD_GUI.MiniBoldLabelStyle);
            GUILayout.Space(2);

            customPresetHierarchyButtonLockColor = HD_GUI.DrawColorField("Button Lock Color", customPresetsLabelWidth, "#404040", customPresetHierarchyButtonLockColor, true, "The hierarchy button lock color value in your custom preset.");
            customPresetHierarchyButtonVisibilityColor = HD_GUI.DrawColorField("Button Visibility Color", customPresetsLabelWidth, "#404040", customPresetHierarchyButtonVisibilityColor, true, "The hierarchy button visibility color value in your custom preset.");
            #endregion

            GUILayout.Space(customPresetsSpacing);

            #region Lock Label
            GUILayout.Label("Lock Label", HD_GUI.MiniBoldLabelStyle);
            GUILayout.Space(2);

            customPresetLockColor = HD_GUI.DrawColorField("Color", customPresetsLabelWidth, "#FFFFFF", customPresetLockColor, true, "The lock label color value in your custom preset.");
            customPresetLockFontSize = HD_GUI.DrawIntSlider("Font Size", customPresetsLabelWidth, customPresetLockFontSize, 12, 7, 21, true, "The lock label font size value in your custom preset.");
            customPresetLockFontStyle = HD_GUI.DrawEnumPopup("Font Style", customPresetsLabelWidth, customPresetLockFontStyle, FontStyle.Normal, true, "The lock label font style value in your custom preset.");
            customPresetLockTextAnchor = HD_GUI.DrawEnumPopup("Text Anchor", customPresetsLabelWidth, customPresetLockTextAnchor, TextAnchor.MiddleCenter, true, "The lock label text anchor value in your custom preset.");
            #endregion

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();        
        }

        private void CustomPresetList()
        {
            if (customPresets.Count < 1) return;

            EditorGUILayout.BeginVertical(HD_GUI.SecondaryPanelStyle);
            presetCreatorListScroll = EditorGUILayout.BeginScrollView(presetCreatorListScroll, GUILayout.MinHeight(80), GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

            EditorGUILayout.LabelField("Custom Presets' List", HD_GUI.FieldsCategoryLabelStyle);
            GUILayout.Space(defaultMarginSpacing);

            if (customPresets.Count > 0)
            {
                List<string> presetNames = customPresets.ConvertAll(preset => preset.presetName);
                float customPresetsLabelWidth = HD_GUI.CalculateMaxLabelWidth(presetNames);

                for (int i = 0; i < customPresets.Count; i++)
                {
                    HD_Presets.HD_Preset customPreset = customPresets[i];

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(customPreset.presetName, HD_GUI.LayoutLabelStyle, GUILayout.Width(customPresetsLabelWidth));
                    if (GUILayout.Button("Delete", GUILayout.Height(secondaryButtonsHeight)))
                    {
                        bool delete = EditorUtility.DisplayDialog("Delete Preset", $"Are you sure you want to delete the custom preset '{customPreset.presetName}'?", "Yes", "No");
                        if (delete)
                        {
                            HD_Presets.CustomPresets.RemoveAt(i);
                            HD_Presets.SaveCustomPresets();
                            LoadPresets();
                            Repaint();
                            EditorGUILayout.EndHorizontal();
                            break;
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
            else
            {
                EditorGUILayout.LabelField("No custom presets found.", HD_GUI.UnassignedLabelLeftStyle);
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private void CreateCustomPreset()
        {
            if (IsPresetNameValid(customPresetName))
            {
                HD_Presets.HD_Preset newPreset = new(
                    customPresetName,
                    customPresetFolderTextColor, customPresetFolderFontSize, customPresetFolderFontStyle, customPresetFolderColor, customPresetFolderImageType,
                    customPresetSeparatorTextColor, customPresetSeparatorIsGradientBackground, customPresetSeparatorBackgroundColor, customPresetSeparatorBackgroundGradient,
                    customPresetSeparatorFontStyle, customPresetSeparatorFontSize, customPresetSeparatorTextAlignment, customPresetSeparatorBackgroundImageType,
                    customPresetTagTextColor, customPresetTagFontStyle, customPresetTagFontSize, customPresetTagTextAnchor,
                    customPresetLayerTextColor, customPresetLayerFontStyle, customPresetLayerFontSize, customPresetLayerTextAnchor,
                    customPresetTreeColor, customPresetHierarchyLineColor, customPresetHierarchyButtonLockColor, customPresetHierarchyButtonVisibilityColor,
                    customPresetLockColor, customPresetLockFontSize, customPresetLockFontStyle, customPresetLockTextAnchor
                );

                HD_Presets.CustomPresets.Add(newPreset);
                HD_Presets.SaveCustomPresets();
                LoadPresets();

                EditorUtility.DisplayDialog("Success", $"Preset '{customPresetName}' created successfully.", "OK");
            }
            else
            {
                EditorUtility.DisplayDialog("Invalid Preset Name", "Preset name is either duplicate or invalid.", "OK");
            }
        }

        private void ResetCustomPresetFields()
        {
            customPresetName = string.Empty;

            customPresetFolderTextColor = Color.white;
            customPresetFolderFontSize = 12;
            customPresetFolderFontStyle = FontStyle.Normal;
            customPresetFolderColor = Color.white;
            customPresetFolderImageType = HD_Folders.FolderImageType.Default;

            customPresetSeparatorTextColor = Color.white;
            customPresetSeparatorIsGradientBackground = false;
            customPresetSeparatorBackgroundColor = Color.gray;
            customPresetSeparatorBackgroundGradient = new Gradient();
            customPresetSeparatorFontSize = 12;
            customPresetSeparatorFontStyle = FontStyle.Normal;
            customPresetSeparatorTextAlignment = TextAnchor.MiddleCenter;
            customPresetSeparatorBackgroundImageType = HD_Separators.SeparatorImageType.Default;

            customPresetTagTextColor = Color.gray;
            customPresetTagFontStyle = FontStyle.BoldAndItalic;
            customPresetTagFontSize = 10;
            customPresetTagTextAnchor = TextAnchor.MiddleRight;

            customPresetLayerTextColor = Color.gray;
            customPresetLayerFontStyle = FontStyle.Bold;
            customPresetLayerFontSize = 10;
            customPresetLayerTextAnchor = TextAnchor.MiddleLeft;

            customPresetTreeColor = Color.white;
            customPresetHierarchyLineColor = HD_Color.HexToColor("00000080");
            customPresetHierarchyButtonLockColor = HD_Color.HexToColor("404040");
            customPresetHierarchyButtonVisibilityColor = HD_Color.HexToColor("404040");

            customPresetLockColor = Color.white;
            customPresetLockFontSize = 11;
            customPresetLockFontStyle = FontStyle.BoldAndItalic;
            customPresetLockTextAnchor = TextAnchor.MiddleCenter;

            Repaint();
        }

        private bool IsPresetNameValid(string presetName)
        {
            if (string.IsNullOrWhiteSpace(presetName)) return false;

            foreach (HD_Presets.HD_Preset preset in HD_Presets.DefaultPresets)
            {
                if (preset.presetName == presetName) return false;
            }

            foreach (HD_Presets.HD_Preset preset in HD_Presets.CustomPresets)
            {
                if (preset.presetName == presetName) return false;
            }

            return true;
        }
        #endregion

        #region General Settings
        private void DrawGeneralSettingsTab()
        {
            #region Body
            generalSettingsMainScroll = EditorGUILayout.BeginScrollView(generalSettingsMainScroll, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            DrawGeneralSettingsCoreFeatures();
            DrawGeneralSettingsMainFeatures();
            DrawGeneralSettingsFilteringFeatures();
            EditorGUILayout.EndScrollView();
            #endregion

            #region Footer
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Enable All Features", GUILayout.Height(secondaryButtonsHeight)))
            {
                EnableAllGeneralSettingsFeatures(true);
            }
            if (GUILayout.Button("Disable All Features", GUILayout.Height(secondaryButtonsHeight)))
            {
                EnableAllGeneralSettingsFeatures(false);
            }
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Update and Save General Settings", GUILayout.Height(primaryButtonsHeight)))
            {
                UpdateAndSaveGeneralSettingsData();
            }
            #endregion
        }

        private void DrawGeneralSettingsCoreFeatures()
        {
            EditorGUILayout.BeginVertical(HD_GUI.SecondaryPanelStyle);
            EditorGUILayout.LabelField("Core Features", HD_GUI.FieldsCategoryLabelStyle);

            EditorGUI.BeginChangeCheck();
            tempLayoutMode = HD_GUI.DrawEnumPopup("Hierarchy Layout Mode", enumPopupLabelWidth, tempLayoutMode, HD_Settings.HierarchyLayoutMode.Split, true, "The layout of your Hierarchy window:\n\n• Consecutive: Elements are displayed after the GameObject's name and are drawn one after the other.\n\n• Docked: Elements are docked to the right side of your Hierarchy window.\n\n• Split: Elements are divided into two parts (consecutive and docked).");
            tempTreeMode = HD_GUI.DrawEnumPopup("Hierarchy Tree Mode", enumPopupLabelWidth, tempTreeMode, HD_Settings.HierarchyTreeMode.Default, true, "The mode of the Hierarchy Tree feature:\n\n• Minimal: Uses the default tree branch and tree branch Type I for all parent-child relationships.\n\n• Default: Uses all four branch types (i.e., Type I, Type L, Type T, and Type T-Bud) for parent-child relationships.");
            if (EditorGUI.EndChangeCheck()) { generalSettingsHasModifiedChanges = true; }
            EditorGUILayout.EndVertical();
        }

        private void DrawGeneralSettingsMainFeatures()
        {
            EditorGUILayout.BeginVertical(HD_GUI.SecondaryPanelStyle);
            EditorGUILayout.LabelField("Main Features", HD_GUI.FieldsCategoryLabelStyle);

            EditorGUI.BeginChangeCheck();
            tempEnableGameObjectMainIcon = HD_GUI.DrawToggle("Enable GameObject's Main Icon", generalSettingsMainToggleLabelWidth, tempEnableGameObjectMainIcon, true, true, "Displays the main icon for GameObjects. Main icons are determined based on the order of components in a GameObject (i.e., default Unity behavior; usually, the second or third component becomes the main icon).\n\nNote: You can modify the main icon of a GameObject by moving the desired component to the second or third position.");
            tempEnableGameObjectComponentIcons = HD_GUI.DrawToggle("Enable GameObject's Component Icons", generalSettingsMainToggleLabelWidth, tempEnableGameObjectComponentIcons, true, true, "Displays all components of the GameObject in the Hierarchy window.");
            tempEnableGameObjectTag = HD_GUI.DrawToggle("Enable GameObject's Tag", generalSettingsMainToggleLabelWidth, tempEnableGameObjectTag, true, true, "Displays the tag of the GameObject in the Hierarchy window.");
            tempEnableGameObjectLayer = HD_GUI.DrawToggle("Enable GameObject's Layer", generalSettingsMainToggleLabelWidth, tempEnableGameObjectLayer, true, true, "Displays the layer of the GameObject in the Hierarchy window.");
            tempEnableHierarchyTree = HD_GUI.DrawToggle("Enable Hierarchy Tree", generalSettingsMainToggleLabelWidth, tempEnableHierarchyTree, true, true, "Displays the parent-child relationship of GameObjects in the Hierarchy window through branch icons.");
            tempEnableHierarchyRows = HD_GUI.DrawToggle("Enable Hierarchy Rows", generalSettingsMainToggleLabelWidth, tempEnableHierarchyRows, true, true, "Draws background rows for alternating GameObjects in the Hierarchy window.");
            tempEnableHierarchyLines = HD_GUI.DrawToggle("Enable Hierarchy Lines", generalSettingsMainToggleLabelWidth, tempEnableHierarchyLines, true, true, "Draws horizontal lines under each GameObject in the Hierarchy window.");
            tempEnableHierarchyButtons = HD_GUI.DrawToggle("Enable Hierarchy Buttons", generalSettingsMainToggleLabelWidth, tempEnableHierarchyButtons, true, true, "Displays utility buttons (i.e., Active State and Lock State) for each GameObject in the Hierarchy window.");
            tempEnableHeaderUtilities = HD_GUI.DrawToggle("Enable Header Utilities", generalSettingsMainToggleLabelWidth, tempEnableHeaderUtilities, true, true, "Displays a “▾” dropdown button in the Hierarchy header for quick scene switching, and a “↕” button for smart, one-click collapse/expand.");
            tempEnableMajorShortcuts = HD_GUI.DrawToggle("Enable Major Shortcuts", generalSettingsMainToggleLabelWidth, tempEnableMajorShortcuts, true, true, "Allows major shortcuts (i.e., Toggle GameObject Active State and Lock State, Chage Selected Tag and Layer; and Rename Selected GameObjects) to be executed.\n\nNote: Disabling this feature improves performance as it will not check for input while interacting with the Hierarchy window.");
            tempDisableHierarchyDesignerDuringPlayMode = HD_GUI.DrawToggle("Disable Hierarchy Designer During PlayMode", generalSettingsMainToggleLabelWidth, tempDisableHierarchyDesignerDuringPlayMode, true, true, "Disables the majority of Hierarchy Designer's features while in Play mode.\n\nNote: It is recommended to disable this feature only when debugging specific aspects of your game where performance is not a concern.");
            if (EditorGUI.EndChangeCheck()) { generalSettingsHasModifiedChanges = true; }
            EditorGUILayout.EndVertical();
        }

        private void DrawGeneralSettingsFilteringFeatures()
        {
            EditorGUILayout.BeginVertical(HD_GUI.SecondaryPanelStyle);
            EditorGUILayout.LabelField("Filtering Features", HD_GUI.FieldsCategoryLabelStyle);

            EditorGUI.BeginChangeCheck();
            tempExcludeFolderProperties = HD_GUI.DrawToggle("Exclude Folder Properties", generalSettingsFilterToggleLabelWidth, tempExcludeFolderProperties, true, true, "Excludes certain main features (i.e., Component Icons, Tag and Layer) for folder GameObjects.");
            string tempExcludedComponentsString = string.Join(", ", tempExcludedComponents);
            tempExcludedComponentsString = HD_GUI.DrawTextField("Excluded Components Icons", generalSettingsFilterToggleLabelWidth, "Transform, RectTransform, CanvasRenderer", tempExcludedComponentsString, true, "Excludes a list of components from being displayed by the Component Icon main feature.\n\nUsage Example: Light, BoxCollider, Image\n\nHow to use: ComponentName (no space) + ,");
            tempExcludedComponents = tempExcludedComponentsString.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToList();
            tempMaximumComponentIconsAmount = HD_GUI.DrawIntSlider("Maximum Component Icons Amount", generalSettingsFilterToggleLabelWidth, tempMaximumComponentIconsAmount, 10 , 1, 25, true, "Limits how many Component Icons are allowed to be displayed for each GameObject.");
            if (EditorGUI.EndChangeCheck()) { generalSettingsHasModifiedChanges = true; }
            GUILayout.Space(defaultMarginSpacing);

            #region Tag
            string[] tags = UnityEditorInternal.InternalEditorUtility.tags;
            int tagMask = GetMaskFromList(tempExcludedTags, tags);
            EditorGUI.BeginChangeCheck();
            tagMask = HD_GUI.DrawMaskField("Excluded Tags", maskFieldLabelWidth, tagMask, 1, tags, true, "Excludes selected tags from being displayed in the GameObject's Tag\nfeature.");
            if (EditorGUI.EndChangeCheck()) { generalSettingsHasModifiedChanges = true; }
            tempExcludedTags = GetListFromMask(tagMask, tags);
            #endregion

            #region Layer
            string[] layers = UnityEditorInternal.InternalEditorUtility.layers;
            int layerMask = GetMaskFromList(tempExcludedLayers, layers);
            layerMask = HD_GUI.DrawMaskField("Excluded Layers", maskFieldLabelWidth, layerMask, 1, layers, true, "Excludes selected layers from being displayed in the GameObject's Layer feature.");
            EditorGUI.BeginChangeCheck();
            tempExcludedLayers = GetListFromMask(layerMask, layers);
            if (EditorGUI.EndChangeCheck()) { generalSettingsHasModifiedChanges = true; }
            #endregion
            EditorGUILayout.EndVertical();
        }

        private void UpdateAndSaveGeneralSettingsData()
        {
            HD_Settings.LayoutMode = tempLayoutMode;
            HD_Settings.TreeMode = tempTreeMode;
            HD_Settings.EnableGameObjectMainIcon = tempEnableGameObjectMainIcon;
            HD_Settings.EnableGameObjectComponentIcons = tempEnableGameObjectComponentIcons;
            HD_Settings.EnableHierarchyTree = tempEnableHierarchyTree;
            HD_Settings.EnableGameObjectTag = tempEnableGameObjectTag;
            HD_Settings.EnableGameObjectLayer = tempEnableGameObjectLayer;
            HD_Settings.EnableHierarchyRows = tempEnableHierarchyRows;
            HD_Settings.EnableHierarchyLines = tempEnableHierarchyLines;
            HD_Settings.EnableHierarchyButtons = tempEnableHierarchyButtons;
            HD_Settings.EnableMajorShortcuts = tempEnableMajorShortcuts;
            HD_Settings.EnableHeaderUtilities = tempEnableHeaderUtilities;
            HD_Settings.DisableHierarchyDesignerDuringPlayMode = tempDisableHierarchyDesignerDuringPlayMode;
            HD_Settings.ExcludeFolderProperties = tempExcludeFolderProperties;
            HD_Settings.ExcludedComponents = tempExcludedComponents;
            HD_Settings.MaximumComponentIconsAmount = tempMaximumComponentIconsAmount;
            HD_Settings.ExcludedTags = tempExcludedTags;
            HD_Settings.ExcludedLayers = tempExcludedLayers;
            HD_Settings.SaveGeneralSettings();
            generalSettingsHasModifiedChanges = false;
        }

        private void LoadGeneralSettingsData()
        {
            tempLayoutMode = HD_Settings.LayoutMode;
            tempTreeMode = HD_Settings.TreeMode;
            tempEnableGameObjectMainIcon = HD_Settings.EnableGameObjectMainIcon;
            tempEnableGameObjectComponentIcons = HD_Settings.EnableGameObjectComponentIcons;
            tempEnableGameObjectTag = HD_Settings.EnableGameObjectTag;
            tempEnableGameObjectLayer = HD_Settings.EnableGameObjectLayer;
            tempEnableHierarchyTree = HD_Settings.EnableHierarchyTree;
            tempEnableHierarchyRows = HD_Settings.EnableHierarchyRows;
            tempEnableHierarchyLines = HD_Settings.EnableHierarchyLines;
            tempEnableHierarchyButtons = HD_Settings.EnableHierarchyButtons;
            tempEnableHeaderUtilities = HD_Settings.EnableHeaderUtilities;
            tempEnableMajorShortcuts = HD_Settings.EnableMajorShortcuts;
            tempDisableHierarchyDesignerDuringPlayMode = HD_Settings.DisableHierarchyDesignerDuringPlayMode;
            tempExcludeFolderProperties = HD_Settings.ExcludeFolderProperties;
            tempExcludedComponents = HD_Settings.ExcludedComponents;
            tempMaximumComponentIconsAmount = HD_Settings.MaximumComponentIconsAmount;
            tempExcludedTags = HD_Settings.ExcludedTags;
            tempExcludedLayers = HD_Settings.ExcludedLayers;
        }

        private void EnableAllGeneralSettingsFeatures(bool enable)
        {
            tempEnableGameObjectMainIcon = enable;
            tempEnableGameObjectComponentIcons = enable;
            tempEnableGameObjectTag = enable;
            tempEnableGameObjectLayer = enable;
            tempEnableHierarchyTree = enable;
            tempEnableHierarchyRows = enable;
            tempEnableHierarchyLines = enable;
            tempEnableHierarchyButtons = enable;
            tempEnableHeaderUtilities = enable;
            tempEnableMajorShortcuts = enable;
            tempDisableHierarchyDesignerDuringPlayMode = enable;
            tempExcludeFolderProperties = enable;
            generalSettingsHasModifiedChanges = true;
        }

        private int GetMaskFromList(List<string> list, string[] allItems)
        {
            int mask = 0;
            for (int i = 0; i < allItems.Length; i++)
            {
                if (list.Contains(allItems[i]))
                {
                    mask |= 1 << i;
                }
            }
            return mask;
        }

        private List<string> GetListFromMask(int mask, string[] allItems)
        {
            List<string> list = new();
            for (int i = 0; i < allItems.Length; i++)
            {
                if ((mask & (1 << i)) != 0)
                {
                    list.Add(allItems[i]);
                }
            }
            return list;
        }
        #endregion

        #region Design Settings
        private void DrawDesignSettingsTab()
        {
            #region Body
            designSettingsMainScroll = EditorGUILayout.BeginScrollView(designSettingsMainScroll, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            DrawDesignSettingsComponentIcons();
            DrawDesignSettingsTag();
            DrawDesignSettingsLayer();
            DrawDesignSettingsTagAndLayer();
            DrawDesignSettingsHierarchyTree();
            DrawDesignSettingsHierarchyLines();
            DrawDesignSettingsHierarchyButtons();
            DrawDesignSettingsFolder();
            DrawDesignSettingsSeparator();
            DrawDesignSettingsLock();
            EditorGUILayout.EndScrollView();
            #endregion

            #region Footer
            if (GUILayout.Button("Update and Save Design Settings", GUILayout.Height(primaryButtonsHeight)))
            {
                UpdateAndSaveDesignSettingsData();
            }
            #endregion
        }

        private void DrawDesignSettingsComponentIcons()
        {
            EditorGUILayout.BeginVertical(HD_GUI.SecondaryPanelStyle);
            EditorGUILayout.LabelField("Component Icons", HD_GUI.FieldsCategoryLabelStyle);

            EditorGUI.BeginChangeCheck();
            tempComponentIconsSize = HD_GUI.DrawFloatSlider("Component Icons Size", designSettingslabelWidth, tempComponentIconsSize, 1.0f, 0.5f, 1.0f, true, "The size of component icons, where a value of 1 represents 100%.");
            tempComponentIconsOffset = HD_GUI.DrawIntSlider("Component Icons Offset", designSettingslabelWidth, tempComponentIconsOffset, 21, 15, 30, true, "The horizontal offset position of component icons relative to their initial position, based on the Hierarchy Layout Mode.");
            tempComponentIconsSpacing = HD_GUI.DrawFloatSlider("Component Icons Spacing", designSettingslabelWidth, tempComponentIconsSpacing, 2f, 0.0f, 10.0f, true, "The spacing between each component icon.");
            if (EditorGUI.EndChangeCheck()) { designSettingsHasModifiedChanges = true; }
            EditorGUILayout.EndVertical();
        }

        private void DrawDesignSettingsTag()
        {
            EditorGUILayout.BeginVertical(HD_GUI.SecondaryPanelStyle);
            EditorGUILayout.LabelField("Tag", HD_GUI.FieldsCategoryLabelStyle);

            EditorGUI.BeginChangeCheck();
            tempTagColor = HD_GUI.DrawColorField("Tag Color", designSettingslabelWidth, "#808080", tempTagColor, true, "The color of the GameObject's tag label.");
            tempTagFontSize = HD_GUI.DrawIntSlider("Tag Font Size", designSettingslabelWidth, tempTagFontSize, 10, 7, 21, true, "The font size of the GameObject's tag label.");
            tempTagFontStyle = HD_GUI.DrawEnumPopup("Tag Font Style", designSettingslabelWidth, tempTagFontStyle, FontStyle.BoldAndItalic, true, "The font style of the GameObject's tag label.");
            tempTagTextAnchor = HD_GUI.DrawEnumPopup("Tag Text Anchor", designSettingslabelWidth, tempTagTextAnchor, TextAnchor.MiddleRight, true, "The text anchor of the GameObject's tag label.");
            if (EditorGUI.EndChangeCheck()) { designSettingsHasModifiedChanges = true; }
            EditorGUILayout.EndVertical();
        }

        private void DrawDesignSettingsLayer()
        {
            EditorGUILayout.BeginVertical(HD_GUI.SecondaryPanelStyle);
            EditorGUILayout.LabelField("Layer", HD_GUI.FieldsCategoryLabelStyle);

            EditorGUI.BeginChangeCheck();
            tempLayerColor = HD_GUI.DrawColorField("Layer Color", designSettingslabelWidth, "#808080", tempLayerColor, true, "The color of the GameObject's layer.");
            tempLayerFontSize = HD_GUI.DrawIntSlider("Layer Font Size", designSettingslabelWidth, tempLayerFontSize, 10, 7, 21, true, "The font size of the GameObject's layer.");
            tempLayerFontStyle = HD_GUI.DrawEnumPopup("Layer Font Style", designSettingslabelWidth, tempLayerFontStyle, FontStyle.BoldAndItalic, true, "The font style of the GameObject's layer.");
            tempLayerTextAnchor = HD_GUI.DrawEnumPopup("Layer Text Anchor", designSettingslabelWidth, tempLayerTextAnchor, TextAnchor.MiddleLeft, true, "The text anchor of the GameObject's\nlayer.");
            if (EditorGUI.EndChangeCheck()) { designSettingsHasModifiedChanges = true; }
            EditorGUILayout.EndVertical();
        }

        private void DrawDesignSettingsTagAndLayer()
        {
            EditorGUILayout.BeginVertical(HD_GUI.SecondaryPanelStyle);
            EditorGUILayout.LabelField("Tag and Layer", HD_GUI.FieldsCategoryLabelStyle);

            EditorGUI.BeginChangeCheck();
            tempTagLayerOffset = HD_GUI.DrawIntSlider("Tag and Layer Offset", designSettingslabelWidth, tempTagLayerOffset, 5, 0, 20, true, "The horizontal offset position of the tag and layer labels relative to their initial position, based on the Hierarchy Layout Mode.");
            tempTagLayerSpacing = HD_GUI.DrawIntSlider("Tag and Layer Spacing", designSettingslabelWidth, tempTagLayerSpacing, 5, 0, 20, true, "The spacing between the tag and layer labels.");
            if (EditorGUI.EndChangeCheck()) { designSettingsHasModifiedChanges = true; }
            EditorGUILayout.EndVertical();
        }

        private void DrawDesignSettingsHierarchyTree()
        {
            EditorGUILayout.BeginVertical(HD_GUI.SecondaryPanelStyle);
            EditorGUILayout.LabelField("Hierarchy Tree", HD_GUI.FieldsCategoryLabelStyle);

            EditorGUI.BeginChangeCheck();
            tempHierarchyTreeColor = HD_GUI.DrawColorField("Tree Color", designSettingslabelWidth, "#FFFFFF", tempHierarchyTreeColor, true, "The color of the Hierarchy Tree branches.");
            tempTreeBranchImageType_I = HD_GUI.DrawEnumPopup("Tree Branch Image Type I", designSettingslabelWidth, tempTreeBranchImageType_I, HD_Settings.TreeBranchImageType.Default, true, "The branch icon of the Hierarchy Tree's Branch Type I.");
            tempTreeBranchImageType_L = HD_GUI.DrawEnumPopup("Tree Branch Image Type L", designSettingslabelWidth, tempTreeBranchImageType_L, HD_Settings.TreeBranchImageType.Default, true, "The branch icon of the Hierarchy Tree's Branch Type L.");
            tempTreeBranchImageType_T = HD_GUI.DrawEnumPopup("Tree Branch Image Type T", designSettingslabelWidth, tempTreeBranchImageType_T, HD_Settings.TreeBranchImageType.Default, true, "The branch icon of the Hierarchy Tree's Branch Type T.");
            tempTreeBranchImageType_TerminalBud = HD_GUI.DrawEnumPopup("Tree Branch Image Type T-Bud", designSettingslabelWidth, tempTreeBranchImageType_TerminalBud, HD_Settings.TreeBranchImageType.Default, true, "The branch icon of the Hierarchy Tree's Branch Type T-Bud.");
            if (EditorGUI.EndChangeCheck()) { designSettingsHasModifiedChanges = true; }
            EditorGUILayout.EndVertical();
        }

        private void DrawDesignSettingsHierarchyLines()
        {
            EditorGUILayout.BeginVertical(HD_GUI.SecondaryPanelStyle);
            EditorGUILayout.LabelField("Hierarchy Lines", HD_GUI.FieldsCategoryLabelStyle);

            EditorGUI.BeginChangeCheck();
            tempHierarchyLineColor = HD_GUI.DrawColorField("Lines Color", designSettingslabelWidth, "#00000080", tempHierarchyLineColor, true, "The color of the Hierarchy Lines.");
            tempHierarchyLineThickness = HD_GUI.DrawIntSlider("Lines Thickness", designSettingslabelWidth, tempHierarchyLineThickness, 1, 1, 3, true, "The thickness of the Hierarchy Lines.");
            if (EditorGUI.EndChangeCheck()) { designSettingsHasModifiedChanges = true; }
            EditorGUILayout.EndVertical();
        }

        private void DrawDesignSettingsHierarchyButtons()
        {
            EditorGUILayout.BeginVertical(HD_GUI.SecondaryPanelStyle);
            EditorGUILayout.LabelField("Hierarchy Buttons", HD_GUI.FieldsCategoryLabelStyle);

            EditorGUI.BeginChangeCheck();
            tempHierarchyButtonLockColor = HD_GUI.DrawColorField("Lock Button Color", designSettingslabelWidth, "#404040", tempHierarchyButtonLockColor, true, "The color of the Hierarchy Lock Button.");
            tempHierarchyButtonVisibilityColor = HD_GUI.DrawColorField("Visibility Button Color", designSettingslabelWidth, "#404040", tempHierarchyButtonVisibilityColor, true, "The color of the Hierarchy Visibility Button.");
            if (EditorGUI.EndChangeCheck()) { designSettingsHasModifiedChanges = true; }
            EditorGUILayout.EndVertical();
        }

        private void DrawDesignSettingsFolder()
        {
            EditorGUILayout.BeginVertical(HD_GUI.SecondaryPanelStyle);
            EditorGUILayout.LabelField("Folder", HD_GUI.FieldsCategoryLabelStyle);

            EditorGUI.BeginChangeCheck();
            tempFolderDefaultTextColor = HD_GUI.DrawColorField("Default Text Color", designSettingslabelWidth, "#FFFFFF", tempFolderDefaultTextColor, true, "The text color for folders that are not present in your folder list, as well as the default text color value in the folder creation field.");
            tempFolderDefaultFontSize = HD_GUI.DrawIntSlider("Default Font Size", designSettingslabelWidth, tempFolderDefaultFontSize, 12, 7, 21, true, "The font size for folders that are not present in your folder list, as well as the default font size value in the folder creation field.");
            tempFolderDefaultFontStyle = HD_GUI.DrawEnumPopup("Default Font Style", designSettingslabelWidth, tempFolderDefaultFontStyle, FontStyle.Normal, true, "The font style for folders that are not present in your folder list, as well as the default font style value in the folder creation field.");
            tempFolderDefaultImageColor = HD_GUI.DrawColorField("Default Image Color", designSettingslabelWidth, "#FFFFFF", tempFolderDefaultImageColor, true, "The image color for folders that are not present in your folder list, as well as the default image color value in the folder creation field.");
            tempFolderDefaultImageType = HD_GUI.DrawEnumPopup("Default Image Type", designSettingslabelWidth, tempFolderDefaultImageType, HD_Folders.FolderImageType.Default, true, "The image type for folders that are not present in your folder list, as well as the default image type value in the folder creation field.");
            if (EditorGUI.EndChangeCheck()) { designSettingsHasModifiedChanges = true; }
            EditorGUILayout.EndVertical();
        }

        private void DrawDesignSettingsSeparator()
        {
            EditorGUILayout.BeginVertical(HD_GUI.SecondaryPanelStyle);
            EditorGUILayout.LabelField("Separator", HD_GUI.FieldsCategoryLabelStyle);

            EditorGUI.BeginChangeCheck();
            tempSeparatorDefaultTextColor = HD_GUI.DrawColorField("Default Text Color", designSettingslabelWidth, "#FFFFFF", tempSeparatorDefaultTextColor, true, "The text color for separators that are not present in your separators list, as well as the default text color value in the separator creation field.");
            tempSeparatorDefaultIsGradientBackground = HD_GUI.DrawToggle("Default Is Gradient Background", designSettingslabelWidth, tempSeparatorDefaultIsGradientBackground, false, true, "The is gradient background for separators that are not present in your separators list, as well as the default is gradient background value in the separator creation field.");
            tempSeparatorDefaultBackgroundColor = HD_GUI.DrawColorField("Default Background Color", designSettingslabelWidth, "#808080", tempSeparatorDefaultBackgroundColor, true, "The background color for separators that are not present in your separators list, as well as the default background color value in the separator creation field.");
            tempSeparatorDefaultBackgroundGradient = HD_GUI.DrawGradientField("Default Background Gradient", designSettingslabelWidth, new(), tempSeparatorDefaultBackgroundGradient ?? new(), true, "The background gradient for separators that are not present in your separators list, as well as the default background gradient value in the separator creation field.");
            tempSeparatorDefaultFontSize = HD_GUI.DrawIntSlider("Default Font Size", designSettingslabelWidth, tempSeparatorDefaultFontSize, 12, 7, 21, true, "The font size for separators that are not present in your separators list, as well as the default font size value in the separator creation field.");
            tempSeparatorDefaultFontStyle = HD_GUI.DrawEnumPopup("Default Font Style", designSettingslabelWidth, tempSeparatorDefaultFontStyle, FontStyle.Normal, true, "The font style for separators that are not present in your separators list, as well as the default font style value in the separator creation field.");
            tempSeparatorDefaultTextAnchor = HD_GUI.DrawEnumPopup("Default Text Anchor", designSettingslabelWidth, tempSeparatorDefaultTextAnchor, TextAnchor.MiddleCenter, true, "The text anchor for separators that are not present in your separators list, as well as the default text anchor value in the separator creation field.");
            tempSeparatorDefaultImageType = HD_GUI.DrawEnumPopup("Default Image Type", designSettingslabelWidth, tempSeparatorDefaultImageType, HD_Separators.SeparatorImageType.Default, true, "The image type for separators that are not present in your separators list, as well as the default image type value in the separator creation field.");
            tempSeparatorLeftSideTextAnchorOffset = HD_GUI.DrawIntSlider("Left Side Text Anchor Offset", designSettingslabelWidth, tempSeparatorLeftSideTextAnchorOffset, 3, 0, 33, true, "The horizontal left-side offset for separators with the following text anchor values: Upper Left, Middle Left, and Lower Left.");
            tempSeparatorCenterTextAnchorOffset = HD_GUI.DrawIntSlider("Center Text Anchor Offset", designSettingslabelWidth, tempSeparatorCenterTextAnchorOffset, -15, -66, 66, true, "The horizontal center offset for separators with the following text anchor values: Middle Center, Upper Center, and Lower Center.");
            tempSeparatorRightSideTextAnchorOffset = HD_GUI.DrawIntSlider("Right Side Text Anchor Offset", designSettingslabelWidth, tempSeparatorRightSideTextAnchorOffset, 36, 33, 66, true, "The horizontal right-side offset for separators with the following text anchor values: Upper Right, Middle Right, and Lower Right.");
            if (EditorGUI.EndChangeCheck()) { designSettingsHasModifiedChanges = true; }
            EditorGUILayout.EndVertical();
        }

        private void DrawDesignSettingsLock()
        {
            EditorGUILayout.BeginVertical(HD_GUI.SecondaryPanelStyle);
            EditorGUILayout.LabelField("Lock Label", HD_GUI.FieldsCategoryLabelStyle);

            EditorGUI.BeginChangeCheck();
            tempLockColor = HD_GUI.DrawColorField("Text Color", designSettingslabelWidth, "#FFFFFF", tempLockColor, true, "The color of the Lock label.");
            tempLockFontSize = HD_GUI.DrawIntSlider("Font Size", designSettingslabelWidth, tempLockFontSize, 11, 7, 21, true, "The font size of the Lock label.");
            tempLockFontStyle = HD_GUI.DrawEnumPopup("Font Style", designSettingslabelWidth, tempLockFontStyle, FontStyle.BoldAndItalic, true, "The font style of the Lock label.");
            tempLockTextAnchor = HD_GUI.DrawEnumPopup("Text Anchor", designSettingslabelWidth, tempLockTextAnchor, TextAnchor.MiddleCenter, true, "The text anchor of the Lock label.");
            if (EditorGUI.EndChangeCheck()) { designSettingsHasModifiedChanges = true; }
            EditorGUILayout.EndVertical();
        }

        private void UpdateAndSaveDesignSettingsData()
        {
            HD_Settings.ComponentIconsSize = tempComponentIconsSize;
            HD_Settings.ComponentIconsOffset = tempComponentIconsOffset;
            HD_Settings.ComponentIconsSpacing = tempComponentIconsSpacing;
            HD_Settings.HierarchyTreeColor = tempHierarchyTreeColor;
            HD_Settings.TreeBranchImageType_I = tempTreeBranchImageType_I;
            HD_Settings.TreeBranchImageType_L = tempTreeBranchImageType_L;
            HD_Settings.TreeBranchImageType_T = tempTreeBranchImageType_T;
            HD_Settings.TreeBranchImageType_TerminalBud = tempTreeBranchImageType_TerminalBud;
            HD_Settings.TagColor = tempTagColor;
            HD_Settings.TagTextAnchor = tempTagTextAnchor;
            HD_Settings.TagFontStyle = tempTagFontStyle;
            HD_Settings.TagFontSize = tempTagFontSize;
            HD_Settings.LayerColor = tempLayerColor;
            HD_Settings.LayerTextAnchor = tempLayerTextAnchor;
            HD_Settings.LayerFontStyle = tempLayerFontStyle;
            HD_Settings.LayerFontSize = tempLayerFontSize;
            HD_Settings.TagLayerOffset = tempTagLayerOffset;
            HD_Settings.TagLayerSpacing = tempTagLayerSpacing;
            HD_Settings.HierarchyLineColor = tempHierarchyLineColor;
            HD_Settings.HierarchyLineThickness = tempHierarchyLineThickness;
            HD_Settings.HierarchyButtonLockColor = tempHierarchyButtonLockColor;
            HD_Settings.HierarchyButtonVisibilityColor = tempHierarchyButtonVisibilityColor;
            HD_Settings.FolderDefaultTextColor = tempFolderDefaultTextColor;
            HD_Settings.FolderDefaultFontSize = tempFolderDefaultFontSize;
            HD_Settings.FolderDefaultFontStyle = tempFolderDefaultFontStyle;
            HD_Settings.FolderDefaultImageColor = tempFolderDefaultImageColor;
            HD_Settings.FolderDefaultImageType = tempFolderDefaultImageType;
            HD_Settings.SeparatorDefaultTextColor = tempSeparatorDefaultTextColor;
            HD_Settings.SeparatorDefaultIsGradientBackground = tempSeparatorDefaultIsGradientBackground;
            HD_Settings.SeparatorDefaultBackgroundColor = tempSeparatorDefaultBackgroundColor;
            HD_Settings.SeparatorDefaultBackgroundGradient = tempSeparatorDefaultBackgroundGradient;
            HD_Settings.SeparatorDefaultFontSize = tempSeparatorDefaultFontSize;
            HD_Settings.SeparatorDefaultFontStyle = tempSeparatorDefaultFontStyle;
            HD_Settings.SeparatorDefaultTextAnchor = tempSeparatorDefaultTextAnchor;
            HD_Settings.SeparatorDefaultImageType = tempSeparatorDefaultImageType;
            HD_Settings.SeparatorLeftSideTextAnchorOffset = tempSeparatorLeftSideTextAnchorOffset;
            HD_Settings.SeparatorCenterTextAnchorOffset = tempSeparatorCenterTextAnchorOffset;
            HD_Settings.SeparatorRightSideTextAnchorOffset = tempSeparatorRightSideTextAnchorOffset;
            HD_Settings.LockColor = tempLockColor;
            HD_Settings.LockTextAnchor = tempLockTextAnchor;
            HD_Settings.LockFontStyle = tempLockFontStyle;
            HD_Settings.LockFontSize = tempLockFontSize;

            HD_Settings.SaveDesignSettings();
            HD_Manager.ClearFolderCache();
            HD_Manager.ClearSeparatorCache();
            designSettingsHasModifiedChanges = false;
        }

        private void LoadDesignSettingsData()
        {
            tempComponentIconsSize = HD_Settings.ComponentIconsSize;
            tempComponentIconsOffset = HD_Settings.ComponentIconsOffset;
            tempComponentIconsSpacing = HD_Settings.ComponentIconsSpacing;
            tempHierarchyTreeColor = HD_Settings.HierarchyTreeColor;
            tempTreeBranchImageType_I = HD_Settings.TreeBranchImageType_I;
            tempTreeBranchImageType_L = HD_Settings.TreeBranchImageType_L;
            tempTreeBranchImageType_T = HD_Settings.TreeBranchImageType_T;
            tempTreeBranchImageType_TerminalBud = HD_Settings.TreeBranchImageType_TerminalBud;
            tempTagColor = HD_Settings.TagColor;
            tempTagTextAnchor = HD_Settings.TagTextAnchor;
            tempTagFontStyle = HD_Settings.TagFontStyle;
            tempTagFontSize = HD_Settings.TagFontSize;
            tempLayerColor = HD_Settings.LayerColor;
            tempLayerTextAnchor = HD_Settings.LayerTextAnchor;
            tempLayerFontStyle = HD_Settings.LayerFontStyle;
            tempLayerFontSize = HD_Settings.LayerFontSize;
            tempTagLayerOffset = HD_Settings.TagLayerOffset;
            tempTagLayerSpacing = HD_Settings.TagLayerSpacing;
            tempHierarchyLineColor = HD_Settings.HierarchyLineColor;
            tempHierarchyLineThickness = HD_Settings.HierarchyLineThickness;
            tempHierarchyButtonLockColor = HD_Settings.HierarchyButtonLockColor;
            tempHierarchyButtonVisibilityColor = HD_Settings.HierarchyButtonVisibilityColor;
            tempFolderDefaultTextColor = HD_Settings.FolderDefaultTextColor;
            tempFolderDefaultFontSize = HD_Settings.FolderDefaultFontSize;
            tempFolderDefaultFontStyle = HD_Settings.FolderDefaultFontStyle;
            tempFolderDefaultImageColor = HD_Settings.FolderDefaultImageColor;
            tempFolderDefaultImageType = HD_Settings.FolderDefaultImageType;
            tempSeparatorDefaultTextColor = HD_Settings.SeparatorDefaultTextColor;
            tempSeparatorDefaultIsGradientBackground = HD_Settings.SeparatorDefaultIsGradientBackground;
            tempSeparatorDefaultBackgroundColor = HD_Settings.SeparatorDefaultBackgroundColor;
            tempSeparatorDefaultBackgroundGradient = HD_Settings.SeparatorDefaultBackgroundGradient;
            tempSeparatorDefaultFontSize = HD_Settings.SeparatorDefaultFontSize;
            tempSeparatorDefaultFontStyle = HD_Settings.SeparatorDefaultFontStyle;
            tempSeparatorDefaultTextAnchor = HD_Settings.SeparatorDefaultTextAnchor;
            tempSeparatorDefaultImageType = HD_Settings.SeparatorDefaultImageType;
            tempSeparatorLeftSideTextAnchorOffset = HD_Settings.SeparatorLeftSideTextAnchorOffset;
            tempSeparatorCenterTextAnchorOffset = HD_Settings.SeparatorCenterTextAnchorOffset;
            tempSeparatorRightSideTextAnchorOffset = HD_Settings.SeparatorRightSideTextAnchorOffset;
            tempLockColor = HD_Settings.LockColor;
            tempLockTextAnchor = HD_Settings.LockTextAnchor;
            tempLockFontStyle = HD_Settings.LockFontStyle;
            tempLockFontSize = HD_Settings.LockFontSize;
        }
        #endregion

        #region Shortcut Settings
        private void DrawShortcutSettingsTab()
        {
            #region Body
            shortcutSettingsMainScroll = EditorGUILayout.BeginScrollView(shortcutSettingsMainScroll, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            DrawMajorShortcuts();
            DrawMinorShortcuts();
            EditorGUILayout.EndScrollView();
            #endregion

            #region Footer
            EditorGUILayout.BeginVertical();
            if (GUILayout.Button("Open Shortcut Manager", GUILayout.Height(primaryButtonsHeight)))
            {
                EditorApplication.ExecuteMenuItem("Edit/Shortcuts...");
            }
            if (GUILayout.Button("Update and Save Major Shortcut Settings", GUILayout.Height(primaryButtonsHeight)))
            {
                UpdateAndSaveShortcutSettingsData();
            }
            EditorGUILayout.EndVertical();
            #endregion
        }

        private void DrawMajorShortcuts()
        {
            EditorGUILayout.BeginVertical(HD_GUI.SecondaryPanelStyle);
            EditorGUILayout.LabelField("Major Shortcuts", HD_GUI.FieldsCategoryLabelStyle);

            EditorGUI.BeginChangeCheck();
            tempOpenIconPickerKeyCode = HD_GUI.DrawEnumPopup("Open Main Icon Override Key Code", majorShortcutEnumToggleLabelWidth, tempOpenIconPickerKeyCode, KeyCode.Mouse0, true, "Press to open the Icon Override for the hovered GameObject's Main Icon in the Hierarchy.");
            tempToggleGameObjectActiveStateKeyCode = HD_GUI.DrawEnumPopup("Toggle GameObject Active State Key Code", majorShortcutEnumToggleLabelWidth, tempToggleGameObjectActiveStateKeyCode, KeyCode.Mouse2, true, "The key code to toggle the active state of the hovered GameObject or selected GameObjects. This input must be entered within the Hierarchy window, as it is only detected while interacting with the Hierarchy.");
            tempToggleLockStateKeyCode = HD_GUI.DrawEnumPopup("Toggle GameObject Lock State Key Code", majorShortcutEnumToggleLabelWidth, tempToggleLockStateKeyCode, KeyCode.F1, true, "The key code to toggle the lock state of the hovered GameObject or selected GameObjects.\n\nNote: The Hierarchy window must be focused for this to work.");
            tempChangeTagLayerKeyCode = HD_GUI.DrawEnumPopup("Change Selected Tag, Layer Key Code", majorShortcutEnumToggleLabelWidth, tempChangeTagLayerKeyCode, KeyCode.Mouse0, true, "The key code to change the current tag or layer of a GameObject. Hover over the tag or layer and press the key code to apply\n\nNote: The Hierarchy window must be focused for this to work.");
            tempRenameSelectedGameObjectsKeyCode = HD_GUI.DrawEnumPopup("Rename Selected GameObjects Key Code", majorShortcutEnumToggleLabelWidth, tempRenameSelectedGameObjectsKeyCode, KeyCode.F3, true, "The key code to rename the selected GameObject(s).\n\nNote: The Hierarchy window must be focused for this to work.");
            if (EditorGUI.EndChangeCheck()) { shortcutSettingsHasModifiedChanges = true; }
            EditorGUILayout.EndVertical();
        }

        private void DrawMinorShortcuts()
        {
            EditorGUILayout.BeginVertical(HD_GUI.SecondaryPanelStyle);
            EditorGUILayout.LabelField("Minor Shortcuts", HD_GUI.FieldsCategoryLabelStyle);
            GUILayout.Space(defaultMarginSpacing);

            minorShortcutSettingsScroll = EditorGUILayout.BeginScrollView(minorShortcutSettingsScroll, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            foreach (string shortcutId in minorShortcutIdentifiers)
            {
                ShortcutBinding currentBinding = ShortcutManager.instance.GetShortcutBinding(shortcutId);
                string[] parts = shortcutId.Split('/');
                string commandName = parts[^1];
                string tooltipText = minorShortcutTooltips.TryGetValue(shortcutId, out string tip) ? tip : string.Empty;

                EditorGUILayout.BeginHorizontal();

                if (!string.IsNullOrEmpty(tooltipText)) HD_GUI.DrawTooltip(tooltipText);
                GUILayout.Space(4);
                GUILayout.Label(commandName + ":", HD_GUI.LayoutLabelStyle, GUILayout.Width(minorShortcutCommandLabelWidth));

                bool hasKeyCombination = false;
                foreach (KeyCombination kc in currentBinding.keyCombinationSequence)
                {
                    if (!hasKeyCombination)
                    {
                        hasKeyCombination = true;
                        GUILayout.Label(kc.ToString(), HD_GUI.AssignedLabelStyle, GUILayout.MinWidth(minorShortcutLabelWidth));
                    }
                    else
                    {
                        GUILayout.Label(" + " + kc.ToString(), HD_GUI.AssignedLabelStyle, GUILayout.MinWidth(minorShortcutLabelWidth));
                    }
                }
                if (!hasKeyCombination)
                {
                    GUILayout.Label("unassigned shortcut", HD_GUI.UnassignedLabelStyle, GUILayout.MinWidth(minorShortcutLabelWidth));
                }

                EditorGUILayout.EndHorizontal();
                GUILayout.Space(4);
            }
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
            EditorGUILayout.HelpBox("To modify minor shortcuts, please go to: Edit/Shortcuts.../Hierarchy Designer.\nYou can click the button below for quick access, then in the category section, search for Hierarchy Designer.", MessageType.Info);
        }

        private void UpdateAndSaveShortcutSettingsData()
        {
            HD_Settings.OpenIconPickerKeyCode = tempOpenIconPickerKeyCode;
            HD_Settings.ToggleGameObjectActiveStateKeyCode = tempToggleGameObjectActiveStateKeyCode;
            HD_Settings.ToggleLockStateKeyCode = tempToggleLockStateKeyCode;
            HD_Settings.ChangeTagLayerKeyCode = tempChangeTagLayerKeyCode;
            HD_Settings.RenameSelectedGameObjectsKeyCode = tempRenameSelectedGameObjectsKeyCode;
            HD_Settings.SaveShortcutSettings();
            shortcutSettingsHasModifiedChanges = false;
        }

        private void LoadShortcutSettingsData()
        {
            tempOpenIconPickerKeyCode = HD_Settings.OpenIconPickerKeyCode;
            tempToggleGameObjectActiveStateKeyCode = HD_Settings.ToggleGameObjectActiveStateKeyCode;
            tempToggleLockStateKeyCode = HD_Settings.ToggleLockStateKeyCode;
            tempChangeTagLayerKeyCode = HD_Settings.ChangeTagLayerKeyCode;
            tempRenameSelectedGameObjectsKeyCode = HD_Settings.RenameSelectedGameObjectsKeyCode;
        }
        #endregion

        #region Advanced Settings
        private void DrawAdvancedSettingsTab()
        {
            #region Body
            advancedSettingsMainScroll = EditorGUILayout.BeginScrollView(advancedSettingsMainScroll, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            DrawAdvancedCoreFeatures();
            DrawAdvancedMainIconFeatures();
            DrawAdvancedComponentIconsFeatures();
            DrawAdvancedHierarchyTreeFeatures();
            DrawAdvancedFolderFeatures();
            DrawAdvancedSeparatorFeatures();
            DrawAdvancedHierarchyToolsFeatures();
            EditorGUILayout.EndScrollView();
            #endregion

            #region Footer
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Enable All Features", GUILayout.Height(secondaryButtonsHeight)))
            {
                EnableAllAdvancedSettingsFeatures(true);
            }
            if (GUILayout.Button("Disable All Features", GUILayout.Height(secondaryButtonsHeight)))
            {
                EnableAllAdvancedSettingsFeatures(false);
            }
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Update and Save Advanced Settings", GUILayout.Height(primaryButtonsHeight)))
            {
                UpdateAndSaveAdvancedSettingsData();
            }
            #endregion
        }

        private void DrawAdvancedCoreFeatures()
        {
            EditorGUILayout.BeginVertical(HD_GUI.SecondaryPanelStyle);
            EditorGUILayout.LabelField("Core Features", HD_GUI.FieldsCategoryLabelStyle);

            EditorGUI.BeginChangeCheck();
            tempHierarchyLocation = HD_GUI.DrawEnumPopup("Hierarchy Designer Location", advancedSettingsEnumPopupLabelWidth, tempHierarchyLocation, HD_Settings.HierarchyDesignerLocation.Tools, true, "The location of Hierarchy Designer in the top menu bar (e.g., Tools/Hierarchy Designer, Plugins/Hierarchy Designer, etc.).\n\nNote: Modifying this setting will trigger a script recompilation.");
            tempMainIconUpdateMode = HD_GUI.DrawEnumPopup("Main Icon Update Mode", advancedSettingsEnumPopupLabelWidth, tempMainIconUpdateMode, HD_Settings.UpdateMode.Dynamic, true, "The update mode for the Main Icon feature:\n\nDynamic: Checks for changes dynamically during Hierarchy events.\n\nSmart: Checks periodically, such as during scene open/reload or script recompilation.\n\nNote: In Smart mode, you can manually check for changes by refreshing through the context menus or using minor shortcuts.");
            tempComponentsIconsUpdateMode = HD_GUI.DrawEnumPopup("Component Icons Update Mode", advancedSettingsEnumPopupLabelWidth, tempComponentsIconsUpdateMode, HD_Settings.UpdateMode.Dynamic, true, "The update mode for the Component Icons feature:\n\nDynamic: Checks for changes dynamically during Hierarchy events.\n\nSmart: Checks periodically, such as during scene open/reload or script recompilation.\n\nNote: In Smart mode, you can manually check for changes by refreshing through the context menus or using minor shortcuts.");
            tempHierarchyTreeUpdateMode = HD_GUI.DrawEnumPopup("Hierarchy Tree Update Mode", advancedSettingsEnumPopupLabelWidth, tempHierarchyTreeUpdateMode, HD_Settings.UpdateMode.Dynamic, true, "The update mode for the Hierarchy Tree feature:\n\nDynamic: Checks for changes dynamically during Hierarchy events.\n\nSmart: Checks periodically, such as during scene open/reload or script recompilation.\n\nNote: In Smart mode, you can manually check for changes by refreshing through the context menus or using minor shortcuts.");
            tempTagUpdateMode = HD_GUI.DrawEnumPopup("Tag Update Mode", advancedSettingsEnumPopupLabelWidth, tempTagUpdateMode, HD_Settings.UpdateMode.Dynamic, true, "The update mode for the Tag feature:\n\nDynamic: Checks for changes dynamically during Hierarchy events.\n\nSmart: Checks periodically, such as during scene open/reload or script recompilation.\n\nNote: In Smart mode, you can manually check for changes by refreshing through the context menus or using minor shortcuts.");
            tempLayerUpdateMode = HD_GUI.DrawEnumPopup("Layer Update Mode", advancedSettingsEnumPopupLabelWidth, tempLayerUpdateMode, HD_Settings.UpdateMode.Dynamic, true, "The update mode for the Layer feature:\n\nDynamic: Checks for changes dynamically during Hierarchy events.\n\nSmart: Checks periodically, such as during scene open/reload or script recompilation.\n\nNote: In Smart mode, you can manually check for changes by refreshing through the context menus or using minor shortcuts.");
            if (EditorGUI.EndChangeCheck()) { advancedSettingsHasModifiedChanges = true; }
            EditorGUILayout.EndVertical();
        }

        private void DrawAdvancedMainIconFeatures()
        {
            EditorGUILayout.BeginVertical(HD_GUI.SecondaryPanelStyle);
            EditorGUILayout.LabelField("Main Icon", HD_GUI.FieldsCategoryLabelStyle);

            EditorGUI.BeginChangeCheck();
            tempEnableDynamicBackgroundForGameObjectMainIcon = HD_GUI.DrawToggle("Enable Dynamic Background", advancedSettingsToggleLabelWidth, tempEnableDynamicBackgroundForGameObjectMainIcon, true, true, "The background of the main icon will match the background color of the Hierarchy window (i.e., Editor Light, Dark Mode, GameObject Selected, Focused, Unfocused).");
            tempEnablePreciseRectForDynamicBackgroundForGameObjectMainIcon = HD_GUI.DrawToggle("Enable Precise Rect For Dynamic Background", advancedSettingsToggleLabelWidth, tempEnablePreciseRectForDynamicBackgroundForGameObjectMainIcon, true, true, "Uses precise rect calculations for pointer/mouse detection utilized by the Dynamic Background feature.");
            tempEnableProjectTexturesInMainIconOverrideWindow = HD_GUI.DrawToggle("Enable Project Textures In Main Icon Override", advancedSettingsToggleLabelWidth, tempEnableProjectTexturesInMainIconOverrideWindow, true, true, "If enabled, the Main Icon Override window will index project textures so you can pick a texture from the project. Disabling this makes the window open faster.");
            if (EditorGUI.EndChangeCheck()) { advancedSettingsHasModifiedChanges = true; }
            EditorGUILayout.EndVertical();
        }

        private void DrawAdvancedComponentIconsFeatures()
        {
            EditorGUILayout.BeginVertical(HD_GUI.SecondaryPanelStyle);
            EditorGUILayout.LabelField("Component Icons", HD_GUI.FieldsCategoryLabelStyle);

            EditorGUI.BeginChangeCheck();
            tempEnableCustomizationForGameObjectComponentIcons = HD_GUI.DrawToggle("Enable Design Customization For Component Icons", advancedSettingsToggleLabelWidth, tempEnableCustomizationForGameObjectComponentIcons, true, true, "Enables calculation of component icon design settings (e.g., Component Icon Size, Offset, and Spacing).\n\nNote: If you are using the default values, you may turn this off to reduce extra calculations in the component icon logic.");
            tempEnableTooltipOnComponentIconHovered = HD_GUI.DrawToggle("Enable Tooltip For Component Icons", advancedSettingsToggleLabelWidth, tempEnableTooltipOnComponentIconHovered, true, true, "Displays the component name when hovering over the component icon.");
            tempEnableActiveStateEffectForComponentIcons = HD_GUI.DrawToggle("Enable Active State Effect For Component Icons", advancedSettingsToggleLabelWidth, tempEnableActiveStateEffectForComponentIcons, true, true, "Displays which components are disabled for a given object.");
            tempDisableComponentIconsForInactiveGameObjects = HD_GUI.DrawToggle("Disable Component Icons For Inactive GameObjects", advancedSettingsToggleLabelWidth, tempDisableComponentIconsForInactiveGameObjects, true, true, "Hides component icons for inactive GameObjects.");
            if (EditorGUI.EndChangeCheck()) { advancedSettingsHasModifiedChanges = true; }
            EditorGUILayout.EndVertical();
        }

        private void DrawAdvancedHierarchyTreeFeatures()
        {
            EditorGUILayout.BeginVertical(HD_GUI.SecondaryPanelStyle);
            EditorGUILayout.LabelField("Hierarchy Tree", HD_GUI.FieldsCategoryLabelStyle);

            EditorGUI.BeginChangeCheck();
            tempUseHierarchyTreeColorForInactiveGameObjects = HD_GUI.DrawToggle("Use Hierarchy Tree Color For Inactive GameObjects", advancedSettingsToggleLabelWidth, tempUseHierarchyTreeColorForInactiveGameObjects, false, true, "Uses a darker faded version of the Hierarchy Tree color for inactive GameObjects instead of the default faded gray color.");
            if (EditorGUI.EndChangeCheck()) { advancedSettingsHasModifiedChanges = true; }

            EditorGUILayout.EndVertical();
        }

        private void DrawAdvancedFolderFeatures()
        {
            EditorGUILayout.BeginVertical(HD_GUI.SecondaryPanelStyle);
            EditorGUILayout.LabelField("Folders", HD_GUI.FieldsCategoryLabelStyle);

            #region Runtime Folder
            EditorGUILayout.LabelField("Runtime Folder", HD_GUI.MiniBoldLabelStyle);
            EditorGUILayout.Space(2);

            EditorGUI.BeginChangeCheck();
            tempEnableCustomInspectorUI = HD_GUI.DrawToggle("Enable Custom Inspector UI", advancedSettingsToggleLabelWidth, tempEnableCustomInspectorUI, true, true, "Enables a custom inspector UI for Folder GameObjects.");
            tempEnableEditorUtilities = HD_GUI.DrawToggle("Enable Editor Utilities", advancedSettingsToggleLabelWidth, tempEnableEditorUtilities, true, true, "Enables editor-only utilities (e.g., Toggle Active State, Delete, Children List, etc.) in the inspector window for Folder GameObjects.");
            if (EditorGUI.EndChangeCheck()) { advancedSettingsHasModifiedChanges = true; }
            EditorGUILayout.EndVertical();
            #endregion
        }

        private void DrawAdvancedSeparatorFeatures()
        {
            EditorGUILayout.BeginVertical(HD_GUI.SecondaryPanelStyle);
            EditorGUILayout.LabelField("Separators", HD_GUI.FieldsCategoryLabelStyle);

            EditorGUI.BeginChangeCheck();
            tempIncludeBackgroundImageForGradientBackground = HD_GUI.DrawToggle("Include Background Image For Gradient Background", advancedSettingsToggleLabelWidth, tempIncludeBackgroundImageForGradientBackground, true, true, "Includes the Background Image Type for Separators that uses a gradient background. The background image type will be used first, followed by the gradient placed on top.");
            if (EditorGUI.EndChangeCheck()) { advancedSettingsHasModifiedChanges = true; }
            EditorGUILayout.EndVertical();
        }

        private void DrawAdvancedHierarchyToolsFeatures()
        {
            EditorGUILayout.BeginVertical(HD_GUI.SecondaryPanelStyle);
            EditorGUILayout.LabelField("Hierarchy Tools", HD_GUI.FieldsCategoryLabelStyle);

            EditorGUI.BeginChangeCheck();
            tempExpandHierarchyOnStartup = HD_GUI.DrawToggle("Expand Hierarchy On Startup", advancedSettingsToggleLabelWidth, tempExpandHierarchyOnStartup, true, true, "Expands the Hierarchy window when the project opens.");
            tempExcludeFoldersFromCountSelectToolCalculations = HD_GUI.DrawToggle("Exclude Folders From Count-Select Tool Calculations", advancedSettingsToggleLabelWidth, tempExcludeFoldersFromCountSelectToolCalculations, true, true, "Excludes Folder GameObjects from Count and Select tool calculations.");
            tempExcludeSeparatorsFromCountSelectToolCalculations = HD_GUI.DrawToggle("Exclude Separators From Count-Select Tool Calculations", advancedSettingsToggleLabelWidth, tempExcludeSeparatorsFromCountSelectToolCalculations, true, true, "Excludes Separator GameObjects from Count and Select tool calculations.");
            if (EditorGUI.EndChangeCheck()) { advancedSettingsHasModifiedChanges = true; }
            EditorGUILayout.EndVertical();
        }

        private void UpdateAndSaveAdvancedSettingsData()
        {
            bool hierarchyLocationChanged = tempHierarchyLocation != HD_Settings.HierarchyLocation;

            HD_Settings.HierarchyLocation = tempHierarchyLocation;
            HD_Settings.MainIconUpdateMode = tempMainIconUpdateMode;
            HD_Settings.ComponentsIconsUpdateMode = tempComponentsIconsUpdateMode;
            HD_Settings.HierarchyTreeUpdateMode = tempHierarchyTreeUpdateMode;
            HD_Settings.TagUpdateMode = tempTagUpdateMode;
            HD_Settings.LayerUpdateMode = tempLayerUpdateMode;
            HD_Settings.EnableDynamicBackgroundForGameObjectMainIcon = tempEnableDynamicBackgroundForGameObjectMainIcon;
            HD_Settings.EnablePreciseRectForDynamicBackgroundForGameObjectMainIcon = tempEnablePreciseRectForDynamicBackgroundForGameObjectMainIcon;
            HD_Settings.EnableProjectTexturesInMainIconOverrideWindow = tempEnableProjectTexturesInMainIconOverrideWindow;
            HD_Settings.EnableCustomizationForGameObjectComponentIcons = tempEnableCustomizationForGameObjectComponentIcons;
            HD_Settings.EnableTooltipOnComponentIconHovered = tempEnableTooltipOnComponentIconHovered;
            HD_Settings.EnableActiveStateEffectForComponentIcons = tempEnableActiveStateEffectForComponentIcons;
            HD_Settings.DisableComponentIconsForInactiveGameObjects = tempDisableComponentIconsForInactiveGameObjects;
            HD_Settings.UseHierarchyTreeColorForInactiveGameObjects = tempUseHierarchyTreeColorForInactiveGameObjects;
            HD_Settings.EnableCustomInspectorGUI = tempEnableCustomInspectorUI;
            HD_Settings.IncludeEditorUtilitiesForHierarchyDesignerRuntimeFolder = tempEnableEditorUtilities;
            HD_Settings.IncludeBackgroundImageForGradientBackground = tempIncludeBackgroundImageForGradientBackground;
            HD_Settings.ExpandHierarchyOnStartup = tempExpandHierarchyOnStartup;
            HD_Settings.ExcludeFoldersFromCountSelectToolCalculations = tempExcludeFoldersFromCountSelectToolCalculations;
            HD_Settings.ExcludeSeparatorsFromCountSelectToolCalculations = tempExcludeSeparatorsFromCountSelectToolCalculations;
            HD_Settings.SaveAdvancedSettings();
            advancedSettingsHasModifiedChanges = false;

            if (hierarchyLocationChanged)
            {
                HD_Settings.GenerateConstantsFile(tempHierarchyLocation);
            }
        }

        private void LoadAdvancedSettingsData()
        {
            tempHierarchyLocation = HD_Settings.HierarchyLocation;
            tempMainIconUpdateMode = HD_Settings.MainIconUpdateMode;
            tempComponentsIconsUpdateMode = HD_Settings.ComponentsIconsUpdateMode;
            tempHierarchyTreeUpdateMode = HD_Settings.HierarchyTreeUpdateMode;
            tempTagUpdateMode = HD_Settings.TagUpdateMode;
            tempLayerUpdateMode = HD_Settings.LayerUpdateMode;
            tempEnableDynamicBackgroundForGameObjectMainIcon = HD_Settings.EnableDynamicBackgroundForGameObjectMainIcon;
            tempEnablePreciseRectForDynamicBackgroundForGameObjectMainIcon = HD_Settings.EnablePreciseRectForDynamicBackgroundForGameObjectMainIcon;
            tempEnableProjectTexturesInMainIconOverrideWindow = HD_Settings.EnableProjectTexturesInMainIconOverrideWindow;
            tempEnableCustomizationForGameObjectComponentIcons = HD_Settings.EnableCustomizationForGameObjectComponentIcons;
            tempEnableTooltipOnComponentIconHovered = HD_Settings.EnableTooltipOnComponentIconHovered;
            tempEnableActiveStateEffectForComponentIcons = HD_Settings.EnableActiveStateEffectForComponentIcons;
            tempDisableComponentIconsForInactiveGameObjects = HD_Settings.DisableComponentIconsForInactiveGameObjects;
            tempUseHierarchyTreeColorForInactiveGameObjects = HD_Settings.UseHierarchyTreeColorForInactiveGameObjects;
            tempEnableCustomInspectorUI = HD_Settings.EnableCustomInspectorGUI;
            tempEnableEditorUtilities = HD_Settings.IncludeEditorUtilitiesForHierarchyDesignerRuntimeFolder;
            tempIncludeBackgroundImageForGradientBackground = HD_Settings.IncludeBackgroundImageForGradientBackground;
            tempExpandHierarchyOnStartup = HD_Settings.ExpandHierarchyOnStartup;
            tempExcludeFoldersFromCountSelectToolCalculations = HD_Settings.ExcludeFoldersFromCountSelectToolCalculations;
            tempExcludeSeparatorsFromCountSelectToolCalculations = HD_Settings.ExcludeSeparatorsFromCountSelectToolCalculations;
        }

        private void EnableAllAdvancedSettingsFeatures(bool enable)
        {
            tempEnableDynamicBackgroundForGameObjectMainIcon = enable;
            tempEnablePreciseRectForDynamicBackgroundForGameObjectMainIcon = enable;
            tempEnableProjectTexturesInMainIconOverrideWindow = enable;
            tempEnableCustomizationForGameObjectComponentIcons = enable;
            tempEnableTooltipOnComponentIconHovered = enable;
            tempEnableActiveStateEffectForComponentIcons = enable;
            tempDisableComponentIconsForInactiveGameObjects = enable;
            tempUseHierarchyTreeColorForInactiveGameObjects = enable;
            tempEnableCustomInspectorUI = enable;
            tempEnableEditorUtilities = enable;
            tempIncludeBackgroundImageForGradientBackground = enable;
            tempExpandHierarchyOnStartup = enable;
            tempExcludeFoldersFromCountSelectToolCalculations = enable;
            tempExcludeSeparatorsFromCountSelectToolCalculations = enable;
            advancedSettingsHasModifiedChanges = true;
        }
        #endregion
        #endregion

        private void OnDestroy()
        {
            string message = "The following settings have been modified: ";
            List<string> modifiedSettingsList = new();

            if (folderHasModifiedChanges) modifiedSettingsList.Add("Folders");
            if (separatorHasModifiedChanges) modifiedSettingsList.Add("Separators");
            if (generalSettingsHasModifiedChanges) modifiedSettingsList.Add("General Settings");
            if (designSettingsHasModifiedChanges) modifiedSettingsList.Add("Design Settings");
            if (shortcutSettingsHasModifiedChanges) modifiedSettingsList.Add("Shortcut Settings");
            if (advancedSettingsHasModifiedChanges) modifiedSettingsList.Add("Advanced Settings");

            if (modifiedSettingsList.Count > 0)
            {
                message += string.Join(", ", modifiedSettingsList) + ".\n\nWould you like to save the changes?";
                bool shouldSave = EditorUtility.DisplayDialog("Data Has Been Modified!", message, "Save", "Don't Save");

                if (shouldSave)
                {
                    if (folderHasModifiedChanges) UpdateAndSaveFoldersData();
                    if (separatorHasModifiedChanges) UpdateAndSaveSeparatorsData();
                    if (generalSettingsHasModifiedChanges) UpdateAndSaveGeneralSettingsData();
                    if (designSettingsHasModifiedChanges) UpdateAndSaveDesignSettingsData();
                    if (shortcutSettingsHasModifiedChanges) UpdateAndSaveShortcutSettingsData();
                    if (advancedSettingsHasModifiedChanges) UpdateAndSaveAdvancedSettingsData();
                }
            }

            folderHasModifiedChanges = false;
            separatorHasModifiedChanges = false;
            generalSettingsHasModifiedChanges = false;
            designSettingsHasModifiedChanges = false;
            shortcutSettingsHasModifiedChanges = false;
            advancedSettingsHasModifiedChanges = false;

            HD_Session.instance.currentWindow = currentWindow;
        }
    }
    #endregion

    #region Component
    internal class HD_Component : EditorWindow
    {
        #region Properties
        private Vector2 scrollPosition;
        private Component targetComponent;
        private Editor componentEditor;
        private Texture2D componentIcon;

        [SerializeField] private bool isPinned = false;
        private static HD_Component reusableWindow = null;

        private static bool s_forceFreshMouseSpawn = false;

        private const float DefaultWidth = 400f;
        private const float DefaultHeight = 400f;
        #endregion

        #region Initialization
        private void OnEnable()
        {
            PropertyInfo style = GetType().GetProperty("wantsMouseMove", BindingFlags.Instance | BindingFlags.NonPublic);
            if (style != null) style.SetValue(this, true);
        }

        public void InitializeWindow(Component component, Vector2 mousePosition)
        {
            targetComponent = component;

            minSize = new(DefaultWidth, 200f);
            maxSize = new(600f, 800f);

            position = MakeSpawnRect(mousePosition, DefaultWidth, DefaultHeight);

            if (targetComponent != null)
            {
                if (componentEditor != null) DestroyImmediate(componentEditor);
                componentEditor = Editor.CreateEditor(targetComponent);
                componentIcon = EditorGUIUtility.ObjectContent(targetComponent, targetComponent.GetType()).image as Texture2D;

                string title = $"{targetComponent.GetType().Name} Properties";
                if (isPinned) title += "  (Pinned)";
                titleContent = new GUIContent(title, componentIcon);
            }

            if (!isPinned) reusableWindow = this;
        }
        #endregion

        private void OnGUI()
        {
            if (targetComponent == null)
            {
                Close();
                return;
            }

            if (componentEditor == null && targetComponent != null)
            {
                componentEditor = Editor.CreateEditor(targetComponent);
            }

            using (new EditorGUILayout.VerticalScope())
            {
                DrawHeaderBar();

                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

                    if (componentEditor != null)
                    {
                        EditorGUI.BeginChangeCheck();
                        componentEditor.OnInspectorGUI();
                        if (EditorGUI.EndChangeCheck())
                        {
                            EditorUtility.SetDirty(targetComponent);
                        }
                    }

                    EditorGUILayout.EndScrollView();
                }
            }

            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Escape)
            {
                Close();
            }
        }

        #region Methods
        private void DrawHeaderBar()
        {
            EditorGUILayout.Space(2);
            using (new EditorGUILayout.HorizontalScope())
            {
                using (new EditorGUILayout.HorizontalScope(GUILayout.Width(120)))
                {
                    bool isEnabled = false;
                    bool canBeDisabled = false;

                    if (targetComponent is Behaviour behaviour)
                    {
                        isEnabled = behaviour.enabled;
                        canBeDisabled = true;
                    }
                    else if (targetComponent is Renderer renderer)
                    {
                        isEnabled = renderer.enabled;
                        canBeDisabled = true;
                    }
                    else if (targetComponent is Collider collider)
                    {
                        isEnabled = collider.enabled;
                        canBeDisabled = true;
                    }

                    EditorGUI.BeginChangeCheck();
                    GUI.enabled = canBeDisabled;
                    bool newEnabled = EditorGUILayout.Toggle(isEnabled, GUILayout.Width(16));
                    GUI.enabled = true;

                    if (EditorGUI.EndChangeCheck() && canBeDisabled)
                    {
                        Undo.RecordObject(targetComponent, "Toggle Component");
                        if (targetComponent is Behaviour behaviour2)
                        {
                            behaviour2.enabled = newEnabled;
                        }
                        else if (targetComponent is Renderer renderer2)
                        {
                            renderer2.enabled = newEnabled;
                        }
                        else if (targetComponent is Collider collider2)
                        {
                            collider2.enabled = newEnabled;
                        }
                    }

                    using (new EditorGUI.DisabledGroupScope(!canBeDisabled))
                    {
                        GUILayout.Label("Enable", HD_GUI.ComponentWindowTitleLabelStyle, GUILayout.Width(50));
                    }

                    EditorGUI.BeginChangeCheck();
                    bool newPinned = EditorGUILayout.Toggle(isPinned, GUILayout.Width(16));
                    if (EditorGUI.EndChangeCheck())
                    {
                        bool pinningNow = (!isPinned && newPinned);

                        isPinned = newPinned;

                        if (isPinned && reusableWindow == this) reusableWindow = null;
                        if (!isPinned) reusableWindow = this;
                        if (pinningNow) s_forceFreshMouseSpawn = true;

                        if (targetComponent != null)
                        {
                            string title = $"{targetComponent.GetType().Name} Properties";
                            if (isPinned) title += " (Pinned)";
                            titleContent = new GUIContent(title, componentIcon);
                        }
                    }

                    GUILayout.Label("Pin", HD_GUI.ComponentWindowTitleLabelStyle, GUILayout.Width(50));
                }         
            }
            EditorGUILayout.Space(2);
        }

        internal static void ShowForComponent(Component component, Vector2 mouseScreenPosition)
        {
            if (component == null) return;

            if (reusableWindow != null && !reusableWindow.isPinned)
            {
                if (s_forceFreshMouseSpawn)
                {
                    s_forceFreshMouseSpawn = false;
                    reusableWindow.Show();
                    reusableWindow.position = MakeSpawnRect(mouseScreenPosition, DefaultWidth, DefaultHeight);
                    reusableWindow.InitializeWindow(component, mouseScreenPosition);
                    reusableWindow.Focus();
                    return;
                }

                reusableWindow.InitializeWindow(component, mouseScreenPosition);
                reusableWindow.Focus();
                return;
            }

            HD_Component win = CreateInstance<HD_Component>();
            win.Show();
            win.position = MakeSpawnRect(mouseScreenPosition, DefaultWidth, DefaultHeight);
            win.InitializeWindow(component, mouseScreenPosition);
            win.Focus();

            if (!win.isPinned) reusableWindow = win;
        }

        private static Rect MakeSpawnRect(Vector2 mouseScreenPosition, float w, float h)
        {
            float screenW = Screen.currentResolution.width;
            float screenH = Screen.currentResolution.height;
            float x = Mathf.Clamp(mouseScreenPosition.x, 0f, screenW - w);
            float y = Mathf.Clamp(mouseScreenPosition.y, 0f, screenH - h);
            return new(x, y, w, h);
        }
        #endregion

        private void OnDisable()
        {
            if (componentEditor != null)
            {
                DestroyImmediate(componentEditor);
                componentEditor = null;
            }

            if (reusableWindow == this) reusableWindow = null;
        }
    }
    #endregion

    #region Folder Inspector
    [CustomEditor(typeof(HierarchyDesignerFolder))]
    internal class HD_Folder : Editor
    {
        #region Properties
        #region GUI
        private Vector2 notesScroll;
        private const int defaultGUISpace = 2;
        private const int sectionGUISpace = 10;
        private const int labelFieldWidth = 150;
        private const int minButtonWidth = 25;
        private const int maxButtonWidth = 100;
        private const string toggle = "Toggle";
        private const string select = "Select";
        private const string viewInScene = "View in Scene";
        private const string delete = "Delete";
        private const int maxAllowedChildren = 500;
        private float maxLabelWidth = 100f;
        #endregion

        #region Serialized
        private SerializedProperty flattenFolderProp;
        private SerializedProperty moveChildrenToHierarchyRootProp;
        private SerializedProperty flattenEventProp;
        private SerializedProperty onFlattenEventProp;
        private SerializedProperty onFolderDestroyProp;
        private SerializedProperty notesProp;
        #endregion

        #region Cache
        private bool doOnce = false;
        private bool showChildren = false;
        private bool displayParentsOnly = false;
        private bool childrenCached = false;
        private HierarchyDesignerFolder folder;
        private readonly List<Transform> cachedChildren = new();
        private int totalChildCount = 0;
        private List<GUILayoutOption[]> cachedGUIOptions;
        #endregion
        #endregion

        #region Initialization
        private void OnEnable()
        {
            folder = (HierarchyDesignerFolder)target;

            flattenFolderProp = serializedObject.FindProperty("flattenFolder");
            moveChildrenToHierarchyRootProp = serializedObject.FindProperty("moveChildrenToHierarchyRoot");
            flattenEventProp = serializedObject.FindProperty("flattenEvent");
            onFlattenEventProp = serializedObject.FindProperty("OnFlattenEvent");
            onFolderDestroyProp = serializedObject.FindProperty("OnFolderDestroy");
            notesProp = serializedObject.FindProperty("notes");

            CacheChildren();
            ProcessChildren();
        }
        #endregion

        #region Main
        public override void OnInspectorGUI()
        {
            if (!HD_Settings.EnableCustomInspectorGUI)
            {
                DrawDefaultInspector();
                return;
            }

            serializedObject.Update();
            EditorGUILayout.BeginVertical(HD_GUI.InspectorFolderPanelStyle);

            #region Runtime
            float originalLabelWidth = EditorGUIUtility.labelWidth;
            float originalFieldWidth = EditorGUIUtility.fieldWidth;
            EditorGUIUtility.labelWidth = 90;
            EditorGUIUtility.fieldWidth = maxButtonWidth;

            EditorGUILayout.BeginVertical(HD_GUI.InspectorFolderInnerPanelStyle);
            EditorGUILayout.LabelField("▷ Hierarchy Designer's Folder", HD_GUI.TabLabelStyle);
            EditorGUILayout.Space(defaultGUISpace);

            EditorGUILayout.BeginVertical();
            EditorGUILayout.Space(defaultGUISpace);

            HD_GUI.DrawPropertyField("Flatten Folder", labelFieldWidth, flattenFolderProp, true, true, "If 'Flatten Folder' is set to true, the folder will unparent all child GameObjects on the 'Flatten Event.' Once the operation is complete, the folder will destroy itself.");
            if (flattenFolderProp.boolValue)
            {
                HD_GUI.DrawPropertyField("Flatten Event", labelFieldWidth, flattenEventProp, HierarchyDesignerFolder.FlattenEvent.Start, true, "The event on which the 'Flatten Folder' action will occur.\n\nIf set to Awake, the folder will be flattened on the Awake event.\n\nIf set to Start, the folder will be flattened on the Start event.");
                EditorGUILayout.Space(6);

                HD_GUI.DrawPropertyField("Move Children To Root", labelFieldWidth, moveChildrenToHierarchyRootProp, true,  true, "If enabled, freed children are moved to the Hierarchy root.\nIf disabled, freed children are moved to the folder's parent (same layer where the folder existed).");

                EditorGUILayout.LabelField("Events", HD_GUI.FieldsCategoryLabelStyle);
                EditorGUILayout.Space(defaultGUISpace);

                EditorGUILayout.PropertyField(onFlattenEventProp);
                EditorGUILayout.Space(defaultGUISpace);

                EditorGUILayout.PropertyField(onFolderDestroyProp);
            }
            EditorGUILayout.EndVertical();
            EditorGUIUtility.labelWidth = originalLabelWidth;
            EditorGUIUtility.fieldWidth = originalFieldWidth;

            EditorGUILayout.EndVertical();
            serializedObject.ApplyModifiedProperties();
            #endregion

            #region Editor
            if (HD_Settings.IncludeEditorUtilitiesForHierarchyDesignerRuntimeFolder)
            {
                if (totalChildCount > maxAllowedChildren)
                {
                    EditorGUILayout.HelpBox($"This folder contains {totalChildCount} gameObject children, which exceeds the maximum allowed limit of {maxAllowedChildren} children. The editor utility is disabled for this folder.", MessageType.Warning);
                    EditorGUILayout.EndVertical();
                    return;
                }

                EditorGUILayout.BeginVertical(HD_GUI.InspectorFolderInnerPanelStyle);
                if (!childrenCached)
                {
                    CacheChildren();
                }
                if (!doOnce)
                {
                    maxLabelWidth = HD_GUI.CalculateMaxLabelWidth(folder.transform);
                    for (int i = 0; i < cachedGUIOptions.Count; i++)
                    {
                        cachedGUIOptions[i][0] = GUILayout.Width(maxLabelWidth);
                    }
                    doOnce = true;
                }

                EditorGUILayout.BeginVertical();
                EditorGUILayout.LabelField("Editor-Only", HD_GUI.FieldsCategoryLabelStyle);
                EditorGUILayout.Space(defaultGUISpace);

                #region Notes
                EditorGUILayout.LabelField("Notes", HD_GUI.RegularLabelStyle);
                EditorGUILayout.Space(defaultGUISpace);

                EditorGUI.BeginChangeCheck();
                notesScroll = EditorGUILayout.BeginScrollView(notesScroll, GUILayout.Height(80f));
                string newNotes = EditorGUILayout.TextArea(notesProp.stringValue, GUILayout.ExpandHeight(true));
                EditorGUILayout.EndScrollView();
                if (EditorGUI.EndChangeCheck())
                {
                    notesProp.stringValue = newNotes;
                }
                #endregion

                EditorGUILayout.Space(sectionGUISpace);

                #region GameObjects' List
                EditorGUILayout.LabelField($"This folder contains: '{totalChildCount}' gameObject children.", HD_GUI.RegularLabelStyle);
                EditorGUILayout.Space(defaultGUISpace);

                EditorGUI.BeginChangeCheck();
                bool newDisplayParentsOnly = EditorGUILayout.Toggle("Display Parents Only", displayParentsOnly);
                if (EditorGUI.EndChangeCheck())
                {
                    displayParentsOnly = newDisplayParentsOnly;
                    RefreshChildrenList();
                }

                EditorGUILayout.Space(defaultGUISpace);

                if (GUILayout.Button("Refresh Children List", GUILayout.Height(20)))
                {
                    RefreshChildrenList();
                }
                EditorGUILayout.Space(defaultGUISpace);

                if (totalChildCount > 0)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(11);
                    showChildren = EditorGUILayout.Foldout(showChildren, "GameObject's Children List");
                    EditorGUILayout.EndHorizontal();
                }
                if (showChildren)
                {
                    DisplayCachedChildren();
                }
                #endregion

                EditorGUILayout.Space(defaultGUISpace);

                EditorGUILayout.EndVertical();
                EditorGUILayout.EndVertical();
                serializedObject.ApplyModifiedProperties();
            }
            #endregion

            EditorGUILayout.EndVertical();
        }

        private void OnDisable()
        {
            cachedChildren.Clear();
            cachedGUIOptions.Clear();
            childrenCached = false;
        }
        #endregion

        #region Editor Operations
        private void CacheChildren()
        {
            cachedChildren.Clear();
            if (displayParentsOnly)
            {
                foreach (Transform child in folder.transform)
                {
                    cachedChildren.Add(child);
                }
            }
            else
            {
                GetChildTransforms(folder.transform, cachedChildren);
            }
            totalChildCount = cachedChildren.Count;
            childrenCached = true;
        }

        private void GetChildTransforms(Transform parent, List<Transform> children)
        {
            foreach (Transform child in parent)
            {
                children.Add(child);
                GetChildTransforms(child, children);
            }
        }

        private void ProcessChildren()
        {
            cachedGUIOptions = new List<GUILayoutOption[]>(totalChildCount);

            for (int i = 0; i < totalChildCount; i++)
            {
                GUILayoutOption[] options = new GUILayoutOption[4];
                options[0] = GUILayout.Width(maxLabelWidth);
                options[1] = GUILayout.MinWidth(minButtonWidth);
                options[2] = GUILayout.ExpandWidth(true);
                options[3] = GUILayout.MinWidth(maxButtonWidth);
                cachedGUIOptions.Add(options);
            }
        }

        private void DisplayCachedChildren()
        {
            for (int i = 0; i < cachedChildren.Count; i++)
            {
                Transform child = cachedChildren[i];
                GUILayoutOption[] options = cachedGUIOptions[i];

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(child.name, (child.gameObject.activeSelf ? HD_GUI.InspectorFolderActiveLabelStyle : HD_GUI.InspectorFolderInactiveLabelStyle), options[0]);

                if (GUILayout.Button(toggle, options[1], options[2]))
                {
                    Undo.RecordObject(child.gameObject, "Toggle Active State");
                    child.gameObject.SetActive(!child.gameObject.activeSelf);
                }
                if (GUILayout.Button(select, options[1], options[2]))
                {
                    EditorApplication.ExecuteMenuItem("Window/General/Hierarchy");
                    Selection.activeGameObject = child.gameObject;
                }
                if (GUILayout.Button(viewInScene, options[3], options[2]))
                {
                    GameObject originalSelection = Selection.activeGameObject;
                    Selection.activeGameObject = child.gameObject;
                    SceneView.FrameLastActiveSceneView();
                    Selection.activeGameObject = originalSelection;
                }
                if (GUILayout.Button(delete, options[1], options[2]))
                {
                    Undo.DestroyObjectImmediate(child.gameObject);
                    cachedChildren.Remove(child);
                    cachedGUIOptions.RemoveAt(i);
                    GUIUtility.ExitGUI();
                }
                EditorGUILayout.EndHorizontal();
            }
        }

        private void RefreshChildrenList()
        {
            childrenCached = false;
            doOnce = false;
            CacheChildren();
            ProcessChildren();
            maxLabelWidth = HD_GUI.CalculateMaxLabelWidth(folder.transform);
        }
        #endregion
    }
    #endregion

    #region Main Icon Override
    internal sealed class HD_IconOverride : EditorWindow
    {
        #region Properties
        private const float TileSize = 32f;
        private const float TilePadding = 5f;

        private GameObject targetGO;
        private string targetGlobalId;
        private string search = string.Empty;

        private Vector2 scroll;

        private static readonly List<Texture2D> s_BuiltinIcons = new List<Texture2D>();
        private static readonly List<(Texture2D tex, string label)> s_ComponentIcons = new List<(Texture2D tex, string label)>();
        private static readonly List<(Texture2D tex, string guid)> s_AssetIcons = new List<(Texture2D tex, string guid)>();
        private static readonly List<(Texture2D tex, string guid)> s_AssetFiltered = new List<(Texture2D tex, string guid)>();

        private static string[] s_AssetGuids;
        private static int s_AssetGuidIndex;
        private static bool s_AssetIndexing;
        private static bool s_AssetIndexed;
        private static bool s_AssetUpdateRegistered;
        private static bool s_AssetFilterDirty;
        private static string s_AssetSearchCache = string.Empty;
        private static int s_AssetPage;

        private static int s_OpenWindows;
        #endregion

        #region Initialization
        public static void Open(GameObject go)
        {
            if (go == null) return;

            HD_IconOverride window = CreateInstance<HD_IconOverride>();
            window.titleContent = new GUIContent("Main Icon Override");
            window.targetGO = go;
            window.targetGlobalId = GlobalObjectId.GetGlobalObjectIdSlow(go).ToString();

            Vector2 pos = Event.current != null
                ? GUIUtility.GUIToScreenPoint(Event.current.mousePosition)
                : new Vector2(100f, 100f);

            window.position = new Rect(pos, new Vector2(560f, 480f));
            window.minSize = new Vector2(420f, 360f);
            window.ShowAuxWindow();
            window.Focus();
        }

        private void OnEnable()
        {
            s_OpenWindows++;

            if (s_BuiltinIcons.Count == 0) EditorApplication.delayCall += GatherBuiltinIcons;
            if (s_ComponentIcons.Count == 0) EditorApplication.delayCall += GatherComponentIcons;

            if (HD_Settings.EnableProjectTexturesInMainIconOverrideWindow)
            {
                EnsureAssetIndexingStarted();
            }
            else
            {
                StopAssetIndexing();
                ResetAssetIndexingData();
            }
        }

        private void OnDisable()
        {
            if (s_OpenWindows > 0) s_OpenWindows--;

            if (s_OpenWindows == 0)
            {
                StopAssetIndexing();
            }
        }
        #endregion

        private void OnGUI()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                GUIStyle tf = GUI.skin.FindStyle("ToolbarSeachTextField")
                               ?? GUI.skin.FindStyle("ToolbarSearchTextField")
                               ?? EditorStyles.toolbarTextField;

                string newSearch = GUILayout.TextField(search, tf, GUILayout.ExpandWidth(true));
                if (!string.Equals(newSearch, search, StringComparison.Ordinal))
                {
                    search = newSearch;
                    s_AssetFilterDirty = true;
                    s_AssetPage = 0;
                }

                if (GUILayout.Button("Clear", EditorStyles.toolbarButton, GUILayout.Width(60f)))
                {
                    if (!string.IsNullOrEmpty(search))
                    {
                        search = string.Empty;
                        s_AssetFilterDirty = true;
                        s_AssetPage = 0;
                    }
                }

                using (new EditorGUI.DisabledScope(targetGO == null))
                {
                    bool has = HD_Icon.Has(targetGlobalId);
                    if (GUILayout.Button(has ? "Clear Override" : "No Override", EditorStyles.toolbarButton, GUILayout.Width(110f)))
                    {
                        if (HD_Icon.Clear(targetGlobalId)) Close();
                    }
                }
            }

            scroll = EditorGUILayout.BeginScrollView(scroll);

            DrawSection("Common Component Icons", DrawComponentGrid);
            GUILayout.Space(6f);

            DrawSection("Built-in / Editor Icons", DrawBuiltinGrid);
            GUILayout.Space(6f);

            if (HD_Settings.EnableProjectTexturesInMainIconOverrideWindow)
            {
                DrawSection("Project Textures", DrawAssetGrid);
            }

            EditorGUILayout.EndScrollView();
        }

        #region Methods
        private void DrawSection(string label, Action drawer)
        {
            EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
            GUILayout.Space(2f);
            drawer?.Invoke();
        }

        private void DrawBuiltinGrid()
        {
            List<Texture2D> list = Filter(s_BuiltinIcons, t => t != null ? t.name : string.Empty);

            DrawIconGrid(list, t =>
            {
                HD_Icon.SetBuiltin(targetGlobalId, t);
                Close();
            }, true);
        }

        private void DrawAssetGrid()
        {
            if (!s_AssetIndexed)
            {
                EditorGUILayout.HelpBox("Indexing project textures...", MessageType.Info);

                if (s_AssetGuids != null && s_AssetGuids.Length > 0)
                {
                    EditorGUILayout.LabelField(s_AssetGuidIndex + " / " + s_AssetGuids.Length + " processed", EditorStyles.miniLabel);
                }

                return;
            }

            List<(Texture2D tex, string guid)> list = GetFilteredAssetIcons();

            if (list == null || list.Count == 0)
            {
                EditorGUILayout.HelpBox("No textures found for current filter.", MessageType.Info);
                return;
            }

            const int PageSize = 256;

            int total = list.Count;
            int totalPages = Mathf.Max(1, Mathf.CeilToInt((float)total / PageSize));

            if (s_AssetPage < 0) s_AssetPage = 0;
            if (s_AssetPage >= totalPages) s_AssetPage = totalPages - 1;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Page " + (s_AssetPage + 1) + " / " + totalPages, EditorStyles.miniLabel);
            GUILayout.FlexibleSpace();

            using (new EditorGUI.DisabledScope(s_AssetPage <= 0))
            {
                if (GUILayout.Button("Prev", EditorStyles.miniButtonLeft, GUILayout.Width(60f)))
                {
                    s_AssetPage--;
                    GUI.FocusControl(null);
                }
            }

            using (new EditorGUI.DisabledScope(s_AssetPage >= totalPages - 1))
            {
                if (GUILayout.Button("Next", EditorStyles.miniButtonRight, GUILayout.Width(60f)))
                {
                    s_AssetPage++;
                    GUI.FocusControl(null);
                }
            }

            EditorGUILayout.EndHorizontal();
            GUILayout.Space(2f);

            int start = s_AssetPage * PageSize;
            int end = Mathf.Min(start + PageSize, total);

            int perRow = Mathf.Max(1, Mathf.FloorToInt((position.width - 20f) / (TileSize + TilePadding)));
            int i = start;

            while (i < end)
            {
                EditorGUILayout.BeginHorizontal();

                for (int c = 0; c < perRow && i < end; c++, i++)
                {
                    (Texture2D tex, string guid) = list[i];

                    using (new EditorGUILayout.VerticalScope(GUILayout.Width(TileSize + TilePadding)))
                    {
                        Rect r = GUILayoutUtility.GetRect(TileSize, TileSize, GUILayout.ExpandWidth(false));

                        if (tex != null) GUI.DrawTexture(r, tex, ScaleMode.ScaleToFit, true);

                        if (GUI.Button(r, GUIContent.none, GUIStyle.none))
                        {
                            HD_Icon.SetAsset(targetGlobalId, guid);
                            Close();
                        }

                        GUILayout.Label(tex != null ? tex.name : "(null)", EditorStyles.miniLabel, GUILayout.Width(TileSize + TilePadding));
                    }
                }

                EditorGUILayout.EndHorizontal();
                GUILayout.Space(2f);
            }
        }

        private void DrawComponentGrid()
        {
            List<(Texture2D tex, string label)> list = Filter(s_ComponentIcons, x => (x.label ?? string.Empty) + " " + (x.tex != null ? x.tex.name : string.Empty));

            if (list == null || list.Count == 0)
            {
                EditorGUILayout.HelpBox("No component icons found for current filter.", MessageType.Info);
                return;
            }

            int perRow = Mathf.Max(1, Mathf.FloorToInt((position.width - 20f) / (TileSize + TilePadding)));
            int i = 0;

            while (i < list.Count)
            {
                EditorGUILayout.BeginHorizontal();

                for (int c = 0; c < perRow && i < list.Count; c++, i++)
                {
                    (Texture2D tex, string label) = list[i];

                    using (new EditorGUILayout.VerticalScope(GUILayout.Width(TileSize + TilePadding)))
                    {
                        Rect r = GUILayoutUtility.GetRect(TileSize, TileSize, GUILayout.ExpandWidth(false));

                        if (tex != null) GUI.DrawTexture(r, tex, ScaleMode.ScaleToFit, true);

                        if (GUI.Button(r, GUIContent.none, GUIStyle.none))
                        {
                            HD_Icon.SetBuiltin(targetGlobalId, tex);
                            Close();
                        }

                        string shown = !string.IsNullOrEmpty(label) ? label : (tex != null ? tex.name : "(null)");
                        GUILayout.Label(shown, EditorStyles.miniLabel, GUILayout.Width(TileSize + TilePadding));
                    }
                }

                EditorGUILayout.EndHorizontal();
                GUILayout.Space(2f);
            }
        }

        private void DrawIconGrid(IList<Texture2D> icons, Action<Texture2D> onPick, bool showName)
        {
            if (icons == null || icons.Count == 0)
            {
                EditorGUILayout.HelpBox("No icons found for current filter.", MessageType.Info);
                return;
            }

            int perRow = Mathf.Max(1, Mathf.FloorToInt((position.width - 20f) / (TileSize + TilePadding)));
            int i = 0;

            while (i < icons.Count)
            {
                EditorGUILayout.BeginHorizontal();

                for (int c = 0; c < perRow && i < icons.Count; c++, i++)
                {
                    Texture2D tex = icons[i];

                    using (new EditorGUILayout.VerticalScope(GUILayout.Width(TileSize + TilePadding)))
                    {
                        Rect r = GUILayoutUtility.GetRect(TileSize, TileSize, GUILayout.ExpandWidth(false));

                        if (tex != null) GUI.DrawTexture(r, tex, ScaleMode.ScaleToFit, true);

                        if (GUI.Button(r, GUIContent.none, GUIStyle.none)) onPick?.Invoke(tex);

                        if (showName)
                        {
                            GUILayout.Label(tex != null ? tex.name : "(null)", EditorStyles.miniLabel, GUILayout.Width(TileSize + TilePadding));
                        }
                    }
                }

                EditorGUILayout.EndHorizontal();
                GUILayout.Space(2f);
            }
        }

        private List<T> Filter<T>(IEnumerable<T> src, Func<T, string> key)
        {
            if (string.IsNullOrEmpty(search)) return src.ToList();

            string s = search.Trim().ToLowerInvariant();
            return src.Where(x => (key(x) ?? string.Empty).ToLowerInvariant().Contains(s)).ToList();
        }

        private static void EnsureAssetIndexingStarted()
        {
            if (!HD_Settings.EnableProjectTexturesInMainIconOverrideWindow)
            {
                StopAssetIndexing();
                return;
            }

            if (s_AssetIndexed || s_AssetIndexing) return;

            if (s_AssetGuids == null)
            {
                s_AssetGuids = AssetDatabase.FindAssets("t:Texture2D");
                s_AssetGuidIndex = 0;
                s_AssetIcons.Clear();
            }

            if (s_AssetGuids == null || s_AssetGuids.Length == 0)
            {
                FinalizeAssetIndexing();
                return;
            }

            if (s_AssetGuidIndex >= s_AssetGuids.Length)
            {
                FinalizeAssetIndexing();
                return;
            }

            s_AssetIndexing = true;
            s_AssetIndexed = false;

            if (!s_AssetUpdateRegistered)
            {
                EditorApplication.update += ProcessAssetIndexing;
                s_AssetUpdateRegistered = true;
            }
        }

        private static void ProcessAssetIndexing()
        {
            if (!HD_Settings.EnableProjectTexturesInMainIconOverrideWindow)
            {
                StopAssetIndexing();
                return;
            }

            if (s_OpenWindows <= 0)
            {
                StopAssetIndexing();
                return;
            }

            if (!s_AssetIndexing)
            {
                StopAssetIndexing();
                return;
            }

            int budget = 16;

            while (budget > 0 && s_AssetGuidIndex < s_AssetGuids.Length)
            {
                string guid = s_AssetGuids[s_AssetGuidIndex++];
                string path = AssetDatabase.GUIDToAssetPath(guid);
                Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);

                if (tex != null && tex.width <= 256 && tex.height <= 256)
                {
                    s_AssetIcons.Add((tex, guid));
                }

                budget--;
            }

            if (s_AssetGuidIndex >= s_AssetGuids.Length)
            {
                FinalizeAssetIndexing();
            }
        }

        private static void StopAssetIndexing()
        {
            s_AssetIndexing = false;

            if (s_AssetUpdateRegistered)
            {
                EditorApplication.update -= ProcessAssetIndexing;
                s_AssetUpdateRegistered = false;
            }
        }

        private static void FinalizeAssetIndexing()
        {
            s_AssetIndexing = false;
            s_AssetIndexed = true;

            s_AssetIcons.Sort((a, b) =>
            {
                string an = a.tex != null ? a.tex.name : string.Empty;
                string bn = b.tex != null ? b.tex.name : string.Empty;
                return string.Compare(an, bn, StringComparison.Ordinal);
            });

            s_AssetFilterDirty = true;

            if (s_AssetUpdateRegistered)
            {
                EditorApplication.update -= ProcessAssetIndexing;
                s_AssetUpdateRegistered = false;
            }
        }

        private static void ResetAssetIndexingData()
        {
            s_AssetGuids = null;
            s_AssetGuidIndex = 0;
            s_AssetIndexing = false;
            s_AssetIndexed = false;
            s_AssetFilterDirty = true;
            s_AssetSearchCache = string.Empty;
            s_AssetPage = 0;

            s_AssetIcons.Clear();
            s_AssetFiltered.Clear();
        }

        private static void GatherBuiltinIcons()
        {
            if (s_BuiltinIcons.Count > 0) return;

            Texture2D[] all = Resources.FindObjectsOfTypeAll<Texture2D>();

            for (int i = 0; i < all.Length; i++)
            {
                Texture2D t = all[i];
                if (t == null) continue;

                if (t.width <= 64 && t.height <= 64)
                {
                    if (t.name.Contains(" Icon") || t.name.StartsWith("d_") || t.name.EndsWith(" Icon") || t.name.EndsWith(" Icon Small"))
                    {
                        s_BuiltinIcons.Add(t);
                    }
                }
            }

            List<Texture2D> dedup = s_BuiltinIcons
                .GroupBy(t => t != null ? t.name : string.Empty)
                .Select(g => g.First())
                .OrderBy(t => t != null ? t.name : string.Empty)
                .ToList();

            s_BuiltinIcons.Clear();
            s_BuiltinIcons.AddRange(dedup);
        }

        private static void GatherComponentIcons()
        {
            if (s_ComponentIcons.Count > 0) return;

            TryAddIconByName("DirectionalLight Icon", "Directional Light");
            TryAddIconByName("SpotLight Icon", "Spot Light");
            TryAddIconByName("AreaLight Icon", "Area Light");
            TryAddIconByType("UnityEngine.Light, UnityEngine.CoreModule", "Light");

            TryAddIconByName("Camera Icon", "Camera");
            TryAddIconByType("UnityEngine.Camera, UnityEngine.CoreModule", "Camera");

            TryAddIconByName("AudioSource Icon", "Audio Source");
            TryAddIconByType("UnityEngine.AudioSource, UnityEngine.CoreModule", "Audio Source");
            TryAddIconByName("AudioListener Icon", "Audio Listener");
            TryAddIconByType("UnityEngine.AudioListener, UnityEngine.CoreModule", "Audio Listener");

            TryAddIconByName("MeshRenderer Icon", "Mesh Renderer");
            TryAddIconByType("UnityEngine.MeshRenderer, UnityEngine.CoreModule", "Mesh Renderer");
            TryAddIconByName("SkinnedMeshRenderer Icon", "Skinned Mesh Renderer");
            TryAddIconByType("UnityEngine.SkinnedMeshRenderer, UnityEngine.CoreModule", "Skinned Mesh Renderer");
            TryAddIconByName("SpriteRenderer Icon", "Sprite Renderer");
            TryAddIconByType("UnityEngine.SpriteRenderer, UnityEngine.CoreModule", "Sprite Renderer");
            TryAddIconByName("LineRenderer Icon", "Line Renderer");
            TryAddIconByType("UnityEngine.LineRenderer, UnityEngine.CoreModule", "Line Renderer");
            TryAddIconByName("TrailRenderer Icon", "Trail Renderer");
            TryAddIconByType("UnityEngine.TrailRenderer, UnityEngine.CoreModule", "Trail Renderer");
            TryAddIconByName("ParticleSystem Icon", "Particle System");
            TryAddIconByType("UnityEngine.ParticleSystem, UnityEngine.ParticleSystemModule", "Particle System");
            TryAddIconByName("ParticleSystemForceField Icon", "Particle Force Field");

            TryAddIconByName("BoxCollider Icon", "Box Collider");
            TryAddIconByType("UnityEngine.BoxCollider, UnityEngine.PhysicsModule", "Box Collider");
            TryAddIconByName("SphereCollider Icon", "Sphere Collider");
            TryAddIconByType("UnityEngine.SphereCollider, UnityEngine.PhysicsModule", "Sphere Collider");
            TryAddIconByName("CapsuleCollider Icon", "Capsule Collider");
            TryAddIconByType("UnityEngine.CapsuleCollider, UnityEngine.PhysicsModule", "Capsule Collider");
            TryAddIconByName("MeshCollider Icon", "Mesh Collider");
            TryAddIconByType("UnityEngine.MeshCollider, UnityEngine.PhysicsModule", "Mesh Collider");

            TryAddIconByName("BoxCollider2D Icon", "Box Collider 2D");
            TryAddIconByType("UnityEngine.BoxCollider2D, UnityEngine.Physics2DModule", "Box Collider 2D");
            TryAddIconByName("CircleCollider2D Icon", "Circle Collider 2D");
            TryAddIconByType("UnityEngine.CircleCollider2D, UnityEngine.Physics2DModule", "Circle Collider 2D");
            TryAddIconByName("CapsuleCollider2D Icon", "Capsule Collider 2D");
            TryAddIconByType("UnityEngine.CapsuleCollider2D, UnityEngine.Physics2DModule", "Capsule Collider 2D");
            TryAddIconByName("EdgeCollider2D Icon", "Edge Collider 2D");
            TryAddIconByType("UnityEngine.EdgeCollider2D, UnityEngine.Physics2DModule", "Edge Collider 2D");
            TryAddIconByName("PolygonCollider2D Icon", "Polygon Collider 2D");
            TryAddIconByType("UnityEngine.PolygonCollider2D, UnityEngine.Physics2DModule", "Polygon Collider 2D");

            TryAddIconByName("Rigidbody Icon", "Rigidbody");
            TryAddIconByType("UnityEngine.Rigidbody, UnityEngine.PhysicsModule", "Rigidbody");
            TryAddIconByName("Rigidbody2D Icon", "Rigidbody 2D");
            TryAddIconByType("UnityEngine.Rigidbody2D, UnityEngine.Physics2DModule", "Rigidbody 2D");

            TryAddIconByName("Animator Icon", "Animator");
            TryAddIconByType("UnityEngine.Animator, UnityEngine.AnimationModule", "Animator");
            TryAddIconByName("Animation Icon", "Animation");
            TryAddIconByType("UnityEngine.Animation, UnityEngine.AnimationModule", "Animation");

            TryAddIconByName("Terrain Icon", "Terrain");
            TryAddIconByType("UnityEngine.Terrain, UnityEngine.TerrainModule", "Terrain");

            TryAddIconByName("ReflectionProbe Icon", "Reflection Probe");
            TryAddIconByType("UnityEngine.ReflectionProbe, UnityEngine.CoreModule", "Reflection Probe");
            TryAddIconByName("LightProbeGroup Icon", "Light Probe Group");
            TryAddIconByType("UnityEngine.LightProbeGroup, UnityEngine.CoreModule", "Light Probe Group");

            TryAddIconByName("Canvas Icon", "Canvas");
            TryAddIconByType("UnityEngine.Canvas, UnityEngine.UIModule", "Canvas");
            TryAddIconByName("RectTransform Icon", "RectTransform");
            TryAddIconByType("UnityEngine.RectTransform, UnityEngine.CoreModule", "RectTransform");
            TryAddIconByName("Image Icon", "UI Image");
            TryAddIconByType("UnityEngine.UI.Image, UnityEngine.UI", "UI Image");
            TryAddIconByName("Text Icon", "UI Text");
            TryAddIconByType("UnityEngine.UI.Text, UnityEngine.UI", "UI Text");
            TryAddIconByName("Button Icon", "UI Button");
            TryAddIconByType("UnityEngine.UI.Button, UnityEngine.UI", "UI Button");
            TryAddIconByName("EventSystem Icon", "Event System");
            TryAddIconByType("UnityEngine.EventSystems.EventSystem, UnityEngine.UI", "Event System");
            TryAddIconByType("TMPro.TextMeshProUGUI, Unity.TextMeshPro", "TextMeshPro UGUI");
            TryAddIconByType("TMPro.TextMeshPro, Unity.TextMeshPro", "TextMeshPro");

            TryAddIconByName("NavMeshAgent Icon", "NavMesh Agent");
            TryAddIconByType("UnityEngine.AI.NavMeshAgent, UnityEngine.AIModule", "NavMesh Agent");
            TryAddIconByType("UnityEngine.AI.NavMeshObstacle, UnityEngine.AIModule", "NavMesh Obstacle");

            TryAddIconByName("Grid Icon", "Grid");
            TryAddIconByType("UnityEngine.Grid, UnityEngine.GridModule", "Grid");
            TryAddIconByName("Tilemap Icon", "Tilemap");
            TryAddIconByType("UnityEngine.Tilemaps.Tilemap, UnityEngine.TilemapModule", "Tilemap");
            TryAddIconByName("TilemapRenderer Icon", "Tilemap Renderer");
            TryAddIconByType("UnityEngine.Tilemaps.TilemapRenderer, UnityEngine.TilemapModule", "Tilemap Renderer");

            TryAddIconByName("VideoPlayer Icon", "Video Player");
            TryAddIconByType("UnityEngine.Video.VideoPlayer, UnityEngine.VideoModule", "Video Player");
            TryAddIconByName("PlayableDirector Icon", "Playable Director");
            TryAddIconByType("UnityEngine.Playables.PlayableDirector, UnityEngine.DirectorModule", "Playable Director");

            TryAddIconByName("Sprite Icon", "Sprite");
            TryAddIconByName("PhysicsMaterial2D Icon", "Physics Material 2D");

            List<(Texture2D tex, string label)> filtered = s_ComponentIcons
                .Where(x => x.tex != null)
                .GroupBy(x => x.tex)
                .Select(g => g.First())
                .OrderBy(x => x.label)
                .ToList();

            s_ComponentIcons.Clear();
            s_ComponentIcons.AddRange(filtered);
        }

        private static void TryAddIconByName(string iconName, string label)
        {
            GUIContent c = EditorGUIUtility.IconContent(iconName);
            Texture2D tex = c != null ? c.image as Texture2D : null;
            if (tex != null) AddComponentIcon(tex, label);
        }

        private static void TryAddIconByType(string qualifiedTypeName, string fallbackLabel)
        {
            Type t = Type.GetType(qualifiedTypeName);
            if (t == null) return;

            GUIContent c = EditorGUIUtility.ObjectContent(null, t);
            Texture2D icon = c != null ? c.image as Texture2D : null;

            if (icon != null) AddComponentIcon(icon, fallbackLabel);
        }

        private static void AddComponentIcon(Texture2D tex, string label)
        {
            s_ComponentIcons.Add((tex, label));
        }

        private List<(Texture2D tex, string guid)> GetFilteredAssetIcons()
        {
            if (!s_AssetFilterDirty && string.Equals(s_AssetSearchCache, search, StringComparison.Ordinal))
            {
                return s_AssetFiltered;
            }

            s_AssetFiltered.Clear();

            string s = string.IsNullOrEmpty(search) ? null : search.Trim().ToLowerInvariant();

            if (string.IsNullOrEmpty(s))
            {
                for (int i = 0; i < s_AssetIcons.Count; i++)
                {
                    s_AssetFiltered.Add(s_AssetIcons[i]);
                }
            }
            else
            {
                for (int i = 0; i < s_AssetIcons.Count; i++)
                {
                    Texture2D tex = s_AssetIcons[i].tex;
                    string name = tex != null ? tex.name : string.Empty;

                    if (!string.IsNullOrEmpty(name) && name.ToLowerInvariant().Contains(s))
                    {
                        s_AssetFiltered.Add(s_AssetIcons[i]);
                    }
                }
            }

            s_AssetSearchCache = search;
            s_AssetFilterDirty = false;

            return s_AssetFiltered;
        }
        #endregion
    }
    #endregion

    #region Rename
    internal class HD_Rename : EditorWindow
    {
        #region Properties
        #region GUI
        private Vector2 mainScroll;
        private const float fieldsWidth = 130;
        #endregion

        #region Info
        private string newName = "";
        private bool automaticIndexing = true;
        private int startingIndex = 0;
        [SerializeField] private List<GameObject> selectedGameObjects = new();
        private ReorderableList reorderableList;
        #endregion
        #endregion

        #region Window
        public static void OpenWindow(List<GameObject> gameObjects, bool autoIndex = true, int startIndex = 0)
        {
            HD_Rename window = GetWindow<HD_Rename>("HD Rename Tool");
            Vector2 size = new(400, 200);
            window.minSize = size;
            window.newName = "";
            window.automaticIndexing = autoIndex;
            window.startingIndex = startIndex;
            window.selectedGameObjects = gameObjects ?? new();
            window.InitializeReorderableList();
        }
        #endregion

        #region Initialization
        private void InitializeReorderableList()
        {
            reorderableList = new(selectedGameObjects, typeof(GameObject), true, true, true, true)
            {
                drawHeaderCallback = (Rect rect) =>
                {
                    EditorGUI.LabelField(rect, "GameObjects' List");
                },
                drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
                {
                    selectedGameObjects[index] = (GameObject)EditorGUI.ObjectField(new(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), selectedGameObjects[index], typeof(GameObject), true);
                },
                onAddCallback = (ReorderableList list) =>
                {
                    selectedGameObjects.Add(null);
                },
                onRemoveCallback = (ReorderableList list) =>
                {
                    selectedGameObjects.RemoveAt(list.index);
                }
            };
        }
        #endregion

        private void OnGUI()
        {
            EditorGUILayout.BeginVertical(HD_GUI.TertiaryPanelStyle);

            #region Body
            mainScroll = EditorGUILayout.BeginScrollView(mainScroll, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

            #region New Values
            EditorGUILayout.BeginVertical(HD_GUI.SecondaryPanelStyle);
            EditorGUILayout.LabelField("▷ RENAME TOOL", HD_GUI.TabLabelStyle);
            GUILayout.Space(5);

            newName = HD_GUI.DrawTextField("New Name", fieldsWidth, string.Empty, newName, true, "The new name that the selected GameObjects will be renamed to.");
            automaticIndexing = HD_GUI.DrawToggle("Auto-Index", fieldsWidth, automaticIndexing, true, true, "The selected GameObjects will be renamed with indexes (e.g., (1), (2), (3), ...).");
            if (automaticIndexing) { startingIndex = HD_GUI.DrawIntField("Starting Index", fieldsWidth, startingIndex, 0, true, "The starting value of the index (e.g., a value of 10 will start the indexing at (10), and so on)."); }
            #endregion

            GUILayout.Space(10);

            #region Selected GameObjects List
            if (reorderableList != null)
            {
                EditorGUILayout.LabelField("Selected GameObjects", HD_GUI.FieldsCategoryLabelStyle);
                GUILayout.Space(2);
                reorderableList.DoLayoutList();
            }
            EditorGUILayout.EndVertical();
            #endregion

            EditorGUILayout.EndScrollView();
            #endregion

            #region Footer
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Reassign Selected GameObjects", GUILayout.Height(25)))
            {
                ReassignSelectedGameObjects();
            }
            if (GUILayout.Button("Clear Selected GameObjects", GUILayout.Height(25)))
            {
                ClearSelectedGameObjects();
            }
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Rename Selected GameObjects", GUILayout.Height(30)))
            {
                RenameSelectedGameObjects();
                Close();
            }
            #endregion

            EditorGUILayout.EndVertical();
        }

        #region Methods
        private void ReassignSelectedGameObjects()
        {
            selectedGameObjects = new(Selection.gameObjects);
            InitializeReorderableList();
        }

        private void ClearSelectedGameObjects()
        {
            selectedGameObjects.Clear();
            InitializeReorderableList();
        }

        private void RenameSelectedGameObjects()
        {
            if (selectedGameObjects == null) return;

            for (int i = 0; i < selectedGameObjects.Count; i++)
            {
                if (selectedGameObjects[i] != null)
                {
                    Undo.RecordObject(selectedGameObjects[i], "Rename GameObject");
                    string objectName = automaticIndexing ? $"{newName} ({startingIndex + i})" : newName;
                    selectedGameObjects[i].name = objectName;
                    EditorUtility.SetDirty(selectedGameObjects[i]);
                }
            }
        }
        #endregion

        private void OnDestroy()
        {
            newName = "";
            selectedGameObjects = null;
            reorderableList = null;
        }
    }
    #endregion
}
#endif