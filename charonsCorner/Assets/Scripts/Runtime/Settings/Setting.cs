using UnityEngine;

namespace CharonsCorner.Runtime
{
    public abstract class Setting : MonoBehaviour
    {
        public abstract void Load();
        public abstract void Save();
    }
}
