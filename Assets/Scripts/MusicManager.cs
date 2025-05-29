using UnityEngine;

public class MenuMusicController : MonoBehaviour
{
    public AudioClip menuMusic; // Menu m�zi�i (opsiyonel)
    private AudioSource audioSource;

    void Start()
    {
        // Sahnedeki t�m AudioSource'lar� bul ve durdur
        AudioSource[] allAudioSources = FindObjectsByType<AudioSource>(FindObjectsSortMode.None);
        foreach (AudioSource source in allAudioSources)
        {
            if (source.isPlaying)
            {
                Debug.Log("Stopping audio: " + source.name);
                source.Stop();
            }
        }

        // E�er menu m�zi�i varsa �al (opsiyonel)
        if (menuMusic != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.clip = menuMusic;
            audioSource.loop = true;
            audioSource.Play();
            Debug.Log("Menu music started");
        }
    }
}