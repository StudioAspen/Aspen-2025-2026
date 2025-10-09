// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Animancer.Editor
{
    /// <summary>[Editor-Only] An <see cref="ICustomGUI"/> for <see cref="MulticastDelegate"/>.</summary>
    /// https://kybernetik.com.au/animancer/api/Animancer.Editor/DelegateGUI
    [CustomGUI(typeof(MulticastDelegate))]
    public class DelegateGUI : CustomGUI<MulticastDelegate>
    {
        /************************************************************************************************************************/

        private static readonly HashSet<MulticastDelegate>
            ExpandedItems = new();

        /************************************************************************************************************************/

        /// <summary>Calculates the number of vertical pixels required to draw the specified <see cref="MulticastDelegate"/>.</summary>
        public static float CalculateHeight(MulticastDelegate del)
            => AnimancerGUI.CalculateHeight(CalculateLineCount(del));

        /// <summary>Calculates the number of lines required to draw the specified <see cref="MulticastDelegate"/>.</summary>
        public static int CalculateLineCount(MulticastDelegate del)
            => del == null || !ExpandedItems.Contains(del)
            ? 1
            : 1 + CalculateLineCount(AnimancerReflection.GetInvocationList(del));

        /// <summary>Calculates the number of lines required to draw the specified `invocationList`.</summary>
        public static int CalculateLineCount(Delegate[] invocationList)
            => invocationList == null
            ? 3
            : invocationList.Length * 3;

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override void DoGUI()
        {
            var area = AnimancerGUI.LayoutRect(CalculateHeight(Value));
            DoGUI(ref area, Label, Value);
        }

        /// <summary>Draws the GUI for the given delegate.</summary>
        public static void DoGUI(
            ref Rect area,
            GUIContent label,
            MulticastDelegate del,
            GUIContent valueLabel = null)
        {
            area.height = AnimancerGUI.LineHeight;

            var delegates = AnimancerReflection.GetInvocationList(del);

            var isExpanded = del != null && AnimancerGUI.DoHashedFoldoutGUI(area, ExpandedItems, del);

            if (valueLabel != null)
            {
                EditorGUI.LabelField(area, label, valueLabel);
            }
            else
            {
                var count = delegates == null ? 0 : delegates.Length;
                using (var countLabel = PooledGUIContent.Acquire(count.ToStringCached()))
                    EditorGUI.LabelField(area, label, countLabel);
            }

            AnimancerGUI.NextVerticalArea(ref area);

            if (!isExpanded)
                return;

            EditorGUI.indentLevel++;

            if (delegates == null)
            {
                DoSingleGUI(ref area, del);
            }
            else
            {
                for (int i = 0; i < delegates.Length; i++)
                    DoSingleGUI(ref area, delegates[i]);
            }

            EditorGUI.indentLevel--;
        }

        /************************************************************************************************************************/

        private const int TargetFieldCacheCapacity = 128;

        private static readonly Dictionary<object, FastObjectField>
            TargetFieldCache = new(TargetFieldCacheCapacity);

        /// <summary>Draws the target and name of the specified <see cref="Delegate"/>.</summary>
        public static void DoSingleGUI(ref Rect area, Delegate del)
        {
            area.height = AnimancerGUI.LineHeight;

            if (del == null)
            {
                EditorGUI.LabelField(area, "Delegate", "Null");
                AnimancerGUI.NextVerticalArea(ref area);
                return;
            }

            var method = del.Method;
            EditorGUI.LabelField(area, "Method", method.ToString());

            AnimancerGUI.NextVerticalArea(ref area);

            EditorGUI.LabelField(area, "Declaring Type", method.DeclaringType.GetNameCS());

            AnimancerGUI.NextVerticalArea(ref area);

            var target = del.Target;

            FastObjectField field;

            if (target is not null)
                TargetFieldCache.TryGetValue(target, out field);
            else
                field = FastObjectField.Null;

            field.Draw(area, "Target", target);

            if (target is not null)
            {
                if (TargetFieldCache.Count == TargetFieldCacheCapacity)
                    TargetFieldCache.Clear();

                TargetFieldCache[target] = field;
            }

            AnimancerGUI.NextVerticalArea(ref area);
        }

        /************************************************************************************************************************/
    }
}

#endif

