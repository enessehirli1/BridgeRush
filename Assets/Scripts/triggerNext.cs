using UnityEngine;

public class LevelComplete : MonoBehaviour
{
    // Bu scripti, level sonu trigger'lar�na ekleyebilirsiniz
    private void OnTriggerEnter(Collider other)
    {
        // E�er player bu trigger'a girerse
        if (other.CompareTag("Player"))
        {
            CompleteLevel();
        }
    }

    public void CompleteLevel()
    {
        // GameManager varsa sonraki seviyeye ge�
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