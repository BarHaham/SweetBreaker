using UnityEngine;

namespace SweetBreaker
{
    /// <summary>
    /// A candy brick, destroyed by its first ball contact. It reports its own destruction to the
    /// LevelManager that built it.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class Brick : MonoBehaviour
    {
        private LevelManager owner;
        private bool isBroken;

        public void Initialize(LevelManager levelManager)
        {
            owner = levelManager;
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (!isBroken && collision.collider.TryGetComponent(out BallController _))
                Break();
        }

        private void Break()
        {
            // Destroy is deferred to the end of the frame, so a second contact this step must not count.
            isBroken = true;
            owner.ReportBrickDestroyed(this);
            Destroy(gameObject);
        }
    }
}
