using UnityEngine;

namespace CharonsCorner.Runtime
{
    /// <summary>
    /// Activates or deactivates this GameObject at the start of the scene 
    /// based on whether the current chapter index falls within a specified range.
    /// </summary>
    public class ActivateOnChapter : MonoBehaviour
    {
        [Header("Chapter Range")]
        [Tooltip("Inclusive minimum chapter index.")]
        [SerializeField] private int _minChapter;
        
        [Tooltip("Inclusive maximum chapter index.")]
        [SerializeField] private int _maxChapter;

        private void Start()
        {
            UpdateActiveState();
        }

        private void UpdateActiveState()
        {
            int currentChapter = FlagManager.Get(ProgressFlag.CurrentChapterIndex);
            
            bool isInRange = currentChapter >= _minChapter && currentChapter <= _maxChapter;
            
            gameObject.SetActive(isInRange);
        }
    }
}
