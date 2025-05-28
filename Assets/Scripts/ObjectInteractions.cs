using UnityEngine;

public class ObjectInteractions : MonoBehaviour
{
    private GameManager gameManager;
    private bool hasTriggered = false; // Aynı objeye birden fazla trigger engellemek için

    void Start()
    {
        gameManager = FindAnyObjectByType<GameManager>();
        if (gameManager == null)
        {
            Debug.LogError("GameManager bulunamadı!");
        }
    }

    void OnEnable()
    {
        // Her sahne yüklendiğinde trigger durumunu sıfırla
        hasTriggered = false;
    }

    void OnTriggerEnter(Collider other)
    {
        // Eğer zaten trigger olduysa tekrar çalışmasın
        if (hasTriggered) return;

        // Breakable tag'ine sahip objeye çarptığında
        if (other.CompareTag("Breakable") || other.CompareTag("Mutant") || other.CompareTag("barbedFence") || other.CompareTag("Bridge"))
        {
            Debug.Log("Breakable objeye çarpıldı! Oyun bitiyor...");

            // GameManager'ın endGame fonksiyonunu çalıştır
            if (gameManager != null)
            {
                hasTriggered = true; // Tekrar trigger olmasını engelle
                if (other.CompareTag("Breakable") || other.CompareTag("barbedFence") || other.CompareTag("Bridge"))
                    gameManager.endGame();
                else if (other.CompareTag("Mutant"))
                    gameManager.endGameMutant();
                
            }
            else
            {
                Debug.LogError("GameManager referansı null!");
                // GameManager'ı tekrar bulmaya çalış
                gameManager = FindAnyObjectByType<GameManager>();
                if (gameManager != null)
                {
                    hasTriggered = true;
                    gameManager.endGame();
                }
            }
        }
    }

    // Debug amaçlı - breakable objelerle ne zaman temas kurduğunu görmek için
    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Breakable") && !hasTriggered)
        {
            Debug.Log("Breakable obje ile temas halinde...");
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Breakable"))
        {
            Debug.Log("Breakable objeden ayrıldı");
        }
    }
}