using System;
using UnityEngine;

namespace SweetBreaker
{
    /// <summary>
    /// Builds the current level from its layout prefab and counts the breakable bricks left in it.
    /// Levels are prefabs swapped inside Game.unity, not scenes (GDD section 7).
    /// </summary>
    public class LevelManager : MonoBehaviour
    {
        [SerializeField] private LevelDefinition[] levels;

        private GameObject currentLayout;
        private int currentLevelIndex;
        private int breakablesLeft;

        /// <summary>Raised after a brick is destroyed, before the level-clear check.</summary>
        public event Action<Brick> BrickDestroyed;

        private void OnEnable()
        {
            GameManager.Instance.LevelAdvanced += BuildLevel;
        }

        private void OnDisable()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.LevelAdvanced -= BuildLevel;
        }

        private void Start()
        {
            BuildLevel(GameManager.Instance.LevelIndex);
        }

        public void ReportBrickCracked(Brick brick, int points)
        {
            GameManager.Instance.AddScore(points);
            GameManager.Instance.Audio.Play(Sfx.BrickCrack);
        }

        public void ReportBrickDestroyed(Brick brick, int points)
        {
            GameManager.Instance.AddScore(points);
            GameManager.Instance.Audio.Play(Sfx.BrickBreak);
            breakablesLeft--;
            BrickDestroyed?.Invoke(brick);

            // "Breakable count == 0" rather than "brick count == 0", as GDD section 3 phrases it.
            if (breakablesLeft == 0)
                GameManager.Instance.CompleteLevel(currentLevelIndex == levels.Length - 1);
        }

        private void BuildLevel(int levelIndex)
        {
            if (currentLayout != null)
                Destroy(currentLayout);

            currentLevelIndex = levelIndex;
            currentLayout = Instantiate(levels[levelIndex].LayoutPrefab, transform);
            Brick[] bricks = currentLayout.GetComponentsInChildren<Brick>();
            breakablesLeft = bricks.Length;
            foreach (Brick brick in bricks)
                brick.Initialize(this);
        }
    }
}
