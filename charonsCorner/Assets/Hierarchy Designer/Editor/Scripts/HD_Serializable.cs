#if UNITY_EDITOR
using System.Collections.Generic;

namespace HierarchyDesigner
{
    [System.Serializable]
    internal class HD_Serializable<T>
    {
        public List<T> items;

        public HD_Serializable(List<T> items)
        {
            this.items = items;
        }
    }
}
#endif