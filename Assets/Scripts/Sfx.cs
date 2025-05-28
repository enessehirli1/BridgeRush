using UnityEngine;

public class Sfx : MonoBehaviour
{
    public AudioClip slashSound;
    public AudioClip swordSound;
    public AudioClip jumpSound;
    public AudioClip rollSound;
    public AudioClip deathSound;
    public AudioClip fallSound;
    private AudioSource audioSource;
    private AudioSource audioSource2;

    public void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource2 = gameObject.AddComponent<AudioSource>();

    }

    public void slashMusic()
    {
        audioSource.clip = slashSound;
        audioSource.Play();
        
    }

    public void swordMusic()
    {
        audioSource2.clip = swordSound;
        audioSource2.Play();
    }

    public void jumpMusic()
    {
        audioSource.clip = jumpSound;
        audioSource.Play();
    }

    public void rollMusic()
    {
        audioSource.clip = rollSound;
        audioSource.Play();
    }

    public void deathMusic()
    {
        audioSource.clip = deathSound;
        audioSource.Play();
    }

    public void fallMusic()
    {
        audioSource.clip = fallSound;
        audioSource.Play();
    }   


    void Update()
    {
        
    }
}
