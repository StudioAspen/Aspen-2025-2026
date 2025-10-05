// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Animancer.Editor
{
    /// <summary>[Editor-Only] Various utilities used throughout Animancer.</summary>
    /// https://kybernetik.com.au/animancer/api/Animancer.Editor/AnimancerEditorUtilities
    public static partial class AnimancerEditorUtilities
    {
        /************************************************************************************************************************/
        #region Misc
        /************************************************************************************************************************/

        /// <summary>[Animancer Extension] [Editor-Only] Is the <see cref="Vector2.x"/> or <see cref="Vector2.y"/> NaN?</summary>
        public static bool IsNaN(this Vector2 vector)
            => float.IsNaN(vector.x)
            || float.IsNaN(vector.y);

        /// <summary>[Animancer Extension] [Editor-Only] Is the <see cref="Vector3.x"/>, <see cref="Vector3.y"/>, or <see cref="Vector3.z"/> NaN?</summary>
        public static bool IsNaN(this Vector3 vector)
            => float.IsNaN(vector.x)
            || float.IsNaN(vector.y)
            || float.IsNaN(vector.z);

        /************************************************************************************************************************/

        /// <summary>Returns the value of `t` linearly interpolated along the X axis of the `rect`.</summary>
        public static float LerpUnclampedX(this Rect rect, float t)
            => rect.x + rect.width * t;

        /// <summary>Returns the value of `t` inverse linearly interpolated along the X axis of the `rect`.</summary>
        public static float InverseLerpUnclampedX(this Rect rect, float t)
            => (t - rect.x) / rect.width;

        /************************************************************************************************************************/

        /// <summary>Finds an asset of the specified type anywhere in the project.</summary>
        public static T FindAssetOfType<T>()
            where T : Object
        {
            var filter = typeof(Component).IsAssignableFrom(typeof(T))
                ? $"t:{nameof(GameObject)}"
                : $"t:{typeof(T).Name}";

            var guids = AssetDatabase.FindAssets(filter);
            if (guids.Length == 0)
                return null;

            for (int i = 0; i < guids.Length; i++)
            {
                var path = AssetDatabase.GUIDToAssetPath(guids[i]);
                var asset = AssetDatabase.LoadAssetAtPath<T>(path);
                if (asset != null)
                    return asset;
            }

            return null;
        }

        /************************************************************************************************************************/

        /// <summary>Finds or creates an instance of <typeparamref name="T"/>.</summary>
        public static T FindOrCreate<T>(ref T scriptableObject, HideFlags hideFlags = default)
            where T : ScriptableObject
        {
            if (scriptableObject != null)
                return scriptableObject;

            var instances = Resources.FindObjectsOfTypeAll<T>();
            if (instances.Length > 0)
            {
                scriptableObject = instances[0];
            }
            else
            {
                scriptableObject = ScriptableObject.CreateInstance<T>();
                scriptableObject.hideFlags = hideFlags;
            }

            return scriptableObject;
        }

        /************************************************************************************************************************/

        /// <summary>The most recent <see cref="PlayModeStateChange"/>.</summary>
        public static PlayModeStateChange PlayModeState { get; private set; }

        /// <summary>Is the Unity Editor is currently changing between Play Mode and Edit Mode?</summary>
        public static bool IsChangingPlayMode =>
            PlayModeState == PlayModeStateChange.ExitingEditMode ||
            PlayModeState == PlayModeStateChange.ExitingPlayMode;

        [InitializeOnLoadMethod]
        private static void WatchForPlayModeChanges()
        {
            PlayModeState = EditorApplication.isPlayingOrWillChangePlaymode
                ? EditorApplication.isPlaying
                    ? PlayModeStateChange.EnteredPlayMode
                    : PlayModeStateChange.ExitingEditMode
                : PlayModeStateChange.EnteredEditMode;

            EditorApplication.playModeStateChanged += change => PlayModeState = change;
        }

        /************************************************************************************************************************/

        /// <summary>Deletes the specified `subAsset`.</summary>
        public static void DeleteSubAsset(Object subAsset)
        {
            AssetDatabase.RemoveObjectFromAsset(subAsset);
            AssetDatabase.SaveAssets();

            Object.DestroyImmediate(subAsset, true);
        }

        /************************************************************************************************************************/

        /// <summary>Calculates the overall bounds of all renderers under the `transform`.</summary>
        public static Bounds CalculateBounds(Transform transform)
        {
            using var _ = ListPool<Renderer>.Instance.Acquire(out var renderers);

            transform.GetComponentsInChildren(renderers);
            if (renderers.Count == 0)
                return default;

            var bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Count; i++)
                bounds.Encapsulate(renderers[i].bounds);

            return bounds;
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Collections
        /************************************************************************************************************************/

        /// <summary>Adds default items or removes items to make the <see cref="List{T}.Count"/> equal to the `count`.</summary>
        public static void SetCount<T>(List<T> list, int count)
        {
            if (list.Count < count)
            {
                while (list.Count < count)
                    list.Add(default);
            }
            else
            {
                list.RemoveRange(count, list.Count - count);
            }
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Removes any items from the `list` that are <c>null</c> and items that appear multiple times.
        /// Returns true if the `list` was modified.
        /// </summary>
        public static bool RemoveMissingAndDuplicates(ref List<GameObject> list)
        {
            if (list == null)
            {
                list = new();
                return false;
            }

            var modified = false;

            using (SetPool<Object>.Instance.Acquire(out var previousItems))
            {
                for (int i = list.Count - 1; i >= 0; i--)
                {
                    var item = list[i];
                    if (item == null || previousItems.Contains(item))
                    {
                        list.RemoveAt(i);
                        modified = true;
                    }
                    else
                    {
                        previousItems.Add(item);
                    }
                }
            }

            return modified;
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Removes any <c>null</c> items and ensures that it contains
        /// an instance of each type derived from <typeparamref name="T"/>.
        /// </summary>
        public static void InstantiateDerivedTypes<T>(ref List<T> list)
            where T : IComparable<T>
        {
            if (list == null)
            {
                list = new();
            }
            else
            {
                for (int i = list.Count - 1; i >= 0; i--)
                    if (list[i] == null)
                        list.RemoveAt(i);
            }

            var types = TypeSelectionMenu.GetDerivedTypes(typeof(T));
            for (int i = 0; i < types.Count; i++)
            {
                var toolType = types[i];
                if (IndexOfType(list, toolType) >= 0)
                    continue;

                var instance = (T)Activator.CreateInstance(toolType);
                list.Add(instance);
            }

            list.Sort();
        }

        /************************************************************************************************************************/

        /// <summary>Finds the index of the first item with the specified `type`.</summary>
        public static int IndexOfType<T>(IList<T> list, Type type)
        {
            for (int i = 0; i < list.Count; i++)
                if (list[i].GetType() == type)
                    return i;

            return -1;
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Context Menus
        /************************************************************************************************************************/

        /// <summary>
        /// Adds a menu function which passes the result of <see cref="CalculateEditorFadeDuration"/> into `startFade`.
        /// </summary>
        public static void AddFadeFunction(
            GenericMenu menu,
            string label,
            bool isEnabled,
            AnimancerNode node,
            Action<float> startFade)
        {
            // Fade functions need to be delayed twice since the context menu itself causes the next frame delta
            // time to be unreasonably high (which would skip the start of the fade).
            menu.AddFunction(label, isEnabled,
                () => EditorApplication.delayCall +=
                () => EditorApplication.delayCall +=
                () =>
                {
                    startFade(node.CalculateEditorFadeDuration());
                });
        }

        /// <summary>[Animancer Extension] [Editor-Only]
        /// Returns the duration of the `node`s current fade (if any), otherwise returns the `defaultDuration`.
        /// </summary>
        public static float CalculateEditorFadeDuration(this AnimancerNode node, float defaultDuration = 1)
            => node.FadeSpeed > 0
            ? 1 / node.FadeSpeed
            : defaultDuration;

        /************************************************************************************************************************/

        /// <summary>
        /// Adds a menu function to open a web page. If the `linkSuffix` starts with a '/' then it will be relative to
        /// the <see cref="Strings.DocsURLs.Documentation"/>.
        /// </summary>
        public static void AddDocumentationLink(GenericMenu menu, string label, string linkSuffix)
        {
            if (linkSuffix[0] == '/')
                linkSuffix = Strings.DocsURLs.Documentation + linkSuffix;

            menu.AddItem(new(label), false, () =>
            {
                EditorUtility.OpenWithDefaultApp(linkSuffix);
            });
        }

        /************************************************************************************************************************/

        /// <summary>Is the <see cref="MenuCommand.context"/> editable?</summary>
        [MenuItem("CONTEXT/" + nameof(AnimationClip) + "/Toggle Looping", validate = true)]
        [MenuItem("CONTEXT/" + nameof(AnimationClip) + "/Toggle Legacy", validate = true)]
        private static bool ValidateEditable(MenuCommand command)
        {
            return (command.context.hideFlags & HideFlags.NotEditable) != HideFlags.NotEditable;
        }

        /************************************************************************************************************************/

        /// <summary>Toggles the <see cref="Motion.isLooping"/> flag between true and false.</summary>
        [MenuItem("CONTEXT/" + nameof(AnimationClip) + "/Toggle Looping")]
        private static void ToggleLooping(MenuCommand command)
        {
            var clip = (AnimationClip)command.context;
            SetLooping(clip, !clip.isLooping);
        }

        /// <summary>Sets the <see cref="Motion.isLooping"/> flag.</summary>
        public static void SetLooping(AnimationClip clip, bool looping)
        {
            var settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = looping;
            AnimationUtility.SetAnimationClipSettings(clip, settings);

            Debug.Log($"Set {clip.name} to be {(looping ? "Looping" : "Not Looping")}." +
                " Note that you may need to restart Unity for this change to take effect.", clip);

            // None of these let us avoid the need to restart Unity.
            //EditorUtility.SetDirty(clip);
            //AssetDatabase.SaveAssets();

            //var path = AssetDatabase.GetAssetPath(clip);
            //AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        }

        /************************************************************************************************************************/

        /// <summary>Swaps the <see cref="AnimationClip.legacy"/> flag between true and false.</summary>
        [MenuItem("CONTEXT/" + nameof(AnimationClip) + "/Toggle Legacy")]
        private static void ToggleLegacy(MenuCommand command)
        {
            var clip = (AnimationClip)command.context;
            clip.legacy = !clip.legacy;
        }

        /************************************************************************************************************************/

        /// <summary>Calls <see cref="Animator.Rebind"/>.</summary>
        [MenuItem("CONTEXT/" + nameof(Animator) + "/Restore Bind Pose", priority = 110)]
        private static void RestoreBindPose(MenuCommand command)
        {
            var animator = (Animator)command.context;

            Undo.RegisterFullObjectHierarchyUndo(animator.gameObject, "Restore bind pose");

            const string TypeName = "UnityEditor.AvatarSetupTool, UnityEditor";
            var type = Type.GetType(TypeName)
                ?? throw new TypeLoadException($"Unable to find the type '{TypeName}'");

            const string MethodName = "SampleBindPose";
            var method = type.GetMethod(MethodName, AnimancerReflection.StaticBindings)
                ?? throw new MissingMethodException($"Unable to find the method '{MethodName}'");

            method.Invoke(null, new object[] { animator.gameObject });
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
    }
}

#endif

