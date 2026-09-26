using UnityEngine;
using UnityEngine.EventSystems;

namespace SweetBreaker
{
    /// <summary>
    /// The ball: served from the paddle, then moved by Box2D at a speed that never changes.
    /// Collisions only ever change its direction (GDD section 3).
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class BallController : MonoBehaviour
    {
        // How far the anti-stall nudge turns a near-horizontal ball: twice the stall threshold.
        private const float StallEscapeFactor = 2f;

        [SerializeField] private GameConfig config;
        [SerializeField] private PaddleController paddle;
        [SerializeField, Tooltip("Height of the ball's centre above the paddle's centre while serving (u).")]
        private float serveHeight = 0.38f;

        private Rigidbody2D rb;
        private Collider2D ballCollider;
        private SpriteRenderer spriteRenderer;
        private bool isServing;
        private float stallTimer;
        private Vector2 velocityBeforeStep;
        private Collider2D ignoredPaddleCollider;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            ballCollider = GetComponent<Collider2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        // On a level clear the ball keeps flying through the slow motion; the banner removes it.
        private void OnEnable()
        {
            GameManager.Instance.ServeStarted += AttachToPaddle;
            GameManager.Instance.LifeLost += Remove;
            GameManager.Instance.LevelClearBannerShown += Remove;
        }

        private void OnDisable()
        {
            if (GameManager.Instance == null)
                return;

            GameManager.Instance.ServeStarted -= AttachToPaddle;
            GameManager.Instance.LifeLost -= Remove;
            GameManager.Instance.LevelClearBannerShown -= Remove;
        }

        private void Update()
        {
            if (isServing && GameManager.Instance.State == GameState.Serve && LaunchPressed())
                Launch();
        }

        private void FixedUpdate()
        {
            if (!rb.simulated)
                return;

            if (isServing)
            {
                FollowPaddle();
                return;
            }

            KeepConstantSpeed();
            PreventStall();
            RestorePaddleContactOnceClear();

            // The physics step runs next, so this is the velocity the ball meets anything with.
            velocityBeforeStep = rb.linearVelocity;
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.collider.TryGetComponent(out PaddleController hitPaddle))
            {
                // A centre level with the top face or above means the top face or its corner was hit.
                if (rb.position.y >= collision.collider.bounds.max.y)
                    ReboundFrom(hitPaddle);
                else
                    GlanceOffSide(hitPaddle, collision.collider);

                GameManager.Instance.Audio.Play(Sfx.PaddleBounce);
            }
            else if (!collision.collider.TryGetComponent(out Brick _))
            {
                // A brick's crack or break sound is played by LevelManager when the brick reports the hit.
                GameManager.Instance.Audio.Play(Sfx.WallBounce);
            }
        }

        /// <summary>Locks the ball to the paddle's centre until the player launches it.</summary>
        private void AttachToPaddle()
        {
            isServing = true;
            stallTimer = 0f;
            rb.simulated = true;
            spriteRenderer.enabled = true;
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.linearVelocity = Vector2.zero;
            RestorePaddleContact();
            FollowPaddle();
            transform.position = rb.position;
        }

        /// <summary>
        /// Takes the ball out of play. The object stays active so it keeps listening for the next serve.
        /// </summary>
        private void Remove()
        {
            isServing = false;
            rb.linearVelocity = Vector2.zero;
            rb.simulated = false;
            spriteRenderer.enabled = false;
        }

        private static bool LaunchPressed()
        {
            if (Input.GetKeyDown(KeyCode.Space))
                return true;

            // A click on a UI button belongs to the UI, not to the serve (GDD section 4).
            bool pointerOverUi = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
            return Input.GetMouseButtonDown(0) && !pointerOverUi;
        }

        private void Launch()
        {
            isServing = false;
            rb.bodyType = RigidbodyType2D.Dynamic;

            // Toward the side the paddle is moving; straight from a still paddle it goes up and to the right.
            float side = paddle.MoveDirection < 0 ? -1f : 1f;
            float angle = config.LaunchAngle * Mathf.Deg2Rad;
            rb.linearVelocity = new Vector2(Mathf.Cos(angle) * side, Mathf.Sin(angle)) * config.BallSpeed;
            GameManager.Instance.NotifyBallLaunched();
        }

        private void FollowPaddle()
        {
            rb.position = (Vector2)paddle.transform.position + Vector2.up * serveHeight;
        }

        /// <summary>
        /// The paddle is an aiming device, not a mirror: where the ball lands on it sets the outgoing
        /// angle, from straight up at the centre to MaxBounceAngle at the edges.
        /// </summary>
        private void ReboundFrom(PaddleController hitPaddle)
        {
            float offset = (rb.position.x - hitPaddle.CentreX) / (hitPaddle.Width * 0.5f);
            float angle = Mathf.Clamp(offset, -1f, 1f) * config.MaxBounceAngle * Mathf.Deg2Rad;
            rb.linearVelocity = new Vector2(Mathf.Sin(angle), Mathf.Cos(angle)) * config.BallSpeed;
        }

        /// <summary>
        /// A ball that has dropped below the paddle's top face is already missed: the paddle's side
        /// only knocks it away, and the paddle stops touching it, so a moving paddle can never carry
        /// the ball along and squeeze it up the side wall.
        /// </summary>
        private void GlanceOffSide(PaddleController hitPaddle, Collider2D paddleCollider)
        {
            float away = rb.position.x < hitPaddle.CentreX ? -1f : 1f;
            rb.linearVelocity = new Vector2(Mathf.Abs(velocityBeforeStep.x) * away, velocityBeforeStep.y);

            Physics2D.IgnoreCollision(ballCollider, paddleCollider, true);
            ignoredPaddleCollider = paddleCollider;
        }

        /// <summary>A ball knocked back up above the paddle can land on it again.</summary>
        private void RestorePaddleContactOnceClear()
        {
            if (ignoredPaddleCollider != null && ballCollider.bounds.min.y > ignoredPaddleCollider.bounds.max.y)
                RestorePaddleContact();
        }

        private void RestorePaddleContact()
        {
            if (ignoredPaddleCollider == null)
                return;

            Physics2D.IgnoreCollision(ballCollider, ignoredPaddleCollider, false);
            ignoredPaddleCollider = null;
        }

        private void KeepConstantSpeed()
        {
            Vector2 velocity = rb.linearVelocity;
            if (velocity.sqrMagnitude < 0.0001f)
                velocity = Vector2.up;
            rb.linearVelocity = velocity.normalized * config.BallSpeed;
        }

        /// <summary>Breaks an otherwise endless horizontal loop between the side walls.</summary>
        private void PreventStall()
        {
            Vector2 velocity = rb.linearVelocity;
            float minVerticalSpeed = config.MinVerticalSpeedFraction * config.BallSpeed;
            if (Mathf.Abs(velocity.y) >= minVerticalSpeed)
            {
                stallTimer = 0f;
                return;
            }

            stallTimer += Time.fixedDeltaTime;
            if (stallTimer < config.StallTimeout)
                return;

            // Tilt away from horizontal, keeping the ball's heading; a perfectly level ball is sent down, back to the player.
            float verticalFraction = Mathf.Min(config.MinVerticalSpeedFraction * StallEscapeFactor, 0.9f);
            float horizontalFraction = Mathf.Sqrt(1f - verticalFraction * verticalFraction);
            float xSign = velocity.x < 0f ? -1f : 1f;
            float ySign = velocity.y > 0f ? 1f : -1f;
            rb.linearVelocity = new Vector2(horizontalFraction * xSign, verticalFraction * ySign) * config.BallSpeed;
            stallTimer = 0f;
        }
    }
}
