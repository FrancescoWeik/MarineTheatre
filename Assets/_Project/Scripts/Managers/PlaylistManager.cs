using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaylistManager : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private List<AudioClip> playlist;

    private int currentIndex = 0;

    void Start()
    {
        StartCoroutine(PlayPlaylist());
    }

    IEnumerator PlayPlaylist()
    {
        while (true) // loop infinito sulla playlist
        {
            audioSource.clip = playlist[currentIndex];
            audioSource.Play();

            // Aspetta finché la clip non è finita
            yield return new WaitWhile(() => audioSource.isPlaying);

            currentIndex++;
            if (currentIndex >= playlist.Count)
                currentIndex = 0; // torna all'inizio
        }
    }
}