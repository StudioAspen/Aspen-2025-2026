#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace HierarchyDesigner
{
    internal static class HD_Operations
    {
        #region Properties
        #if UNITY_6000_5_OR_NEWER
        private static Action<UnityEngine.EntityId, bool> setExpandedEntityIdDelegate;
        private static Action<UnityEngine.EntityId, bool> setExpandedRecursiveEntityIdDelegate;
        private static EditorWindow cachedHierarchyWindow;
        private static bool isReflectionInitialized;
        #elif UNITY_6000_4_OR_NEWER
        private static Action<int, bool> setExpandedDelegate;
        private static Action<int, bool> setExpandedRecursiveDelegate;
        private static EditorWindow cachedHierarchyWindow;
        private static bool isReflectionInitialized;
        #endif
        #endregion

        #region Folder
        public static void CreateFolder(string folderName, bool shouldRename)
        {
            GameObject folder = new(folderName);
            folder.AddComponent<HierarchyDesignerFolder>();
            EditorGUIUtility.SetIconForObject(folder, HD_Resources.Textures.FolderScene);
            if (shouldRename) EditorApplication.delayCall += () => BeginRename(folder);
            Undo.RegisterCreatedObjectUndo(folder, $"Create {folderName}");
        }

        public static void BeginRename(GameObject obj)
        {
            Selection.activeObject = obj;
            EditorApplication.ExecuteMenuItem("Window/General/Hierarchy");
            EditorWindow.focusedWindow.SendEvent(EditorGUIUtility.CommandEvent("Rename"));
        }

        public static bool FolderExists(string folderName)
        {
#if UNITY_6000_4_OR_NEWER
            Transform[] allTransforms = GameObject.FindObjectsByType<Transform>();
#elif UNITY_6000_0_OR_NEWER
            Transform[] allTransforms = GameObject.FindObjectsByType<Transform>(FindObjectsSortMode.None);
#else
            Transform[] allTransforms = UnityEngine.Object.FindObjectsOfType<Transform>(true);
#endif
            foreach (Transform t in allTransforms)
            {
                if (t.gameObject.GetComponent<HierarchyDesignerFolder>() && t.gameObject.name.Equals(folderName))
                {
                    return true;
                }
            }

            return false;
        }
        #endregion

        #region Separator
        public static void CreateSeparator(string separatorName)
        {
            GameObject separator = new($"{HD_Constants.SeparatorPrefix}{separatorName}");
            separator.tag = HD_Constants.SeparatorTag;
            SetSeparatorState(separator, false);
            separator.SetActive(false);
            EditorGUIUtility.SetIconForObject(separator, HD_Resources.Textures.SeparatorInspectorIcon);
            Undo.RegisterCreatedObjectUndo(separator, $"Create {separatorName}");
        }

        public static void SetSeparatorState(GameObject gameObject, bool editable)
        {
            foreach (Component component in gameObject.GetComponents<Component>())
            {
                if (component)
                {
                    component.hideFlags = editable ? HideFlags.None : HideFlags.NotEditable;
                }
            }
            gameObject.hideFlags = editable ? HideFlags.None : HideFlags.NotEditable;
            gameObject.transform.hideFlags = HideFlags.HideInInspector;
            EditorUtility.SetDirty(gameObject);
        }

        public static bool SeparatorExists(string separatorName)
        {
            string fullSeparatorName = HD_Constants.SeparatorPrefix + separatorName;

#if UNITY_6000_4_OR_NEWER
            Transform[] allTransforms = GameObject.FindObjectsByType<Transform>(FindObjectsInactive.Include);
#elif UNITY_6000_0_OR_NEWER
            Transform[] allTransforms = GameObject.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
#else
            Transform[] allTransforms = UnityEngine.Object.FindObjectsOfType<Transform>(true);
#endif
            foreach (Transform t in allTransforms)
            {
                if (t.gameObject.CompareTag(HD_Constants.SeparatorTag) && t.gameObject.name.Equals(fullSeparatorName))
                {
                    return true;
                }
            }

            return false;
        }

        public static string StripPrefix(string name)
        {
            if (name.StartsWith(HD_Constants.SeparatorPrefix))
            {
                return name[HD_Constants.SeparatorPrefix.Length..].Trim();
            }

            return name.Trim();
        }
        #endregion

        #region Tag & Layer
        internal static void HandleTagClick(GameObject gameObject, Vector2 position)
        {
            GenericMenu menu = new();
            menu.AddDisabledItem(new("Tag"));
            menu.AddSeparator("");

            string[] tags = InternalEditorUtility.tags;
            foreach (string tag in tags)
            {
                bool isOn = gameObject.CompareTag(tag);
                menu.AddItem(new(tag), isOn, () =>
                {
                    if (!isOn)
                    {
                        Undo.RecordObject(gameObject, "Change Tag");
                        gameObject.tag = tag;
                        EditorUtility.SetDirty(gameObject);
                    }
                });
            }

            menu.AddSeparator("");
            menu.AddItem(new("Add Tag..."), false, () =>
            {
                Selection.activeObject = AssetDatabase.LoadAssetAtPath<Object>("ProjectSettings/TagManager.asset");
                EditorApplication.ExecuteMenuItem("Window/General/Inspector");
            });

            menu.DropDown(new(position, Vector2.zero));
        }

        internal static void HandleLayerClick(GameObject gameObject, Vector2 position)
        {
            GenericMenu menu = new();
            menu.AddDisabledItem(new("Layer"));
            menu.AddSeparator("");

            for (int i = 0; i < 32; i++)
            {
                string layerName = LayerMask.LayerToName(i);
                if (string.IsNullOrEmpty(layerName)) continue;

                bool isOn = gameObject.layer == i;
                int layerIndex = i;
                menu.AddItem(new(layerName), isOn, () =>
                {
                    if (!isOn)
                    {
                        if (gameObject.transform.childCount > 0)
                        {
                            int result = AskToChangeChildrenLayers(gameObject, layerIndex);
                            if (result == 2) return;
                        }

                        Undo.RecordObject(gameObject, "Change Layer");
                        gameObject.layer = layerIndex;
                        EditorUtility.SetDirty(gameObject);
                    }
                });
            }

            menu.AddSeparator("");
            menu.AddItem(new("Add Layer..."), false, () =>
            {
                Selection.activeObject = AssetDatabase.LoadAssetAtPath<Object>("ProjectSettings/TagManager.asset");
                EditorApplication.ExecuteMenuItem("Window/General/Inspector");
            });

            menu.DropDown(new(position, Vector2.zero));
        }

        private static int AskToChangeChildrenLayers(GameObject obj, int newLayer)
        {
            int option = EditorUtility.DisplayDialogComplex(
                "Change Layer",
                $"Do you want to set the layer to '{LayerMask.LayerToName(newLayer)}' for all child objects as well?",
                "Yes, change children",
                "No, this object only",
                "Cancel"
            );

            if (option == 0)
            {
                SetLayerRecursively(obj, newLayer);
            }

            return option;
        }

        private static void SetLayerRecursively(GameObject obj, int newLayer)
        {
            foreach (Transform child in obj.transform)
            {
                Undo.RecordObject(child.gameObject, "Change Layer");
                child.gameObject.layer = newLayer;
                SetLayerRecursively(child.gameObject, newLayer);
            }
        }
        #endregion

        #region Tools
        #region General
        private static IEnumerable<GameObject> GetAllGameObjectsInActiveScene()
        {
            GameObject[] rootObjects = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
            List<GameObject> allGameObjects = new();

            Stack<GameObject> stack = new(rootObjects);
            while (stack.Count > 0)
            {
                GameObject current = stack.Pop();
                allGameObjects.Add(current);
                foreach (Transform child in current.transform) { stack.Push(child.gameObject); }
            }
            return allGameObjects;
        }

        private static bool IsGameObjectActive(GameObject gameObject)
        {
            if (gameObject == null)
            {
                return false;
            }
            return gameObject.activeSelf;
        }

        private static bool IsGameObjectParent(GameObject gameObject)
        {
            return gameObject.transform.childCount > 0;
        }

        private static bool IsGameObjectEmpty(GameObject gameObject)
        {
            Component[] components = gameObject.GetComponents<Component>();
            return components.Length == 1 && (components[0] is Transform || components[0] is RectTransform);
        }

        public static bool IsGameObjectLocked(GameObject gameObject)
        {
            if (gameObject == null)
            {
                return false;
            }
            return (gameObject.hideFlags & HideFlags.NotEditable) == HideFlags.NotEditable;
        }

        private static bool IsGameObjectFolder(GameObject gameObject)
        {
            return gameObject.GetComponent<HierarchyDesignerFolder>();
        }

        private static bool IsGameObjectSeparator(GameObject gameObject)
        {
            return gameObject.CompareTag(HD_Constants.SeparatorTag) && gameObject.name.StartsWith(HD_Constants.SeparatorPrefix);
        }

        private static bool HasMissingScript(GameObject gameObject)
        {
            if (gameObject == null)
            {
                return false;
            }

            Component[] components = gameObject.GetComponents<Component>();
            foreach (Component component in components)
            {
                if (component == null)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool ShouldIncludeFolderAndSeparator(GameObject gameObject, bool addFolderToCount, bool addSeparatorToCount)
        {
            if (!addFolderToCount && IsGameObjectFolder(gameObject))
            {
                return false;
            }

            if (!addSeparatorToCount && IsGameObjectSeparator(gameObject))
            {
                return false;
            }

            return true;
        }
        #endregion

        #region Activate
        public static void SetActiveState(GameObject gameObject, bool isActive)
        {
            if (IsGameObjectSeparator(gameObject)) return;
            Undo.RegisterCompleteObjectUndo(gameObject, $"{(isActive ? "Activate" : "Deactivate")} GameObject");

            if (gameObject.activeSelf != isActive)
            {
                gameObject.SetActive(isActive);
                EditorUtility.SetDirty(gameObject);
            }
        }

        public static void Activate_SelectedGameObjects(bool activeState)
        {
            GameObject[] selectedGameObjects = Selection.gameObjects;
            if (selectedGameObjects.Length < 1)
            {
                Debug.Log($"No GameObject(s) selected. Please select at least one GameObject to {(activeState ? "activate" : "deactivate")}.");
                return;
            }

            foreach (GameObject gameObject in selectedGameObjects)
            {
                if (IsGameObjectActive(gameObject) != activeState)
                {
                    SetActiveState(gameObject, activeState);
                }
            }
        }

        public static void Activate_AllGameObjects(bool activeState)
        {
            foreach (GameObject gameObject in GetAllGameObjectsInActiveScene())
            {
                SetActiveState(gameObject, activeState);
            }
        }

        public static void Activate_AllParentGameObjects(bool activeState)
        {
            foreach (GameObject gameObject in GetAllGameObjectsInActiveScene())
            {
                if (IsGameObjectParent(gameObject))
                {
                    SetActiveState(gameObject, activeState);
                }
            }
        }

        public static void Activate_AllEmptyGameObjects(bool activeState)
        {
            foreach (GameObject gameObject in GetAllGameObjectsInActiveScene())
            {
                if (IsGameObjectEmpty(gameObject))
                {
                    SetActiveState(gameObject, activeState);
                }
            }
        }

        public static void Activate_AllLockedGameObjects(bool activeState)
        {
            foreach (GameObject gameObject in GetAllGameObjectsInActiveScene())
            {
                if (IsGameObjectLocked(gameObject))
                {
                    SetActiveState(gameObject, activeState);
                }
            }
        }

        public static void Activate_AllFolders(bool activeState)
        {
            foreach (GameObject gameObject in GetAllGameObjectsInActiveScene())
            {
                if (IsGameObjectFolder(gameObject))
                {
                    SetActiveState(gameObject, activeState);
                }
            }
        }

        public static void Activate_AllComponentOfType<T>(bool isActive) where T : Component
        {
            IEnumerable<GameObject> allGameObjects = GetAllGameObjectsInActiveScene();
            foreach (GameObject gameObject in allGameObjects)
            {
                if (gameObject.GetComponent<T>() != null)
                {
                    SetActiveState(gameObject, isActive);
                }
            }
        }

        public static void Activate_All2DSpritesByType(string spriteType, bool isActive)
        {
            IEnumerable<GameObject> allGameObjects = GetAllGameObjectsInActiveScene();
            foreach (GameObject gameObject in allGameObjects)
            {
                SpriteRenderer spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
                if (spriteRenderer != null && spriteRenderer.sprite != null && spriteRenderer.sprite.name.Contains(spriteType))
                {
                    SetActiveState(gameObject, isActive);
                }
            }
        }

        public static void Activate_AllPhysicsDynamicSprites(bool isActive)
        {
            IEnumerable<GameObject> allGameObjects = GetAllGameObjectsInActiveScene();
            foreach (GameObject gameObject in allGameObjects)
            {
                SpriteRenderer spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
                Rigidbody2D rigidbody2D = gameObject.GetComponent<Rigidbody2D>();
                if (spriteRenderer != null && rigidbody2D != null && rigidbody2D.bodyType == RigidbodyType2D.Dynamic)
                {
                    SetActiveState(gameObject, isActive);
                }
            }
        }

        public static void Activate_AllPhysicsStaticSprites(bool isActive)
        {
            IEnumerable<GameObject> allGameObjects = GetAllGameObjectsInActiveScene();
            foreach (GameObject gameObject in allGameObjects)
            {
                SpriteRenderer spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
                Rigidbody2D rigidbody2D = gameObject.GetComponent<Rigidbody2D>();
                if (spriteRenderer != null && rigidbody2D != null && (rigidbody2D.bodyType == RigidbodyType2D.Static || rigidbody2D.bodyType == RigidbodyType2D.Kinematic))
                {
                    SetActiveState(gameObject, isActive);
                }
            }
        }

        public static void Activate_All3DObjectsByType(string objectType, bool isActive)
        {
            IEnumerable<GameObject> allGameObjects = GetAllGameObjectsInActiveScene();
            foreach (GameObject gameObject in allGameObjects)
            {
                MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();
                if (meshFilter != null && meshFilter.sharedMesh != null && meshFilter.sharedMesh.name.Contains(objectType))
                {
                    SetActiveState(gameObject, isActive);
                }
            }
        }

        public static void Activate_AllHalos(bool isActive)
        {
            IEnumerable<GameObject> allGameObjects = GetAllGameObjectsInActiveScene();
            foreach (GameObject gameObject in allGameObjects)
            {
                Behaviour halo = (Behaviour)gameObject.GetComponent("Halo");
                if (halo != null)
                {
                    SetActiveState(gameObject, isActive);
                }
            }
        }

        public static void Activate_AllComponentOfType<T>(bool isActive, Func<T, bool> predicate = null) where T : Component
        {
            IEnumerable<GameObject> allGameObjects = GetAllGameObjectsInActiveScene();
            foreach (GameObject gameObject in allGameObjects)
            {
                T component = gameObject.GetComponent<T>();
                if (component != null && (predicate == null || predicate(component)))
                {
                    SetActiveState(gameObject, isActive);
                }
            }
        }

        public static void Activate_AllTMPComponentIfAvailable<T>(bool isActive) where T : Component
        {
            if (HD_Validation.IsTMPAvailable())
            {
                Activate_AllComponentOfType<T>(isActive);
            }
            else
            {
                EditorUtility.DisplayDialog("TMP Not Found", "TMP wasn't found in the project, make sure you have it enabled.", "OK");
            }
        }
        #endregion

        #region Count
        public static void CountGameObjects(string description, Func<GameObject, bool> predicate, bool includeFolders = false, bool includeSeparators = false)
        {
            IEnumerable<GameObject> allGameObjects = GetAllGameObjectsInActiveScene();
            int count = 0;
            string gameObjectNames = "";
            bool addFolderToCount = includeFolders || !HD_Settings.ExcludeFoldersFromCountSelectToolCalculations;
            bool addSeparatorToCount = includeSeparators || !HD_Settings.ExcludeSeparatorsFromCountSelectToolCalculations;

            foreach (GameObject gameObject in allGameObjects)
            {
                if (!ShouldIncludeFolderAndSeparator(gameObject, addFolderToCount, addSeparatorToCount)) { continue; }
                if (predicate(gameObject))
                {
                    count++;
                    gameObjectNames += gameObject.name + ", ";
                }
            }
            gameObjectNames = count == 0 ? "none" : gameObjectNames.TrimEnd(',', ' ');
            Debug.Log($"Total <color=#73FF7A>{description}</color> in the scene: <b>{count}</b>\n<i>All {description} found:</i> <b>{gameObjectNames}</b>.\n");
        }

        public static void Count_SelectedGameObjects()
        {
            GameObject[] selectedGameObjects = Selection.gameObjects;
            if (selectedGameObjects.Length < 1)
            {
                Debug.Log($"No GameObject(s) selected. Please select at least one GameObject to count.");
                return;
            }

            int count = selectedGameObjects.Length;
            string selectedGameObjectNames = "";
            for (int i = 0; i < selectedGameObjects.Length; i++)
            {
                selectedGameObjectNames += selectedGameObjects[i].name;
                if (i < selectedGameObjects.Length - 1)
                {
                    selectedGameObjectNames += ", ";
                }
            }
            Debug.Log($"Total <color=#73FF7A>Selected GameObjects</color> in the scene: <b>{count}</b>\n<i>All <Selected GameObjects> found:</i> <b>{selectedGameObjectNames}</b>.\n");
        }

        public static void Count_AllGameObjects()
        {
            CountGameObjects("GameObjects", gameObject => true);
        }

        public static void Count_AllParentGameObjects()
        {
            CountGameObjects("Parent GameObjects", IsGameObjectParent);
        }

        public static void Count_AllEmptyGameObjects()
        {
            CountGameObjects("Empty GameObjects", IsGameObjectEmpty);
        }

        public static void Count_AllLockedGameObjects()
        {
            CountGameObjects("Locked GameObjects", IsGameObjectLocked);
        }

        public static void Count_AllActiveGameObjects()
        {
            CountGameObjects("Active GameObjects", IsGameObjectActive);
        }

        public static void Count_AllInactiveGameObjects()
        {
            CountGameObjects("Inactive GameObjects", gameObject => !IsGameObjectActive(gameObject));
        }

        public static void Count_AllFolders()
        {
            CountGameObjects("Folders", IsGameObjectFolder, includeFolders: true);
        }

        public static void Count_AllSeparators()
        {
            CountGameObjects("Separators", IsGameObjectSeparator, includeSeparators: true);
        }

        public static void Count_AllComponentOfType<T>(string componentName) where T : Component
        {
            CountGameObjects($"{componentName}", gameObject => gameObject.GetComponent<T>() != null);
        }

        public static void Count_All2DSpritesByType(string spriteType)
        {
            CountGameObjects($"2D {spriteType} sprites", gameObject =>
            {
                SpriteRenderer spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
                return spriteRenderer != null && spriteRenderer.sprite != null && spriteRenderer.sprite.name.Contains(spriteType);
            });
        }

        public static void Count_AllPhysicsDynamicSprites()
        {
            CountGameObjects("2D Dynamic physics sprites", gameObject =>
            {
                SpriteRenderer spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
                Rigidbody2D rigidbody2D = gameObject.GetComponent<Rigidbody2D>();
                return spriteRenderer != null && rigidbody2D != null && rigidbody2D.bodyType == RigidbodyType2D.Dynamic;
            });
        }

        public static void Count_AllPhysicsStaticSprites()
        {
            CountGameObjects("2D Static physics sprites", gameObject =>
            {
                SpriteRenderer spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
                Rigidbody2D rigidbody2D = gameObject.GetComponent<Rigidbody2D>();
                return spriteRenderer != null && rigidbody2D != null && (rigidbody2D.bodyType == RigidbodyType2D.Static || rigidbody2D.bodyType == RigidbodyType2D.Kinematic);
            });
        }

        public static void Count_All3DObjectsByType(string objectType)
        {
            CountGameObjects($"3D {objectType} objects", gameObject =>
            {
                MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();
                return meshFilter != null && meshFilter.sharedMesh != null && meshFilter.sharedMesh.name.Contains(objectType);
            });
        }

        public static void Count_AllHalos()
        {
            CountGameObjects("Halos", gameObject =>
            {
                Behaviour halo = (Behaviour)gameObject.GetComponent("Halo");
                return halo != null;
            });
        }

        public static void Count_AllComponentOfType<T>(string componentName, Func<T, bool> predicate = null) where T : Component
        {
            CountGameObjects($"{componentName}", gameObject =>
            {
                T component = gameObject.GetComponent<T>();
                return component != null && (predicate == null || predicate(component));
            });
        }

        public static void Count_AllTMPComponentIfAvailable<T>(string componentName) where T : Component
        {
            if (HD_Validation.IsTMPAvailable())
            {
                Count_AllComponentOfType<T>(componentName);
            }
            else
            {
                EditorUtility.DisplayDialog("TMP Not Found", "TMP wasn't found in the project, make sure you have it enabled.", "OK");
            }
        }
        #endregion

        #region Lock 
        public static void LockGameObject(GameObject gameObject, bool isLocked)
        {
            SetLockState(gameObject, !isLocked);
        }

        public static void SetLockState(GameObject gameObject, bool editable)
        {
            if (IsGameObjectSeparator(gameObject)) return;
            Undo.RegisterCompleteObjectUndo(gameObject, $"{(editable ? "Unlock" : "Lock")} GameObject");

            HideFlags newFlags = editable ? HideFlags.None : HideFlags.NotEditable;
            if (gameObject.hideFlags != newFlags)
            {
                gameObject.hideFlags = newFlags;
                EditorUtility.SetDirty(gameObject);
            }

            foreach (Component component in gameObject.GetComponents<Component>())
            {
                if (component && component.hideFlags != newFlags)
                {
                    component.hideFlags = newFlags;
                    EditorUtility.SetDirty(component);
                }
            }

            if (editable)
            {
                SceneVisibilityManager.instance.EnablePicking(gameObject, true);
            }
            else
            {
                SceneVisibilityManager.instance.DisablePicking(gameObject, true);
            }

            EditorWindow[] allEditorWindows = Resources.FindObjectsOfTypeAll<EditorWindow>();
            foreach (EditorWindow inspector in allEditorWindows)
            {
                if (inspector.GetType().Name == HD_Constants.InspectorWindow)
                {
                    inspector.Repaint();
                }
            }
        }

        public static void Lock_SelectedGameObjects(bool lockState)
        {
            GameObject[] selectedGameObjects = Selection.gameObjects;
            if (selectedGameObjects.Length < 1)
            {
                Debug.Log($"No GameObject(s) selected. Please select at least one GameObject to {(lockState ? "lock" : "unlock")}.");
                return;
            }

            foreach (GameObject gameObject in selectedGameObjects)
            {
                if (IsGameObjectLocked(gameObject) != lockState)
                {
                    SetLockState(gameObject, !lockState);
                }
            }
        }

        public static void Lock_AllGameObjects(bool lockState)
        {
            foreach (GameObject gameObject in GetAllGameObjectsInActiveScene())
            {
                SetLockState(gameObject, !lockState);
            }
        }

        public static void Lock_AllParentGameObjects(bool lockState)
        {
            foreach (GameObject gameObject in GetAllGameObjectsInActiveScene())
            {
                if (IsGameObjectParent(gameObject))
                {
                    SetLockState(gameObject, !lockState);
                }
            }
        }

        public static void Lock_AllEmptyGameObjects(bool lockState)
        {
            foreach (GameObject gameObject in GetAllGameObjectsInActiveScene())
            {
                if (IsGameObjectEmpty(gameObject))
                {
                    SetLockState(gameObject, !lockState);
                }
            }
        }

        public static void Lock_AllActiveGameObjects(bool lockState)
        {
            foreach (GameObject gameObject in GetAllGameObjectsInActiveScene())
            {
                if (IsGameObjectActive(gameObject))
                {
                    SetLockState(gameObject, !lockState);
                }
            }
        }

        public static void Lock_AllInactiveGameObjects(bool lockState)
        {
            foreach (GameObject gameObject in GetAllGameObjectsInActiveScene())
            {
                if (!IsGameObjectActive(gameObject))
                {
                    SetLockState(gameObject, !lockState);
                }
            }
        }

        public static void Lock_AllFolders(bool lockState)
        {
            foreach (GameObject gameObject in GetAllGameObjectsInActiveScene())
            {
                if (IsGameObjectFolder(gameObject))
                {
                    SetLockState(gameObject, !lockState);
                }
            }
        }

        public static void Lock_AllComponentOfType<T>(bool isLocked) where T : Component
        {
            IEnumerable<GameObject> allGameObjects = GetAllGameObjectsInActiveScene();
            foreach (GameObject gameObject in allGameObjects)
            {
                if (gameObject.GetComponent<T>() != null)
                {
                    LockGameObject(gameObject, isLocked);
                }
            }
        }

        public static void Lock_All2DSpritesByType(string spriteType, bool isLocked)
        {
            IEnumerable<GameObject> allGameObjects = GetAllGameObjectsInActiveScene();
            foreach (GameObject gameObject in allGameObjects)
            {
                SpriteRenderer spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
                if (spriteRenderer != null && spriteRenderer.sprite != null && spriteRenderer.sprite.name.Contains(spriteType))
                {
                    LockGameObject(gameObject, isLocked);
                }
            }
        }

        public static void Lock_AllPhysicsDynamicSprites(bool isLocked)
        {
            IEnumerable<GameObject> allGameObjects = GetAllGameObjectsInActiveScene();
            foreach (GameObject gameObject in allGameObjects)
            {
                SpriteRenderer spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
                Rigidbody2D rigidbody2D = gameObject.GetComponent<Rigidbody2D>();
                if (spriteRenderer != null && rigidbody2D != null && rigidbody2D.bodyType == RigidbodyType2D.Dynamic)
                {
                    LockGameObject(gameObject, isLocked);
                }
            }
        }

        public static void Lock_AllPhysicsStaticSprites(bool isLocked)
        {
            IEnumerable<GameObject> allGameObjects = GetAllGameObjectsInActiveScene();
            foreach (GameObject gameObject in allGameObjects)
            {
                SpriteRenderer spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
                Rigidbody2D rigidbody2D = gameObject.GetComponent<Rigidbody2D>();
                if (spriteRenderer != null && rigidbody2D != null && (rigidbody2D.bodyType == RigidbodyType2D.Static || rigidbody2D.bodyType == RigidbodyType2D.Kinematic))
                {
                    LockGameObject(gameObject, isLocked);
                }
            }
        }

        public static void Lock_All3DObjectsByType(string objectType, bool isLocked)
        {
            IEnumerable<GameObject> allGameObjects = GetAllGameObjectsInActiveScene();
            foreach (GameObject gameObject in allGameObjects)
            {
                MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();
                if (meshFilter != null && meshFilter.sharedMesh != null && meshFilter.sharedMesh.name.Contains(objectType))
                {
                    LockGameObject(gameObject, isLocked);
                }
            }
        }

        public static void Lock_AllHalos(bool isLocked)
        {
            IEnumerable<GameObject> allGameObjects = GetAllGameObjectsInActiveScene();
            foreach (GameObject gameObject in allGameObjects)
            {
                Behaviour halo = (Behaviour)gameObject.GetComponent("Halo");
                if (halo != null)
                {
                    LockGameObject(gameObject, isLocked);
                }
            }
        }

        public static void Lock_AllComponentOfType<T>(bool isLocked, Func<T, bool> predicate = null) where T : Component
        {
            IEnumerable<GameObject> allGameObjects = GetAllGameObjectsInActiveScene();
            foreach (GameObject gameObject in allGameObjects)
            {
                T component = gameObject.GetComponent<T>();
                if (component != null && (predicate == null || predicate(component)))
                {
                    LockGameObject(gameObject, isLocked);
                }
            }
        }

        public static void Lock_AllTMPComponentIfAvailable<T>(bool isLocked) where T : Component
        {
            if (HD_Validation.IsTMPAvailable())
            {
                Lock_AllComponentOfType<T>(isLocked);
            }
            else
            {
                EditorUtility.DisplayDialog("TMP Not Found", "TMP wasn't found in the project, make sure you have it enabled.", "OK");
            }
        }
        #endregion

        #region Rename
        public static void Rename_SelectedGameObjects(string sortingActionDescription, bool autoIndex)
        {
            List<GameObject> selectedGameObjects = new(Selection.gameObjects);
            if (selectedGameObjects.Count < 1)
            {
                Debug.Log($"No GameObject(s) selected. Please select at least one GameObject to {sortingActionDescription}.");
                return;
            }

            HD_Rename.OpenWindow(selectedGameObjects, autoIndex, 0);
        }

        public static void Rename_OpenRenameToolWindow()
        {
            HD_Rename.OpenWindow(null, true, 0);
        }
        #endregion

        #region Select
        public static void SelectGameObjects(string description, Func<GameObject, bool> predicate, bool includeFolders = false, bool includeSeparators = false)
        {
            IEnumerable<GameObject> allGameObjects = GetAllGameObjectsInActiveScene();
            List<GameObject> selectedGameObjects = new();
            bool addFolderToSelect = includeFolders || !HD_Settings.ExcludeFoldersFromCountSelectToolCalculations;
            bool addSeparatorToSelect = includeSeparators || !HD_Settings.ExcludeSeparatorsFromCountSelectToolCalculations;

            foreach (GameObject gameObject in allGameObjects)
            {
                if (!ShouldIncludeFolderAndSeparator(gameObject, addFolderToSelect, addSeparatorToSelect)) continue;

                if (predicate(gameObject))
                {
                    selectedGameObjects.Add(gameObject);
                }
            }

            SelectOrDisplayMessage(selectedGameObjects, description);
        }

        public static void SelectOrDisplayMessage(List<GameObject> gameObjects, string message)
        {
            if (gameObjects.Count > 0)
            {
                FocusHierarchyWindow();
                Selection.objects = gameObjects.ToArray();
            }
            else
            {
                Debug.Log($"No {message} found in the current scene.");
            }
        }

        public static void FocusHierarchyWindow()
        {
            EditorApplication.ExecuteMenuItem("Window/General/Hierarchy");
        }

        public static void Select_AllGameObjects()
        {
            SelectGameObjects("GameObjects", gameObject => true);
        }

        public static void Select_AllParentGameObjects()
        {
            SelectGameObjects("Parent GameObjects", IsGameObjectParent);
        }

        public static void Select_AllEmptyGameObjects()
        {
            SelectGameObjects("Empty GameObjects", IsGameObjectEmpty);
        }

        public static void Select_AllLockedGameObjects()
        {
            SelectGameObjects("Locked GameObjects", IsGameObjectLocked);
        }

        public static void Select_AllActiveGameObjects()
        {
            SelectGameObjects("Active GameObjects", IsGameObjectActive);
        }

        public static void Select_AllInactiveGameObjects()
        {
            SelectGameObjects("Inactive GameObjects", gameObject => !IsGameObjectActive(gameObject));
        }

        public static void Select_AllFolders()
        {
            SelectGameObjects("Folders", IsGameObjectFolder, includeFolders: true);
        }

        public static void Select_AllSeparators()
        {
            SelectGameObjects("Separators", IsGameObjectSeparator, includeSeparators: true);
        }

        public static void Select_AllGameObjectsWithMissingScripts()
        {
            SelectGameObjects("GameObjects with Missing Scripts", HasMissingScript);
        }

        public static void Select_AllComponentOfType<T>() where T : Component
        {
            SelectGameObjects(typeof(T).Name, gameObject => gameObject.GetComponent<T>() != null);
        }

        public static void Select_All2DSpritesByType(string spriteType)
        {
            SelectGameObjects(spriteType, gameObject =>
            {
                SpriteRenderer spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
                return spriteRenderer != null && spriteRenderer.sprite != null && spriteRenderer.sprite.name.Contains(spriteType);
            });
        }

        public static void Select_AllPhysicsDynamicSprites()
        {
            SelectGameObjects("Dynamic Sprites", gameObject =>
            {
                SpriteRenderer spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
                Rigidbody2D rigidbody2D = gameObject.GetComponent<Rigidbody2D>();
                return spriteRenderer != null && rigidbody2D != null && rigidbody2D.bodyType == RigidbodyType2D.Dynamic;
            });
        }

        public static void Select_AllPhysicsStaticSprites()
        {
            SelectGameObjects("Static Sprites", gameObject =>
            {
                SpriteRenderer spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
                Rigidbody2D rigidbody2D = gameObject.GetComponent<Rigidbody2D>();
                return spriteRenderer != null && rigidbody2D != null && (rigidbody2D.bodyType == RigidbodyType2D.Static || rigidbody2D.bodyType == RigidbodyType2D.Kinematic);
            });
        }

        public static void Select_All3DObjectsByType(string objectType)
        {
            SelectGameObjects(objectType, gameObject =>
            {
                MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();
                return meshFilter != null && meshFilter.sharedMesh != null && meshFilter.sharedMesh.name.Contains(objectType);
            });
        }

        public static void Select_AllHalos()
        {
            SelectGameObjects("Halos", gameObject =>
            {
                Behaviour halo = (Behaviour)gameObject.GetComponent("Halo");
                return halo != null;
            });
        }

        public static void Select_AllComponentOfType<T>(Func<T, bool> predicate) where T : Component
        {
            SelectGameObjects(typeof(T).Name, gameObject =>
            {
                T component = gameObject.GetComponent<T>();
                return component != null && predicate(component);
            });
        }

        public static void Select_AllTMPComponentIfAvailable<T>() where T : Component
        {
            if (HD_Validation.IsTMPAvailable())
            {
                Select_AllComponentOfType<T>();
            }
            else
            {
                EditorUtility.DisplayDialog("TMP Not Found", "TMP wasn't found in the project, make sure you have it enabled.", "OK");
            }
        }
        #endregion

        #region Sort
        public static int AlphanumericComparison(GameObject x, GameObject y)
        {
            string xName = x.name;
            string yName = y.name;

            string[] xParts = System.Text.RegularExpressions.Regex.Split(xName.Replace(" ", ""), "([0-9]+)");
            string[] yParts = System.Text.RegularExpressions.Regex.Split(yName.Replace(" ", ""), "([0-9]+)");

            for (int i = 0; i < Math.Min(xParts.Length, yParts.Length); i++)
            {
                if (int.TryParse(xParts[i], out int xPartNum) && int.TryParse(yParts[i], out int yPartNum))
                {
                    if (xPartNum != yPartNum) return xPartNum.CompareTo(yPartNum);
                }
                else
                {
                    int compareResult = xParts[i].CompareTo(yParts[i]);
                    if (compareResult != 0) return compareResult;
                }
            }
            return xParts.Length.CompareTo(yParts.Length);
        }

        public static void Sort_GameObjectChildren(Comparison<GameObject> comparison, string sortingActionDescription)
        {
            GameObject[] selectedGameObjects = Selection.gameObjects;
            if (selectedGameObjects.Length == 0)
            {
                Debug.Log($"No Parent GameObjects selected. Please select at least one Parent GameObject to {sortingActionDescription}.");
                return;
            }

            foreach (GameObject selectedGameObject in selectedGameObjects)
            {
                Undo.RegisterCompleteObjectUndo(selectedGameObject.transform, sortingActionDescription);

                List<GameObject> children = new();
                foreach (Transform child in selectedGameObject.transform)
                {
                    children.Add(child.gameObject);
                }

                children.Sort(comparison);
                for (int i = 0; i < children.Count; i++)
                {
                    children[i].transform.SetSiblingIndex(i);
                }
            }
        }

        public static void Sort_GameObjectChildrenByTag(bool ascending, string sortingActionDescription)
        {
            Sort_GameObjectChildren((x, y) =>
            {
                if (ascending)
                {
                    return string.Compare(x.tag, y.tag, StringComparison.Ordinal);
                }
                else
                {
                    return string.Compare(y.tag, x.tag, StringComparison.Ordinal);
                }
            }, sortingActionDescription);
        }

        public static void Sort_GameObjectChildrenByTagListOrder(bool ascending, string sortingActionDescription)
        {
            string[] predefinedOrder = new string[] { "Untagged", "Respawn", "Finish", "EditorOnly", "MainCamera", "Player", "GameController" };

            List<string> allTags = new(UnityEditorInternal.InternalEditorUtility.tags);
            allTags.Sort((x, y) =>
            {
                int indexX = Array.IndexOf(predefinedOrder, x);
                int indexY = Array.IndexOf(predefinedOrder, y);
                indexX = indexX == -1 ? int.MaxValue : indexX;
                indexY = indexY == -1 ? int.MaxValue : indexY;
                return indexX.CompareTo(indexY);
            });

            Sort_GameObjectChildren((x, y) =>
            {
                int indexX = allTags.IndexOf(x.tag);
                int indexY = allTags.IndexOf(y.tag);
                return ascending ? indexX.CompareTo(indexY) : indexY.CompareTo(indexX);
            }, sortingActionDescription);
        }

        public static void Sort_GameObjectChildrenByLayer(bool ascending, string sortingActionDescription)
        {
            Sort_GameObjectChildren((x, y) =>
            {
                string layerX = LayerMask.LayerToName(x.layer);
                string layerY = LayerMask.LayerToName(y.layer);
                return ascending ? string.Compare(layerX, layerY, StringComparison.Ordinal) : string.Compare(layerY, layerX, StringComparison.Ordinal);
            }, sortingActionDescription);
        }

        public static void Sort_GameObjectChildrenByLayerListOrder(bool ascending, string sortingActionDescription)
        {
            List<string> allLayers = new();
            for (int i = 0; i < 32; i++)
            {
                string layerName = LayerMask.LayerToName(i);
                if (!string.IsNullOrEmpty(layerName))
                {
                    allLayers.Add(layerName);
                }
            }

            Sort_GameObjectChildren((x, y) =>
            {
                int indexX = allLayers.IndexOf(LayerMask.LayerToName(x.layer));
                int indexY = allLayers.IndexOf(LayerMask.LayerToName(y.layer));
                return ascending ? indexX.CompareTo(indexY) : indexY.CompareTo(indexX);
            }, sortingActionDescription);
        }

        public static void Sort_GameObjectChildrenRandomly(string sortingActionDescription)
        {
            GameObject[] selectedGameObjects = Selection.gameObjects;
            if (selectedGameObjects.Length == 0)
            {
                Debug.Log($"No Parent GameObjects selected. Please select at least one Parent GameObject to {sortingActionDescription}.");
                return;
            }

            foreach (GameObject selectedGameObject in selectedGameObjects)
            {
                Undo.RegisterCompleteObjectUndo(selectedGameObject.transform, sortingActionDescription);

                List<GameObject> children = new();
                foreach (Transform child in selectedGameObject.transform)
                {
                    children.Add(child.gameObject);
                }

                System.Random rng = new();
                int n = children.Count;
                while (n > 1)
                {
                    n--;
                    int k = rng.Next(n + 1);
                    GameObject value = children[k];
                    children[k] = children[n];
                    children[n] = value;
                }

                for (int i = 0; i < children.Count; i++)
                {
                    children[i].transform.SetSiblingIndex(i);
                }
            }
        }
        #endregion
        #endregion

        #region Refresh
        public static void RefreshAllGameObjectsData()
        {
            if (!HD_Settings.EnableGameObjectMainIcon &&
                !HD_Settings.EnableGameObjectComponentIcons &&
                !HD_Settings.EnableHierarchyTree &&
                !HD_Settings.EnableGameObjectTag &&
                !HD_Settings.EnableGameObjectLayer)
            {
                Debug.Log("No GameObject data is enabled for refreshing.");
                return;
            }

#if UNITY_6000_4_OR_NEWER
            GameObject[] allGameObjects = UnityEngine.Object.FindObjectsByType<GameObject>();
#elif UNITY_6000_0_OR_NEWER
            GameObject[] allGameObjects = UnityEngine.Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
#else
            GameObject[] allGameObjects = UnityEngine.Object.FindObjectsOfType<GameObject>();
#endif

            foreach (GameObject gameObject in allGameObjects)
            {
                RefreshGameObjectData(gameObject);
            }
        }

        public static void RefreshSelectedGameObjectsData()
        {
            GameObject[] selectedGameObjects = Selection.gameObjects;
            if (selectedGameObjects.Length > 0)
            {
                foreach (GameObject selectedGameObject in selectedGameObjects)
                {
                    RefreshGameObjectData(selectedGameObject);
                }
            }
            else
            {
                Debug.LogWarning("No GameObject selected. Please select one or more GameObjects to refresh their data.");
            }
        }

        public static void RefreshMainIconForSelectedGameObject()
        {
            if (!HD_Settings.EnableGameObjectMainIcon || HD_Settings.MainIconUpdateMode != HD_Settings.UpdateMode.Smart) return;

            GameObject[] selectedGameObjects = Selection.gameObjects;
            if (selectedGameObjects.Length > 0)
            {
                foreach (GameObject selectedGameObject in selectedGameObjects)
                {
                    int instanceID = GetGameObjectCacheID(selectedGameObject);
                    if (HD_Manager.gameObjectDataCache.TryGetValue(instanceID, out HD_Manager.GameObjectData data))
                    {
                        data.MainIcon = HD_Manager.GetGameObjectMainIcon(selectedGameObject);
                        HD_Manager.gameObjectDataCache[instanceID] = data;
                    }
                }
            }
            else
            {
                Debug.LogWarning("No GameObject selected. Please select a GameObject to refresh its main icon.");
            }
        }

        public static void RefreshComponentIconsForSelectedGameObjects()
        {
            if (!HD_Settings.EnableGameObjectComponentIcons || HD_Settings.ComponentsIconsUpdateMode != HD_Settings.UpdateMode.Smart) return;

            GameObject[] selectedGameObjects = Selection.gameObjects;
            if (selectedGameObjects.Length > 0)
            {
                foreach (GameObject selectedGameObject in selectedGameObjects)
                {
                    int instanceID = GetGameObjectCacheID(selectedGameObject);
                    if (HD_Manager.gameObjectDataCache.TryGetValue(instanceID, out HD_Manager.GameObjectData data))
                    {
                        data.ComponentIcons = HD_Manager.GetComponentIcons(selectedGameObject);
                        HD_Manager.gameObjectDataCache[instanceID] = data;
                    }
                }
            }
            else
            {
                Debug.LogWarning("No GameObject selected. Please select one or more GameObjects to refresh their component icons.");
            }
        }

        public static void RefreshHierarchyTreeIconForSelectedGameObjects()
        {
            if (!HD_Settings.EnableHierarchyTree || HD_Settings.HierarchyTreeUpdateMode != HD_Settings.UpdateMode.Smart) return;

            GameObject[] selectedGameObjects = Selection.gameObjects;
            if (selectedGameObjects.Length > 0)
            {
                foreach (GameObject selectedGameObject in selectedGameObjects)
                {
                    int instanceID = GetGameObjectCacheID(selectedGameObject);
                    if (HD_Manager.gameObjectDataCache.TryGetValue(instanceID, out HD_Manager.GameObjectData data))
                    {
                        data.HierarchyTreeIcon = HD_Manager.GetOrCreateBranchIcon(selectedGameObject.transform);
                        HD_Manager.gameObjectDataCache[instanceID] = data;
                    }
                }
            }
            else
            {
                Debug.LogWarning("No GameObject selected. Please select one or more GameObjects to refresh their hierarchy tree icons.");
            }
        }

        public static void RefreshTagForSelectedGameObjects()
        {
            if (!HD_Settings.EnableGameObjectTag || HD_Settings.TagUpdateMode != HD_Settings.UpdateMode.Smart) return;

            GameObject[] selectedGameObjects = Selection.gameObjects;
            if (selectedGameObjects.Length > 0)
            {
                foreach (GameObject selectedGameObject in selectedGameObjects)
                {
                    int instanceID = GetGameObjectCacheID(selectedGameObject);
                    if (HD_Manager.gameObjectDataCache.TryGetValue(instanceID, out HD_Manager.GameObjectData data))
                    {
                        data.Tag = selectedGameObject.tag;
                        HD_Manager.gameObjectDataCache[instanceID] = data;
                    }
                }
            }
            else
            {
                Debug.LogWarning("No GameObject selected. Please select one or more GameObjects to refresh their tags.");
            }
        }

        public static void RefreshLayerForSelectedGameObjects()
        {
            if (!HD_Settings.EnableGameObjectLayer || HD_Settings.LayerUpdateMode != HD_Settings.UpdateMode.Smart) return;

            GameObject[] selectedGameObjects = Selection.gameObjects;
            if (selectedGameObjects.Length > 0)
            {
                foreach (GameObject selectedGameObject in selectedGameObjects)
                {
                    int instanceID = GetGameObjectCacheID(selectedGameObject);
                    if (HD_Manager.gameObjectDataCache.TryGetValue(instanceID, out HD_Manager.GameObjectData data))
                    {
                        data.Layer = LayerMask.LayerToName(selectedGameObject.layer);
                        HD_Manager.gameObjectDataCache[instanceID] = data;
                    }
                }
            }
            else
            {
                Debug.LogWarning("No GameObject selected. Please select one or more GameObjects to refresh their layers.");
            }
        }

        public static void RefreshGameObjectData(GameObject gameObject)
        {
            int instanceID = GetGameObjectCacheID(gameObject);
            if (!HD_Manager.gameObjectDataCache.TryGetValue(instanceID, out HD_Manager.GameObjectData data))
            {
                data = new HD_Manager.GameObjectData();
            }

            if (HD_Settings.EnableGameObjectMainIcon)
            {
                data.MainIcon = HD_Manager.GetGameObjectMainIcon(gameObject);
            }

            if (HD_Settings.EnableGameObjectComponentIcons)
            {
                data.ComponentIcons = HD_Manager.GetComponentIcons(gameObject);
            }

            if (HD_Settings.EnableHierarchyTree && gameObject.transform.parent != null)
            {
                data.HierarchyTreeIcon = HD_Manager.GetOrCreateBranchIcon(gameObject.transform);
            }

            if (HD_Settings.EnableGameObjectTag)
            {
                data.Tag = gameObject.tag;
            }

            if (HD_Settings.EnableGameObjectLayer)
            {
                data.Layer = LayerMask.LayerToName(gameObject.layer);
            }

            HD_Manager.gameObjectDataCache[instanceID] = data;
        }
        #endregion

        #region Helpers
        private static int GetGameObjectCacheID(GameObject gameObject)
        {
#if UNITY_6000_5_OR_NEWER
            return gameObject.GetEntityId().GetHashCode();
#elif UNITY_6000_4_OR_NEWER
#pragma warning disable CS0618
    int instanceID = gameObject.GetInstanceID();
#pragma warning restore CS0618
    return instanceID;
#else
    return gameObject.GetInstanceID();
#endif
        }
        #endregion

        #region General
        public static void CollapseAllGameObjects()
        {
            Scene activeScene = SceneManager.GetActiveScene();
            if (!activeScene.IsValid()) return;

            foreach (GameObject rootObject in activeScene.GetRootGameObjects())
            {
                CollapseRecursive(rootObject);
            }
        }

        public static void ExpandAllGameObjects()
        {
            Scene activeScene = SceneManager.GetActiveScene();
            if (!activeScene.IsValid()) return;

            foreach (GameObject rootObject in activeScene.GetRootGameObjects())
            {
                ExpandRecursive(rootObject);
            }
        }

        #if UNITY_6000_5_OR_NEWER
        private static void CollapseRecursive(GameObject obj)
        {
            SetExpandedRecursive(obj.GetEntityId(), false);
        }

        private static void ExpandRecursive(GameObject obj)
        {
            SetExpandedRecursive(obj.GetEntityId(), true);
        }

        private static void InitializeHierarchyExpandDelegates(EditorWindow hierarchyWindow)
        {
            setExpandedEntityIdDelegate = null;
            setExpandedRecursiveEntityIdDelegate = null;

            Type entityIdType = typeof(UnityEngine.EntityId);
            Type hierarchyWindowType = hierarchyWindow.GetType();

            setExpandedEntityIdDelegate = CompileHierarchyExpandDelegate(hierarchyWindow, hierarchyWindowType, entityIdType, "SetExpanded");
            setExpandedRecursiveEntityIdDelegate = CompileHierarchyExpandDelegate(hierarchyWindow, hierarchyWindowType, entityIdType, "SetExpandedRecursive");
        }

        private static void SetExpandedRecursive(UnityEngine.EntityId entityId, bool expand)
        {
            EditorWindow hierarchyWindow = GetHierarchyWindow();
            if (hierarchyWindow == null) return;

            if (!isReflectionInitialized || cachedHierarchyWindow != hierarchyWindow)
            {
                cachedHierarchyWindow = hierarchyWindow;
                InitializeHierarchyExpandDelegates(hierarchyWindow);
                isReflectionInitialized = true;
            }

            if (setExpandedRecursiveEntityIdDelegate != null)
            {
                setExpandedRecursiveEntityIdDelegate(entityId, expand);
            }
            else
            {
                setExpandedEntityIdDelegate?.Invoke(entityId, expand);
            }
        }

        private static Action<UnityEngine.EntityId, bool> CompileHierarchyExpandDelegate(EditorWindow hierarchyWindow, Type hierarchyWindowType, Type entityIdType, string methodName)
        {
            MethodInfo method = hierarchyWindowType.GetMethod(
                methodName,
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public,
                null,
                new Type[] { entityIdType, typeof(bool) },
                null);

            if (method == null) return null;

            System.Linq.Expressions.ParameterExpression entityIdParameter = System.Linq.Expressions.Expression.Parameter(typeof(UnityEngine.EntityId), "entityId");
            System.Linq.Expressions.ParameterExpression expandParameter = System.Linq.Expressions.Expression.Parameter(typeof(bool), "expand");
            System.Linq.Expressions.ConstantExpression hierarchyWindowExpression = System.Linq.Expressions.Expression.Constant(hierarchyWindow);
            System.Linq.Expressions.MethodCallExpression methodCallExpression = System.Linq.Expressions.Expression.Call(hierarchyWindowExpression, method, entityIdParameter, expandParameter);

            return System.Linq.Expressions.Expression
                .Lambda<Action<UnityEngine.EntityId, bool>>(methodCallExpression, entityIdParameter, expandParameter)
                .Compile();
        }
       
        #elif UNITY_6000_4_OR_NEWER
        private static void CollapseRecursive(GameObject obj)
        {
        #pragma warning disable CS0618
            SetExpandedRecursive(obj.GetInstanceID(), false);
        #pragma warning restore CS0618
        }

        private static void ExpandRecursive(GameObject obj)
        {
        #pragma warning disable CS0618
            SetExpandedRecursive(obj.GetInstanceID(), true);
        #pragma warning restore CS0618
        }

        private static void InitializeHierarchyExpandDelegates(EditorWindow hierarchyWindow)
        {
            setExpandedDelegate = null;
            setExpandedRecursiveDelegate = null;

            Type entityIdType = typeof(UnityEngine.EntityId);
            Type hierarchyWindowType = hierarchyWindow.GetType();

            MethodInfo conversionOperation = null;
            MethodInfo[] operations = entityIdType.GetMethods(BindingFlags.Static | BindingFlags.Public);
            foreach (MethodInfo operation in operations)
            {
                if ((operation.Name == "op_Implicit" || operation.Name == "op_Explicit") && operation.ReturnType == entityIdType)
                {
                    ParameterInfo[] parameters = operation.GetParameters();
                    if (parameters.Length == 1 && parameters[0].ParameterType == typeof(int))
                    {
                        conversionOperation = operation;
                        break;
                    }
                }
            }

            if (conversionOperation == null)
            {
                Debug.LogWarning("Hierarchy Designer could not find the int to EntityId conversion operation.");
                return;
            }

            setExpandedDelegate = CompileHierarchyExpandDelegate(hierarchyWindow, hierarchyWindowType, entityIdType, conversionOperation, "SetExpanded");
            setExpandedRecursiveDelegate = CompileHierarchyExpandDelegate(hierarchyWindow, hierarchyWindowType, entityIdType, conversionOperation, "SetExpandedRecursive");
        }

        private static void SetExpandedRecursive(int instanceID, bool expand)
        {
            EditorWindow hierarchyWindow = GetHierarchyWindow();
            if (hierarchyWindow == null) return;

            if (!isReflectionInitialized || cachedHierarchyWindow != hierarchyWindow)
            {
                cachedHierarchyWindow = hierarchyWindow;
                InitializeHierarchyExpandDelegates(hierarchyWindow);
                isReflectionInitialized = true;
            }

            if (setExpandedRecursiveDelegate != null)
            {
                setExpandedRecursiveDelegate(instanceID, expand);
            }
            else
            {
                setExpandedDelegate?.Invoke(instanceID, expand);
            }
        }

        private static Action<int, bool> CompileHierarchyExpandDelegate(EditorWindow hierarchyWindow, Type hierarchyWindowType, Type entityIdType, MethodInfo conversionOperation, string methodName)
        {
            MethodInfo method = hierarchyWindowType.GetMethod(
                methodName,
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public,
                null,
                new Type[] { entityIdType, typeof(bool) },
                null);

            if (method == null) return null;

            System.Linq.Expressions.ParameterExpression idParameter = System.Linq.Expressions.Expression.Parameter(typeof(int), "instanceID");
            System.Linq.Expressions.ParameterExpression expandParameter = System.Linq.Expressions.Expression.Parameter(typeof(bool), "expand");
            System.Linq.Expressions.ConstantExpression hierarchyWindowExpression = System.Linq.Expressions.Expression.Constant(hierarchyWindow);
            System.Linq.Expressions.MethodCallExpression entityIdExpression = System.Linq.Expressions.Expression.Call(conversionOperation, idParameter);
            System.Linq.Expressions.MethodCallExpression methodCallExpression = System.Linq.Expressions.Expression.Call(hierarchyWindowExpression, method, entityIdExpression, expandParameter);

            return System.Linq.Expressions.Expression
                .Lambda<Action<int, bool>>(methodCallExpression, idParameter, expandParameter)
                .Compile();
        }
#else
        private static void CollapseRecursive(GameObject obj)
        {
            int instanceID = obj.GetInstanceID();
            SetExpanded(instanceID, false);

            foreach (Transform child in obj.transform)
            {
                CollapseRecursive(child.gameObject);
            }
        }

        private static void ExpandRecursive(GameObject obj)
        {
            int instanceID = obj.GetInstanceID();
            SetExpanded(instanceID, true);

            foreach (Transform child in obj.transform)
            {
                ExpandRecursive(child.gameObject);
            }
        }

        private static void SetExpanded(int instanceID, bool expand)
        {
            EditorWindow hierarchyWindow = GetHierarchyWindow();
            if (hierarchyWindow == null) return;

            MethodInfo expandMethod = hierarchyWindow.GetType().GetMethod("SetExpanded", BindingFlags.Instance | BindingFlags.NonPublic);
            if (expandMethod != null)
            {
                expandMethod.Invoke(hierarchyWindow, new object[] { instanceID, expand });
            }
        }
#endif

        public static bool IsHierarchyMostlyExpanded()
        {
            try
            {
                Type shwType = Type.GetType("UnityEditor.SceneHierarchyWindow, UnityEditor");
                if (shwType == null) return false;

                EditorWindow shw = Resources.FindObjectsOfTypeAll(shwType).FirstOrDefault() as EditorWindow;
                if (shw == null) return false;

                BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

                object sceneHierarchy = shwType.GetField("m_SceneHierarchy", flags)?.GetValue(shw) ??
                                        shwType.GetProperty("sceneHierarchy", flags)?.GetValue(shw);

                if (sceneHierarchy == null) return false;

                MethodInfo getExpanded = sceneHierarchy.GetType().GetMethod("GetExpandedIDs", flags);
                if (getExpanded == null) return false;

                if (getExpanded.Invoke(sceneHierarchy, null) is not IEnumerable expandedEnum) return false;

#if UNITY_6000_5_OR_NEWER
                HashSet<UnityEngine.EntityId> expanded = new();
                foreach (object o in expandedEnum)
                {
                    if (o is UnityEngine.EntityId entityId)
                    {
                        expanded.Add(entityId);
                    }
                }
#else
HashSet<int> expanded = new();
foreach (object o in expandedEnum)
{
    if (o == null) continue;

    if (int.TryParse(o.ToString(), out int id))
    {
        expanded.Add(id);
    }
}
#endif

                int totalWithChildren = 0;
                int expandedWithChildren = 0;

                for (int s = 0; s < SceneManager.sceneCount; s++)
                {
                    Scene scene = SceneManager.GetSceneAt(s);
                    if (!scene.IsValid() || !scene.isLoaded) continue;

                    foreach (GameObject root in scene.GetRootGameObjects())
                    {
                        Tally(root.transform, expanded, ref totalWithChildren, ref expandedWithChildren);
                    }
                }

                if (totalWithChildren == 0) return false;

                float ratio = (float)expandedWithChildren / totalWithChildren;
                return ratio >= 0.6f;
            }
            catch
            {
                return false;
            }

#if UNITY_6000_5_OR_NEWER
            static void Tally(Transform t, HashSet<UnityEngine.EntityId> expanded, ref int total, ref int expandedCount)
#else
static void Tally(Transform t, HashSet<int> expanded, ref int total, ref int expandedCount)
#endif
            {
                if (t.childCount > 0)
                {
                    total++;

#if UNITY_6000_5_OR_NEWER
                    if (expanded.Contains(t.gameObject.GetEntityId())) expandedCount++;
#elif UNITY_6000_4_OR_NEWER
#pragma warning disable CS0618
        if (expanded.Contains(t.gameObject.GetInstanceID())) expandedCount++;
#pragma warning restore CS0618
#else
        if (expanded.Contains(t.gameObject.GetInstanceID())) expandedCount++;
#endif
                }

                for (int i = 0; i < t.childCount; i++)
                {
                    Tally(t.GetChild(i), expanded, ref total, ref expandedCount);
                }
            }
        }

        private static EditorWindow GetHierarchyWindow()
        {
            EditorWindow[] windows = Resources.FindObjectsOfTypeAll<EditorWindow>();
            foreach (EditorWindow window in windows)
            {
                if (window.titleContent.text == HD_Constants.HierarchyWindow)
                {
                    return window;
                }
            }
            return null;
        }
        #endregion

        #region Presets
        public static void ApplyPresetToFolders(HD_Presets.HD_Preset preset)
        {
            Dictionary<string, HD_Folders.HD_FolderData> foldersData = HD_Folders.GetAllFoldersData(false);
            foreach (KeyValuePair<string, HD_Folders.HD_FolderData> folder in foldersData)
            {
                HD_Folders.SetFolderData(folder.Key, preset.folderTextColor, preset.folderFontSize, preset.folderFontStyle, preset.folderColor, preset.folderImageType);
            }
        }

        public static void ApplyPresetToSeparators(HD_Presets.HD_Preset preset)
        {
            Dictionary<string, HD_Separators.HD_SeparatorData> separatorsData = HD_Separators.GetAllSeparatorsData(false);
            foreach (KeyValuePair<string, HD_Separators.HD_SeparatorData> separator in separatorsData)
            {
                HD_Separators.SetSeparatorData(separator.Key,
                    preset.separatorTextColor,
                    preset.separatorIsGradientBackground,
                    preset.separatorBackgroundColor,
                    preset.separatorBackgroundGradient,
                    preset.separatorFontSize,
                    preset.separatorFontStyle,
                    preset.separatorTextAlignment,
                    preset.separatorBackgroundImageType);
            }
        }

        public static void ApplyPresetToTag(HD_Presets.HD_Preset preset)
        {
            HD_Settings.TagColor = preset.tagTextColor;
            HD_Settings.TagFontStyle = preset.tagFontStyle;
            HD_Settings.TagFontSize = preset.tagFontSize;
            HD_Settings.TagTextAnchor = preset.tagTextAnchor;
        }

        public static void ApplyPresetToLayer(HD_Presets.HD_Preset preset)
        {
            HD_Settings.LayerColor = preset.layerTextColor;
            HD_Settings.LayerFontStyle = preset.layerFontStyle;
            HD_Settings.LayerFontSize = preset.layerFontSize;
            HD_Settings.LayerTextAnchor = preset.layerTextAnchor;
        }

        public static void ApplyPresetToTree(HD_Presets.HD_Preset preset)
        {
            HD_Settings.HierarchyTreeColor = preset.treeColor;
        }

        public static void ApplyPresetToLines(HD_Presets.HD_Preset preset)
        {
            HD_Settings.HierarchyLineColor = preset.hierarchyLineColor;
        }

        public static void ApplyPresetToHierarchyButtons(HD_Presets.HD_Preset preset)
        {
            HD_Settings.HierarchyButtonLockColor = preset.hierarchyButtonLockColor;
            HD_Settings.HierarchyButtonVisibilityColor = preset.hierarchyButtonVisibilityColor;
        }

        public static void ApplyPresetToDefaultFolderValues(HD_Presets.HD_Preset preset)
        {
            HD_Settings.FolderDefaultTextColor = preset.folderTextColor;
            HD_Settings.FolderDefaultFontSize = preset.folderFontSize;
            HD_Settings.FolderDefaultFontStyle = preset.folderFontStyle;
            HD_Settings.FolderDefaultImageColor = preset.folderColor;
            HD_Settings.FolderDefaultImageType = preset.folderImageType;
        }

        public static void ApplyPresetToDefaultSeparatorValues(HD_Presets.HD_Preset preset)
        {
            HD_Settings.SeparatorDefaultTextColor = preset.separatorTextColor;
            HD_Settings.SeparatorDefaultIsGradientBackground = preset.separatorIsGradientBackground;
            HD_Settings.SeparatorDefaultBackgroundColor = preset.separatorBackgroundColor;
            HD_Settings.SeparatorDefaultBackgroundGradient = preset.separatorBackgroundGradient;
            HD_Settings.SeparatorDefaultFontSize = preset.separatorFontSize;
            HD_Settings.SeparatorDefaultFontStyle = preset.separatorFontStyle;
            HD_Settings.SeparatorDefaultTextAnchor = preset.separatorTextAlignment;
            HD_Settings.SeparatorDefaultImageType = preset.separatorBackgroundImageType;
        }

        public static void ApplyPresetToLockLabel(HD_Presets.HD_Preset preset)
        {
            HD_Settings.LockColor = preset.lockColor;
            HD_Settings.LockFontSize = preset.lockFontSize;
            HD_Settings.LockFontStyle = preset.lockFontStyle;
            HD_Settings.LockTextAnchor = preset.lockTextAnchor;
        }
        #endregion
    }
}
#endif