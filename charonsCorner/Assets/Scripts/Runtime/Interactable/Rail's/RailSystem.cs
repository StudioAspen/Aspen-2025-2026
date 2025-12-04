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
        [SerializeField] private bool _isAutoBounce = true;
        [SerializeField] private Vector3 _manualRepel = Vector3.zero;
        [Space]
        [SerializeField] private float _repelForceMultiplier;
        [SerializeField] private float _minimumForce;
        [SerializeField] private float _maximumForce;

        public bool IsAutoBounce => _isAutoBounce;
        public Vector3 ManualRepel => _manualRepel;
        public float RepelForceMultiplier => _repelForceMultiplier;
        public float MinimumForce => _minimumForce;
        public float MaximumForce => _maximumForce;


        [Header("Linked Node Reference: ")]
        [SerializeField] private RailSystem _nextNode;
        public RailSystem NextNode => _nextNode;

        [Header("Appearance: ")]
        [SerializeField] [Range(0.001f, 10f)] private float _railWidthX = 1f;
        [SerializeField] [Range(0.001f, 10f)] private float _railWidthY = 1f;

        public float RailWidthX => _railWidthX;
        public float RailWidthY => _railWidthY;


        private Transform railVisual;
        private BoxCollider railCollider;
        private BoxCollider railTriggerCollider;

        private bool visualNeedsSetup = false;

        private void OnValidate()
        {
            if (_nextNode != null)
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
            if (_nextNode == null)
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
                    //Physical Collider:
                    railCollider = go.AddComponent<BoxCollider>();
                    railCollider.isTrigger = false;

                    //Trigger Collider:
                    railTriggerCollider = go.AddComponent<BoxCollider>();
                    railTriggerCollider.isTrigger = true;
                }
            }

            railVisual.gameObject.SetActive(true);
        }

        private void SetupCollider()
        {
            if (_nextNode == null || railVisual == null) return;

            //Set Up Collider Settings Of Child Object For The Rails:
            Vector3 start = transform.position;
            Vector3 end = _nextNode.transform.position;
            Vector3 mid = (start + end) * 0.5f;

            //Calculate and Set Middle Position To Be Where the Collider Is Set:
            railVisual.position = mid;

            //Get The Length From The Start -> End:
            Vector3 direction = end - start;
            float length = direction.magnitude;

            //Rotate The Collider So That It Is Alligned With The Direction:
            if (length > 0.001f) railVisual.rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);

            //Apply The Values To The Box Collider:
            railVisual.localScale = new Vector3(_railWidthX, _railWidthY, length);

            //Box Collider Settings:
            if (railCollider != null)
            {
                railCollider.center = Vector3.zero;
                railCollider.size = Vector3.one;
                railCollider.isTrigger = false;
            }

            //Trigger Collider Settings:
            if (railTriggerCollider != null)
            {
                railTriggerCollider.center = Vector3.zero;
                railTriggerCollider.size = Vector3.one;
                railTriggerCollider.isTrigger = true;
            }
        }
    }
}