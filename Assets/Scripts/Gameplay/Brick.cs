using UnityEngine;

namespace SweetBreaker
{
    /// <summary>Candy bricks break on the first hit; chocolate bricks crack first (GDD section 6).</summary>
    public enum BrickType
    {
        OneHit,
        TwoHit,
    }

    /// <summary>
    /// A brick with one or two hit points. Damage is applied once per collision event, and the brick
    /// reports every hit, with the points it is worth, to the LevelManager that built it.
    /// </summary>
    [RequireComponent(typeof(Collider2D), typeof(SpriteRenderer))]
    public class Brick : MonoBehaviour
    {
        [SerializeField] private GameConfig config;
        [SerializeField] private BrickType type = BrickType.OneHit;
        [SerializeField, Tooltip("Two-hit bricks only: the sprite shown after the first hit.")]
        private Sprite crackedSprite;

        private SpriteRenderer spriteRenderer;
        private LevelManager owner;
        private int hitPointsLeft;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            hitPointsLeft = type == BrickType.TwoHit ? 2 : 1;
        }

        public void Initialize(LevelManager levelManager)
        {
            owner = levelManager;
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            // Destroy is deferred to the end of the frame, so a second contact this step must not count.
            if (hitPointsLeft > 0 && collision.collider.TryGetComponent(out BallController _))
                TakeHit();
        }

        private void TakeHit()
        {
            hitPointsLeft--;

            if (hitPointsLeft > 0)
            {
                // A different sprite, not a tint, so the damage reads mid-rally.
                spriteRenderer.sprite = crackedSprite;
                owner.ReportBrickCracked(this, config.ScoreTwoHitCrack);
                return;
            }

            int points = type == BrickType.TwoHit ? config.ScoreTwoHitBreak : config.ScoreOneHitBreak;
            owner.ReportBrickDestroyed(this, points);
            Destroy(gameObject);
        }
    }
}
