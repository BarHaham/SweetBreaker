using System;
using System.Collections;
using UnityEngine;

namespace SweetBreaker
{
    /// <summary>
    /// Moves the paddle along X only. Input is read in Update and applied in FixedUpdate; keyboard
    /// and mouse are both live, and whichever moved last owns the paddle (GDD section 4).
    /// Also owns the paddle's width, which the expansion power-up changes for a while.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
    public class PaddleController : MonoBehaviour
    {
        [SerializeField] private GameConfig config;
        [SerializeField, Tooltip("The 9-sliced sprite. Width changes set its Size, never its Scale.")]
        private SpriteRenderer body;
        [SerializeField] private Collider2D leftWall;
        [SerializeField] private Collider2D rightWall;

        private Rigidbody2D rb;
        private BoxCollider2D box;
        private Camera cam;

        private float keyboardAxis;
        private bool mouseOwnsPaddle;
        private float mouseTargetX;
        private Vector3 lastMousePosition;

        private Coroutine expansionRoutine;
        private float expansionEndTime;

        /// <summary>Current width in world units.</summary>
        public float Width { get; private set; }

        /// <summary>-1 or +1 while moving left or right during the last physics step, 0 when still.</summary>
        public int MoveDirection { get; private set; }

        public bool IsExpanded => expansionRoutine != null;

        /// <summary>The visual body, which ImpactFeedback squashes. The collider is never scaled.</summary>
        public Transform Body => body.transform;

        public event Action BallHit;

        /// <summary>How much of the expansion is left, from 1 when caught down to 0 when it ends.</summary>
        public float ExpansionTimeLeft01 =>
            IsExpanded ? Mathf.Clamp01((expansionEndTime - Time.time) / config.PowerUpDuration) : 0f;

        private float MinX => leftWall.bounds.max.x + Width * 0.5f;
        private float MaxX => rightWall.bounds.min.x - Width * 0.5f;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            box = GetComponent<BoxCollider2D>();
            cam = Camera.main;
            lastMousePosition = Input.mousePosition;
            SetWidth(config.PaddleWidth);
        }

        private void OnEnable()
        {
            GameManager.Instance.StateChanged += HandleStateChanged;
            GameManager.Instance.ServeStarted += Recentre;
            GameManager.Instance.LifeLost += EndExpansion;
        }

        private void OnDisable()
        {
            if (GameManager.Instance == null)
                return;

            GameManager.Instance.StateChanged -= HandleStateChanged;
            GameManager.Instance.ServeStarted -= Recentre;
            GameManager.Instance.LifeLost -= EndExpansion;
        }

        private void Update()
        {
            if (GameManager.Instance.IsInPlay)
                ReadInput();
            else
                keyboardAxis = 0f;
        }

        private void FixedUpdate()
        {
            if (GameManager.Instance.IsInPlay)
                Move();
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.collider.TryGetComponent(out BallController _))
                BallHit?.Invoke();
        }

        /// <summary>
        /// Widens the paddle for PowerUpDuration. Catching another capsule while it runs restarts the
        /// timer and never stacks the width (GDD section 3).
        /// </summary>
        public void Expand()
        {
            if (expansionRoutine != null)
                StopCoroutine(expansionRoutine);

            SetWidth(config.PaddleWidth * config.PaddleExpandMultiplier);
            expansionRoutine = StartCoroutine(ExpansionTimer());
        }

        private IEnumerator ExpansionTimer()
        {
            expansionEndTime = Time.time + config.PowerUpDuration;
            yield return new WaitForSeconds(config.PowerUpDuration);
            expansionRoutine = null;
            SetWidth(config.PaddleWidth);
        }

        /// <summary>The effect ends at once when a life is lost or the level is cleared.</summary>
        private void EndExpansion()
        {
            if (expansionRoutine == null)
                return;

            StopCoroutine(expansionRoutine);
            expansionRoutine = null;
            SetWidth(config.PaddleWidth);
        }

        private void HandleStateChanged(GameState state)
        {
            if (state == GameState.LevelClear)
                EndExpansion();
        }

        /// <summary>
        /// Puts the paddle back in the middle of the field for a new serve and hands control back to
        /// the keyboard.
        /// </summary>
        private void Recentre()
        {
            float centreX = (leftWall.bounds.max.x + rightWall.bounds.min.x) * 0.5f;
            rb.position = new Vector2(centreX, rb.position.y);
            transform.position = rb.position;
            mouseOwnsPaddle = false;
            lastMousePosition = Input.mousePosition;
            MoveDirection = 0;
        }

        private void ReadInput()
        {
            keyboardAxis = Input.GetAxisRaw("Horizontal");
            if (keyboardAxis != 0f)
                mouseOwnsPaddle = false;

            Vector3 mousePosition = Input.mousePosition;
            if (mousePosition != lastMousePosition)
            {
                mouseOwnsPaddle = true;
                lastMousePosition = mousePosition;
            }

            if (mouseOwnsPaddle)
                mouseTargetX = cam.ScreenToWorldPoint(mousePosition).x;
        }

        private void Move()
        {
            float maxStep = config.PaddleSpeed * Time.fixedDeltaTime;
            float currentX = rb.position.x;
            float targetX = mouseOwnsPaddle ? mouseTargetX : currentX + keyboardAxis * maxStep;

            // The mouse is followed at PaddleSpeed too, so both controls reach the same balls.
            float newX = Mathf.Clamp(Mathf.MoveTowards(currentX, targetX, maxStep), MinX, MaxX);

            MoveDirection = Mathf.Approximately(newX, currentX) ? 0 : (int)Mathf.Sign(newX - currentX);
            rb.MovePosition(new Vector2(newX, rb.position.y));
        }

        private void SetWidth(float width)
        {
            Width = width;
            body.size = new Vector2(width, body.size.y);
            box.size = new Vector2(width, box.size.y);

            // Re-clamp at once, so a paddle that widens next to a wall is pushed back inside it.
            rb.position = new Vector2(Mathf.Clamp(rb.position.x, MinX, MaxX), rb.position.y);
        }
    }
}
