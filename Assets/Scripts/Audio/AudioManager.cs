using System;
using System.Collections.Generic;
using UnityEngine;

namespace SweetBreaker
{
    /// <summary>The eight one-shot sounds from GDD section 6.</summary>
    public enum Sfx
    {
        PaddleBounce,
        WallBounce,
        BrickCrack,
        BrickBreak,
        PowerUpPickup,
        LifeLost,
        LevelClear,
        GameOver,
    }

    /// <summary>
    /// Plays one-shot sound effects on request. It lives on the GameManager object, so it survives
    /// scene loads with it; reach it through GameManager.Instance.Audio.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class AudioManager : MonoBehaviour
    {
        [Serializable]
        private struct SfxClip
        {
            public Sfx sound;
            public AudioClip clip;
            [Range(0f, 1f)] public float volume;
        }

        [SerializeField] private SfxClip[] clips;

        private readonly Dictionary<Sfx, SfxClip> clipsBySound = new Dictionary<Sfx, SfxClip>();
        private AudioSource source;

        private void Awake()
        {
            source = GetComponent<AudioSource>();
            foreach (SfxClip entry in clips)
                clipsBySound[entry.sound] = entry;
        }

        public void Play(Sfx sound)
        {
            if (clipsBySound.TryGetValue(sound, out SfxClip entry))
                source.PlayOneShot(entry.clip, entry.volume);
        }
    }
}
