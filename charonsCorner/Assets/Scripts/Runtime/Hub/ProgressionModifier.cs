using UnityEngine;

namespace CharonsCorner.Runtime
{
    public class ProgressionModifier : MonoBehaviour
    {
        public void SetChapter(int chapterIndex)
        {
            FlagManager.Set(ProgressFlag.CurrentChapterIndex, chapterIndex);
        }

        public void ResetProgression()
        {
            FlagManager.ResetAll();
        }
    }
}
