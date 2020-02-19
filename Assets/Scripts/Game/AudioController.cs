using Utils;
using System;
using System.Linq;
using UnityEngine;

using static UnityEngine.Random;

[Serializable] public class Sound {
    public string tag;
    public AudioClip clip;

    [Range(0f, 1f)] public float volume = 1f;
    [Range(-3f, 3f)] public float pitch = 1f;
    public bool loop = false;

    [HideInInspector] public AudioSource source;
}

public class AudioController : MonoBehaviour {
    #region editor
    public bool on = true;
    [SerializeField] private Sound[] sounds = default;
    #endregion

    #region private
    private void Awake() {
        foreach (Sound s in sounds) {
            GameObject sourceObject = new GameObject(s.tag);
            AudioSource source = sourceObject.AddComponent<AudioSource>();
            Utility.parentTo(sourceObject, gameObject);

            s.source = source;
            s.source.clip = s.clip;
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
        }
    }
    #endregion

    #region public
    public void play(string tag) {
        if (!on) return;
        Sound sound = sounds.FirstOrDefault(s => s.tag == tag);
        if (sound != null) sound.source.Play();
    }

    public void playRandom() {
        if (!on) return;
        sounds[Range(0, sounds.Length)].source.Play();
    }

    public void jump() {
        play("jump");
    }

    public void brokenPlatform() {
        play($"glassSmash{Range(0, 5)}");
    }

    public void stop(string tag) {
        if (!on) return;
        Sound sound = sounds.FirstOrDefault(s => s.tag == tag);
        if (sound != null) sound.source.Play();
    }
    #endregion
}
