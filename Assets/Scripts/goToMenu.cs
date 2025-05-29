using UnityEngine;
using UnityEngine.SceneManagement;

public class goToMenu : MonoBehaviour
{
    // Bu scripti, level sonu trigger'lar�na ekleyebilirsiniz
    private void OnTriggerEnter(Collider other)
    {
        // E�er player bu trigger'a girerse
        if (other.CompareTag("Player"))
        {
            goMenu();
        }
    }

    public void goMenu()
    {
        // GameManager varsa sonraki seviyeye ge�
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