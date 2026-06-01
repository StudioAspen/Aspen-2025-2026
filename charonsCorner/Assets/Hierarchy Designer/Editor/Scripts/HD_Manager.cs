#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace HierarchyDesigner
{
    internal static class HD_Manager
    {
        #region Properties
        #region GUI
        private static readonly Color activeColor = new(1f, 1f, 1f, 1f);
        private static readonly Color inactiveColor = new(1f, 1f, 1f, alphaValueForInactiveGameObjects);
        private static GUIStyle tagStyle;
        private static GUIStyle TagStyle
        {
            get
            {
                if (tagStyle == null)
                {
                    tagStyle = new(GUI.skin.label)
                    {
                        alignment = tagTextAnchor,
                        fontStyle = tagFontStyle,
                        fontSize = tagFontSize,
                        normal = { textColor = tagColor }
                    };
                }
                return tagStyle;
            }
        }
        private static GUIStyle layerStyle;
        private static GUIStyle LayerStyle
        {
            get
            {
                if (layerStyle == null)
                {
                    layerStyle = new(GUI.skin.label)
                    {
                        alignment = layerTextAnchor,
                        fontStyle = layerFontStyle,
                        fontSize = layerFontSize,
                        normal = { textColor = layerColor }
                    };
                }
                return layerStyle;
            }
        }
        private static GUIStyle lockStyle;
        private static GUIStyle LockStyle
        {
            get
            {
                if (lockStyle == null)
                {
                    lockStyle = new(GUI.skin.label)
                    {
                        alignment = lockTextAnchor,
                        fontStyle = lockFontStyle,
                        fontSize = lockFontSize,
                        normal = { textColor = lockColor }
                    };
                }
                return lockStyle;
            }
        }
        private static GUIStyle folderStyle;
        private static GUIStyle FolderStyle
        {
            get
            {
                if (folderStyle == null)
                {
                    folderStyle = new(GUI.skin.label)
                    {
                        alignment = TextAnchor.MiddleLeft
                    };
                }
                return folderStyle;
            }
        }

        private static readonly GUIContent tempContent = new();
        #endregion

        #region Const
        private const string warningIconTexture = "console.warnicon";
        private const string missingComponentMessage = "Missing Component";
        private const int hierarchyWindowOffsetLeft = 32;
        private const int hierarchyWindowOffsetRight = -53;
        private const float alphaValueForInactiveGameObjects = 0.5f;
        private const int defaultIconSelectionHeight = 16;
        private const float defaultComponentIconsSize = 1f;
        private const float defaultComponentIconsSpacing = 2f;
        private const int defaultComponentIconsOffset = 20;
        private const float mainIconBackgroundRectLeftOffset = 0f;
        private const float mainIconBackgroundRectRightOffset = 15.25f;
        private const int hierarchyTreeOffset = 22;
        private const int hierarchyTreeFillLinesOffset = 14;
        private const int buttonsWidth = 25;
        private const int totalButtonsWidth = 50;
        private const int defaultXOffset = 5;
        private const int layoutBaseOffset = 15;
        private const int splitModeOffset = 12;
        private const float currentViewWidthOffset = 78;
        private const float folderChildIndent = 14f;
        private const string lockedLabel = "(Locked)";
        private const string separatorMessage = "Separators are EditorOnly, meaning they will not be present in your game's build. If you want a GameObject parent to organize your GameObjects, use a folder instead.";
        private const string lockedGameObjectMessage = "This gameObject is locked, components are not available for editing.";
        private static readonly Type typeTransform = typeof(Transform);
        private static readonly Texture2D prefabOverlayIcon = EditorGUIUtility.IconContent("PrefabOverlayAdded Icon").image as Texture2D;
        #endregion

        #region Settings
        private static HD_Settings.HierarchyLayoutMode layoutMode;
        private static HD_Settings.HierarchyTreeMode treeMode;
        private static bool enableGameObjectMainIcon;
        private static bool enableGameObjectComponentIcons;
        private static bool enableHierarchyTree;
        private static bool enableGameObjectTag;
        private static bool enableGameObjectLayer;
        private static bool enableHierarchyRows;
        private static bool enableHierarchyLines;
        private static bool enableHierarchyButtons;
        private static bool enableHeaderUtilities;
        private static bool enableMajorShortcuts;
        private static bool disableEditorDesignerMajorOperationsDuringPlayMode;
        private static bool excludeFolderProperties;
        private static List<string> excludedComponents = new() { "Transform", "RectTransform", "CanvasRenderer" };
        private static int maximumComponentIconsAmount;
        private static List<string> excludedTags;
        private static List<string> excludedLayers;
        private static HD_Settings.UpdateMode mainIconUpdateMode;
        private static HD_Settings.UpdateMode componentsIconsUpdateMode;
        private static HD_Settings.UpdateMode hierarchyTreeUpdateMode;
        private static HD_Settings.UpdateMode tagUpdateMode;
        private static HD_Settings.UpdateMode layerUpdateMode;
        private static bool enableDynamicBackgroundForGameObjectMainIcon;
        private static bool enablePreciseRectForDynamicBackgroundForGameObjectMainIcon;
        private static bool enableCustomizationForGameObjectComponentIcons;
        private static bool enableTooltipOnComponentIconHovered;
        private static bool enableActiveStateEffectForComponentIcons;
        private static bool disableComponentIconsForInactiveGameObjects;
        private static bool useHierarchyTreeColorForInactiveGameObjects;
        private static bool includeBackgroundImageForGradientBackground;
        private static float componentIconsSize;
        private static int componentIconsOffset;
        private static float componentIconsSpacing;
        private static Color hierarchyTreeColor;
        private static HD_Settings.TreeBranchImageType treeBranchImageType_I;
        private static HD_Settings.TreeBranchImageType treeBranchImageType_L;
        private static HD_Settings.TreeBranchImageType treeBranchImageType_T;
        private static HD_Settings.TreeBranchImageType treeBranchImageType_TerminalBud;
        private static Color tagColor;
        private static TextAnchor tagTextAnchor;
        private static FontStyle tagFontStyle;
        private static int tagFontSize;
        private static Color layerColor;
        private static TextAnchor layerTextAnchor;
        private static FontStyle layerFontStyle;
        private static int layerFontSize;
        private static int tagLayerOffset;
        private static int tagLayerSpacing;
        private static Color hierarchyLineColor;
        private static int hierarchyLineThickness;
        private static int separatorLeftSideTextAnchorOffset;
        private static int separatorCenterTextAnchorOffset;
        private static int separatorRightSideTextAnchorOffset;
        private static Color lockColor;
        private static TextAnchor lockTextAnchor;
        private static FontStyle lockFontStyle;
        private static int lockFontSize;
        private static KeyCode toggleGameObjectActiveStateKeyCode;
        private static KeyCode toggleLockStateKeyCode;
        private static KeyCode changeTagLayerKeyCode;
        private static KeyCode renameSelectedGameObjectsKeyCode;
        private static KeyCode openIconPickerKeyCode;
        #endregion

        #region Data and Cache
        #region General
        private static int hierarchyChangeCount = 0;
        private static readonly int updateThreshold = 10;
        private static float currentViewWidth = 0f;
        #endregion

        #region GameObject Data
        internal struct GameObjectData
        {
            public Texture2D MainIcon { get; set; }
            public List<(Component component, Texture2D icon)> ComponentIcons { get; set; }
            public Texture2D HierarchyTreeIcon { get; set; }
            public string Tag { get; set; }
            public string Layer { get; set; }
        }
        internal static Dictionary<int, GameObjectData> gameObjectDataCache = new();
        #endregion

        #region Folder and Separator
        private static Dictionary<int, (Color textColor, int fontSize, FontStyle fontStyle, Color folderColor, HD_Folders.FolderImageType folderImageType)> folderCache = new();
        private static Dictionary<int, (Color textColor, bool isGradientBackground, Color backgroundColor, Gradient backgroundGradient, int fontSize, FontStyle fontStyle, TextAnchor textAnchor, HD_Separators.SeparatorImageType separatorImageType)> separatorCache = new();
        private static readonly Dictionary<int, Texture2D> folderBackgroundTextureCache = new();
        private static readonly Dictionary<int, Texture2D> separatorBackgroundTextureCache = new();
        private static readonly Dictionary<int, Texture2D> gradientTextureCache = new();
        #endregion
        #endregion
        #endregion

        #region Initialization
        public static void Initialize()
        {
            SubscribeToEvents();
        }

        private static void SubscribeToEvents()
        {
#if UNITY_6000_4_OR_NEWER
            EditorApplication.hierarchyWindowItemByEntityIdOnGUI -= OnHierarchyWindowItemGUI;
            EditorApplication.hierarchyWindowItemByEntityIdOnGUI += OnHierarchyWindowItemGUI;
#else
            EditorApplication.hierarchyWindowItemOnGUI -= OnHierarchyWindowItemGUI;
            EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyWindowItemGUI;
#endif
            EditorApplication.hierarchyChanged -= OnHierarchyChanged;
            EditorApplication.hierarchyChanged += OnHierarchyChanged;
            Editor.finishedDefaultHeaderGUI -= OnPostHeaderGUI;
            Editor.finishedDefaultHeaderGUI += OnPostHeaderGUI;
        }
        #endregion

        #region Events
#if UNITY_6000_4_OR_NEWER
        private static void OnHierarchyWindowItemGUI(EntityId instanceID, Rect selectionRect)
#else
        private static void OnHierarchyWindowItemGUI(int instanceID, Rect selectionRect)
#endif
        {
            #region Header
            if (HD_Editor.IsPlaying && disableEditorDesignerMajorOperationsDuringPlayMode) { return; }
            #if UNITY_6000_3_OR_NEWER
            GameObject gameObject = EditorUtility.EntityIdToObject(instanceID) as GameObject;
            #else
            GameObject gameObject = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
            #endif

            if (gameObject == null) 
            {
                if (enableHeaderUtilities) HD_Header.DrawHeader(selectionRect); 
                return;
            }
            #endregion

            #region Events
            currentViewWidth = EditorGUIUtility.currentViewWidth;
            Event currentEvent = Event.current;
            if (currentEvent.type != EventType.Repaint && !(gameObject.CompareTag(HD_Constants.SeparatorTag) && gameObject.name.StartsWith(HD_Constants.SeparatorPrefix)))
            {
                if (enableHierarchyButtons) { ProcessHierarchyButtons(gameObject, selectionRect); }
#if UNITY_6000_4_OR_NEWER
                if (enableGameObjectComponentIcons)
                {
#pragma warning disable CS0618
                    ProcessComponentIconsClick(gameObject, selectionRect, instanceID, currentEvent);
#pragma warning restore CS0618
                }
#else
                if (enableGameObjectComponentIcons) { ProcessComponentIconsClick(gameObject, selectionRect, instanceID, currentEvent); }
#endif

                if (enableMajorShortcuts)
                {
                    if (IsShortcutPressed(toggleGameObjectActiveStateKeyCode))
                    {
                        GameObject[] selectedGameObjects = Selection.gameObjects;
                        if (selectedGameObjects != null && selectedGameObjects.Length > 1)
                        {
                            ProcessToggleGameObjectActiveStateMajorShortcut(selectedGameObjects);
                        }
                        else
                        {
                            if (selectionRect.Contains(currentEvent.mousePosition)) { ProcessToggleGameObjectActiveStateMajorShortcut(new GameObject[] { gameObject }); }
                        }
                    }
                    if (IsShortcutPressed(toggleLockStateKeyCode))
                    {
                        GameObject[] selectedGameObjects = Selection.gameObjects;
                        if (selectedGameObjects != null && selectedGameObjects.Length > 1)
                        {
                            ProcessToggleGameObjectLockStateMajorShortcut(selectedGameObjects);
                        }
                        else
                        {
                            if (selectionRect.Contains(currentEvent.mousePosition)) { ProcessToggleGameObjectLockStateMajorShortcut(new GameObject[] { gameObject }); }
                        }
                    }
#if UNITY_6000_4_OR_NEWER
                    if (IsShortcutPressed(changeTagLayerKeyCode))
                    {
#pragma warning disable CS0618
                        ProcessTagLayerMajorShortcut(gameObject, selectionRect, instanceID);
#pragma warning restore CS0618
                    }
#else
                    if (IsShortcutPressed(changeTagLayerKeyCode)) { ProcessTagLayerMajorShortcut(gameObject, selectionRect, instanceID); }
#endif
                    if (IsShortcutPressed(renameSelectedGameObjectsKeyCode)) { ProcessRenameMajorShortcut(); }
                    if (IsShortcutPressed(openIconPickerKeyCode)) { ProcessOpenIconPickerMajorShortcut(gameObject, selectionRect, currentEvent); }

                }
                return;
            }
            #endregion

            #region Features
#if UNITY_6000_4_OR_NEWER
#pragma warning disable CS0618
#endif
            if (separatorCache.TryGetValue(gameObject.GetInstanceID(), out _) || (gameObject.CompareTag(HD_Constants.SeparatorTag) && gameObject.name.StartsWith(HD_Constants.SeparatorPrefix))) { DrawSeparator(gameObject, selectionRect, instanceID); return; }
            if (enableHierarchyRows) { DrawHierarchyRows(selectionRect); }
            if (enableGameObjectMainIcon) { DrawGameObjectMainIcon(gameObject, selectionRect, instanceID); }
            bool isFolder = folderCache.TryGetValue(instanceID, out _) || gameObject.GetComponent<HierarchyDesignerFolder>();
            if (isFolder) { DrawFolder(gameObject, selectionRect, instanceID); }
            if (enableHierarchyButtons) { DrawHierarchyButtons(gameObject, selectionRect); }
            if (enableHierarchyLines) { DrawHierarchyLines(gameObject, selectionRect); }
            if (enableHierarchyTree) { DrawHierarchyTree(gameObject, selectionRect, instanceID); }
            if ((gameObject.hideFlags & HideFlags.NotEditable) == HideFlags.NotEditable) { DrawGameObjectLock(gameObject, selectionRect); return; }
            if (isFolder && excludeFolderProperties) return;
            if (enableGameObjectComponentIcons) { DrawGameObjectComponentIcons(gameObject, selectionRect, instanceID); }
            if (enableGameObjectTag) { DrawGameObjectTag(gameObject, selectionRect, instanceID); }
            if (enableGameObjectLayer) { DrawGameObjectLayer(gameObject, selectionRect, instanceID); }
#if UNITY_6000_4_OR_NEWER
#pragma warning restore CS0618
#endif
            #endregion
        }

        private static void OnHierarchyChanged()
        {
            #region Header
            if ((EditorWindow.focusedWindow != null && EditorWindow.focusedWindow.titleContent.text != HD_Constants.HierarchyWindow) || HD_Editor.IsPlaying && disableEditorDesignerMajorOperationsDuringPlayMode) return;
            #endregion

            #region Features
            if (enableGameObjectMainIcon || enableGameObjectComponentIcons || enableHierarchyTree || enableGameObjectTag || enableGameObjectLayer)
            {
                hierarchyChangeCount++;
                if (hierarchyChangeCount >= updateThreshold)
                {
                    hierarchyChangeCount = 0;
                    #region Update GameObject Data Cache
                    List<int> keysToRemove = new();
                    foreach (int key in gameObjectDataCache.Keys)
                    {
#if UNITY_6000_4_OR_NEWER
#pragma warning disable CS0618
#endif
#if UNITY_6000_3_OR_NEWER
    if (EditorUtility.EntityIdToObject(key) == null)
#else
                        if (EditorUtility.InstanceIDToObject(key) == null)
#endif
#if UNITY_6000_4_OR_NEWER
#pragma warning restore CS0618
#endif
                        {
                            keysToRemove.Add(key);
                        }
                    }
                    foreach (int key in keysToRemove)
                    {
                        gameObjectDataCache.Remove(key);
                    }
                    #endregion
                }
            }
            #endregion
        }

        private static void OnPostHeaderGUI(Editor editor)
        {
            #region Header
            if (editor.target is not GameObject gameObject || HD_Editor.IsPlaying && disableEditorDesignerMajorOperationsDuringPlayMode) { return; }
            #endregion

            #region Features
            if ((gameObject.hideFlags & HideFlags.NotEditable) != HideFlags.NotEditable) return;
#if UNITY_6000_4_OR_NEWER
#pragma warning disable CS0618
#endif
            if (separatorCache.TryGetValue(gameObject.GetInstanceID(), out _) || (gameObject.CompareTag(HD_Constants.SeparatorTag) && gameObject.name.StartsWith(HD_Constants.SeparatorPrefix))) { EditorGUILayout.HelpBox(separatorMessage, MessageType.Info, true); }
#if UNITY_6000_4_OR_NEWER
#pragma warning restore CS0618
#endif
            else { EditorGUILayout.HelpBox(lockedGameObjectMessage, MessageType.Info, true); }
            #endregion
        }
        #endregion

        #region Methods
        #region Main Icon
        private static void DrawGameObjectMainIcon(GameObject gameObject, Rect selectionRect, int instanceID)
        {
            DrawBackground(selectionRect, instanceID);
            GUI.color = gameObject.activeInHierarchy ? activeColor : inactiveColor;
            string id = GlobalObjectId.GetGlobalObjectIdSlow(gameObject).ToString();
            if (HD_Icon.TryGetTexture(id, out Texture2D custom))
            {
                GUI.DrawTexture(new(selectionRect.x, selectionRect.y, selectionRect.height, defaultIconSelectionHeight), custom, ScaleMode.ScaleToFit, true);
            }
            else
            {
                GUI.DrawTexture(new(selectionRect.x, selectionRect.y, selectionRect.height, defaultIconSelectionHeight), DecideGameObjectMainIcon(gameObject, instanceID));
            }
            if (gameObject.transform.parent != null && PrefabUtility.IsPartOfPrefabInstance(gameObject.transform.parent.gameObject) && PrefabUtility.GetPrefabInstanceStatus(gameObject) == PrefabInstanceStatus.NotAPrefab) { GUI.DrawTexture(new(selectionRect.x, selectionRect.y, selectionRect.height, defaultIconSelectionHeight), prefabOverlayIcon); }
            GUI.color = activeColor;
        }

        private static void DrawBackground(Rect selectionRect, int instanceID)
        {
            EditorGUI.DrawRect(new(selectionRect.x, selectionRect.y, selectionRect.height, defaultIconSelectionHeight), SetBackgroundColorBasedOnState(selectionRect, instanceID));
        }

        private static Color SetBackgroundColorBasedOnState(Rect selectionRect, int instanceID)
        {
            bool isRow = enableHierarchyRows && ((int)(selectionRect.y / selectionRect.height) % 2 != 0);
            if (enableDynamicBackgroundForGameObjectMainIcon)
            {
                Rect finalSelectionRect = selectionRect;
                if (enablePreciseRectForDynamicBackgroundForGameObjectMainIcon)
                {
                    finalSelectionRect = new(mainIconBackgroundRectLeftOffset, selectionRect.yMin, selectionRect.width + mainIconBackgroundRectRightOffset + selectionRect.xMin, selectionRect.height);
                }

                bool isHovering = finalSelectionRect.Contains(Event.current.mousePosition);
#if UNITY_6000_4_OR_NEWER
#pragma warning disable CS0618
#endif
#if UNITY_6000_3_OR_NEWER
bool isSelected = Array.IndexOf(Selection.entityIds, instanceID) >= 0;
#else
                bool isSelected = Array.IndexOf(Selection.instanceIDs, instanceID) >= 0;
#endif
#if UNITY_6000_4_OR_NEWER
#pragma warning restore CS0618
#endif
                bool isHierarchyFocused = EditorWindow.focusedWindow != null && EditorWindow.focusedWindow.titleContent.text == HD_Constants.HierarchyWindow;

                if (isSelected && !isHierarchyFocused)
                {
                    return isRow ? HD_Color.GetHighlightedFocusedEditorRowColor() : HD_Color.GetHighlightedFocusedEditorColor();
                }
                else if (isSelected)
                {
                    return isRow ? HD_Color.GetSelectedEditorRowColor() : HD_Color.GetSelectedEditorColor();
                }
                else if (isHovering)
                {
                    return isRow ? HD_Color.GetHighlightedEditorRowColor() : HD_Color.GetHighlightedEditorColor();
                }
                return isRow ? HD_Color.GetDefaultEditorRowBackgroundColor() : HD_Color.GetDefaultEditorBackgroundColor();
            }
            return isRow ? HD_Color.GetDefaultEditorRowBackgroundColor() : HD_Color.GetDefaultEditorBackgroundColor();
        }

        private static Texture2D DecideGameObjectMainIcon(GameObject gameObject, int instanceID)
        {
            if (gameObjectDataCache.TryGetValue(instanceID, out GameObjectData data))
            {
                if(mainIconUpdateMode == HD_Settings.UpdateMode.Smart)
                {
                    return data.MainIcon;
                }
                else
                {
                    data.MainIcon = GetGameObjectMainIcon(gameObject);
                    gameObjectDataCache[instanceID] = data;
                    return data.MainIcon;
                }
            }
            else
            {
                Texture2D icon = GetGameObjectMainIcon(gameObject);
                data.MainIcon = icon;
                gameObjectDataCache[instanceID] = data;
                return icon;
            }
        }

        internal static Texture2D GetGameObjectMainIcon(GameObject gameObject)
        {
            Texture2D icon;
            Component[] components = gameObject.GetComponents<Component>();
            if (components.Length == 1)
            {
                icon = EditorGUIUtility.ObjectContent(components[0], typeTransform).image as Texture2D;
            }
            else
            {
                if (components[0] is not RectTransform)
                {
                    Component component = components[1];
                    if (component != null)
                    {
                        icon = EditorGUIUtility.ObjectContent(component, component.GetType()).image as Texture2D;
                    }
                    else
                    {
                        icon = EditorGUIUtility.FindTexture(warningIconTexture);
                    }
                }
                else
                {
                    icon = DetermineUIIcon(components);
                }
            }
            return icon;
        }

        private static Texture2D DetermineUIIcon(Component[] components)
        {
            if (components.Length < 2) return null;

            int mainIconIndex = -1;
            if (components.Length >= 4 && components[1] is CanvasRenderer && components[2] is UnityEngine.UI.Image)
            {
                mainIconIndex = 3;
            }
            else if (components.Length >= 3 && components[1] is CanvasRenderer)
            {
                mainIconIndex = 2;
            }
            else if (components.Length >= 2)
            {
                mainIconIndex = 1;
            }

            if (mainIconIndex != -1 && mainIconIndex < components.Length)
            {
                Component mainComponent = components[mainIconIndex];
                return mainComponent == null ? EditorGUIUtility.FindTexture(warningIconTexture) : EditorGUIUtility.ObjectContent(mainComponent, mainComponent.GetType())?.image as Texture2D;
            }
            return null;
        }

        private static void ProcessOpenIconPickerMajorShortcut(GameObject gameObject, Rect selectionRect, Event evt)
        {
            if (!enableGameObjectMainIcon) return;
            if ((gameObject.hideFlags & HideFlags.NotEditable) == HideFlags.NotEditable) return;
#if UNITY_6000_4_OR_NEWER
#pragma warning disable CS0618
#endif
            if (folderCache.TryGetValue(gameObject.GetInstanceID(), out _) || gameObject.GetComponent<HierarchyDesignerFolder>()) return;
#if UNITY_6000_4_OR_NEWER
#pragma warning restore CS0618
#endif

            Rect iconRect = GetMainIconRect(selectionRect);
            if (!iconRect.Contains(Event.current.mousePosition)) return;

            HD_IconOverride.Open(gameObject);

            if (evt != null)
            {
                evt.Use();
                GUIUtility.ExitGUI();
            }
        }

        private static Rect GetMainIconRect(Rect selectionRect)
        {
            float size = defaultIconSelectionHeight;
            float y = selectionRect.y + (selectionRect.height - size) * 0.5f;
            return new Rect(selectionRect.x, y, size, size);
        }
        #endregion

        #region Component Icons
        private static void DrawGameObjectComponentIcons(GameObject gameObject, Rect selectionRect, int instanceID)
        {
            if (disableComponentIconsForInactiveGameObjects && !gameObject.activeInHierarchy) { return; }

            List<(Component component, Texture2D icon)> componentIcons = DecideComponentIcons(gameObject, instanceID);

            float iconSizeMultiplier = defaultComponentIconsSize;
            float iconsSpacing = defaultComponentIconsSpacing;
            int iconAdditionalOffset = defaultComponentIconsOffset;
            if (enableCustomizationForGameObjectComponentIcons)
            {
                iconSizeMultiplier = componentIconsSize;
                iconsSpacing = componentIconsSpacing;
                iconAdditionalOffset = componentIconsOffset;
            }
            float iconSize = selectionRect.height * iconSizeMultiplier;
            float iconOffset = 0f;

            switch (layoutMode)
            {
                case HD_Settings.HierarchyLayoutMode.Consecutive:
                    float nameWidth = CalcWidthFast(GUI.skin.label, gameObject.name);
                    if (folderCache.TryGetValue(instanceID, out (Color textColor, int fontSize, FontStyle fontStyle, Color folderColor, HD_Folders.FolderImageType folderImageType) folderInfo))
                    {
                        GUIStyle folderLabelStyle = FolderStyle;
                        folderLabelStyle.fontSize = folderInfo.fontSize;
                        folderLabelStyle.fontStyle = folderInfo.fontStyle;
                        nameWidth = CalcWidthFast(folderLabelStyle, gameObject.name);
                    }
                    iconOffset = selectionRect.x + nameWidth + iconAdditionalOffset;
                    break;

                case HD_Settings.HierarchyLayoutMode.Split:
                    if (enableHierarchyButtons) { iconOffset += totalButtonsWidth + defaultXOffset; }
                    iconOffset += componentIcons.Count * (iconSize + iconsSpacing);
                    selectionRect.x = currentViewWidth - layoutBaseOffset - iconOffset;
                    iconOffset = selectionRect.x - componentIconsOffset + defaultComponentIconsOffset;
                    break;

                default:
                    if (enableHierarchyButtons) { iconOffset += totalButtonsWidth + defaultXOffset; }

                    bool hasData = gameObjectDataCache.TryGetValue(instanceID, out GameObjectData dataForOffsets);

                    if (enableGameObjectLayer)
                    {
                        string layer = hasData ? dataForOffsets.Layer : null;
                        if (string.IsNullOrEmpty(layer)) layer = DecideGameObjectLayer(gameObject, instanceID);

                        if (!excludedLayers.Contains(layer))
                        {
                            iconOffset += CalcWidthFast(LayerStyle, layer);
                        }
                    }

                    if (enableGameObjectTag)
                    {
                        string tag = hasData ? dataForOffsets.Tag : null;
                        if (string.IsNullOrEmpty(tag)) tag = DecideGameObjectTag(gameObject, instanceID);

                        if (!excludedTags.Contains(tag))
                        {
                            iconOffset += CalcWidthFast(TagStyle, tag);
                        }
                    }
                    if (enableGameObjectLayer || enableGameObjectTag) { iconOffset += tagLayerOffset + tagLayerSpacing; }

                    iconOffset += componentIcons.Count * (iconSize + iconsSpacing);
                    selectionRect.x = currentViewWidth - layoutBaseOffset - iconOffset;
                    iconOffset = selectionRect.x - componentIconsOffset + defaultComponentIconsOffset;
                    break;
            }

            foreach ((Component component, Texture2D icon) in componentIcons)
            {
                bool isComponentDisabled = false;
                if (enableActiveStateEffectForComponentIcons && component != null)
                {
                    if (component is Behaviour b) isComponentDisabled = !b.enabled;
                    else if (component is Renderer r) isComponentDisabled = !r.enabled;
                    else if (component is Collider c) isComponentDisabled = !c.enabled;
                    else if (component is Collider2D c2) isComponentDisabled = !c2.enabled;
                }

                GUI.color = (!gameObject.activeInHierarchy || isComponentDisabled) ? inactiveColor : activeColor;
                Rect iconRect = new(iconOffset, selectionRect.y + (selectionRect.height - iconSize) / 2, iconSize, iconSize);
                GUI.DrawTexture(iconRect, icon);

                if (enableTooltipOnComponentIconHovered)
                {
                    string tooltip = component != null ? component.GetType().Name : missingComponentMessage;
                    GUIContent iconContent = new(string.Empty, tooltip);
                    DrawComponentIconTooltip(iconOffset, iconRect, iconSize, iconContent);
                }

                iconOffset += iconSize + iconsSpacing;
            }
            GUI.color = activeColor;
        }

        private static List<(Component component, Texture2D icon)> DecideComponentIcons(GameObject gameObject, int instanceID)
        {
            if (gameObjectDataCache.TryGetValue(instanceID, out GameObjectData data))
            {
                if (componentsIconsUpdateMode == HD_Settings.UpdateMode.Smart)
                {
                    if (data.ComponentIcons == null)
                    {
                        data.ComponentIcons = GetComponentIcons(gameObject);
                        gameObjectDataCache[instanceID] = data;
                    }
                    return data.ComponentIcons;
                }
                else
                {
                    if (CheckComponentsChanged(gameObject, data.ComponentIcons))
                    {
                        data.ComponentIcons = GetComponentIcons(gameObject);
                        gameObjectDataCache[instanceID] = data;
                    }
                    return data.ComponentIcons;
                }
            }
            else
            {
                List<(Component component, Texture2D icon)> icons = GetComponentIcons(gameObject);
                GameObjectData newData = new()
                {
                    ComponentIcons = icons
                };
                gameObjectDataCache[instanceID] = newData;
                return icons;
            }
        }

        private static void ProcessComponentIconsClick(GameObject gameObject, Rect selectionRect, int instanceID, Event currentEvent)
        {
            if (currentEvent.type != EventType.MouseDown || currentEvent.button != 0 || !gameObject.activeInHierarchy || (gameObject.hideFlags & HideFlags.NotEditable) == HideFlags.NotEditable) return;

            List<(Component component, Texture2D icon)> componentIcons = DecideComponentIcons(gameObject, instanceID);
            if (componentIcons == null || componentIcons.Count == 0) return;

            float iconSizeMultiplier = enableCustomizationForGameObjectComponentIcons ? componentIconsSize : defaultComponentIconsSize;
            float iconsSpacing = enableCustomizationForGameObjectComponentIcons ? componentIconsSpacing : defaultComponentIconsSpacing;
            float iconSize = selectionRect.height * iconSizeMultiplier;
            int iconAdditionalOffset = enableCustomizationForGameObjectComponentIcons ? componentIconsOffset : defaultComponentIconsOffset;

            float iconOffset = CalculateComponentIconsOffset(gameObject, selectionRect, instanceID, componentIcons, iconSize, iconsSpacing, iconAdditionalOffset);
            float iconY = selectionRect.y + (selectionRect.height - iconSize) * 0.5f;

            foreach ((Component component, Texture2D icon) in componentIcons)
            {
                if (component == null)
                {
                    iconOffset += iconSize + iconsSpacing;
                    continue;
                }

                Rect iconRect = new(iconOffset, iconY, iconSize, iconSize);
                if (iconRect.Contains(currentEvent.mousePosition))
                {
                    HandleComponentIconClick(component);
                    currentEvent.Use();
                    GUIUtility.ExitGUI();
                    return;
                }
                iconOffset += iconSize + iconsSpacing;
            }
        }

        private static float CalculateComponentIconsOffset(GameObject gameObject, Rect selectionRect, int instanceID, List<(Component component, Texture2D icon)> componentIcons, float iconSize, float iconsSpacing, int iconAdditionalOffset)
        {
            float offset = 0f;
            switch (layoutMode)
            {
                case HD_Settings.HierarchyLayoutMode.Consecutive:
                    float nameWidth = CalcWidthFast(GUI.skin.label, gameObject.name);
                    if (folderCache.TryGetValue(instanceID, out var folderInfo))
                    {
                        GUIStyle folderLabelStyle = FolderStyle;
                        folderLabelStyle.fontSize = folderInfo.fontSize;
                        folderLabelStyle.fontStyle = folderInfo.fontStyle;
                        nameWidth = CalcWidthFast(folderLabelStyle, gameObject.name);
                    }
                    return selectionRect.x + nameWidth + iconAdditionalOffset;

                case HD_Settings.HierarchyLayoutMode.Split:
                    if (enableHierarchyButtons) offset += totalButtonsWidth + defaultXOffset;
                    offset += componentIcons.Count * (iconSize + iconsSpacing);
                    float newX = currentViewWidth - layoutBaseOffset - offset;
                    return newX - componentIconsOffset + defaultComponentIconsOffset;

                default:
                    if (enableHierarchyButtons) offset += totalButtonsWidth + defaultXOffset;
                    if (gameObjectDataCache.TryGetValue(instanceID, out var data))
                    {
                        if (enableGameObjectLayer && !excludedLayers.Contains(data.Layer)) offset += CalcWidthFast(LayerStyle, data.Layer);
                        if (enableGameObjectTag && !excludedTags.Contains(data.Tag)) offset += CalcWidthFast(TagStyle, data.Tag);
                    }
                    if (enableGameObjectLayer || enableGameObjectTag) offset += tagLayerOffset + tagLayerSpacing;
                    offset += componentIcons.Count * (iconSize + iconsSpacing);
                    float finalX = currentViewWidth - layoutBaseOffset - offset;
                    return finalX - componentIconsOffset + defaultComponentIconsOffset;
            }
        }

        private static void HandleComponentIconClick(Component component)
        {
            Vector2 mousePos = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);
            HD_Component.ShowForComponent(component, mousePos);
        }

        private static bool CheckComponentsChanged(GameObject gameObject, List<(Component component, Texture2D icon)> cachedIcons)
        {
            if (cachedIcons == null) return true;

            Component[] currentComponents = gameObject.GetComponents<Component>();
            if (currentComponents.Length != cachedIcons.Count) { return true; }

            for (int i = 0; i < currentComponents.Length; i++)
            {
                if (currentComponents[i] != cachedIcons[i].component)
                {
                    return true;
                }
            }
            return false;
        }

        internal static List<(Component component, Texture2D icon)> GetComponentIcons(GameObject gameObject)
        {
            List<(Component component, Texture2D icon)> icons = new();
            foreach (Component component in gameObject.GetComponents<Component>())
            {
                if (component == null)
                {
                    Texture2D warningIcon = EditorGUIUtility.FindTexture(warningIconTexture);
                    icons.Add((null, warningIcon));
                    continue;
                }

                if (excludedComponents.Contains(component.GetType().Name)) continue;

                Texture2D tex = EditorGUIUtility.ObjectContent(component, component.GetType()).image as Texture2D;
                if (!tex) tex = HD_Resources.Textures.DefaultTexture;
                
                icons.Add((component, tex));

                if (icons.Count >= maximumComponentIconsAmount)
                {
                    break;
                }
            }
            return icons;
        }

        private static void DrawComponentIconTooltip(float iconOffset, Rect selectionRect, float iconSize, GUIContent content)
        {
            Rect tooltipRect = new(iconOffset, selectionRect.y, iconSize, iconSize);
            GUI.Box(tooltipRect, content, GUIStyle.none);
        }
        #endregion

        #region Hierarchy Tree
        private static void DrawHierarchyTree(GameObject gameObject, Rect selectionRect, int instanceID)
        {
            if (gameObject.transform.parent == null) return;

            Transform transform = gameObject.transform;
            float selectionRectX = selectionRect.x - hierarchyTreeOffset;

            GUI.color = GetHierarchyTreeColor(gameObject.activeInHierarchy);
            GUI.DrawTexture(new(selectionRectX, selectionRect.y, selectionRect.height, selectionRect.height), DecideHierarchyTreeIcon(transform, instanceID), ScaleMode.ScaleToFit);
            DrawParentTreeFillLines(transform, selectionRectX, hierarchyTreeFillLinesOffset, selectionRect.height, selectionRect.y);
            GUI.color = activeColor;
        }

        private static Color GetHierarchyTreeColor(bool isActive)
        {
            if (isActive)
            {
                return hierarchyTreeColor;
            }

            if (!useHierarchyTreeColorForInactiveGameObjects)
            {
                return inactiveColor;
            }

            return HD_Color.GetDarkenedInactiveColor(hierarchyTreeColor, alphaValueForInactiveGameObjects);
        }

        private static Texture2D DecideHierarchyTreeIcon(Transform transform, int instanceID)
        {
            if (gameObjectDataCache.TryGetValue(instanceID, out GameObjectData data))
            {
                if (hierarchyTreeUpdateMode == HD_Settings.UpdateMode.Smart)
                {
                    if (data.HierarchyTreeIcon == null)
                    {
                        data.HierarchyTreeIcon = GetOrCreateBranchIcon(transform);
                        gameObjectDataCache[instanceID] = data;
                    }
                    return data.HierarchyTreeIcon;
                }
                else
                {
                    data.HierarchyTreeIcon = GetOrCreateBranchIcon(transform);
                    gameObjectDataCache[instanceID] = data;
                    return data.HierarchyTreeIcon;
                }
            }
            else
            {
                Texture2D icon = GetOrCreateBranchIcon(transform);
                data.HierarchyTreeIcon = icon;
                gameObjectDataCache[instanceID] = data;
                return icon;
            }
        }

        internal static Texture2D GetOrCreateBranchIcon(Transform transform)
        {
            Texture2D icon = null;
            switch (treeMode)
            {
                case HD_Settings.HierarchyTreeMode.Minimal:
                    icon = HD_Resources.Textures.TreeBranchIDefault;
                    break;

                case HD_Settings.HierarchyTreeMode.Default:
                    if (transform.GetSiblingIndex() == transform.parent.childCount - 1)
                    {
                        icon = transform.childCount > 0 ? HD_Resources.GetTreeBranchImageTypeL(treeBranchImageType_L) : HD_Resources.GetTreeBranchImageTypeTerminalBud(treeBranchImageType_TerminalBud);
                    }
                    else
                    {
                        icon = HD_Resources.GetTreeBranchImageTypeT(treeBranchImageType_T);
                    }
                    break;
            }
            return icon;
        }

        private static void DrawParentTreeFillLines(Transform transform, float rectX, float offsetX, float rectHeight, float rectY)
        {
            Transform parentTransform = transform.parent;
            while (parentTransform != null)
            {
                if (parentTransform.parent == null) break;
                rectX -= offsetX;
                bool isLastSibling = parentTransform.GetSiblingIndex() == parentTransform.parent.childCount - 1;
                if (!isLastSibling)
                {
                    GUI.DrawTexture(new(rectX, rectY, rectHeight, rectHeight), HD_Resources.GetTreeBranchImageTypeI(treeBranchImageType_I), ScaleMode.ScaleToFit);
                }
                parentTransform = parentTransform.parent;
            }
        }
        #endregion

        #region Tag and Layer
        private static void DrawGameObjectTag(GameObject gameObject, Rect selectionRect, int instanceID)
        {
            Rect tagRect = CalculateTagRect(gameObject, selectionRect, instanceID);
            if (tagRect == Rect.zero) return;

            GUI.color = gameObject.activeInHierarchy ? activeColor : inactiveColor;
            GUI.Label(tagRect, DecideGameObjectTag(gameObject, instanceID), tagStyle);
            GUI.color = activeColor;
        }

        private static void DrawGameObjectLayer(GameObject gameObject, Rect selectionRect, int instanceID)
        {
            Rect layerRect = CalculateLayerRect(gameObject, selectionRect, instanceID);
            if (layerRect == Rect.zero) return;

            GUI.color = gameObject.activeInHierarchy ? activeColor : inactiveColor;
            GUI.Label(layerRect, DecideGameObjectLayer(gameObject, instanceID), layerStyle);
            GUI.color = activeColor;
        }

        private static string DecideGameObjectTag(GameObject gameObject, int instanceID)
        {
            if (gameObjectDataCache.TryGetValue(instanceID, out GameObjectData data))
            {
                if (tagUpdateMode == HD_Settings.UpdateMode.Smart)
                {
                    if (data.Tag == null)
                    {
                        data.Tag = gameObject.tag;
                        gameObjectDataCache[instanceID] = data;
                    }
                    return data.Tag;
                }
                else
                {
                    data.Tag = gameObject.tag;
                    gameObjectDataCache[instanceID] = data;
                    return data.Tag;
                }
            }
            else
            {
                data.Tag = gameObject.tag;
                gameObjectDataCache[instanceID] = data;
                return data.Tag;
            }
        }

        private static string DecideGameObjectLayer(GameObject gameObject, int instanceID)
        {
            if (gameObjectDataCache.TryGetValue(instanceID, out GameObjectData data))
            {
                if (layerUpdateMode == HD_Settings.UpdateMode.Smart)
                {
                    if (data.Layer == null)
                    {
                        data.Layer = LayerMask.LayerToName(gameObject.layer);
                        gameObjectDataCache[instanceID] = data;
                    }
                    return data.Layer;
                }
                else
                {
                    data.Layer = LayerMask.LayerToName(gameObject.layer);
                    gameObjectDataCache[instanceID] = data;
                    return data.Layer;
                }
            }
            else
            {
                data.Layer = LayerMask.LayerToName(gameObject.layer);
                gameObjectDataCache[instanceID] = data;
                return data.Layer;
            }
        }

        private static Rect CalculateTagRect(GameObject gameObject, Rect selectionRect, int instanceID)
        {
            string tag = DecideGameObjectTag(gameObject, instanceID);
            if (excludedTags.Contains(tag)) return Rect.zero;

            float iconOffset = 0f;
            GUIStyle tagStyle = TagStyle;
            float tagLabelWidth = CalcWidthFast(tagStyle, tag);
            float nameWidth = CalcWidthFast(GUI.skin.label, gameObject.name);
            if (folderCache.TryGetValue(instanceID, out (Color textColor, int fontSize, FontStyle fontStyle, Color folderColor, HD_Folders.FolderImageType folderImageType) folderInfo))
            {
                GUIStyle folderLabelStyle = FolderStyle;
                folderLabelStyle.fontSize = folderInfo.fontSize;
                folderLabelStyle.fontStyle = folderInfo.fontStyle;
                nameWidth = CalcWidthFast(folderLabelStyle, gameObject.name);
            }

            bool hasData = gameObjectDataCache.TryGetValue(instanceID, out GameObjectData data);

            switch (layoutMode)
            {
                case HD_Settings.HierarchyLayoutMode.Docked:
                    if (enableHierarchyButtons) { iconOffset += totalButtonsWidth; }
                    if (enableGameObjectLayer)
                    {
                        string layer = hasData ? data.Layer : null;
                        if (string.IsNullOrEmpty(layer)) layer = DecideGameObjectLayer(gameObject, instanceID);

                        if (!excludedLayers.Contains(layer))
                        {
                            float layerLabelWidth = CalcWidthFast(LayerStyle, layer);
                            iconOffset += layerLabelWidth + tagLayerOffset + tagLayerSpacing;
                        }
                        else iconOffset += defaultXOffset;
                    }
                    selectionRect.x = currentViewWidth - tagLabelWidth - layoutBaseOffset - iconOffset;
                    iconOffset = selectionRect.x;
                    break;

                case HD_Settings.HierarchyLayoutMode.Split:
                    iconOffset = selectionRect.x + nameWidth + tagLayerOffset + splitModeOffset;
                    break;

                default:
                    float iconSizeMultiplier = enableCustomizationForGameObjectComponentIcons ? componentIconsSize : defaultComponentIconsSize;
                    float iconsSpacing = enableCustomizationForGameObjectComponentIcons ? componentIconsSpacing : defaultComponentIconsSpacing;
                    int iconAdditionalOffset = enableCustomizationForGameObjectComponentIcons ? componentIconsOffset : defaultComponentIconsOffset;
                    iconOffset = selectionRect.x + nameWidth + iconAdditionalOffset + tagLayerOffset;

                    List<(Component component, Texture2D icon)> componentIcons = hasData ? data.ComponentIcons : null;
                    if (enableGameObjectComponentIcons && componentIcons != null)
                    {
                        if (!(disableComponentIconsForInactiveGameObjects && !gameObject.activeInHierarchy))
                        {
                            float iconSize = selectionRect.height * iconSizeMultiplier;
                            iconOffset += componentIcons.Count * (iconSize + iconsSpacing);
                        }
                    }
                    break;
            }

            return new(iconOffset, selectionRect.y, tagLabelWidth, selectionRect.height);
        }

        private static Rect CalculateLayerRect(GameObject gameObject, Rect selectionRect, int instanceID)
        {
            string layer = DecideGameObjectLayer(gameObject, instanceID);
            if (excludedLayers.Contains(layer)) return Rect.zero;

            float iconOffset;
            GUIStyle layerStyle = LayerStyle;
            float layerLabelWidth = CalcWidthFast(layerStyle, layer);
            float nameWidth = CalcWidthFast(GUI.skin.label, gameObject.name);
            if (folderCache.TryGetValue(instanceID, out (Color textColor, int fontSize, FontStyle fontStyle, Color folderColor, HD_Folders.FolderImageType folderImageType) folderInfo))
            {
                GUIStyle folderLabelStyle = FolderStyle;
                folderLabelStyle.fontSize = folderInfo.fontSize;
                folderLabelStyle.fontStyle = folderInfo.fontStyle;
                nameWidth = CalcWidthFast(folderLabelStyle, gameObject.name);
            }

            bool hasData = gameObjectDataCache.TryGetValue(instanceID, out GameObjectData data);

            switch (layoutMode)
            {
                case HD_Settings.HierarchyLayoutMode.Docked:
                    if (enableHierarchyButtons) { selectionRect.x = currentViewWidth - totalButtonsWidth - layerLabelWidth - defaultXOffset; }
                    else { selectionRect.x = currentViewWidth - layerLabelWidth - defaultXOffset; }
                    iconOffset = selectionRect.x - tagLayerOffset - layoutBaseOffset;
                    break;

                case HD_Settings.HierarchyLayoutMode.Split:
                    iconOffset = selectionRect.x + nameWidth + tagLayerOffset + splitModeOffset;
                    if (enableGameObjectTag)
                    {
                        string tag = hasData ? data.Tag : null;
                        if (string.IsNullOrEmpty(tag)) tag = DecideGameObjectTag(gameObject, instanceID);

                        if (!excludedTags.Contains(tag))
                        {
                            float tagLabelWidth = CalcWidthFast(TagStyle, tag) + 2;
                            iconOffset += tagLabelWidth + tagLayerSpacing;
                        }
                    }
                    break;

                default:
                    float iconSizeMultiplier = enableCustomizationForGameObjectComponentIcons ? componentIconsSize : defaultComponentIconsSize;
                    float iconsSpacing = enableCustomizationForGameObjectComponentIcons ? componentIconsSpacing : defaultComponentIconsSpacing;
                    int iconAdditionalOffset = enableCustomizationForGameObjectComponentIcons ? componentIconsOffset : defaultComponentIconsOffset;
                    iconOffset = selectionRect.x + nameWidth + iconAdditionalOffset + tagLayerOffset;

                    List<(Component component, Texture2D icon)> componentIcons = hasData ? data.ComponentIcons : null;
                    if (enableGameObjectComponentIcons && componentIcons != null)
                    {
                        if (!(disableComponentIconsForInactiveGameObjects && !gameObject.activeInHierarchy))
                        {
                            float iconSize = selectionRect.height * iconSizeMultiplier;
                            iconOffset += componentIcons.Count * (iconSize + iconsSpacing);
                        }
                    }
                    if (enableGameObjectTag)
                    {
                        string tag = hasData ? data.Tag : null;
                        if (string.IsNullOrEmpty(tag)) tag = DecideGameObjectTag(gameObject, instanceID);

                        if (!excludedTags.Contains(tag))
                        {
                            float tagLabelWidth = CalcWidthFast(TagStyle, tag) + 2;
                            iconOffset += tagLabelWidth + tagLayerSpacing;
                        }
                    }
                    break;
            }

            return new(iconOffset, selectionRect.y, layerLabelWidth, selectionRect.height);
        }
        #endregion

        #region Hierarchy Rows
        private static void DrawHierarchyRows(Rect selectionRect)
        {
            if (((int)(selectionRect.y / selectionRect.height) & 1) == 0) return;
            selectionRect.x = hierarchyWindowOffsetLeft;
            selectionRect.width = currentViewWidth;
            EditorGUI.DrawRect(selectionRect, HD_Color.GetRowColor());
        }
        #endregion

        #region Hierarchy Lines
        private static void DrawHierarchyLines(GameObject gameObject, Rect selectionRect)
        {
            GUI.color = gameObject.activeInHierarchy ? hierarchyLineColor : ConvertColorToInactive(hierarchyLineColor, alphaValueForInactiveGameObjects);
            EditorGUI.DrawRect(new(hierarchyWindowOffsetLeft, selectionRect.y + selectionRect.height - hierarchyLineThickness, currentViewWidth, hierarchyLineThickness), GUI.color);
            GUI.color = activeColor;
        }
        #endregion

        #region Hierarchy Buttons
        private static void DrawHierarchyButtons(GameObject gameObject, Rect selectionRect)
        {
            float iconStartX = CalculateButtonsRect(gameObject, selectionRect);
            bool isActive = gameObject.activeSelf;
            bool isLocked = (gameObject.hideFlags & HideFlags.NotEditable) == HideFlags.NotEditable;

            GUI.color = isActive ? activeColor : inactiveColor;
            if (GUI.Button(new(iconStartX, selectionRect.y, buttonsWidth, selectionRect.height), isLocked ? HD_Resources.Icons.Lock : HD_Resources.Icons.Unlock, HD_GUI.HierarchyButtonLockStyle))
            {
                ToggleLockState(gameObject, !isLocked);
            }
            if (GUI.Button(new(iconStartX + buttonsWidth, selectionRect.y, buttonsWidth, selectionRect.height), isActive ? HD_Resources.Icons.VisibilityOn : HD_Resources.Icons.VisibilityOff, HD_GUI.HierarchyButtonVisibilityStyle))
            {
                ToggleActiveState(gameObject, !isActive);
            }
            GUI.color = activeColor;
        }

        private static float CalculateButtonsRect(GameObject gameObject, Rect selectionRect)
        {
            switch (layoutMode)
            {
                case HD_Settings.HierarchyLayoutMode.Consecutive:
                    float offsetX = selectionRect.x + 8f;
                    float nameWidth = CalcWidthFast(GUI.skin.label, gameObject.name);
#if UNITY_6000_4_OR_NEWER
#pragma warning disable CS0618
#endif
                    if (folderCache.TryGetValue(gameObject.GetInstanceID(), out (Color textColor, int fontSize, FontStyle fontStyle, Color folderColor, HD_Folders.FolderImageType folderImageType) folderInfo))
#if UNITY_6000_4_OR_NEWER
#pragma warning restore CS0618
#endif
                    {
                        GUIStyle folderLabelStyle = FolderStyle;
                        folderLabelStyle.fontSize = folderInfo.fontSize;
                        folderLabelStyle.fontStyle = folderInfo.fontStyle;
                        nameWidth = CalcWidthFast(folderLabelStyle, gameObject.name);
                    }
                    offsetX += nameWidth;

                    if ((gameObject.hideFlags & HideFlags.NotEditable) == HideFlags.NotEditable)
                    {
                        float lockLabelWidth = CalcWidthFast(LockStyle, lockedLabel);
                        return offsetX + layoutBaseOffset + lockLabelWidth + defaultXOffset;
                    }

#if UNITY_6000_4_OR_NEWER
#pragma warning disable CS0618
#endif
                    if ((folderCache.TryGetValue(gameObject.GetInstanceID(), out _) || gameObject.GetComponent<HierarchyDesignerFolder>()) && excludeFolderProperties) return offsetX += layoutBaseOffset;
#if UNITY_6000_4_OR_NEWER
#pragma warning restore CS0618
#endif

#if UNITY_6000_4_OR_NEWER
#pragma warning disable CS0618
#endif
                    int id = gameObject.GetInstanceID();
#if UNITY_6000_4_OR_NEWER
#pragma warning restore CS0618
#endif
                    bool hasData = gameObjectDataCache.TryGetValue(id, out GameObjectData data);

                    if (enableGameObjectComponentIcons)
                    {
                        float iconSizeMultiplier = defaultComponentIconsSize;
                        float iconsSpacing = defaultComponentIconsSpacing;
                        int iconAdditionalOffset = defaultComponentIconsOffset;
                        if (enableCustomizationForGameObjectComponentIcons)
                        {
                            iconSizeMultiplier = componentIconsSize;
                            iconsSpacing = componentIconsSpacing;
                            iconAdditionalOffset = componentIconsOffset;
                        }

                        if (!(disableComponentIconsForInactiveGameObjects && !gameObject.activeInHierarchy))
                        {
                            if (hasData && data.ComponentIcons != null)
                            {
                                int count = data.ComponentIcons.Count;
                                float iconSize = selectionRect.height * iconSizeMultiplier;
                                float iconsBlockW = (count > 0)
                                    ? count * (iconSize + iconsSpacing) + iconAdditionalOffset
                                    : iconAdditionalOffset;
                                offsetX += iconsBlockW;
                            }
                        }
                        else
                        {
                            offsetX += defaultComponentIconsOffset;
                        }
                    }
                    else
                    {
                        offsetX += 18f;
                    }

                    if (enableGameObjectTag && hasData)
                    {
                        string tag = data.Tag;
                        if (string.IsNullOrEmpty(tag)) tag = DecideGameObjectTag(gameObject, id);

                        if (!excludedTags.Contains(tag))
                            offsetX += CalcWidthFast(TagStyle, tag) + tagLayerOffset;
                    }

                    if (enableGameObjectLayer && hasData)
                    {
                        string layer = data.Layer;
                        if (string.IsNullOrEmpty(layer)) layer = DecideGameObjectLayer(gameObject, id);

                        if (!excludedLayers.Contains(layer))
                            offsetX += CalcWidthFast(LayerStyle, layer) + tagLayerSpacing;
                    }

                    return offsetX;

                default:
                    selectionRect.x = hierarchyWindowOffsetRight - layoutBaseOffset;
                    selectionRect.width = currentViewWidth;
                    return selectionRect.x + selectionRect.width;
            }
        }

        private static void ToggleLockState(GameObject gameObject, bool newState)
        {
            HD_Operations.LockGameObject(gameObject, newState);
        }

        private static void ToggleActiveState(GameObject gameObject, bool newState)
        {
            Undo.RecordObject(gameObject, "Toggle Active State");
            gameObject.SetActive(newState);
        }

        private static void ProcessHierarchyButtons(GameObject gameObject, Rect selectionRect)
        {
            GetButtonRects(gameObject, selectionRect, out Rect lockIconRect, out Rect activeToggleRect);

            bool isMouseDown = Event.current.type == EventType.MouseDown;
            if (isMouseDown && lockIconRect.Contains(Event.current.mousePosition))
            {
                ToggleLockState(gameObject, !HD_Operations.IsGameObjectLocked(gameObject));
                Event.current.Use();
            }
            else if (isMouseDown && activeToggleRect.Contains(Event.current.mousePosition))
            {
                ToggleActiveState(gameObject, !gameObject.activeSelf);
                Event.current.Use();
            }
        }

        private static void GetButtonRects(GameObject gameObject, Rect selectionRect, out Rect lockIconRect, out Rect activeToggleRect)
        {
            float iconStartX = CalculateButtonsRect(gameObject, selectionRect);
            lockIconRect = new(iconStartX, selectionRect.y, buttonsWidth, selectionRect.height);
            activeToggleRect = new(iconStartX + buttonsWidth, selectionRect.y, buttonsWidth, selectionRect.height);
        }
        #endregion

        #region Major Shortcuts
        private static bool IsShortcutPressed(KeyCode shortcutKey)
        {
            Event currentEvent = Event.current;
            if (shortcutKey >= KeyCode.Alpha0 && shortcutKey <= KeyCode.Menu) { return currentEvent.type == EventType.KeyDown && currentEvent.keyCode == shortcutKey; }
            int mouseButton = GetMouseButtonFromKeyCode(shortcutKey);
            if (mouseButton != -1) { return currentEvent.type == EventType.MouseDown && currentEvent.button == mouseButton; }
            return false;
        }

        private static int GetMouseButtonFromKeyCode(KeyCode keyCode)
        {
            switch (keyCode)
            {
                case KeyCode.Mouse0: return 0;
                case KeyCode.Mouse1: return 1;
                case KeyCode.Mouse2: return 2;
                case KeyCode.Mouse3: return 3;
                case KeyCode.Mouse4: return 4;
                case KeyCode.Mouse5: return 5;
                case KeyCode.Mouse6: return 6;
                default: return -1;
            }
        }

        private static void ProcessToggleGameObjectActiveStateMajorShortcut(GameObject[] gameObjects)
        {
            foreach (GameObject gameObject in gameObjects)
            {
                Undo.RecordObject(gameObject, "Toggle Active State");
                gameObject.SetActive(!gameObject.activeSelf);
            }
            Event.current.Use();
        }

        private static void ProcessToggleGameObjectLockStateMajorShortcut(GameObject[] gameObjects)
        {
            foreach (GameObject gameObject in gameObjects)
            {
                bool isLocked = (gameObject.hideFlags & HideFlags.NotEditable) == HideFlags.NotEditable;
                HD_Operations.SetLockState(gameObject, isLocked);
            }
            Event.current.Use();
        }

        private static void ProcessTagLayerMajorShortcut(GameObject gameObject, Rect selectionRect, int instanceID)
        {
            if (!enableGameObjectTag && !enableGameObjectLayer) return;

            Vector2 mousePosition = Event.current.mousePosition;
            if (enableGameObjectTag)
            {
                Rect tagRect = CalculateTagRect(gameObject, selectionRect, instanceID);
                if (tagRect.Contains(mousePosition))
                {
                    HD_Operations.HandleTagClick(gameObject, Event.current.mousePosition);
                    Event.current.Use();
                }
            }
            if (enableGameObjectLayer)
            {
                Rect layerRect = CalculateLayerRect(gameObject, selectionRect, instanceID);
                if (layerRect.Contains(mousePosition))
                {
                    HD_Operations.HandleLayerClick(gameObject, Event.current.mousePosition);
                    Event.current.Use();
                }
            }
        }

        private static void ProcessRenameMajorShortcut()
        {
            List<GameObject> selectedGameObjects = new(Selection.gameObjects);
            if (selectedGameObjects.Count < 1) { return; }
            HD_Rename.OpenWindow(selectedGameObjects, true, 0);
        }
        #endregion

        #region Lock State
        private static void DrawGameObjectLock(GameObject gameObject, Rect selectionRect)
        {
            GUIStyle lockStyle = LockStyle;
            float lockLabelWidth = CalcWidthFast(lockStyle, lockedLabel);
            float offset = 0;

            switch (layoutMode)
            {
                case HD_Settings.HierarchyLayoutMode.Docked:
                    if (enableHierarchyButtons) { selectionRect.x = currentViewWidth - totalButtonsWidth - lockLabelWidth - defaultXOffset - layoutBaseOffset; }
                    else { selectionRect.x = currentViewWidth - lockLabelWidth - layoutBaseOffset; }
                    break;

                default:
                    GUIContent nameContent = new(gameObject.name);
                    float nameWidth = CalcWidthFast(GUI.skin.label, gameObject.name);
#if UNITY_6000_4_OR_NEWER
#pragma warning disable CS0618
#endif
                    if (folderCache.TryGetValue(gameObject.GetInstanceID(), out (Color textColor, int fontSize, FontStyle fontStyle, Color folderColor, HD_Folders.FolderImageType folderImageType) folderInfo))
#if UNITY_6000_4_OR_NEWER
#pragma warning restore CS0618
#endif
                    {
                        GUIStyle folderLabelStyle = FolderStyle;
                        folderLabelStyle.fontSize = folderInfo.fontSize;
                        folderLabelStyle.fontStyle = folderInfo.fontStyle;
                        nameWidth = CalcWidthFast(folderLabelStyle, gameObject.name);
                    }
                    offset += nameWidth + layoutBaseOffset;
                    break;
            }

            Rect lockRect = new(selectionRect.x + offset, selectionRect.y, lockLabelWidth + defaultXOffset, selectionRect.height);
            GUI.color = gameObject.activeInHierarchy ? activeColor : inactiveColor;
            GUI.Label(lockRect, lockedLabel, lockStyle);
            GUI.color = activeColor;
        }
        #endregion

        #region Folder
        private static void DrawFolder(GameObject gameObject, Rect selectionRect, int instanceID)
        {
            if (!folderCache.TryGetValue(instanceID, out (Color textColor, int fontSize, FontStyle fontStyle, Color folderColor, HD_Folders.FolderImageType folderImageType) folderInfo))
            {
                HD_Folders.HD_FolderData folderData = HD_Folders.GetFolderData(gameObject.name) ?? new HD_Folders.HD_FolderData();
                folderInfo = (folderData.TextColor, folderData.FontSize, folderData.FontStyle, folderData.ImageColor, folderData.ImageType);
                folderCache[instanceID] = folderInfo;
            }
            if (!folderBackgroundTextureCache.TryGetValue(instanceID, out Texture2D folderBackgroundTexture))
            {
                folderBackgroundTexture = EditorGUIUtility.whiteTexture;
                folderBackgroundTextureCache[instanceID] = folderBackgroundTexture;
            }

            int depth = 0;
            for (Transform parent = gameObject.transform.parent; parent != null; parent = parent.parent) depth++;
            float depthOffset = depth * folderChildIndent;

            GUI.color = SetBackgroundColorBasedOnState(selectionRect, instanceID);
            GUI.DrawTexture(new(selectionRect.x, selectionRect.y, currentViewWidth - currentViewWidthOffset - depthOffset, selectionRect.height), folderBackgroundTexture);
            GUI.color = activeColor;

            Texture2D folderIcon = HD_Resources.GetFolderImageType(folderInfo.folderImageType);
            GUI.color = gameObject.activeInHierarchy ? folderInfo.folderColor : ConvertColorToInactive(folderInfo.folderColor, alphaValueForInactiveGameObjects);
            GUI.DrawTexture(new(selectionRect.x, selectionRect.y, selectionRect.height, selectionRect.height), folderIcon);
            GUI.color = activeColor;

            selectionRect.width = currentViewWidth;

            GUIStyle folderStyle = FolderStyle;
            folderStyle.fontSize = folderInfo.fontSize;
            folderStyle.fontStyle = folderInfo.fontStyle;
            folderStyle.normal.textColor = gameObject.activeInHierarchy ? folderInfo.textColor : ConvertColorToInactive(folderInfo.textColor, alphaValueForInactiveGameObjects);
 
            Rect labelRect = new(selectionRect.x + defaultIconSelectionHeight, selectionRect.y, selectionRect.width - defaultIconSelectionHeight, selectionRect.height);
            GUI.Label(labelRect, gameObject.name, folderStyle);
        }

        public static void ClearFolderCache()
        {
            folderCache.Clear();
            folderBackgroundTextureCache.Clear();
            EditorApplication.RepaintHierarchyWindow();
        }
        #endregion

        #region Separator
        private static void DrawSeparator(GameObject gameObject, Rect selectionRect, int instanceID)
        {
            string separatorKey = gameObject.name.Replace(HD_Constants.SeparatorPrefix, "").Trim();
            if (!separatorCache.TryGetValue(instanceID, out (Color textColor, bool isGradientBackground, Color backgroundColor, Gradient backgroundGradient, int fontSize, FontStyle fontStyle, TextAnchor textAnchor, HD_Separators.SeparatorImageType separatorImageType) separatorInfo))
            {
                HD_Separators.HD_SeparatorData separatorData = HD_Separators.GetSeparatorData(separatorKey) ?? new HD_Separators.HD_SeparatorData();
                separatorInfo = (separatorData.TextColor, separatorData.IsGradientBackground, separatorData.BackgroundColor, separatorData.BackgroundGradient, separatorData.FontSize, separatorData.FontStyle, separatorData.TextAnchor, separatorData.ImageType);
                separatorCache[instanceID] = separatorInfo;
            }

            if (!separatorBackgroundTextureCache.TryGetValue(instanceID, out Texture2D separatorBackgroundTexture))
            {
                separatorBackgroundTexture = EditorGUIUtility.whiteTexture;
                separatorBackgroundTextureCache[instanceID] = separatorBackgroundTexture;
            }
            GUI.color = SetBackgroundColorBasedOnState(selectionRect, instanceID);
            GUI.DrawTexture(new Rect(hierarchyWindowOffsetLeft, selectionRect.y, currentViewWidth, selectionRect.height), separatorBackgroundTexture);
            GUI.color = activeColor;

            selectionRect.x = hierarchyWindowOffsetLeft;
            selectionRect.width = currentViewWidth;

            GUIStyle textStyle = new()
            {
                alignment = separatorInfo.textAnchor,
                fontSize = separatorInfo.fontSize,
                fontStyle = separatorInfo.fontStyle,
                normal = { textColor = separatorInfo.textColor }
            };

            Texture2D backgroundTexture = HD_Resources.GetSeparatorImageType(separatorInfo.separatorImageType);

            if (separatorInfo.isGradientBackground)
            {
                if (!gradientTextureCache.TryGetValue(instanceID, out Texture2D gradientTexture) || gradientTexture == null)
                {
                    gradientTexture = CreateGradientTexture(separatorInfo.backgroundGradient, Mathf.FloorToInt(selectionRect.width));
                    gradientTextureCache[instanceID] = gradientTexture;
                }
                if (includeBackgroundImageForGradientBackground) { GUI.DrawTexture(selectionRect, backgroundTexture); }
                GUI.DrawTexture(selectionRect, gradientTexture);
            }
            else
            {
                GUI.color = separatorInfo.backgroundColor;
                GUI.DrawTexture(selectionRect, backgroundTexture);
            }
            GUI.color = activeColor;

            Rect textRect = AdjustRect(selectionRect, separatorInfo.textAnchor);
            GUI.Label(textRect, separatorKey, textStyle);
        }

        private static Texture2D CreateGradientTexture(Gradient gradient, int width)
        {
            Texture2D gradientTexture = new(width, 1, TextureFormat.ARGB32, false);
            for (int x = 0; x < width; x++)
            {
                float t = (float)x / (width - 1);
                Color color = gradient.Evaluate(t);
                gradientTexture.SetPixel(x, 0, color);
            }
            gradientTexture.Apply();
            return gradientTexture;
        }

        private static Rect AdjustRect(Rect rect, TextAnchor textAlignment)
        {
            switch (textAlignment)
            {     
                case TextAnchor.MiddleLeft:
                case TextAnchor.UpperLeft:
                case TextAnchor.LowerLeft:
                    rect.x += separatorLeftSideTextAnchorOffset;
                    break;
                case TextAnchor.MiddleCenter:
                case TextAnchor.UpperCenter:
                case TextAnchor.LowerCenter:
                    rect.x += separatorCenterTextAnchorOffset;
                    break;
                case TextAnchor.MiddleRight:
                case TextAnchor.UpperRight:
                case TextAnchor.LowerRight:
                    rect.x -= separatorRightSideTextAnchorOffset;
                    break;
            }
            return rect;
        }

        public static void ClearSeparatorCache()
        {
            separatorCache.Clear();
            separatorBackgroundTextureCache.Clear();
            ClearGradientTextureCache();
            EditorApplication.RepaintHierarchyWindow();
        }

        private static void ClearGradientTextureCache()
        {
            foreach (Texture2D texture in gradientTextureCache.Values)
            {
                GameObject.DestroyImmediate(texture);
            }
            gradientTextureCache.Clear();
        }
        #endregion
        #endregion

        #region Operations
        private static float CalcWidthFast(GUIStyle style, string text)
        {
            tempContent.text = text;
            return style.CalcSize(tempContent).x;
        }

        public static void ClearGameObjectDataCache()
        {
            gameObjectDataCache.Clear();
        }

        private static Color ConvertColorToInactive(Color color, float alphaFactor)
        {
            alphaFactor = Mathf.Clamp01(alphaFactor);
            return new(color.r, color.g, color.b, color.a * alphaFactor);
        }
        #endregion

        #region Cache Setters
        public static HD_Settings.HierarchyLayoutMode LayoutModeCache
        {
            set
            {
                layoutMode = value;
            }
        }

        public static HD_Settings.HierarchyTreeMode TreeModeCache
        {
            set
            {
                treeMode = value;
            }
        }

        public static bool EnableGameObjectMainIconCache
        {
            set
            {
                enableGameObjectMainIcon = value;
            }
        }

        public static bool EnableGameObjectComponentIconsCache
        {
            set
            {
                enableGameObjectComponentIcons = value;
            }
        }

        public static bool EnableHierarchyTreeCache
        {
            set
            {
                enableHierarchyTree = value;
            }
        }

        public static bool EnableGameObjectTagCache
        {
            set
            {
                enableGameObjectTag = value;
            }
        }

        public static bool EnableGameObjectLayerCache
        {
            set
            {
                enableGameObjectLayer = value;
            }
        }

        public static bool EnableHierarchyRowsCache
        {
            set
            {
                enableHierarchyRows = value;
            }
        }

        public static bool EnableHierarchyLinesCache
        {
            set
            {
                enableHierarchyLines = value;
            }
        }

        public static bool EnableHierarchyButtonsCache
        {
            set
            {
                enableHierarchyButtons = value;
            }
        }

        public static bool EnableHeaderUtilitiesrCache
        {
            set
            {
                enableHeaderUtilities = value;
            }
        }

        public static bool EnableMajorShortcutsCache
        {
            set
            {
                enableMajorShortcuts = value;
            }
        }

        public static bool DisableHierarchyDesignerDuringPlayModeCache
        {
            set
            {
                disableEditorDesignerMajorOperationsDuringPlayMode = value;
            }
        }

        public static bool ExcludeFolderProperties
        {
            set
            {
                excludeFolderProperties = value;
            }
        }

        public static List<string> ExcludedComponentsCache
        {
            set
            {
                excludedComponents = value;
            }
        }

        public static int MaximumComponentIconsAmountCache
        {
            set
            {
                maximumComponentIconsAmount = value;
            }
        }

        public static List<string> ExcludedTagsCache
        {
            set
            {
                excludedTags = value;
            }
        }

        public static List<string> ExcludedLayersCache
        {
            set
            {
                excludedLayers = value;
            }
        }

        public static HD_Settings.UpdateMode MainIconUpdateModeCache
        {
            set
            {
                mainIconUpdateMode = value;
            }
        }

        public static HD_Settings.UpdateMode ComponentsIconsUpdateModeCache
        {
            set
            {
                componentsIconsUpdateMode = value;
            }
        }

        public static HD_Settings.UpdateMode HierarchyTreeUpdateModeCache
        {
            set
            {
                hierarchyTreeUpdateMode = value;
            }
        }

        public static HD_Settings.UpdateMode TagUpdateModeCache
        {
            set
            {
                tagUpdateMode = value;
            }
        }

        public static HD_Settings.UpdateMode LayerUpdateModeCache
        {
            set
            {
                layerUpdateMode = value;
            }
        }

        public static bool EnableDynamicBackgroundForGameObjectMainIconCache
        {
            set
            {
                enableDynamicBackgroundForGameObjectMainIcon = value;
            }
        }

        public static bool EnablePreciseRectForDynamicBackgroundForGameObjectMainIconCache
        {
            set
            {
                enablePreciseRectForDynamicBackgroundForGameObjectMainIcon = value;
            }
        }

        public static bool EnableCustomizationForGameObjectComponentIconsCache
        {
            set
            {
                enableCustomizationForGameObjectComponentIcons = value;
            }
        }

        public static bool EnableTooltipOnComponentIconHoveredCache
        {
            set
            {
                enableTooltipOnComponentIconHovered = value;
            }
        }

        public static bool EnableActiveStateEffectForComponentIconsCache
        {
            set
            {
                enableActiveStateEffectForComponentIcons = value;
            }
        }

        public static bool DisableComponentIconsForInactiveGameObjectsCache
        {
            set
            {
                disableComponentIconsForInactiveGameObjects = value;
            }
        }

        public static bool UseHierarchyTreeColorForInactiveGameObjectsCache
        {
            set
            {
                useHierarchyTreeColorForInactiveGameObjects = value;
            }
        }

        public static bool IncludeBackgroundImageForGradientBackgroundCache
        {
            set
            {
                includeBackgroundImageForGradientBackground = value;
            }
        }

        public static float ComponentIconsSizeCache
        {
            set
            {
                componentIconsSize = value;
            }
        }

        public static int ComponentIconsOffsetCache
        {
            set
            {
                componentIconsOffset = value;
            }
        }

        public static float ComponentIconsSpacingCache
        {
            set
            {
                componentIconsSpacing = value;
            }
        }

        public static Color HierarchyTreeColorCache
        {
            set
            {
                hierarchyTreeColor = value;
            }
        }

        public static HD_Settings.TreeBranchImageType TreeBranchImageType_ICache
        {
            set
            {
                treeBranchImageType_I = value;
            }
        }

        public static HD_Settings.TreeBranchImageType TreeBranchImageType_LCache
        {
            set
            {
                treeBranchImageType_L = value;
            }
        }

        public static HD_Settings.TreeBranchImageType TreeBranchImageType_TCache
        {
            set
            {
                treeBranchImageType_T = value;
            }
        }

        public static HD_Settings.TreeBranchImageType TreeBranchImageType_TerminalBudCache
        {
            set
            {
                treeBranchImageType_TerminalBud = value;
            }
        }

        public static Color TagColorCache
        {
            set
            {
                tagColor = value;
                if (tagStyle != null)
                {
                    tagStyle.normal.textColor = tagColor;
                }
            }
        }

        public static TextAnchor TagTextAnchorCache
        {
            set
            {
                tagTextAnchor = value;
                if (tagStyle != null)
                {
                    tagStyle.alignment = tagTextAnchor;
                }
            }
        }

        public static FontStyle TagFontStyleCache
        {
            set
            {
                tagFontStyle = value;
                if (tagStyle != null)
                {
                    tagStyle.fontStyle = tagFontStyle;
                }
            }
        }

        public static int TagFontSizeCache
        {
            set
            {
                tagFontSize = value;
                if (tagStyle != null)
                {
                    tagStyle.fontSize = tagFontSize;
                }
            }
        }

        public static Color LayerColorCache
        {
            set
            {
                layerColor = value;
                if (layerStyle != null)
                {
                    layerStyle.normal.textColor = layerColor;
                }
            }
        }

        public static TextAnchor LayerTextAnchorCache
        {
            set
            {
                layerTextAnchor = value;
                if (layerStyle != null)
                {
                    layerStyle.alignment = layerTextAnchor;
                }
            }
        }

        public static FontStyle LayerFontStyleCache
        {
            set
            {
                layerFontStyle = value;
                if (layerStyle != null)
                {
                    layerStyle.fontStyle = layerFontStyle;
                }
            }
        }

        public static int LayerFontSizeCache
        {
            set
            {
                layerFontSize = value;
                if (layerStyle != null)
                {
                    layerStyle.fontSize = layerFontSize;
                }
            }
        }

        public static int TagLayerOffsetCache
        {
            set
            {
                tagLayerOffset = value;
            }
        }

        public static int TagLayerSpacingCache
        {
            set
            {
                tagLayerSpacing = value;
            }
        }

        public static Color HierarchyLineColorCache
        {
            set
            {
                hierarchyLineColor = value;
            }
        }

        public static int HierarchyLineThicknessCache
        {
            set
            {
                hierarchyLineThickness = value;
            }
        }

        public static int SeparatorLeftSideTextAnchorOffsetCache
        {
            set
            {
                separatorLeftSideTextAnchorOffset = value;
            }
        }

        public static int SeparatorCenterTextAnchorOffsetCache
        {
            set
            {
                separatorCenterTextAnchorOffset = value;
            }
        }

        public static int SeparatorRightSideTextAnchorOffsetCache
        {
            set
            {
                separatorRightSideTextAnchorOffset = value;
            }
        }

        public static Color LockColorCache
        {
            set
            {
                lockColor = value;
                if (lockStyle != null)
                {
                    lockStyle.normal.textColor = lockColor;
                }
            }
        }

        public static TextAnchor LockTextAnchorCache
        {
            set
            {
                lockTextAnchor = value;
                if (lockStyle != null)
                {
                    lockStyle.alignment = lockTextAnchor;
                }
            }
        }

        public static FontStyle LockFontStyleCache
        {
            set
            {
                lockFontStyle = value;
                if (lockStyle != null)
                {
                    lockStyle.fontStyle = lockFontStyle;
                }
            }
        }

        public static int LockFontSizeCache
        {
            set
            {
                lockFontSize = value;
                if (lockStyle != null)
                {
                    lockStyle.fontSize = lockFontSize;
                }
            }
        }

        public static KeyCode ToggleGameObjectActiveStateKeyCodeCache
        {
            set
            {
                toggleGameObjectActiveStateKeyCode = value;
            }
        }

        public static KeyCode ToggleLockStateKeyCodeCache
        {
            set
            {
                toggleLockStateKeyCode = value;
            }
        }

        public static KeyCode ChangeTagLayerKeyCodeCache
        {
            set
            {
                changeTagLayerKeyCode = value;
            }
        }

        public static KeyCode RenameSelectedGameObjectsKeyCodeCache
        {
            set
            {
                renameSelectedGameObjectsKeyCode = value;
            }
        }

        public static KeyCode OpenIconPickerKeyCodeCache
        {
            set
            { 
                openIconPickerKeyCode = value; 
            }
        }

        public static Dictionary<int, (Color textColor, int fontSize, FontStyle fontStyle, Color folderColor, HD_Folders.FolderImageType folderImageType)> FolderCache
        {
            set
            {
                folderCache = value;
            }
        }

        public static Dictionary<int, (Color textColor, bool isGradientBackground, Color backgroundColor, Gradient backgroundGradient, int fontSize, FontStyle fontStyle, TextAnchor textAnchor, HD_Separators.SeparatorImageType separatorImageType)> SeparatorCache
        {
            set
            {
                separatorCache = value;
            }
        }
        #endregion
    }
}
#endif