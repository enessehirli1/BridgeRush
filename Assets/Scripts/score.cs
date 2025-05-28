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
        // Eðer GameManager varsa, mevcut skoru ondan al
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
        // Oyuncunun x pozisyonuna göre hesaplanan skoru güncelle
        float calculatedScore = player.position.x - 17;

        // Hesaplanan skor, mevcut skordan büyükse güncelle
        // Bu sayede yeni sahnede baþlangýç skoru korunur ve onun üzerine ekleme yapýlýr
        if (calculatedScore > currentScore)
        {
            currentScore = calculatedScore;
        }

        // Skoru tam sayý olarak göster
        scoreText.text = currentScore.ToString("0");

        // GameManager'ý güncelle - bu þekilde sahne geçiþlerinde skor korunacak
        if (GameManager.instance != null)
        {
            GameManager.instance.UpdateScore(currentScore);
        }
    }
}
*/