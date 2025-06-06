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
    private float savedHighScore = 0f;

    [Header("Enemy Killing Points")]
    public float breakableScore = 250f;
    public float mutantScore = 750f;

    [Header("Menu Settings")]
    [SerializeField] private List<string> menuSceneNames = new List<string>() { "Menu", "MainMenu", "StartMenu" }; // Menu sahne isimleri
    private bool hasResetForMenu = false; // Menu'ya girildi�inde bir kez reset yapmas� i�in

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
        Debug.Log("Sahne y�klendi: " + scene.name + " (index: " + scene.buildIndex + ")");

        // Menu sahnesine girildi�ini kontrol et
        if (IsMenuScene(scene.name))
        {
            HandleMenuSceneEntered();
            return; // Menu sahnesi ise di�er i�lemleri yapma
        }

        // Menu'dan ��kt���m�zda reset flag'ini s�f�rla
        hasResetForMenu = false;

        InitializeCurrentScene();

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

    // Menu sahnesine girildi�inde �a�r�lacak fonksiyon
    private void HandleMenuSceneEntered()
    {
        if (!hasResetForMenu)
        {
            Debug.Log("Menu sahnesine girildi. High score s�f�rlan�yor...");

            // T�m skor verilerini temizle
            scenesScoreData.Clear();

            // Saved high score'u s�f�rla
            savedHighScore = 0f;

            // Reset flag'ini set et
            hasResetForMenu = true;

            Debug.Log("T�m skorlar s�f�rland�.");
        }
    }

    // Sahnenin menu sahne olup olmad���n� kontrol et
    private bool IsMenuScene(string sceneName)
    {
        foreach (string menuName in menuSceneNames)
        {
            if (sceneName.Equals(menuName, System.StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }
        return false;
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
        // Menu sahnesindeyse Update'i �al��t�rma
        if (IsMenuScene(SceneManager.GetActiveScene().name))
        {
            return;
        }

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
        // �nce en y�ksek skoru kaydet
        savedHighScore = GetOverallHighScore();

        // Sonra t�m skor verilerini temizle
        scenesScoreData.Clear();
        Debug.Log("Game Over: En y�ksek skor kaydedildi (" + savedHighScore + ") ve di�er veriler s�f�rland�.");
    }

    // Manuel olarak skorlar� s�f�rlama fonksiyonu (Menu butonlar� i�in kullan�labilir)
    public void ResetAllScores()
    {
        scenesScoreData.Clear();
        savedHighScore = 0f;
        hasResetForMenu = false;
        Debug.Log("T�m skorlar manuel olarak s�f�rland�.");
    }

    public float GetSavedHighScore()
    {
        return savedHighScore;
    }

    public void ResetSavedHighScore()
    {
        savedHighScore = 0f;
        Debug.Log("Saved high score reset to 0");
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

    // Son sahnenin (Level06) en y�ksek skorunu verir
    public float GetLastSceneHighScore()
    {
        int lastSceneIndex = 6; // Level06'n�n build index'i
        if (scenesScoreData.ContainsKey(lastSceneIndex))
        {
            return scenesScoreData[lastSceneIndex].sceneHighScore;
        }
        return 0;
    }

    // Genel en y�ksek skoru verir (t�m sahneler aras�ndan)
    public float GetOverallHighScore()
    {
        float highestScore = 0;
        foreach (var kvp in scenesScoreData)
        {
            if (kvp.Value.sceneHighScore > highestScore)
            {
                highestScore = kvp.Value.sceneHighScore;
            }
        }
        return highestScore;
    }

    // Menu sahne isimleri listesine yeni isim ekleme fonksiyonu
    public void AddMenuSceneName(string sceneName)
    {
        if (!menuSceneNames.Contains(sceneName))
        {
            menuSceneNames.Add(sceneName);
            Debug.Log("Menu sahne listesine eklendi: " + sceneName);
        }
    }
}