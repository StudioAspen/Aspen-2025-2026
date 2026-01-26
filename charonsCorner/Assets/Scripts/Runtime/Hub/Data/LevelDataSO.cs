using Eflatun.SceneReference;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    /// <summary>
    /// Stores all level related data to be displayed in the hub level selector
    /// </summary>
    [CreateAssetMenu(fileName = "LevelData", menuName = "CharonsCorner/LevelData")]
    public class LevelDataSO : ScriptableObject
    {
        [field: Header("Display")]
        [field: SerializeField] public string LevelTitle { get; private set; }
        [field: SerializeField, TextArea(3, 10)] public string LevelDescription { get; private set; }
        [field: SerializeField] public Sprite PreviewImage { get; private set; }
        
        [field: Header("Data")]
        [field: SerializeField] public int WorldFlagIndex { get; private set; }
        [field: SerializeField] public SceneReference LevelScene { get; private set; }
    }
}

