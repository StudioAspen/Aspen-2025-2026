// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

#if UNITY_EDITOR && UNITY_IMGUI

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static Animancer.Editor.AnimancerGUI;

namespace Animancer.Editor.Previews
{
    /// <summary>[Editor-Only] GUI utilities for <see cref="AnimancerPreviewObject"/>.</summary>
    /// https://kybernetik.com.au/animancer/api/Animancer.Editor.Previews/AnimancerPreviewObjectGUI
    public static class AnimancerPreviewObjectGUI
    {
        /************************************************************************************************************************/

        /// <summary>Calculates the pixel height required to draw the `preview`.</summary>
        public static float CalculateHeight(AnimancerPreviewObject preview)
        {
            var lines = 1;

            var instanceAnimators = preview.InstanceAnimators;
            if (instanceAnimators != null &&
                instanceAnimators.Length > 1)
                lines++;

            return AnimancerGUI.CalculateHeight(lines);
        }

        /************************************************************************************************************************/

        /// <summary>Draws the model selection GUI.</summary>
        public static void DoModelGUI(AnimancerPreviewObject preview)
        {
            var area = LayoutRect(CalculateHeight(preview));
            DoModelGUI(area, preview);
        }

        /************************************************************************************************************************/

        /// <summary>Draws the model selection GUI.</summary>
        public static void DoModelGUI(Rect area, AnimancerPreviewObject preview)
        {
            var root = preview.OriginalObject;
            var model = root != null ? root.gameObject : null;

            EditorGUI.BeginChangeCheck();

            var warning = GetModelWarning(model, preview);
            var color = GUI.color;
            if (warning != null)
                GUI.color = WarningFieldColor;

            using (var label = PooledGUIContent.Acquire("Model"))
            {
                var objectFieldArea = StealLineFromTop(ref area);
                if (DoDropdownObjectFieldGUI(objectFieldArea, label, true, ref model))
                {
                    var menu = new GenericMenu();

                    menu.AddItem(
                        new("Default Humanoid"),
                        model != null && model == TransitionPreviewSettings.GetOrCreateDefaultHumanoid(null),
                        () => preview.OriginalObject
                            = TransitionPreviewSettings.GetOrCreateDefaultHumanoid(preview.InstanceRoot).transform);
                    menu.AddItem(
                        new("Default Sprite"),
                        model != null && model == TransitionPreviewSettings.GetDefaultSpriteIfAlreadyCreated(),
                        () => preview.OriginalObject
                            = TransitionPreviewSettings.GetOrCreateDefaultSprite(preview.InstanceRoot).transform);

                    var persistentModels = TransitionPreviewSettings.Models;
                    var temporaryModels = TemporarySettings.PreviewModels;
                    if (persistentModels.Count == 0 && temporaryModels.Count == 0)
                    {
                        menu.AddDisabledItem(new("No model prefabs have been used yet"));
                    }
                    else
                    {
                        AddModelSelectionFunctions(menu, preview, persistentModels, model);
                        AddModelSelectionFunctions(menu, preview, temporaryModels, model);
                    }

                    menu.ShowAsContext();
                }
            }

            GUI.color = color;

            if (EditorGUI.EndChangeCheck())
                preview.OriginalObject = model != null ? model.transform : null;

            if (warning != null)
                EditorGUILayout.HelpBox(warning, MessageType.Warning, true);

            DoAnimatorSelectorGUI(preview);
        }

        /************************************************************************************************************************/

        /// <summary>Adds menu functions for selecting each of the `models`.</summary>
        private static void AddModelSelectionFunctions(
            GenericMenu menu,
            AnimancerPreviewObject preview,
            List<GameObject> models,
            GameObject selected)
        {
            for (int i = models.Count - 1; i >= 0; i--)
            {
                var model = models[i];
                var path = AssetDatabase.GetAssetPath(model);
                if (!string.IsNullOrEmpty(path))
                    path = path.Replace('/', '\\');
                else
                    path = model.name;

                menu.AddItem(new(path), model == selected,
                    () => preview.OriginalObject = model.transform);
            }
        }

        /************************************************************************************************************************/

        /// <summary>Returns a warning about the selected model or <c>null</c>.</summary>
        private static string GetModelWarning(
            GameObject model,
            AnimancerPreviewObject preview)
        {
            if (model == null)
                return "No Model is selected so nothing can be previewed.";

            if (preview.SelectedInstanceAnimator == null)
                return "The selected Model has no Animator component.";

            return null;
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Draws a button for selecting which <see cref="Animator"/> to control if there's more than one.
        /// </summary>
        private static void DoAnimatorSelectorGUI(AnimancerPreviewObject preview)
        {
            var instanceAnimators = preview.InstanceAnimators;
            if (instanceAnimators == null ||
                instanceAnimators.Length <= 1)
                return;

            var area = LayoutSingleLineRect(SpacingMode.After);
            var labelArea = StealFromLeft(ref area, EditorGUIUtility.labelWidth, StandardSpacing);
            GUI.Label(labelArea, nameof(Animator));

            var selectedAnimator = preview.SelectedInstanceAnimator;
            using (var label = PooledGUIContent.Acquire(
                selectedAnimator != null ? selectedAnimator.name : "None"))
            {
                var clicked = EditorGUI.DropdownButton(area, label, FocusType.Passive);

                if (!clicked)
                    return;

                var menu = new GenericMenu();

                for (int i = 0; i < instanceAnimators.Length; i++)
                {
                    var animator = instanceAnimators[i];
                    var index = i;
                    menu.AddItem(new(animator.name), animator == selectedAnimator, () =>
                    {
                        preview.SetSelectedAnimator(index);
                    });
                }

                menu.ShowAsContext();
            }
        }

        /************************************************************************************************************************/

        private static DragAndDropHandler<GameObject> _ModelDropHandler;
        private static AnimancerPreviewObject _ModelDropPreview;

        /// <summary>Handles drag and drop events for preview models.</summary>
        public static void HandleDragAndDrop(Rect area, AnimancerPreviewObject preview)
        {
            _ModelDropPreview = preview;

            _ModelDropHandler ??= (gameObject, isDrop) =>
            {
                if (!gameObject.TryGetComponent<Animator>(out _))
                    return false;

                if (isDrop)
                    _ModelDropPreview.OriginalObject = gameObject.transform;

                return true;
            };

            _ModelDropPreview = null;
        }

        /************************************************************************************************************************/
    }
}

#endif

