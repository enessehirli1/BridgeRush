using UnityEngine;
using UnityEngine.SceneManagement;

public class goToMenu : MonoBehaviour
{
    // Bu scripti, level sonu trigger'larýna ekleyebilirsiniz
    private void OnTriggerEnter(Collider other)
    {
        // Eðer player bu trigger'a girerse
        if (other.CompareTag("Player"))
        {
            goMenu();
        }
    }

    public void goMenu()
    {
        // GameManager varsa sonraki seviyeye geç
        if (GameManager.instance != null)
        {
            Debug.Log("Trainer completed! Moving to Menu...");
            SceneManager.LoadScene("Menu");
        }
        else
        {
            Debug.LogError("GameManager instance not found!");
        }
    }
}