using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    [SerializeField] private Transform musicSources;
    [SerializeField] private TMP_Text currentlyPlayingText;

    private readonly List<AudioSource> tracks = new List<AudioSource>();
    private readonly List<int> playCounts = new List<int>();
    private int currentTrackIndex = -1;

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
        foreach (AudioSource source in childSources)
        {
            if (source.clip == null)
                continue;

            source.playOnAwake = false;
            source.loop = false;
            source.Stop();
            tracks.Add(source);
            playCounts.Add(0);
        }

        if (tracks.Count == 0)
        {
            Debug.LogWarning("MusicManager found no AudioSources with clips under 'music-sources'.", this);
            SetCurrentlyPlaying("No music");
            return;
        }

        PlayNextTrack();
    }

    private void Update()
    {
        if (tracks.Count > 0 && !tracks[currentTrackIndex].isPlaying)
            PlayNextTrack();
    }

    private void PlayNextTrack()
    {
        if (tracks.Count == 1)
        {
            currentTrackIndex = 0;
        }
        else
        {
            float totalWeight = 0f;
            for (int i = 0; i < tracks.Count; i++)
            {
                if (i != currentTrackIndex)
                    totalWeight += 1f / (playCounts[i] + 1f);
            }

            float selection = Random.value * totalWeight;
            for (int i = 0; i < tracks.Count; i++)
            {
                if (i == currentTrackIndex)
                    continue;

                selection -= 1f / (playCounts[i] + 1f);
                if (selection <= 0f)
                {
                    currentTrackIndex = i;
                    break;
                }
            }
        }

        AudioSource track = tracks[currentTrackIndex];
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
