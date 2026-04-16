using UnityEngine;

namespace CharonsCorner.Runtime
{
    public class PauseBlocker : MonoBehaviour
    {
        public void BlockPausing()
        {
            PauseCanvas.Instance.BlockPause(true);
        }

        public void EnablePausing()
        {
            PauseCanvas.Instance.BlockPause(false);
        }
    }
}