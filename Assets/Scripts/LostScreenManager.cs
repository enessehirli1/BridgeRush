using UnityEngine;
using TMPro;

public class GameOverScoreDisplay : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI scoreText; // Inspector'dan atayýn veya otomatik bulacak

    void Start()
    {
        // Eðer scoreText referansý atanmamýþsa otomatik bul
        if (scoreText == null)
        {
            FindScoreText();
        }

        // Skoru göster
        DisplayHighScore();
    }

    void FindScoreText()
    {
        // "score" adýndaki TextMeshProUGUI objesini bul
        GameObject scoreObj = GameObject.Find("Score");
        if (scoreObj != null)
        {
            scoreText = scoreObj.GetComponent<TextMeshProUGUI>();
            if (scoreText != null)
            {
                Debug.Log("Score text found automatically!");
            }
            else
            {
                Debug.LogError("score GameObject found but no TextMeshProUGUI component!");
            }
        }
        else
        {
            Debug.LogError("score GameObject not found in Lost scene!");
        }
    }

    void DisplayHighScore()
    {
        if (scoreText == null)
        {
            Debug.LogError("Score text reference is null!");
            return;
        }

        // Font kontrolü
        if (scoreText.font == null)
        {
            Debug.LogWarning("TextMeshPro font asset is null! Using default font.");
            // Default TextMeshPro font'unu kullan
            scoreText.font = Resources.GetBuiltinResource<TMPro.TMP_FontAsset>("LiberationSans SDF");
        }

        if (ScoreManager.instance == null)
        {
            Debug.LogError("ScoreManager instance not found!");
            scoreText.text = "0";
            return;
        }

        // En yüksek skoru al ve göster
        float highScore = ScoreManager.instance.GetSavedHighScore();

        // Eðer kaydedilmiþ skor 0 ise, mevcut en yüksek skoru kullan (fallback)
        if (highScore <= 0)
        {
            highScore = ScoreManager.instance.GetOverallHighScore();
        }

        scoreText.text = highScore.ToString("0");

        Debug.Log("Game Over - Saved High Score Displayed: " + highScore);
        Debug.Log("Font Asset: " + (scoreText.font != null ? scoreText.font.name : "NULL"));

        // Debug için tüm skorlarý yazdýr
        ScoreManager.instance.DebugAllScores();
    }
}