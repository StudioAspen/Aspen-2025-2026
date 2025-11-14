using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CharonsCorner.Runtime
{
    [ExecuteAlways]
    public class RailSystem : MonoBehaviour
    {
        [Header("Bounce Settings: ")]
        public bool isAutoBounce = true;
        public Vector3 manualRepel = Vector3.zero;
        public float repelForceMultiplier;

        [Header("Linked Node Reference: ")]
        public RailSystem nextNode;

        [Header("Appearance: ")]
        [Range(0.001f, 10f)] public float railWidthX = 1f;
        [Range(0.001f, 10f)] public float railWidthY = 1f;

        private Transform railVisual;
        private BoxCollider railCollider;
        private bool visualNeedsSetup = false;

        private void OnValidate()
        {
            if (nextNode != null)
            {
#if UNITY_EDITOR
            // Delay the creation call so Unity editor is in a safe state
            EditorApplication.delayCall += () =>
            {
                if (this != null) // check object wasn't destroyed meanwhile
                {
                    SetupRailVisual();
                    SetupCollider();
                    EditorUtility.SetDirty(this); // Mark dirty so scene saves changes
                }
            };
#endif
            }
        }

        private void Awake()
        {
            if (visualNeedsSetup)
            {
                SetupRailVisual();
                SetupCollider();
                visualNeedsSetup = false;
            }
        }

        private void Update()
        {
            SetupCollider();
        }

        private void SetupRailVisual()
        {
            if (nextNode == null)
            {
                if (railVisual != null)
                    railVisual.gameObject.SetActive(false);
                return;
            }

            if (railVisual == null)
            {
                Transform existing = transform.Find("RailVisual");
                if (existing != null)
                {
                    railVisual = existing;
                    railCollider = railVisual.GetComponent<BoxCollider>();
                }
                else
                {
                    GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    go.name = "RailVisual";
                    go.transform.SetParent(transform, false);
                    railVisual = go.transform;

#if UNITY_EDITOR
        DestroyImmediate(go.GetComponent<Collider>());
#else
                    Destroy(go.GetComponent<Collider>());
#endif

                    railCollider = go.AddComponent<BoxCollider>();
                    railCollider.isTrigger = true;
                }
            }

            railVisual.gameObject.SetActive(true);
        }

        private void SetupCollider()
        {
            if (nextNode == null || railVisual == null) return;

            //Set Up Collider Settings Of Child Object For The Rails:
            Vector3 start = transform.position;
            Vector3 end = nextNode.transform.position;
            Vector3 mid = (start + end) * 0.5f;

            //Calculate and Set Middle Position To Be Where the Collider Is Set:
            railVisual.position = mid;

            //Get The Length From The Start -> End:
            Vector3 direction = end - start;
            float length = direction.magnitude;

            //Rotate The Collider So That It Is Alligned With The Direction:
            if (length > 0.001f) railVisual.rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);

            //Apply The Values To The Box Collider:
            railVisual.localScale = new Vector3(railWidthX, railWidthY, length);

            if (railCollider != null)
            {
                railCollider.center = Vector3.zero;
                railCollider.size = Vector3.one;
                railCollider.isTrigger = true;
            }
        }
    }
}
