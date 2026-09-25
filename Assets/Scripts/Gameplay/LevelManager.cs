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
        [SerializeField] private GameConfig config;
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

        public void ReportBrickCracked(int points)
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

            LevelDefinition level = levels[levelIndex];
            currentLevelIndex = levelIndex;
            currentLayout = Instantiate(level.LayoutPrefab, transform);
            currentLayout.name = level.LevelName;

            Brick[] bricks = currentLayout.GetComponentsInChildren<Brick>();
            breakablesLeft = bricks.Length;
            foreach (Brick brick in bricks)
                brick.Initialize(this);

            WarnAboutBricksUnderHud(bricks);
        }

        /// <summary>
        /// Layouts are drawn by hand, so this catches a brick placed in the top HudBandHeight of the
        /// field, where the HUD would cover it (GDD section 5). The field is centred on this object.
        /// </summary>
        private void WarnAboutBricksUnderHud(Brick[] bricks)
        {
            float hudBandBottom = transform.position.y + config.PlayFieldSize.y * 0.5f - config.HudBandHeight;
            foreach (Brick brick in bricks)
            {
                if (brick.GetComponent<SpriteRenderer>().bounds.max.y > hudBandBottom)
                    Debug.LogWarning($"{brick.name} in {currentLayout.name} reaches into the HUD band.", brick);
            }
        }
    }
}
