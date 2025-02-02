using UnityEngine;

/// <summary>
/// A singleton SoundManager that controls overall volume, music, SFX, and dialogue channels.
/// </summary>
public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;

    [Header("Master Volume")]
    [Range(0f, 1f)]
    [Tooltip("Overall volume that affects all sound channels.")]
    public float overallVolume = 1f;

    [Header("Channel Volumes (0 = mute, 1 = full volume)")]
    [Range(0f, 1f)]
    [Tooltip("Volume for background music.")]
    public float musicVolume = 1f;
    [Range(0f, 1f)]
    [Tooltip("Volume for sound effects.")]
    public float sfxVolume = 1f;
    [Range(0f, 1f)]
    [Tooltip("Volume for dialogue.")]
    public float dialogueVolume = 1f;

    [Header("Audio Sources & Prefabs")]
    [Tooltip("AudioSource for playing background music. Should be set to 2D (Spatial Blend = 0).")]
    public AudioSource musicSource;

    [Tooltip("Prefab with an AudioSource for playing 3D SFX.")]
    public AudioSource sfxSourcePrefab;

    [Tooltip("Prefab with an AudioSource for playing 3D dialogue.")]
    public AudioSource dialogueSourcePrefab;

    private void Awake()
    {
        // Enforce the singleton pattern.
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // Persist across scenes.
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        // Update the music source volume if it exists.
        if (musicSource != null)
        {
            // Effective volume is the product of overallVolume and musicVolume.
            musicSource.volume = overallVolume * musicVolume;
        }
    }

    #region Overall Volume Control

    /// <summary>
    /// Sets the overall volume (master volume for all channels).
    /// </summary>
    /// <param name="value">Volume value between 0 and 1.</param>
    public void SetOverallVolume(float value)
    {
        overallVolume = Mathf.Clamp01(value);
        // Update the musicSource immediately (other channels will use this value when instantiated).
        if (musicSource != null)
        {
            musicSource.volume = overallVolume * musicVolume;
        }
    }

    #endregion

    #region Music Controls

    /// <summary>
    /// Plays background music.
    /// </summary>
    /// <param name="clip">The AudioClip to play.</param>
    /// <param name="loop">Should the clip loop?</param>
    public void PlayMusic(AudioClip clip, bool loop = true)
    {
        if (musicSource != null && clip != null)
        {
            musicSource.clip = clip;
            musicSource.loop = loop;
            // Effective volume is overallVolume multiplied by musicVolume.
            musicSource.volume = overallVolume * musicVolume;
            musicSource.Play();
        }
        else
        {
            Debug.LogWarning("MusicSource or AudioClip is missing!");
        }
    }

    /// <summary>
    /// Stops the currently playing background music.
    /// </summary>
    public void StopMusic()
    {
        if (musicSource != null)
        {
            musicSource.Stop();
        }
    }

    /// <summary>
    /// Sets the music channel volume (call this from your UI slider).
    /// </summary>
    /// <param name="value">Volume value between 0 and 1.</param>
    public void SetMusicVolume(float value)
    {
        musicVolume = Mathf.Clamp01(value);
        if (musicSource != null)
        {
            musicSource.volume = overallVolume * musicVolume;
        }
    }

    #endregion

    #region SFX Controls

    /// <summary>
    /// Plays a sound effect at a specific 3D location.
    /// </summary>
    /// <param name="clip">The AudioClip for the SFX.</param>
    /// <param name="position">The world position to play the sound from.</param>
    public void PlaySFX(AudioClip clip, Vector3 position)
    {
        if (clip == null)
        {
            Debug.LogWarning("SFX clip is null!");
            return;
        }

        // Instantiate a new AudioSource from the SFX prefab at the specified position.
        AudioSource newSFX = Instantiate(sfxSourcePrefab, position, Quaternion.identity);
        newSFX.spatialBlend = 1f; // Ensure it's fully 3D.
        // Effective volume is overallVolume multiplied by sfxVolume.
        newSFX.volume = overallVolume * sfxVolume;
        newSFX.clip = clip;
        newSFX.Play();

        // Destroy the temporary AudioSource after the clip finishes playing.
        Destroy(newSFX.gameObject, clip.length);
    }

    /// <summary>
    /// Sets the SFX channel volume (call this from your UI slider).
    /// </summary>
    /// <param name="value">Volume value between 0 and 1.</param>
    public void SetSFXVolume(float value)
    {
        sfxVolume = Mathf.Clamp01(value);
    }

    #endregion

    #region Dialogue Controls

    /// <summary>
    /// Plays a dialogue clip from a speaker's location.
    /// </summary>
    /// <param name="clip">The dialogue AudioClip.</param>
    /// <param name="speakerTransform">The transform of the speaker so the sound appears to come from their location.</param>
    public void PlayDialogue(AudioClip clip, Transform speakerTransform)
    {
        if (clip == null)
        {
            Debug.LogWarning("Dialogue clip is null!");
            return;
        }

        if (speakerTransform == null)
        {
            Debug.LogWarning("Speaker Transform is null!");
            return;
        }

        // Instantiate a new AudioSource from the dialogue prefab at the speaker's location.
        AudioSource newDialogue = Instantiate(dialogueSourcePrefab, speakerTransform.position, speakerTransform.rotation);
        // Optionally parent it to the speaker so it moves with them.
        newDialogue.transform.parent = speakerTransform;
        newDialogue.spatialBlend = 1f; // Ensure 3D audio.
        // Effective volume is overallVolume multiplied by dialogueVolume.
        newDialogue.volume = overallVolume * dialogueVolume;
        newDialogue.clip = clip;
        newDialogue.Play();

        // Destroy the temporary AudioSource after the clip finishes playing.
        Destroy(newDialogue.gameObject, clip.length);
    }

    /// <summary>
    /// Sets the dialogue channel volume (call this from your UI slider).
    /// </summary>
    /// <param name="value">Volume value between 0 and 1.</param>
    public void SetDialogueVolume(float value)
    {
        dialogueVolume = Mathf.Clamp01(value);
    }

    #endregion
}
