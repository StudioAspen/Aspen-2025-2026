using UnityEngine;

namespace CharonsCorner.Runtime
{
    public class MementoActiveController : MonoBehaviour
    {
        [SerializeField] private int _chapterThreshold;

        private void Start()
        {
            UpdateActiveState();
        }

        private void UpdateActiveState()
        {
            int currentChapter = FlagManager.Get(ProgressFlag.CurrentChapterIndex);
            gameObject.SetActive(currentChapter > _chapterThreshold);
        }
    }
}
