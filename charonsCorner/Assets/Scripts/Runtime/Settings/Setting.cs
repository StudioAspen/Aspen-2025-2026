using UnityEngine;

namespace CharonsCorner.Runtime
{
    public abstract class Setting : MonoBehaviour
    {
        private protected abstract string playerPrefsKey { get; }
        public abstract void Apply();
        public abstract void Load();
        public abstract void Discard();
        public abstract bool IsDirty();
    }
}
