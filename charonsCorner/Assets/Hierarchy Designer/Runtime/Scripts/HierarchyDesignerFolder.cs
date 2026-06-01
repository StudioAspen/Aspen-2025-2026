namespace HierarchyDesigner
{
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Events;

    public class HierarchyDesignerFolder : MonoBehaviour
    {
        #region Properties
        [Tooltip("Flatten Folder = Free all the folder's GameObject children in the Awake/Start method (FlattenEvent), then once the operation is complete, destroy the folder.")]
        [SerializeField] private bool flattenFolder = true;
        public bool ShouldFlatten => flattenFolder;

        [Tooltip("If enabled, children are moved to the Hierarchy root when flattening. If disabled, children are moved to the folder's parent (same layer where the folder existed).")]
        [SerializeField] private bool moveChildrenToHierarchyRoot = true;
        public bool MoveChildrenToHierarchyRoot => moveChildrenToHierarchyRoot;

        public enum FlattenEvent { Awake, Start }
        [Tooltip("FlattenEvent.Awake = The Flatten Folder Operation will occur in the Awake method.\nFlattenEvent.Start = The Flatten Folder Operation will occur in the Start method.\n\n*Use FlattenEvent.Awake if you have gameObjects with Singleton patterns with DontDestroyOnLoad in the Start Method or similar.*")]
        [SerializeField] private FlattenEvent flattenEvent = FlattenEvent.Start;

        [Tooltip("Event(s) called just before the flatten event occurs.")]
        [SerializeField] private UnityEvent OnFlattenEvent;

        [Tooltip("Event(s) called just before the folder is destroyed.")]
        [SerializeField] private UnityEvent OnFolderDestroy;

#if UNITY_EDITOR
        [HideInInspector]
        [SerializeField] private string notes;
#endif

        private Transform cachedTransform;
        #endregion

        #region Initialization
        private void Awake()
        {
            cachedTransform = transform;
            HandleFlattenEvent(FlattenEvent.Awake);
        }

        private void Start()
        {
            HandleFlattenEvent(FlattenEvent.Start);
        }
        #endregion

        #region Operations
        private void HandleFlattenEvent(FlattenEvent eventToCheck)
        {
            if (!flattenFolder || flattenEvent != eventToCheck)
            {
                return;
            }

            OnFlattenEvent?.Invoke();
            FlattenFolderIfRequired();
        }

        private void FlattenFolderIfRequired()
        {
            RecursivelyFlatten(cachedTransform);
            OnFolderDestroy?.Invoke();
            Destroy(gameObject);
        }

        private void RecursivelyFlatten(Transform folderTransform)
        {
            HashSet<Transform> foldersToDestroy = new HashSet<Transform>();

            for (int i = folderTransform.childCount - 1; i >= 0; i--)
            {
                Transform childTransform = folderTransform.GetChild(i);
                HierarchyDesignerFolder childFolder = childTransform.GetComponent<HierarchyDesignerFolder>();

                if (childFolder != null && childFolder.ShouldFlatten)
                {
                    childFolder.RecursivelyFlatten(childTransform);
                    foldersToDestroy.Add(childTransform);
                }
            }

            Transform destinationParent = moveChildrenToHierarchyRoot ? null : folderTransform.parent;

            int nextSiblingIndex = -1;
            if (!moveChildrenToHierarchyRoot)
            {
                nextSiblingIndex = folderTransform.GetSiblingIndex();
            }

            int childCount = folderTransform.childCount;
            Transform[] children = new Transform[childCount];

            for (int i = 0; i < childCount; i++)
            {
                children[i] = folderTransform.GetChild(i);
            }

            for (int i = 0; i < children.Length; i++)
            {
                Transform childTransform = children[i];

                if (foldersToDestroy.Contains(childTransform))
                {
                    continue;
                }

                childTransform.SetParent(destinationParent, true);

                if (nextSiblingIndex >= 0)
                {
                    childTransform.SetSiblingIndex(nextSiblingIndex);
                    nextSiblingIndex++;
                }
            }

            foreach (Transform folderToDestroy in foldersToDestroy)
            {
                Destroy(folderToDestroy.gameObject);
            }
        }
        #endregion
    }
}
