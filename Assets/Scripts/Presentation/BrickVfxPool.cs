using System.Collections;
using UnityEngine;
using UnityEngine.Pool;

namespace SweetBreaker
{
    /// <summary>
    /// Holds and recycles the brick-break effects: the particle bursts and the thrown shards.
    /// A good rally destroys several bricks a second, which makes these the one thing in the game
    /// created and thrown away over and over, so they come from ObjectPools instead of
    /// Instantiate/Destroy (GDD sections 6 and 7).
    /// </summary>
    public class BrickVfxPool : MonoBehaviour
    {
        [SerializeField] private LevelManager levelManager;

        [Header("Particle bursts")]
        [SerializeField] private ParticleSystem burstPrefab;
        [SerializeField, Min(1)] private int burstPoolSize = 8;

        [Header("Shards")]
        [SerializeField] private Shard shardPrefab;
        [SerializeField] private Sprite[] shardSprites;
        [SerializeField, Min(1)] private int shardPoolSize = 48;
        [SerializeField, Min(1)] private int shardsPerBreak = 6;
        [SerializeField] private Vector2 shardSpeedRange = new Vector2(2.5f, 5.5f);
        [SerializeField, Tooltip("Degrees per second, either way.")]
        private float maxShardSpin = 720f;

        private ObjectPool<ParticleSystem> bursts;
        private ObjectPool<Shard> shards;

        private void Awake()
        {
            bursts = new ObjectPool<ParticleSystem>(
                createFunc: () => Instantiate(burstPrefab, transform),
                actionOnGet: burst => burst.gameObject.SetActive(true),
                actionOnRelease: burst => burst.gameObject.SetActive(false),
                actionOnDestroy: burst => Destroy(burst.gameObject),
                defaultCapacity: burstPoolSize,
                maxSize: burstPoolSize);

            shards = new ObjectPool<Shard>(
                createFunc: () => Instantiate(shardPrefab, transform),
                actionOnGet: shard => shard.gameObject.SetActive(true),
                actionOnRelease: shard => shard.gameObject.SetActive(false),
                actionOnDestroy: shard => Destroy(shard.gameObject),
                defaultCapacity: shardPoolSize,
                maxSize: shardPoolSize);
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
            Vector2 position = brick.transform.position;
            PlayBurst(position, brick.BreakColor);
            ThrowShards(position, brick.BreakColor);
        }

        private void PlayBurst(Vector2 position, Color color)
        {
            ParticleSystem burst = bursts.Get();
            burst.transform.position = position;
            ParticleSystem.MainModule main = burst.main;
            main.startColor = color;
            burst.Play();
            StartCoroutine(ReleaseWhenFinished(burst));
        }

        private IEnumerator ReleaseWhenFinished(ParticleSystem burst)
        {
            yield return new WaitWhile(() => burst.IsAlive());
            bursts.Release(burst);
        }

        /// <summary>Shards fly up and out from the break point, then fall back under gravity.</summary>
        private void ThrowShards(Vector2 position, Color color)
        {
            for (int i = 0; i < shardsPerBreak; i++)
            {
                float angle = Random.Range(20f, 160f) * Mathf.Deg2Rad;
                Vector2 velocity = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * Random.Range(shardSpeedRange.x, shardSpeedRange.y);
                Vector2 start = position + new Vector2(Random.Range(-0.5f, 0.5f), Random.Range(-0.15f, 0.15f));
                Sprite sprite = shardSprites[Random.Range(0, shardSprites.Length)];

                shards.Get().Throw(start, velocity, Random.Range(-maxShardSpin, maxShardSpin), sprite, color, shards.Release);
            }
        }
    }
}
