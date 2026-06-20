using System.Collections.Generic;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    public class RandomizeActiveObject : MonoBehaviour
    {
        [SerializeField] private List<GameObject> objects;

        public void Randomize()
        {
            if (objects == null || objects.Count == 0) return;

            int activeIndex = Random.Range(0, objects.Count);

            for (int i = 0; i < objects.Count; i++)
            {
                if (objects[i] != null)
                {
                    objects[i].SetActive(i == activeIndex);
                }
            }
        }
    }
}
