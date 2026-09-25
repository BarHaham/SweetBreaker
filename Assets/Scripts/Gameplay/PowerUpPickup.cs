using UnityEngine;

namespace SweetBreaker
{
    /// <summary>
    /// The paddle-expansion capsule. It falls straight down; the paddle catching it widens the
    /// paddle, and a capsule that falls into the dead zone is simply gone, with no penalty.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class PowerUpPickup : MonoBehaviour
    {
        private Rigidbody2D rb;
        private float fallSpeed;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
        }

        public void Launch(float speed)
        {
            fallSpeed = speed;
        }

        private void FixedUpdate()
        {
            rb.linearVelocity = Vector2.down * fallSpeed;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.TryGetComponent(out PaddleController paddle))
            {
                paddle.Expand();
                Destroy(gameObject);
            }
            else if (other.TryGetComponent(out DeadZone _))
            {
                Destroy(gameObject);
            }
        }
    }
}
