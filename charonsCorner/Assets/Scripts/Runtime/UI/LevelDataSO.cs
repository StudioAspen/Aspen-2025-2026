using Eflatun.SceneReference;
using UnityEngine;

//Handle opening of assets in editor
[CreateAssetMenu(fileName = "LevelData", menuName = "ScriptableObjects/LevelData")]
public class LevelDataSO : ScriptableObject
{
    //Hold references for all level data components
    [Header("Level Information")]
    public string levelTitle;
    [TextArea] public string levelDescription;
    public Sprite previewImage;
    public SceneReference levelScene;
}
