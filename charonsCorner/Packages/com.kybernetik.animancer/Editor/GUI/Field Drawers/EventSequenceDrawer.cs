// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

#if UNITY_EDITOR

using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;
using Sequence = Animancer.AnimancerEvent.Sequence;

namespace Animancer.Editor
{
    /// <summary>[Editor-Only] Draws the Inspector GUI for a <see cref="Sequence"/>.</summary>
    /// https://kybernetik.com.au/animancer/api/Animancer.Editor/EventSequenceDrawer
    public class EventSequenceDrawer
    {
        /************************************************************************************************************************/

        private static readonly ConditionalWeakTable<Sequence, EventSequenceDrawer>
            SequenceToDrawer = new();

        /// <summary>Returns a cached <see cref="EventSequenceDrawer"/> for the `events`.</summary>
        /// <remarks>
        /// The cache uses a <see cref="ConditionalWeakTable{TKey, TValue}"/> so it doesn't prevent the `events`
        /// from being garbage collected.
        /// </remarks>
        public static EventSequenceDrawer Get(Sequence events)
        {
            if (events == null)
                return null;

            if (!SequenceToDrawer.TryGetValue(events, out var drawer))
                SequenceToDrawer.Add(events, drawer = new());

            return drawer;
        }

        /************************************************************************************************************************/

        /// <summary>Calculates the number of vertical pixels required to draw the contents of the `events`.</summary>
        public float CalculateHeight(Sequence events)
            => AnimancerGUI.CalculateHeight(CalculateLineCount(events));

        /// <summary>Calculates the number of lines required to draw the contents of the `events`.</summary>
        public int CalculateLineCount(Sequence events)
        {
            if (events == null)
                return 0;

            if (!IsExpanded)
                return 1;

            var count = 1;

            for (int i = 0; i < events.Count; i++)
                count += DelegateGUI.CalculateLineCount(events[i].callback);

            count += DelegateGUI.CalculateLineCount(events.EndEvent.callback);

            return count;
        }

        /************************************************************************************************************************/

        /// <summary>Should the target's default be shown?</summary>
        public bool IsExpanded { get; set; }

        private static ConversionCache<int, string> _EventNumberCache;
        private static float _LogButtonWidth = float.NaN;

        /************************************************************************************************************************/

        /// <summary>Draws the GUI for the `events`.</summary>
        public void DoGUI(ref Rect area, Sequence events, GUIContent label)
        {
            using (var content = PooledGUIContent.Acquire(GetSummary(events)))
                DoGUI(ref area, events, label, content);
        }

        /// <summary>Draws the GUI for the `events`.</summary>
        public void DoGUI(ref Rect area, Sequence events, GUIContent label, GUIContent summary)
        {
            if (events == null)
                return;

            area.height = AnimancerGUI.LineHeight;

            var headerArea = area;

            const string LogLabel = "Log";
            if (float.IsNaN(_LogButtonWidth))
                _LogButtonWidth = EditorStyles.miniButton.CalculateWidth(LogLabel);
            var logArea = AnimancerGUI.StealFromRight(ref headerArea, _LogButtonWidth);
            if (GUI.Button(logArea, LogLabel, EditorStyles.miniButton))
                Debug.Log(events.DeepToString());

            IsExpanded = EditorGUI.Foldout(headerArea, IsExpanded, GUIContent.none, true);
            EditorGUI.LabelField(headerArea, label, summary);

            AnimancerGUI.NextVerticalArea(ref area);

            if (!IsExpanded)
                return;

            var enabled = GUI.enabled;
            GUI.enabled = false;

            EditorGUI.indentLevel++;

            for (int i = 0; i < events.Count; i++)
            {
                var name = events.GetName(i);
                if (string.IsNullOrEmpty(name))
                {
                    _EventNumberCache ??= new(index => $"Event {index}");

                    name = _EventNumberCache.Convert(i);
                }

                Draw(ref area, name, events[i]);
            }

            Draw(ref area, "End Event", events.EndEvent);

            EditorGUI.indentLevel--;

            GUI.enabled = enabled;
        }

        /************************************************************************************************************************/

        private static readonly ConversionCache<int, string>
            SummaryCache = new((count) => $"[{count}]"),
            EndSummaryCache = new((count) => $"[{count}] + End");

        /// <summary>Returns a summary of the `events`.</summary>
        public static string GetSummary(Sequence events)
        {
            var cache = float.IsNaN(events.NormalizedEndTime) && AnimancerEvent.IsNullOrDummy(events.OnEnd)
                ? SummaryCache
                : EndSummaryCache;
            return cache.Convert(events.Count);
        }

        /************************************************************************************************************************/

        private static ConversionCache<float, string> _EventTimeCache;

        /// <summary>Draws the GUI for the `animancerEvent`.</summary>
        public static void Draw(ref Rect area, string name, AnimancerEvent animancerEvent)
        {
            _EventTimeCache ??= new((time)
                => float.IsNaN(time) ? "Time = Auto" : $"Time = {time.ToStringCached()}x");

            var timeText = _EventTimeCache.Convert(animancerEvent.normalizedTime);

            using (var label = PooledGUIContent.Acquire(name))
            using (var value = PooledGUIContent.Acquire(timeText))
                DelegateGUI.DoGUI(ref area, label, animancerEvent.callback, value);
        }

        /************************************************************************************************************************/
    }
}

#endif

