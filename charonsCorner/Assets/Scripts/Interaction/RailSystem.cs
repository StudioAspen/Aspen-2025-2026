using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(LineRenderer))]
public class RailSystem : MonoBehaviour
{
    [Header("Bounce Settings: ")]
    public bool isAutoBounce = true;        
    public Vector3 manualRepel = Vector3.zero;
    public float repelForceMultiplier;

    [Header("Linked Node Reference: ")]
    public RailSystem nextNode;

    [Header("Appearance: ")]
    public Color startColor;
    public Color endColor;
    [Range(0.001f, 10f)] public float lineWidth = 0.05f;

    private LineRenderer lineRenderer;
    private Vector3 lastStart, lastEnd;

    private void OnValidate()
    {
        SetupLineRenderer();

        DrawLink();

        SetupCollider();
    }

    private void Awake()
    {
        SetupLineRenderer();

        SetupCollider();
    }

    private void Update()
    {
        DrawLink();

        SetupCollider();
    }


    private void SetupLineRenderer()
    {
        //Attach Line Render Component and Initial Parameters:
        if (lineRenderer == null) lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.useWorldSpace = true;
        lineRenderer.positionCount = 2;
        lineRenderer.widthMultiplier = lineWidth;

        //Apply Color Parameters To Line Render:
        lineRenderer.startColor = startColor;
        lineRenderer.endColor = endColor;
    }

    private void DrawLink()
    {
        //If There Is A Linked Node -> Set Up Start & End Points:
        if (nextNode == null)
        {
            lineRenderer.enabled = false;
            return;
        }
        lineRenderer.enabled = true;

        //Get Initial Positions:
        Vector3 start = transform.position;
        Vector3 direction = (nextNode.transform.position - start).normalized;
        Vector3 endPoint = nextNode.transform.position;

        //Connect The Points Through Line Render:
        if (start != lastStart || endPoint != lastEnd)
        {
            lineRenderer.SetPosition(0, start);
            lineRenderer.SetPosition(1, endPoint);
            lastStart = start;
            lastEnd = endPoint;
        }
    }

    private void SetupCollider()
    {
        if (nextNode == null) return;

        //Create A Child Object That Will Hold The Collider For The Rails:
        Transform colliderChild = transform.Find("RailCollider");
        if (colliderChild == null)
        {
            GameObject child = new GameObject("RailCollider");
            child.transform.parent = transform;
            child.transform.localRotation = Quaternion.identity;
            child.transform.localPosition = Vector3.zero;
            colliderChild = child.transform;

            BoxCollider box = child.AddComponent<BoxCollider>();
            box.isTrigger = true;
            child.gameObject.tag = "Rail";
        }

        //Set Up Collider Settings Of Child Object For The Rails:
        BoxCollider collider = colliderChild.GetComponent<BoxCollider>();
        Vector3 start = transform.position;
        Vector3 end = nextNode.transform.position;
        Vector3 mid = (start + end) / 2f;

        //Calculate and Set Middle Position To Be Where the Collider Is Set:
        colliderChild.position = mid;

        //Get The Length From The Start -> End:
        Vector3 direction = end - start;
        float length = direction.magnitude;

        //Rotate The Collider So That It Is Alligned With The Direction:
        if (length > 0.001f) colliderChild.rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);

        //Apply The Values To The Box Collider:
        collider.size = new Vector3(lineWidth, lineWidth, length);
    }
}
