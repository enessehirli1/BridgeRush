using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class ScoreManager : MonoBehaviour
{
    // Singleton
    public static ScoreManager instance;

    // Skor bilgilerini tutacak veri yap�s�
    [System.Serializable]
    public class SceneScoreData
    {
        public float baseScore; // Sahne ba�lang�� skoru
        public float currentScore; // Sahnedeki mevcut skor
        public float sceneHighScore; // Sahnede eri�ilen en y�ksek skor
    }

    // Her sahne i�in bir SceneScoreData tutuyoruz
    private Dictionary<int, SceneScoreData> scenesScoreData = new Dictionary<int, SceneScoreData>();

    // UI Referanslar�
    public TextMeshPro scoreText;

    // Oyuncu Referans�
    private Transform playerTransform;

    // �ld�kten sonra tekrar do�du�umuzda kullan�lacak flag
    private bool isRespawning = false;

    // Skor Hesaplama ��in Offset
    public float scoreOffset = 17f;

    [Header("Enemy Killing Points")]
    public float breakableScore = 250f;
    public float mutantScore = 750f;


    private void Awake()
    {
        // Singleton olu�turma
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }

    private void OnEnable()
    {
        // Sahne de�i�ti�inde �a�r�lacak fonksiyonu kaydedelim
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        InitializeCurrentScene();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Sahne y�klendi�inde �a�r�l�r
        InitializeCurrentScene();

        Debug.Log("Sahne y�klendi: " + scene.name + " (index: " + scene.buildIndex + ")");

        // E�er bu bir respawn de�ilse ve yeni bir sahneye ge�iyorsak:
        if (!isRespawning)
        {
            int sceneIndex = scene.buildIndex;

            // Yeni bir sahneye ge�ildi�inde, �nceki sahnenin highScore'unu bir sonraki sahnenin baseScore'u yap
            int previousSceneIndex = sceneIndex - 1;

            if (previousSceneIndex >= 0 && scenesScoreData.ContainsKey(previousSceneIndex))
            {
                float previousHighScore = scenesScoreData[previousSceneIndex].sceneHighScore;

                // Bu sahne i�in veri yoksa olu�tur
                if (!scenesScoreData.ContainsKey(sceneIndex))
                {
                    scenesScoreData[sceneIndex] = new SceneScoreData();
                }

                // �nceki sahnenin en y�ksek skorunu bu sahnenin ba�lang�� skoru yap
                scenesScoreData[sceneIndex].baseScore = previousHighScore;
                scenesScoreData[sceneIndex].currentScore = previousHighScore;

                Debug.Log("Yeni sahnenin ba�lang�� skoru ayarland�: " + previousHighScore);
            }
            // �lk sahne veya ba�ka bir durumda, base score'u 0 olarak ayarla (e�er yoksa)
            else if (!scenesScoreData.ContainsKey(sceneIndex))
            {
                scenesScoreData[sceneIndex] = new SceneScoreData();
                scenesScoreData[sceneIndex].baseScore = 0;
                scenesScoreData[sceneIndex].currentScore = 0;
                scenesScoreData[sceneIndex].sceneHighScore = 0;
            }
        }
        else
        {
            // Bu bir respawn ise, isRespawning bayra��n� s�f�rla
            isRespawning = false;
        }

        // UI ve Player referanslar�n� bul
        FindReferences();

        // Skor g�stergesini g�ncelle
        UpdateScoreDisplay();
    }

    private void InitializeCurrentScene()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;

        // E�er bu sahne i�in veri yoksa olu�tur
        if (!scenesScoreData.ContainsKey(currentSceneIndex))
        {
            scenesScoreData[currentSceneIndex] = new SceneScoreData();
            scenesScoreData[currentSceneIndex].baseScore = 0;
            scenesScoreData[currentSceneIndex].currentScore = 0;
            scenesScoreData[currentSceneIndex].sceneHighScore = 0;
        }
    }

    private void FindReferences()
    {
        // Score Text referans�n� bul
        if (scoreText == null)
        {
            GameObject scoreObj = GameObject.Find("score");
            if (scoreObj != null)
            {
                scoreText = scoreObj.GetComponent<TextMeshPro>();
            }
        }

        // Player referans�n� bul
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
        }
    }

    private void Update()
    {
        // Player referans� yoksa bulmay� dene
        if (playerTransform == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                playerTransform = playerObj.transform;
            }
            else
            {
                return;
            }
        }

        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        if (!scenesScoreData.ContainsKey(currentSceneIndex))
        {
            InitializeCurrentScene();
        }

        SceneScoreData sceneData = scenesScoreData[currentSceneIndex];

        // Pozisyona g�re skor hesaplama
        float calculatedScore = playerTransform.position.x - scoreOffset;
        float totalScore = sceneData.baseScore + calculatedScore;

        // Skoru g�ncelle
        sceneData.currentScore = Mathf.Max(sceneData.baseScore, totalScore);

        // HighScore'u g�ncelle
        sceneData.sceneHighScore = Mathf.Max(sceneData.sceneHighScore, sceneData.currentScore);

        // UI'yi her frame g�ncelle
        UpdateScoreDisplay();
    }

    private void UpdateScoreDisplay()
    {
        if (scoreText != null)
        {
            int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
            if (scenesScoreData.ContainsKey(currentSceneIndex))
            {
                // Tam say� format�nda g�ster
                scoreText.text = scenesScoreData[currentSceneIndex].currentScore.ToString("0");
            }
        }
    }

    public void AddEnemyScore(string enemyType)
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;

        if (!scenesScoreData.ContainsKey(currentSceneIndex))
        {
            InitializeCurrentScene();
        }

        SceneScoreData sceneData = scenesScoreData[currentSceneIndex];

        float scoreToAdd = 0f;

        switch (enemyType.ToLower())
        {
            case "breakable":
                scoreToAdd = breakableScore;
                Debug.Log("Breakable obje yok edildi! +" + breakableScore + " puan");
                break;
            case "mutant":
                scoreToAdd = mutantScore;
                Debug.Log("Mutant �ld�r�ld�! +" + mutantScore + " puan");
                break;
            default:
                Debug.LogWarning("Bilinmeyen d��man tipi: " + enemyType);
                return;
        }

        // Puan� ekle
        sceneData.baseScore += scoreToAdd;
        sceneData.currentScore += scoreToAdd;
        sceneData.sceneHighScore = Mathf.Max(sceneData.sceneHighScore, sceneData.currentScore);

        // UI'yi g�ncelle
        UpdateScoreDisplay();

        Debug.Log("Toplam skor: " + sceneData.currentScore);
    }

    // Oyuncu �ld���nde �a�r�lacak metod
    public void PlayerDied()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;

        // Respawn oldu�unu i�aretle
        isRespawning = true;

        // Mevcut skoru, ba�lang�� skoruna geri d�nd�r
        if (scenesScoreData.ContainsKey(currentSceneIndex))
        {
            scenesScoreData[currentSceneIndex].currentScore = scenesScoreData[currentSceneIndex].baseScore;
            Debug.Log("Oyuncu �ld�. Skor, sahne ba�lang�� de�erine d�nd�r�ld�: " + scenesScoreData[currentSceneIndex].baseScore);
        }
    }

    // T�m canlar bitti�inde �a�r�lacak metod (tam Game Over)
    public void GameOver()
    {
        // T�m skor verilerini temizle
        scenesScoreData.Clear();
        Debug.Log("Game Over: T�m skor verileri s�f�rland�.");
    }

    // Sahnenin mevcut skorunu verir
    public float GetCurrentScore()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        if (scenesScoreData.ContainsKey(currentSceneIndex))
        {
            return scenesScoreData[currentSceneIndex].currentScore;
        }
        return 0;
    }

    // Debug i�in t�m sahne skorlar�n� yazd�r
    public void DebugAllScores()
    {
        foreach (var kvp in scenesScoreData)
        {
            Debug.Log("Sahne " + kvp.Key +
                      " - Base Score: " + kvp.Value.baseScore +
                      " - Current Score: " + kvp.Value.currentScore +
                      " - High Score: " + kvp.Value.sceneHighScore);
        }
    }
}