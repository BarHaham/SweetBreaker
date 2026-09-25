using System;
using UnityEngine;

namespace SweetBreaker
{
    /// <summary>
    /// One candy or chocolate fragment thrown from a broken brick. It falls, spins and fades, then
    /// hands itself back to the pool. It moves on scaled time, so hit-stop freezes it mid-air.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class Shard : MonoBehaviour
    {
        [SerializeField] private float gravity = 14f;
        [SerializeField] private float lifetime = 0.65f;

        private SpriteRenderer spriteRenderer;
        private Vector2 velocity;
        private float spin;
        private float age;
        private Color color;
        private Action<Shard> onFinished;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        public void Throw(Vector2 position, Vector2 startVelocity, float spinSpeed, Sprite sprite, Color tint, Action<Shard> finished)
        {
            transform.SetPositionAndRotation(position, Quaternion.Euler(0f, 0f, UnityEngine.Random.Range(0f, 360f)));
            velocity = startVelocity;
            spin = spinSpeed;
            age = 0f;
            color = tint;
            spriteRenderer.sprite = sprite;
            spriteRenderer.color = tint;
            onFinished = finished;
        }

        private void Update()
        {
            float deltaTime = Time.deltaTime;
            age += deltaTime;
            if (age >= lifetime)
            {
                onFinished(this);
                return;
            }

            velocity.y -= gravity * deltaTime;
            transform.position += (Vector3)(velocity * deltaTime);
            transform.Rotate(0f, 0f, spin * deltaTime);

            float fade = age / lifetime;
            spriteRenderer.color = new Color(color.r, color.g, color.b, color.a * (1f - fade * fade));
        }
    }
}
