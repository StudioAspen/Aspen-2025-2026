using UnityEngine;

namespace CharonsCorner.Runtime
{
    /// <summary>
    /// Activates or deactivates this GameObject at the start of the scene 
    /// based on whether the current chapter index falls within a specified range.
    /// </summary>
    public class ActivateOnChapter : MonoBehaviour
    {
        public enum MementoCondition { None, ActivateIfSeen, DeactivateIfSeen }

        [Header("Chapter Range")]
        [Tooltip("Inclusive minimum chapter index.")]
        [SerializeField] private int _minChapter;
        
        [Tooltip("Inclusive maximum chapter index.")]
        [SerializeField] private int _maxChapter;

        [Header("Memento Cutscene Condition")]
        [Tooltip("Optional condition based on whether the Memento Cutscene has been seen. Takes precedence over chapter range.")]
        [SerializeField] private MementoCondition _mementoCondition = MementoCondition.None;

        private void Start()
        {
            UpdateActiveState();
        }

        private void UpdateActiveState()
        {
            bool hasSeenMemento = FlagManager.Get(ProgressFlag.SeenMementoCutscene) == 1;

            if (hasSeenMemento && _mementoCondition != MementoCondition.None)
            {
                if (_mementoCondition == MementoCondition.ActivateIfSeen)
                {
                    gameObject.SetActive(true);
                    return;
                }
                
                if (_mementoCondition == MementoCondition.DeactivateIfSeen)
                {
                    gameObject.SetActive(false);
                    return;
                }
            }

            int currentChapter = FlagManager.Get(ProgressFlag.CurrentChapterIndex);
            bool isInRange = currentChapter >= _minChapter && currentChapter <= _maxChapter;
            gameObject.SetActive(isInRange);
        }
    }
}
