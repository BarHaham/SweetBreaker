using UnityEngine;

namespace SweetBreaker
{
    /// <summary>
    /// Moves the paddle along X only. Input is read in Update and applied in FixedUpdate; keyboard
    /// and mouse are both live, and whichever moved last owns the paddle (GDD section 4).
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

        /// <summary>Current width in world units.</summary>
        public float Width { get; private set; }

        /// <summary>-1 or +1 while moving left or right during the last physics step, 0 when still.</summary>
        public int MoveDirection { get; private set; }

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

        private void Update()
        {
            ReadInput();
        }

        private void FixedUpdate()
        {
            Move();
        }

        /// <summary>Puts the paddle back in the middle of the field and hands control back to the keyboard.</summary>
        public void Recentre()
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
        }
    }
}
