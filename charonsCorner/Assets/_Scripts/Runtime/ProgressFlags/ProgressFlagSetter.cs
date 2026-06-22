using Sirenix.OdinInspector;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    public class ProgressFlagSetter : MonoBehaviour
    {
        [SerializeField] private ProgressFlag _targetFlag;
        [SerializeField, ShowIf("_dontUseIncrement")] private int _newValue;
        
        [SerializeField] private bool _useIncrement = false;
        private bool _dontUseIncrement => !_useIncrement;
        
        public void SetIfGreater()
        {
            SetIfGreater(_newValue);
        }

        [Button("Increment")]
        public void Increment()
        {
            FlagManager.Increment(_targetFlag);
        }
        
        [Button("Set Chapter Index")]
        public void SetIfGreater(int chapterIndex)
        {
            int currentChapterIndex = FlagManager.Get(_targetFlag);
            if (currentChapterIndex > chapterIndex)
                return;
            
            FlagManager.Set(ProgressFlag.CurrentChapterIndex, chapterIndex);
        }
    }
}