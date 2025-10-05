// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

#if UNITY_EDITOR && UNITY_IMGUI

using UnityEditor;
using UnityEngine;

namespace Animancer.Editor
{
    /// <summary>[Editor-Only] A custom Inspector for <see cref="AnimancerEvent.Invoker"/>s.</summary>
    /// https://kybernetik.com.au/animancer/api/Animancer.Editor/AnimancerEventInvokerEditor
    /// 
    [CustomEditor(typeof(AnimancerEvent.Invoker), true), CanEditMultipleObjects]
    public class AnimancerEventInvokerEditor : UnityEditor.Editor
    {
        /************************************************************************************************************************/

        private readonly Field[]
            Fields = new Field[100];

        private struct Field
        {
            public FastObjectField component;
            public FastObjectField state;
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override void OnInspectorGUI()
        {
            if (target is Behaviour behaviour &&
                !behaviour.enabled)
            {
                EditorGUILayout.HelpBox(
                    "This component is disabled so it won't invoke any events.",
                    MessageType.Warning);
            }

            int index = 0;

            var isLayoutEvent = Event.current.type == EventType.Layout;

            var enumerator = AnimancerEvent.Invoker.EnumerateInvocationQueue();
            while (enumerator.MoveNext())
            {
                if (index < Fields.Length)
                {
                    var invocation = enumerator.Current;

                    if (invocation.State == null)
                    {
                        GUILayout.Label("State is Null");
                        return;
                    }

                    ref var field = ref Fields[index];

                    var area = AnimancerGUI.LayoutSingleLineRect();

                    var labelArea = AnimancerGUI.StealFromLeft(ref area, EditorGUIUtility.labelWidth);
                    labelArea = EditorGUI.IndentedRect(labelArea);

                    if (isLayoutEvent)
                    {
                        field.component.SetValue(invocation.State.Graph?.Component);
                        field.state.SetValue(invocation.State, invocation.State.GetPath());
                    }

                    field.component.Draw(labelArea);
                    field.state.Draw(area);

                    EditorGUI.indentLevel++;

                    EditorGUILayout.LabelField("Event Name", invocation.Name);
                    EditorGUILayout.LabelField("Normalized Time", invocation.Event.normalizedTime.ToString());
                    NamedEventDictionaryDrawer.DoEventGUI("Direct Callback", invocation.Event.callback);
                    NamedEventDictionaryDrawer.DoEventGUI("Bound Callback", invocation.GetBoundCallback());

                    EditorGUI.indentLevel--;
                }

                index++;
            }

            if (index > Fields.Length)
                GUILayout.Label($"And {index - Fields.Length} more events.");

            Repaint();
        }

        /************************************************************************************************************************/
    }
}

#endif

