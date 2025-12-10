using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(FlashVFX))]
public class FlashVFXEditor : Editor
{

    void OnSceneGUI()
    {
        FlashVFX FlashVFXObject = target as FlashVFX;
    
        Handles.color = Color.blue;
        Handles.DrawSolidDisc( // Origin Point
            center: FlashVFXObject.transform.position,
            normal: Vector3.up,
            radius: 1f
        );

        Handles.color = Color.red;
        Handles.DrawWireDisc( // MinRange Radius
            center: FlashVFXObject.transform.position,
            normal: Vector3.up,
            radius: FlashVFXObject.minRange,
            thickness: 1f
        );

        Handles.color = Color.green;
        Handles.DrawWireDisc( // MaxRange Radius
            center: FlashVFXObject.transform.position,
            normal: Vector3.up,
            radius: FlashVFXObject.maxRange,
            thickness: 1f
        );
    }

}
