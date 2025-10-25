using Eflatun.SceneReference;
using UnityEngine;

//Handle opening of assets in editor
[CreateAssetMenu(fileName = "LevelData", menuName = "CharonsCorner/LevelData")]
public class LevelDataSO : ScriptableObject
{
    //Hold references for all level data components
    [field: Header("Level Information")]
    [field: SerializeField] public string LevelTitle { get; private set; }
    [field: SerializeField, TextArea(3, 10)] public string LevelDescription { get; private set; }
    [field: SerializeField] public Sprite PreviewImage { get; private set; }
    [field: SerializeField] public SceneReference LevelScene { get; private set; }
}
