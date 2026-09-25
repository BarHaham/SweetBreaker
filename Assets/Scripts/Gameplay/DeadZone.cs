using UnityEngine;

namespace SweetBreaker
{
    /// <summary>
    /// A trigger below the paddle line. A ball that enters it costs a life (GDD section 3).
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class DeadZone : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.TryGetComponent(out BallController _))
                GameManager.Instance.LoseLife();
        }
    }
}
