using System.Collections;
using UnityEngine;
using UnityEngine.Pool;

namespace SweetBreaker
{
    /// <summary>
    /// Holds and recycles the brick-break particle bursts. A good rally destroys several bricks a
    /// second, which makes these the one thing in the game created and thrown away over and over,
    /// so they come from an ObjectPool instead of Instantiate/Destroy (GDD section 7).
    /// </summary>
    public class BrickVfxPool : MonoBehaviour
    {
        [SerializeField] private LevelManager levelManager;
        [SerializeField] private ParticleSystem burstPrefab;
        [SerializeField, Min(1)] private int burstPoolSize = 8;

        private ObjectPool<ParticleSystem> bursts;

        private void Awake()
        {
            bursts = new ObjectPool<ParticleSystem>(
                createFunc: () => Instantiate(burstPrefab, transform),
                actionOnGet: burst => burst.gameObject.SetActive(true),
                actionOnRelease: burst => burst.gameObject.SetActive(false),
                actionOnDestroy: burst => Destroy(burst.gameObject),
                defaultCapacity: burstPoolSize,
                maxSize: burstPoolSize);
        }

        private void OnEnable()
        {
            levelManager.BrickDestroyed += PlayBreak;
        }

        private void OnDisable()
        {
            levelManager.BrickDestroyed -= PlayBreak;
        }

        private void PlayBreak(Brick brick)
        {
            ParticleSystem burst = bursts.Get();
            burst.transform.position = brick.transform.position;
            ParticleSystem.MainModule main = burst.main;
            main.startColor = brick.BreakColor;
            burst.Play();
            StartCoroutine(ReleaseWhenFinished(burst));
        }

        private IEnumerator ReleaseWhenFinished(ParticleSystem burst)
        {
            yield return new WaitWhile(() => burst.IsAlive());
            bursts.Release(burst);
        }
    }
}
