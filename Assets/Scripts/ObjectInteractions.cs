using UnityEngine;

public class ObjectInteractions : MonoBehaviour
{
    private GameManager gameManager;

    void Start()
    {
        // GameManager'� sahne i�inde bul
        gameManager = FindAnyObjectByType<GameManager>();

        if (gameManager == null)
        {
            Debug.LogError("GameManager bulunamad�!");
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // Breakable tag'ine sahip objeye �arpt���nda
        if (other.CompareTag("Breakable"))
        {
            // GameManager'�n endGame fonksiyonunu �al��t�r
            if (gameManager != null)
            {
                Debug.Log("Breakable objeye �arp�ld�! Oyun bitiyor...");
                gameManager.endGame();
            }

            Debug.Log("Breakable objeye �arp�ld�! Oyun bitiyor...");
        }
    }

}