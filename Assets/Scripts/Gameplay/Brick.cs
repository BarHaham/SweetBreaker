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
        [SerializeField, Tooltip("Colour of the pieces it throws when it breaks: bright for candy, dark for chocolate.")]
        private Color breakColor = Color.white;

        private SpriteRenderer spriteRenderer;
        private LevelManager owner;
        private int hitPointsLeft;

        public Color BreakColor => breakColor;

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
            if (collision.collider.TryGetComponent(out BallController _))
                TakeHit();
        }

        private void TakeHit()
        {
            // Destroy is deferred to the end of the frame, so a hit on an already-broken brick must not count.
            if (hitPointsLeft <= 0)
                return;

            hitPointsLeft--;

            if (hitPointsLeft > 0)
            {
                // A different sprite, not a tint, so the damage reads mid-rally.
                spriteRenderer.sprite = crackedSprite;
                owner.ReportBrickCracked(config.ScoreTwoHitCrack);
                return;
            }

            int points = type == BrickType.TwoHit ? config.ScoreTwoHitBreak : config.ScoreOneHitBreak;
            owner.ReportBrickDestroyed(this, points);
            Destroy(gameObject);
        }
    }
}
