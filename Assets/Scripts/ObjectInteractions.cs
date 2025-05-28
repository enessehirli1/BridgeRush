using UnityEngine;

public class ObjectInteractions : MonoBehaviour
{
    private GameManager gameManager;

    void Start()
    {
        // GameManager'ý sahne içinde bul
        gameManager = FindAnyObjectByType<GameManager>();

        if (gameManager == null)
        {
            Debug.LogError("GameManager bulunamadý!");
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // Breakable tag'ine sahip objeye çarptýðýnda
        if (other.CompareTag("Breakable"))
        {
            // GameManager'ýn endGame fonksiyonunu çalýþtýr
            if (gameManager != null)
            {
                Debug.Log("Breakable objeye çarpýldý! Oyun bitiyor...");
                gameManager.endGame();
            }

            Debug.Log("Breakable objeye çarpýldý! Oyun bitiyor...");
        }
    }

}