using System;
using UnityEngine;

namespace SweetBreaker
{
    /// <summary>
    /// Builds the current level from its layout prefab and counts the breakable bricks left in it.
    /// </summary>
    public class LevelManager : MonoBehaviour
    {
        [SerializeField] private LevelDefinition[] levels;

        private GameObject currentLayout;
        private int breakablesLeft;

        /// <summary>Raised after a brick is destroyed, before the level-clear check.</summary>
        public event Action<Brick> BrickDestroyed;

        private void Start()
        {
            BuildLevel(0);
        }

        public void BuildLevel(int levelIndex)
        {
            if (currentLayout != null)
                Destroy(currentLayout);

            currentLayout = Instantiate(levels[levelIndex].LayoutPrefab, transform);
            Brick[] bricks = currentLayout.GetComponentsInChildren<Brick>();
            breakablesLeft = bricks.Length;
            foreach (Brick brick in bricks)
                brick.Initialize(this);
        }

        public void ReportBrickDestroyed(Brick brick)
        {
            breakablesLeft--;
            BrickDestroyed?.Invoke(brick);

            // "Breakable count == 0" rather than "brick count == 0", as GDD section 3 phrases it.
            if (breakablesLeft == 0)
                Debug.Log("Level cleared");
        }
    }
}
