// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Animancer.Editor
{
    /// <summary>[Editor-Only]
    /// Keeps track of <see cref="AnimancerGraph"/> instances
    /// to ensure that they're properly cleaned up.
    /// </summary>
    /// https://kybernetik.com.au/animancer/api/Animancer.Editor/AnimancerGraphCleanup
    public static class AnimancerGraphCleanup
    {
        /************************************************************************************************************************/

        private static List<AnimancerGraph> _AllGraphs;

        /// <summary>[Editor-Only] Registers a `graph` to make sure it gets cleaned up properly.</summary>
        public static void AddGraph(AnimancerGraph graph)
        {
            if (_AllGraphs == null)
            {
                _AllGraphs = new();

                AssemblyReloadEvents.beforeAssemblyReload +=
                    () => DestroyAll(EditorApplication.isPlaying);

                EditorApplication.playModeStateChanged += change =>
                {
                    switch (change)
                    {
                        case PlayModeStateChange.EnteredEditMode:
                            DestroyAll(true);
                            break;

                        case PlayModeStateChange.ExitingEditMode:
                            DestroyAll(false);
                            break;
                    }
                };
            }
            else
            {
                EditModeDestroyOldInstances();
            }

            _AllGraphs.Add(graph);
        }

        /// <summary>[Editor-Only] Removes the `graph` from the list of instances.</summary>
        public static void RemoveGraph(AnimancerGraph graph)
        {
            _AllGraphs?.Remove(graph);
            AnimancerGraph.ClearInactiveInitializationStackTrace(graph);
        }

        /************************************************************************************************************************/

        private static void DestroyAll(bool isPlaying)
        {
            for (int i = _AllGraphs.Count - 1; i >= 0; i--)
            {
                var graph = _AllGraphs[i];
                if (graph.IsValidOrDispose())
                {
                    if (isPlaying && graph.InactiveInitializationStackTrace != null)
                    {
                        Debug.LogWarning(
                            $"{graph} was not properly destroyed." +
                            $" Its {nameof(GameObject)} was inactive and never activated," +
                            $" meaning that Unity didn't call its AnimancerComponent.OnDestroy." +
                            $"\n\nIf you need to use Animancer on an object that never gets activated," +
                            $" you must call animancerComponent.Graph.Destroy() on it manually." +
                            $"\n\nThis graph was created:\n{graph.InactiveInitializationStackTrace}\n",
                            graph.Component as Object);

                        AnimancerGraph.ClearInactiveInitializationStackTrace(graph);
                    }

                    graph.Destroy();
                }
            }

            _AllGraphs.Clear();
        }

        /************************************************************************************************************************/

        private static void EditModeDestroyOldInstances()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                return;

            for (int i = _AllGraphs.Count - 1; i >= 0; i--)
            {
                var graph = _AllGraphs[i];
                if (!ShouldStayAlive(graph))
                {
                    if (graph.IsValidOrDispose())
                        graph.Destroy();// This will remove it.
                    else
                        _AllGraphs.RemoveAt(i);
                }
            }
        }

        /************************************************************************************************************************/

        /// <summary>Should this graph should stay alive instead of being destroyed?</summary>
        private static bool ShouldStayAlive(AnimancerGraph graph)
        {
            if (!graph.IsValidOrDispose())
                return false;

            if (graph.Component == null)
                return true;

            if (graph.Component is Object obj && obj == null)
                return false;

            if (graph.Component.Animator == null)
                return false;

            return true;
        }

        /************************************************************************************************************************/

        /// <summary>[Editor-Only]
        /// Returns true if the `initial` mode was <see cref="AnimatorUpdateMode.AnimatePhysics"/>
        /// and the `current` has changed to another mode or if the `initial` mode was something else
        /// and the `current` has changed to <see cref="AnimatorUpdateMode.AnimatePhysics"/>.
        /// </summary>
        public static bool HasChangedToOrFromAnimatePhysics(AnimatorUpdateMode? initial, AnimatorUpdateMode current)
        {
            if (initial == null)
                return false;

#if UNITY_2023_1_OR_NEWER
            var wasAnimatePhysics = initial.Value == AnimatorUpdateMode.Fixed;
            var isAnimatePhysics = current == AnimatorUpdateMode.Fixed;
#else
            var wasAnimatePhysics = initial.Value == AnimatorUpdateMode.AnimatePhysics;
            var isAnimatePhysics = current == AnimatorUpdateMode.AnimatePhysics;
#endif

            return wasAnimatePhysics != isAnimatePhysics;
        }

        /************************************************************************************************************************/
    }
}

namespace Animancer
{
    /// https://kybernetik.com.au/animancer/api/Animancer/AnimancerGraph
    public partial class AnimancerGraph
    {
        /************************************************************************************************************************/

        /// <summary>[Editor-Only] [Internal]
        /// A stack trace captured in <see cref="Initialize(IAnimancerComponent, bool)"/>
        /// if the <see cref="GameObject.activeInHierarchy"/> is false.
        /// </summary>
        /// <remarks>
        /// This is used to warn if the graph isn't destroyed
        /// because Unity won't call <c>OnDestroy</c> if the object is never activated.
        /// </remarks>
        internal System.Diagnostics.StackTrace InactiveInitializationStackTrace { get; private set; }

        /************************************************************************************************************************/

        /// <summary>[Editor-Only] Captures the <see cref="InactiveInitializationStackTrace"/>.</summary>
        private void CaptureInactiveInitializationStackTrace(IAnimancerComponent animancer)
        {
            if (!animancer.gameObject.activeInHierarchy &&
                EditorApplication.isPlayingOrWillChangePlaymode)
                InactiveInitializationStackTrace = new(1, true);
        }

        /************************************************************************************************************************/

        /// <summary>[Editor-Only] [Internal] Discards the <see cref="InactiveInitializationStackTrace"/>.</summary>
        public static void ClearInactiveInitializationStackTrace(AnimancerGraph graph)
        {
            graph.InactiveInitializationStackTrace = null;
        }

        /************************************************************************************************************************/
    }
}

#endif

