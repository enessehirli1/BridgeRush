using UnityEngine;
using UnityEngine.SceneManagement;

public class startMusic : MonoBehaviour
{
    public AudioClip mainMusic;
    private AudioSource audioSource;
    private bool hasInitialized = false;

    void Start()
    {
        if (!hasInitialized)
        {
            // Ýlk çalýþmada setup yap
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.loop = true;
            hasInitialized = true;

            // Sahne deðiþim olayýný dinle
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        // Her Start()'ta sahne kontrolü yap
        CheckAndPlayMusic();
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        CheckAndPlayMusic();
    }

    void CheckAndPlayMusic()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        Debug.Log("Current scene: " + sceneName);

        if (sceneName == "Level01" || sceneName == "Level02")
        {
            if (!audioSource.isPlaying || audioSource.clip != mainMusic)
            {
                audioSource.clip = mainMusic;
                audioSource.Play();
                Debug.Log("Music started in: " + sceneName);
            }
        }
        else
        {
            if (audioSource.isPlaying)
            {
                audioSource.Stop();
                Debug.Log("Music stopped in: " + sceneName);
            }
        }
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}