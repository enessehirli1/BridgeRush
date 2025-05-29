using UnityEngine;

public class MenuMusicController : MonoBehaviour
{
    public AudioClip menuMusic; // Menu müziði (opsiyonel)
    private AudioSource audioSource;

    void Start()
    {
        // Sahnedeki tüm AudioSource'larý bul ve durdur
        AudioSource[] allAudioSources = FindObjectsByType<AudioSource>(FindObjectsSortMode.None);
        foreach (AudioSource source in allAudioSources)
        {
            if (source.isPlaying)
            {
                Debug.Log("Stopping audio: " + source.name);
                source.Stop();
            }
        }

        // Eðer menu müziði varsa çal (opsiyonel)
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