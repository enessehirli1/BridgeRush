using UnityEngine;

public class LevelComplete : MonoBehaviour
{
    // Bu scripti, level sonu trigger'larýna ekleyebilirsiniz
    private void OnTriggerEnter(Collider other)
    {
        // Eðer player bu trigger'a girerse
        if (other.CompareTag("Player"))
        {
            CompleteLevel();
        }
    }

    public void CompleteLevel()
    {
        // GameManager varsa sonraki seviyeye geç
        if (GameManager.instance != null)
        {
            Debug.Log("Level completed! Moving to next level...");
            GameManager.instance.GoToNextLevel();
        }
        else
        {
            Debug.LogError("GameManager instance not found!");
        }
    }
}