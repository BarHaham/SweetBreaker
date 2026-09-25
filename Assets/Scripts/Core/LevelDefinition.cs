using UnityEngine;

namespace SweetBreaker
{
    /// <summary>
    /// One level: its name and the hand-authored brick layout prefab that LevelManager instantiates.
    /// </summary>
    [CreateAssetMenu(fileName = "Level", menuName = "Sweet Breaker/Level Definition")]
    public class LevelDefinition : ScriptableObject
    {
        [field: SerializeField, Tooltip("Also the name of the built layout in the Hierarchy.")]
        public string LevelName { get; private set; } = "Level";

        [field: SerializeField, Tooltip("A prefab whose children are the level's bricks.")]
        public GameObject LayoutPrefab { get; private set; }
    }
}
