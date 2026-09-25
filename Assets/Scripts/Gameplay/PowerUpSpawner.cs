using UnityEngine;

namespace SweetBreaker
{
    /// <summary>
    /// Rolls PowerUpDropChance whenever a brick is destroyed and drops the paddle-expansion capsule
    /// from that brick. There is never more than one capsule, and none while the effect is running,
    /// so the player is never juggling two (GDD section 3).
    /// </summary>
    public class PowerUpSpawner : MonoBehaviour
    {
        [SerializeField] private GameConfig config;
        [SerializeField] private LevelManager levelManager;
        [SerializeField] private PaddleController paddle;
        [SerializeField] private PowerUpPickup capsulePrefab;

        private PowerUpPickup activeCapsule;

        private void OnEnable()
        {
            levelManager.BrickDestroyed += TryDrop;
            GameManager.Instance.StateChanged += HandleStateChanged;
        }

        private void OnDisable()
        {
            levelManager.BrickDestroyed -= TryDrop;
            if (GameManager.Instance != null)
                GameManager.Instance.StateChanged -= HandleStateChanged;
        }

        private void TryDrop(Brick brick)
        {
            // A caught or missed capsule destroys itself, which makes this reference null again.
            if (activeCapsule != null || paddle.IsExpanded)
                return;

            if (Random.value >= config.PowerUpDropChance)
                return;

            activeCapsule = Instantiate(capsulePrefab, brick.transform.position, Quaternion.identity);
            activeCapsule.Launch(config.PowerUpFallSpeed);
        }

        private void HandleStateChanged(GameState state)
        {
            // A capsule still falling when the level ends would otherwise land in the next one.
            bool levelOver = state == GameState.LevelClear || state == GameState.GameOver || state == GameState.Victory;
            if (levelOver && activeCapsule != null)
                Destroy(activeCapsule.gameObject);
        }
    }
}
