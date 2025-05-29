using UnityEngine;
using UnityEngine.SceneManagement;

public class startMusic : MonoBehaviour
{
    public AudioClip mainMusic;
    private AudioSource audioSource;

    void Start()
    {
        // Sahne baþýnda tüm ayný tipte objeleri kontrol et
        startMusic[] musicManagers = FindObjectsByType<startMusic>(FindObjectsSortMode.None);
        if (musicManagers.Length > 1)
        {
            // Eðer birden fazla varsa, bu objeyi yok et
            Destroy(gameObject);
            return;
        }

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.loop = true;

        string sceneName = SceneManager.GetActiveScene().name;

        if (sceneName == "Level01" || sceneName == "Level02")
        {
            audioSource.clip = mainMusic;
            audioSource.Play();
            Debug.Log("Music started in: " + sceneName);
        }
    }
}