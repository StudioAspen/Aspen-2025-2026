using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(WhiteFlash))]
public class WhiteFlashEditor : Editor
{

    void OnSceneGUI()
    {
        WhiteFlash WhiteFlashObject = target as WhiteFlash;
    
        Handles.color = Color.blue;
        Handles.DrawSolidDisc( // Origin Point
            center: WhiteFlashObject.transform.position,
            normal: Vector3.up,
            radius: 1f
        );

        Handles.color = Color.red;
        Handles.DrawWireDisc( // MinRange Radius
            center: WhiteFlashObject.transform.position,
            normal: Vector3.up,
            radius: WhiteFlashObject.minRange,
            thickness: 1f
        );

        Handles.color = Color.green;
        Handles.DrawWireDisc( // MaxRange Radius
            center: WhiteFlashObject.transform.position,
            normal: Vector3.up,
            radius: WhiteFlashObject.maxRange,
            thickness: 1f
        );
    }

}
