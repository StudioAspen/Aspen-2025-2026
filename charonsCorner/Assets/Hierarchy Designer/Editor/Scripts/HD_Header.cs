#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HierarchyDesigner
{
    internal static class HD_Header
    {
        #region Properties
        #region Scene
        private struct SceneItem { public string guid, path, name; }

        [Serializable]
        private class FavoriteGuidList { public List<string> items = new(); }
        private static readonly HashSet<string> s_favorites = new(StringComparer.Ordinal);

        private const float Gap = 2f;
        private const float DirtyStarW = 8f; 
        private const float DirtyStarPad = 2f;
        private const float ArrowSize = 16f;
        private static readonly GUIContent ArrowGC = new("▾", "Open scene switcher");
        private const float MinSearchWidth = 235f;

        private static readonly List<SceneItem> s_allScenes = new();
        private static string s_search = "";

        private struct RowAnchor { public float yMin, yMax; public int sceneIndex; }
        private static readonly List<RowAnchor> s_rowAnchors = new();
        private static int s_sceneHeaderSeqThisFrame = 0;
        #endregion

        #region Collapse/Expand
        private const float ToggleSize = 16f;
        private const float RightPadForContext = 5f;
        private static readonly GUIContent CollapseExpandGC = new("↕", "Collapse/Expand All");
        #endregion

        #region Remove Missing Scripts
        private static readonly GUIContent RemoveMissingGC = new("X", "Remove Missing Scripts");
        #endregion
        #endregion

        #region Initialization
        public static void Initialize()
        {
            Load();
            RefreshCache();

            EditorApplication.projectChanged -= RefreshCache;
            EditorApplication.projectChanged += RefreshCache;
        }
        #endregion

        #region Methods
        public static void DrawHeader(Rect selectionRect)
        {
            Event evt = Event.current;

            if (evt.type == EventType.Layout)
            {
                s_rowAnchors.Clear();
                s_sceneHeaderSeqThisFrame = 0;
            }

            int sceneIndex = -1;

            if (evt.type == EventType.Repaint)
            {
                sceneIndex = Mathf.Clamp(s_sceneHeaderSeqThisFrame, 0, SceneManager.sceneCount - 1);
                s_rowAnchors.Add(new RowAnchor
                {
                    yMin = selectionRect.y,
                    yMax = selectionRect.y + selectionRect.height,
                    sceneIndex = sceneIndex
                });
                s_sceneHeaderSeqThisFrame++;
            }
            else
            {
                float y = selectionRect.y;
                const float tol = 0.5f;
                for (int i = 0; i < s_rowAnchors.Count; i++)
                {
                    RowAnchor a = s_rowAnchors[i];
                    if (y >= a.yMin - tol && y <= a.yMax + tol) { sceneIndex = a.sceneIndex; break; }
                }

                if (sceneIndex < 0) sceneIndex = Mathf.Max(0, SceneManager.GetActiveScene().buildIndex);
                if (sceneIndex < 0 || sceneIndex >= SceneManager.sceneCount) sceneIndex = 0;
            }

            if (SceneManager.sceneCount == 0) return;
            string sceneName = SceneManager.GetSceneAt(sceneIndex).name;
            if (string.IsNullOrEmpty(sceneName)) return;

            Rect arrowRect = ComputeSceneArrowRect(selectionRect, sceneIndex);

            Color prev = GUI.color;
            GUI.color = GUI.skin.label.normal.textColor;
            GUI.Label(arrowRect, ArrowGC);
            GUI.color = prev;

            if (GUI.Button(arrowRect, GUIContent.none, GUIStyle.none))
            {
                float popupW = ScenePopup.ComputePopupWidth();
                PopupWindow.Show(arrowRect, new ScenePopup(popupW));
                evt.Use();
                GUIUtility.ExitGUI();
            }

            Rect collapseRect = ComputeCollapseExpandRect(selectionRect);
            Rect removeMissingRect = ComputeRemoveMissingRect(selectionRect);

            prev = GUI.color;
            GUI.color = GUI.skin.label.normal.textColor;
            GUI.Label(removeMissingRect, RemoveMissingGC);
            GUI.Label(collapseRect, CollapseExpandGC);
            GUI.color = prev;

            if (GUI.Button(removeMissingRect, GUIContent.none, GUIStyle.none))
            {
                RemoveMissingScripts(sceneIndex, evt.shift);

                evt.Use();
                EditorApplication.RepaintHierarchyWindow();
                GUIUtility.ExitGUI();
            }

            if (GUI.Button(collapseRect, GUIContent.none, GUIStyle.none))
            {
                bool expand = !HD_Operations.IsHierarchyMostlyExpanded();
                if (expand) HD_Operations.ExpandAllGameObjects();
                else HD_Operations.CollapseAllGameObjects();

                evt.Use();
                EditorApplication.RepaintHierarchyWindow();
                GUIUtility.ExitGUI();
            }
        }

        private static Rect ComputeSceneArrowRect(Rect row, int sceneIndex)
        {
            Scene scene = (sceneIndex >= 0 && sceneIndex < SceneManager.sceneCount) ? SceneManager.GetSceneAt(sceneIndex) : SceneManager.GetActiveScene();

            bool isActive = scene == SceneManager.GetActiveScene();
            bool isDirty = scene.isDirty;

            float iconW = row.height;
            float nameStartX = row.x + iconW + Gap;

            GUIContent gc = new(scene.name);
            GUIStyle labelStyle = isActive ? EditorStyles.boldLabel : EditorStyles.label;

            float measuredNameW = labelStyle.CalcSize(gc).x;
            if (isDirty) measuredNameW += DirtyStarPad + DirtyStarW;

            float reservedRight = RightPadForContext + ToggleSize + Gap + ToggleSize + 2f;
            float maxArrowX = Mathf.Round(row.xMax - reservedRight - Gap - ArrowSize);

            float availableNameW = Mathf.Max(0f, maxArrowX - (nameStartX + Gap));
            float visibleNameW = Mathf.Min(measuredNameW, availableNameW);

            float arrowX = Mathf.Round(nameStartX + visibleNameW + Gap);
            float arrowY = Mathf.Round(row.y + (row.height - ArrowSize) * 0.5f);
            arrowX = Mathf.Min(arrowX, maxArrowX);

            return new Rect(arrowX, arrowY, ArrowSize, ArrowSize);
        }

        private static Rect ComputeRemoveMissingRect(Rect row)
        {
            float x = Mathf.Round(row.xMax - RightPadForContext - ToggleSize - Gap - ToggleSize);
            float y = Mathf.Round(row.y + (row.height - ToggleSize) * 0.5f);
            return new Rect(x, y, ToggleSize, ToggleSize);
        }

        private static Rect ComputeCollapseExpandRect(Rect row)
        {
            float x = Mathf.Round(row.xMax - RightPadForContext - ToggleSize);
            float y = Mathf.Round(row.y + (row.height - ToggleSize) * 0.5f);
            return new Rect(x, y, ToggleSize, ToggleSize);
        }

        private static void RemoveMissingScripts(int sceneIndex, bool wholeScene)
        {
            Scene scene = (sceneIndex >= 0 && sceneIndex < SceneManager.sceneCount) ? SceneManager.GetSceneAt(sceneIndex) : SceneManager.GetActiveScene();

            int removed = 0;

            if (wholeScene)
            {
                GameObject[] roots = scene.GetRootGameObjects();
                for (int i = 0; i < roots.Length; i++)
                    removed += RemoveMissingScriptsInHierarchy(roots[i]);
            }
            else
            {
                GameObject[] selection = Selection.gameObjects;
                if (selection == null || selection.Length == 0)
                {
                    GameObject[] roots = scene.GetRootGameObjects();
                    for (int i = 0; i < roots.Length; i++)
                        removed += RemoveMissingScriptsInHierarchy(roots[i]);
                }
                else
                {
                    HashSet<GameObject> unique = new HashSet<GameObject>();
                    for (int i = 0; i < selection.Length; i++)
                    {
                        GameObject go = selection[i];
                        if (go == null) continue;
                        if (go.scene != scene) continue;
                        unique.Add(go);
                    }

                    if (unique.Count == 0)
                    {
                        GameObject[] roots = scene.GetRootGameObjects();
                        for (int i = 0; i < roots.Length; i++)
                            removed += RemoveMissingScriptsInHierarchy(roots[i]);
                    }
                    else
                    {
                        foreach (GameObject go in unique)
                            removed += RemoveMissingScriptsInHierarchy(go);
                    }
                }
            }

            if (removed > 0)
            {
                EditorSceneManager.MarkSceneDirty(scene);
                Debug.Log("Hierarchy Designer: Removed " + removed + " missing scripts.");
            }
            else
            {
                Debug.Log("Hierarchy Designer: No missing scripts found.");
            }
        }

        private static int RemoveMissingScriptsInHierarchy(GameObject root)
        {
            if (root == null) return 0;

            Undo.RegisterFullObjectHierarchyUndo(root, "Remove Missing Scripts");

            int removed = 0;
            Transform[] transforms = root.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < transforms.Length; i++)
                removed += GameObjectUtility.RemoveMonoBehavioursWithMissingScript(transforms[i].gameObject);

            return removed;
        }
        #endregion

        #region Scene Operations
        private static void Open(string path, OpenSceneMode mode, bool setActive)
        {
            if (mode == OpenSceneMode.Single)
                if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            Scene scene = EditorSceneManager.OpenScene(path, mode);
            if (setActive) SceneManager.SetActiveScene(scene);
        }

        private static void Ping(string guid)
        {
            string p = AssetDatabase.GUIDToAssetPath(guid);
            SceneAsset obj = AssetDatabase.LoadAssetAtPath<SceneAsset>(p);
            EditorGUIUtility.PingObject(obj);
        }

        public static bool IsFavorite(string guid) => !string.IsNullOrEmpty(guid) && s_favorites.Contains(guid);

        public static void Add(string guid)
        {
            if (string.IsNullOrEmpty(guid)) return;
            if (s_favorites.Add(guid)) Save();
        }

        public static void Remove(string guid)
        {
            if (string.IsNullOrEmpty(guid)) return;
            if (s_favorites.Remove(guid)) Save();
        }

        public static void Toggle(string guid)
        {
            if (string.IsNullOrEmpty(guid)) return;
            if (!s_favorites.Add(guid)) s_favorites.Remove(guid);
            Save();
        }

        public static IEnumerable<string> GetAllGuids() => s_favorites;
        #endregion

        #region Save/Load
        private static void Save()
        {
            string path = HD_File.GetSavedDataFilePath(HD_Constants.FavoriteScenesTextFileName);
            FavoriteGuidList payload = new FavoriteGuidList { items = s_favorites.ToList() };
            File.WriteAllText(path, JsonUtility.ToJson(payload, true));
            AssetDatabase.Refresh();
        }

        private static void Load()
        {
            s_favorites.Clear();
            string path = HD_File.GetSavedDataFilePath(HD_Constants.FavoriteScenesTextFileName);
            if (!File.Exists(path)) return;
            FavoriteGuidList data = JsonUtility.FromJson<FavoriteGuidList>(File.ReadAllText(path));
            if (data?.items != null)
            {
                foreach (string g in data.items) if (!string.IsNullOrEmpty(g)) s_favorites.Add(g);
            }
        }
        #endregion

        #region Popup
        private class ScenePopup : PopupWindowContent
        {
            #region Properties
            private Vector2 scroll;
            private readonly float windowWidth;

            private const float RowDotW = 14f;
            private const float RowStarW = 16f;
            private const float RowMenuW = 20f;
            private const float OptionsBtnW = 22f;

            private static readonly GUIContent OptionsArrowGC = new("▾", "More actions");

            public ScenePopup(float width) => windowWidth = width;
            public override Vector2 GetWindowSize() => new(windowWidth, 260f);
            #endregion

            public override void OnGUI(Rect rect)
            {
                using (new EditorGUILayout.VerticalScope(GUILayout.ExpandHeight(true)))
                {
                    using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
                    {
                        GUIStyle tf = GUI.skin.FindStyle("ToolbarSeachTextField") ?? GUI.skin.FindStyle("ToolbarSearchTextField") ?? EditorStyles.toolbarTextField;

                        float h = (EditorStyles.toolbar.fixedHeight > 0f) ? EditorStyles.toolbar.fixedHeight : 18f;
                        Rect searchRect = GUILayoutUtility.GetRect(0, h, GUILayout.ExpandWidth(true));

                        GUI.SetNextControlName("HD_Scene_Search");
                        s_search = EditorGUI.TextField(searchRect, s_search, tf);
                    }

                    IEnumerable<SceneItem> query = s_allScenes;
                    if (!string.IsNullOrEmpty(s_search))
                    {
                        string s = s_search.Trim();
                        query = query.Where(it => it.name.IndexOf(s, StringComparison.OrdinalIgnoreCase) >= 0 || it.path.IndexOf(s, StringComparison.OrdinalIgnoreCase) >= 0);
                    }
                    List<SceneItem> items = query.ToList();
                    List<SceneItem> favItems = items.Where(it => IsFavorite(it.guid)).ToList();
                    List<SceneItem> otherItems = items.Where(it => !IsFavorite(it.guid)).ToList();

                    using (new EditorGUILayout.HorizontalScope())
                    {
                        GUILayout.Space(5f);

                        using (new EditorGUILayout.VerticalScope(GUILayout.ExpandHeight(true)))
                        {
                            scroll = GUILayout.BeginScrollView(scroll, false, true, GUIStyle.none, GUI.skin.verticalScrollbar, GUIStyle.none, GUILayout.ExpandHeight(true));

                            if (favItems.Count > 0)
                            {
                                GUILayout.Label("Favorites", EditorStyles.miniBoldLabel);
                                foreach (SceneItem it in favItems) DrawSceneRow(it, isFavorite: true);
                                GUILayout.Space(6);
                            }

                            GUILayout.Label("All Scenes", EditorStyles.miniBoldLabel);
                            foreach (SceneItem it in otherItems) DrawSceneRow(it);

                            GUILayout.EndScrollView();
                        }
                    }
                }
            }

            #region Methods
            private void DrawSceneRow(SceneItem it, bool isFavorite = false)
            {
                bool loaded = IsPathLoaded(it.path);
                using (new EditorGUILayout.HorizontalScope())
                {
                    Rect dotRect = GUILayoutUtility.GetRect(18f, 16f, GUILayout.Width(16f));
                    if (GUI.Button(dotRect, GUIContent.none, GUIStyle.none))
                    {
                        PerformOpen(it);
                        ClosePopup();
                    }
                    GUI.Label(dotRect, new GUIContent(loaded ? "●" : "○", "Open this scene"), EditorStyles.miniLabel);

                    bool favState = isFavorite || IsFavorite(it.guid);
                    GUIContent starGC = new(favState ? "★" : "☆", favState ? "Remove from Favorites" : "Add to Favorites");

                    Rect starRect = GUILayoutUtility.GetRect(18f, 16f, GUILayout.Width(16f));
                    if (GUI.Button(starRect, GUIContent.none, GUIStyle.none))
                    {
                        Toggle(it.guid);
                        editorWindow.Repaint();
                    }
                    GUI.Label(starRect, starGC, EditorStyles.miniLabel);

                    if (GUILayout.Button(new GUIContent(it.name, it.path), EditorStyles.label, GUILayout.ExpandWidth(true)))
                    {
                        PerformOpen(it);
                        ClosePopup();
                    }

                    if (GUILayout.Button(OptionsArrowGC, EditorStyles.miniButton, GUILayout.Width(OptionsBtnW)))
                    {
                        GenericMenu m = new();
                        m.AddItem(new GUIContent("Open Single"), false, () => { Open(it.path, OpenSceneMode.Single, setActive: true); ClosePopup(); });
                        m.AddItem(new GUIContent("Open Additive (Shift)"), false, () => { Open(it.path, OpenSceneMode.Additive, setActive: true); ClosePopup(); });
                        m.AddItem(new GUIContent("Ping in Project"), false, () => { Ping(it.guid); });

                        m.AddSeparator("");
                        bool fav = IsFavorite(it.guid);
                        string favLabel = fav ? "Remove from Favorites" : "Add to Favorites";
                        m.AddItem(new GUIContent(favLabel), false, () =>
                        {
                            Toggle(it.guid);
                            editorWindow.Repaint();
                        });

                        m.ShowAsContext();
                    }
                }
            }

            public static float ComputePopupWidth()
            {
                GUIStyle nameStyle = EditorStyles.label;
                float maxNameW = 0f;
                for (int i = 0; i < s_allScenes.Count; i++)
                {
                    float w = nameStyle.CalcSize(new GUIContent(s_allScenes[i].name)).x;
                    if (w > maxNameW) maxNameW = w;
                }

                float left = RowDotW + Gap + RowStarW + Gap;
                float right = Gap + RowMenuW + 24f;
                float content = left + maxNameW + right;
                float minBar = MinSearchWidth + 24f;

                return Mathf.Ceil(Mathf.Max(content, minBar));
            }

            private void PerformOpen(SceneItem it)
            {
                bool additive = Event.current != null && Event.current.shift;
                Open(it.path, additive ? OpenSceneMode.Additive : OpenSceneMode.Single, setActive: true);
            }

            private void ClosePopup()
            {
                editorWindow.Close();
            }
            #endregion
        }
        #endregion

        #region Cache
        private static void RefreshCache()
        {
            s_allScenes.Clear();
            foreach (string guid in AssetDatabase.FindAssets("t:Scene"))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                s_allScenes.Add(new SceneItem
                {
                    guid = guid,
                    path = path,
                    name = Path.GetFileNameWithoutExtension(path)
                });
            }
            s_allScenes.Sort((a, b) => string.Compare(a.name, b.name, StringComparison.OrdinalIgnoreCase));
        }

        private static bool IsPathLoaded(string path)
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
                if (SceneManager.GetSceneAt(i).path == path) return true;
            return false;
        }
        #endregion
    }
}
#endif