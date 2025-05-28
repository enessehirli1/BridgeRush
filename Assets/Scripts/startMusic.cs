using UnityEngine;
using UnityEngine.SceneManagement;
public class startMusic : MonoBehaviour
{
    public AudioClip mainMusic;
    private AudioSource audioSource;

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();

        if (SceneManager.GetActiveScene().name == "Level01")
        {
            audioSource.clip = mainMusic;
            audioSource.loop = true;
            audioSource.Play();
        }

    }

    /*
    void Update()
    {
        // Oyun sahnesi deðiþtiðinde müziði durdur
        if (SceneManager.GetActiveScene().name == "Level06" && audioSource.isPlaying)
        {
            Debug.Log("Stopping music in Level06");
            audioSource.Stop();
        }
    }
    */
}
