using UnityEngine;

namespace CharonsCorner.Runtime
{
    /// <summary>
    /// A simple script to modify the player's chapter progression via a public function.
    /// Can be called from Unity Events (e.g., UI Buttons or Triggers).
    /// </summary>
    public class ProgressionModifier : MonoBehaviour
    {
        public void SetChapterIndex(int _targetChapterIndex)
        {
            SetChapterIndex(_targetChapterIndex);
        }
    }
}
