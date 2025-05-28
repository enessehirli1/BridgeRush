/*
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Score : MonoBehaviour
{
    public Transform player;
    public TextMeshPro scoreText;
    private float currentScore;

    void Start()
    {
        // E�er GameManager varsa, mevcut skoru ondan al
        if (GameManager.instance != null)
        {
            currentScore = GameManager.instance.GetCurrentScore();
        }
        else
        {
            currentScore = 0;
            Debug.LogWarning("GameManager instance not found!");
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Oyuncunun x pozisyonuna g�re hesaplanan skoru g�ncelle
        float calculatedScore = player.position.x - 17;

        // Hesaplanan skor, mevcut skordan b�y�kse g�ncelle
        // Bu sayede yeni sahnede ba�lang�� skoru korunur ve onun �zerine ekleme yap�l�r
        if (calculatedScore > currentScore)
        {
            currentScore = calculatedScore;
        }

        // Skoru tam say� olarak g�ster
        scoreText.text = currentScore.ToString("0");

        // GameManager'� g�ncelle - bu �ekilde sahne ge�i�lerinde skor korunacak
        if (GameManager.instance != null)
        {
            GameManager.instance.UpdateScore(currentScore);
        }
    }
}
*/