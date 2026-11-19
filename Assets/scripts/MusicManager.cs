using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    [SerializeField] private Transform musicSources;
    [SerializeField] private TMP_Text currentlyPlayingText;

    [Header("Low Health Mood")]
    [SerializeField, Range(0.5f, 1f)] private float lowHealthPitch = 0.65f;
    [SerializeField, Range(0f, 1f)] private float lowHealthVolume = 0.3f;
    [SerializeField, Range(500f, 22000f)] private float lowHealthCutoff = 850f;
    [SerializeField, Min(0.1f)] private float moodTransitionTime = 2.5f;

    private readonly List<AudioSource> tracks = new List<AudioSource>();
    private readonly List<int> playCounts = new List<int>();
    private readonly List<AudioLowPassFilter> lowPassFilters = new List<AudioLowPassFilter>();
    private int currentTrackIndex = -1;
    private float targetHealthPercent = 1f;
    private float appliedHealthPercent = 1f;
    private float healthMoodVelocity;
    private float appliedPitch = 1f;
    private float appliedVolume = 1f;
    private float appliedCutoff = 22000f;
    private float pitchVelocity;
    private float volumeVelocity;
    private float cutoffVelocity;
    private int lastLoggedHealthBand = -1;
    private int lastLoggedAppliedHealthBand = -1;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning($"Duplicate MusicManager '{name}' found; stopping its audio and removing it.", this);
            foreach (AudioSource source in GetComponentsInChildren<AudioSource>(true))
                source.Stop();
            enabled = false;
            Destroy(this);
            return;
        }

        Instance = this;
        Debug.Log($"MusicManager '{name}' registered as MusicManager.Instance.", this);
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private void Start()
    {
        if (musicSources == null)
        {
            Transform foundSources = transform.Find("music-sources");
            if (foundSources != null)
                musicSources = foundSources;
        }

        if (musicSources == null)
        {
            Debug.LogWarning("MusicManager could not find a child named 'music-sources'.", this);
            return;
        }

        AudioSource[] childSources = musicSources.GetComponentsInChildren<AudioSource>(true);
        Debug.Log($"MusicManager found '{musicSources.name}' with {childSources.Length} AudioSource component(s).", this);
        foreach (AudioSource source in childSources)
        {
            if (source.clip == null)
                continue;

            source.playOnAwake = false;
            source.loop = false;
            source.Stop();
            tracks.Add(source);
            playCounts.Add(0);
            AudioLowPassFilter lowPassFilter = source.GetComponent<AudioLowPassFilter>();
            if (lowPassFilter == null)
                lowPassFilter = source.gameObject.AddComponent<AudioLowPassFilter>();
            lowPassFilter.cutoffFrequency = 22000f;
            lowPassFilter.enabled = true;
            lowPassFilters.Add(lowPassFilter);
        }

        if (tracks.Count == 0)
        {
            Debug.LogWarning("MusicManager found no AudioSources with clips under 'music-sources'.", this);
            SetCurrentlyPlaying("No music");
            return;
        }

        Debug.Log($"MusicManager registered {tracks.Count} playable track(s).", this);
        PlayNextTrack();
    }

    private void OnEnable()
    {
        ApplyMusicVolume();
    }

    public void ApplyMusicVolume()
    {
        foreach (AudioSource source in GetComponentsInChildren<AudioSource>(true))
            source.volume = GameSettings.MusicVolume;
    }

    public void ApplyHealthMood(float healthPercent)
    {
        targetHealthPercent = Mathf.Clamp01(healthPercent);
        int healthBand = Mathf.Clamp(Mathf.FloorToInt(targetHealthPercent * 10f), 0, 10);
        if (healthBand != lastLoggedHealthBand)
        {
            lastLoggedHealthBand = healthBand;
            Debug.Log($"MusicManager received health: {targetHealthPercent:P0}.", this);
        }
    }

    private void Update()
    {
        appliedHealthPercent = Mathf.SmoothDamp(
            appliedHealthPercent,
            targetHealthPercent,
            ref healthMoodVelocity,
            moodTransitionTime,
            Mathf.Infinity,
            Time.unscaledDeltaTime);
        ApplyCurrentHealthMood();
        EnsureSingleTrackPlaying();
    }

    private void EnsureSingleTrackPlaying()
    {
        if (tracks.Count == 0)
            return;

        int playingTrackIndex = -1;
        for (int i = 0; i < tracks.Count; i++)
        {
            if (tracks[i] == null || !tracks[i].isPlaying)
                continue;

            if (i == currentTrackIndex)
            {
                playingTrackIndex = i;
                break;
            }

            if (playingTrackIndex < 0)
                playingTrackIndex = i;
        }

        if (playingTrackIndex < 0)
        {
            PlayNextTrack();
            return;
        }

        currentTrackIndex = playingTrackIndex;
        for (int i = 0; i < tracks.Count; i++)
        {
            if (i != currentTrackIndex && tracks[i] != null && tracks[i].isPlaying)
                tracks[i].Stop();
        }
    }

    private void ApplyCurrentHealthMood()
    {
        float danger = 1f - appliedHealthPercent;
        float targetPitch = Mathf.Lerp(1f, lowHealthPitch, danger);
        float targetVolume = Mathf.Lerp(1f, lowHealthVolume, danger);
        float targetCutoff = Mathf.Lerp(22000f, lowHealthCutoff, danger);

        appliedPitch = Mathf.SmoothDamp(appliedPitch, targetPitch, ref pitchVelocity, moodTransitionTime, Mathf.Infinity, Time.unscaledDeltaTime);
        appliedVolume = Mathf.SmoothDamp(appliedVolume, targetVolume, ref volumeVelocity, moodTransitionTime, Mathf.Infinity, Time.unscaledDeltaTime);
        appliedCutoff = Mathf.SmoothDamp(appliedCutoff, targetCutoff, ref cutoffVelocity, moodTransitionTime, Mathf.Infinity, Time.unscaledDeltaTime);

        for (int i = 0; i < tracks.Count; i++)
        {
            tracks[i].pitch = appliedPitch;
            tracks[i].volume = GameSettings.MusicVolume * appliedVolume;
            lowPassFilters[i].cutoffFrequency = appliedCutoff;
        }

        int healthBand = Mathf.Clamp(Mathf.FloorToInt(appliedHealthPercent * 10f), 0, 10);
        if (healthBand != lastLoggedAppliedHealthBand)
        {
            lastLoggedAppliedHealthBand = healthBand;
            Debug.Log($"MusicManager applied mood: health {appliedHealthPercent:P0}, pitch {appliedPitch:F2}, volume {appliedVolume:P0}, cutoff {appliedCutoff:F0} Hz, tracks {tracks.Count}.", this);
        }
    }

    private void PlayNextTrack()
    {
        if (tracks.Count == 0)
            return;

        int previousTrackIndex = currentTrackIndex;
        if (tracks.Count == 1)
        {
            currentTrackIndex = 0;
        }
        else
        {
            float totalWeight = 0f;
            for (int i = 0; i < tracks.Count; i++)
            {
                if (i != previousTrackIndex)
                    totalWeight += 1f / (playCounts[i] + 1f);
            }

            float selection = Random.value * totalWeight;
            currentTrackIndex = -1;
            for (int i = 0; i < tracks.Count; i++)
            {
                if (i == previousTrackIndex)
                    continue;

                selection -= 1f / (playCounts[i] + 1f);
                if (selection <= 0f)
                {
                    currentTrackIndex = i;
                    break;
                }
            }

            if (currentTrackIndex < 0)
            {
                for (int i = 0; i < tracks.Count; i++)
                {
                    if (i != previousTrackIndex)
                    {
                        currentTrackIndex = i;
                        break;
                    }
                }
            }
        }

        for (int i = 0; i < tracks.Count; i++)
        {
            if (tracks[i] != null)
                tracks[i].Stop();
        }

        AudioSource track = tracks[currentTrackIndex];
        if (track == null)
        {
            Debug.LogError("MusicManager selected a missing AudioSource; no track can be started.", this);
            return;
        }

        ApplyCurrentHealthMood();
        track.Play();
        playCounts[currentTrackIndex]++;
        SetCurrentlyPlaying(track.clip.name);
    }

    private void SetCurrentlyPlaying(string trackName)
    {
        if (currentlyPlayingText != null)
            currentlyPlayingText.text = "Now Playing: " + trackName;
    }
}
