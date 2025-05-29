using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class ScoreManager : MonoBehaviour
{
    // Singleton
    public static ScoreManager instance;

    // Skor bilgilerini tutacak veri yapýsý
    [System.Serializable]
    public class SceneScoreData
    {
        public float baseScore; // Sahne baþlangýç skoru
        public float currentScore; // Sahnedeki mevcut skor
        public float sceneHighScore; // Sahnede eriþilen en yüksek skor
    }

    // Her sahne için bir SceneScoreData tutuyoruz
    private Dictionary<int, SceneScoreData> scenesScoreData = new Dictionary<int, SceneScoreData>();

    // UI Referanslarý
    public TextMeshPro scoreText;

    // Oyuncu Referansý
    private Transform playerTransform;

    // Öldükten sonra tekrar doðduðumuzda kullanýlacak flag
    private bool isRespawning = false;

    // Skor Hesaplama Ýçin Offset
    public float scoreOffset = 17f;

    [Header("Enemy Killing Points")]
    public float breakableScore = 250f;
    public float mutantScore = 750f;


    private void Awake()
    {
        // Singleton oluþturma
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
        // Sahne deðiþtiðinde çaðrýlacak fonksiyonu kaydedelim
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
        // Sahne yüklendiðinde çaðrýlýr
        InitializeCurrentScene();

        Debug.Log("Sahne yüklendi: " + scene.name + " (index: " + scene.buildIndex + ")");

        // Eðer bu bir respawn deðilse ve yeni bir sahneye geçiyorsak:
        if (!isRespawning)
        {
            int sceneIndex = scene.buildIndex;

            // Yeni bir sahneye geçildiðinde, önceki sahnenin highScore'unu bir sonraki sahnenin baseScore'u yap
            int previousSceneIndex = sceneIndex - 1;

            if (previousSceneIndex >= 0 && scenesScoreData.ContainsKey(previousSceneIndex))
            {
                float previousHighScore = scenesScoreData[previousSceneIndex].sceneHighScore;

                // Bu sahne için veri yoksa oluþtur
                if (!scenesScoreData.ContainsKey(sceneIndex))
                {
                    scenesScoreData[sceneIndex] = new SceneScoreData();
                }

                // Önceki sahnenin en yüksek skorunu bu sahnenin baþlangýç skoru yap
                scenesScoreData[sceneIndex].baseScore = previousHighScore;
                scenesScoreData[sceneIndex].currentScore = previousHighScore;

                Debug.Log("Yeni sahnenin baþlangýç skoru ayarlandý: " + previousHighScore);
            }
            // Ýlk sahne veya baþka bir durumda, base score'u 0 olarak ayarla (eðer yoksa)
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
            // Bu bir respawn ise, isRespawning bayraðýný sýfýrla
            isRespawning = false;
        }

        // UI ve Player referanslarýný bul
        FindReferences();

        // Skor göstergesini güncelle
        UpdateScoreDisplay();
    }

    private void InitializeCurrentScene()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;

        // Eðer bu sahne için veri yoksa oluþtur
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
        // Score Text referansýný bul
        if (scoreText == null)
        {
            GameObject scoreObj = GameObject.Find("score");
            if (scoreObj != null)
            {
                scoreText = scoreObj.GetComponent<TextMeshPro>();
            }
        }

        // Player referansýný bul
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
        }
    }

    private void Update()
    {
        // Player referansý yoksa bulmayý dene
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

        // Pozisyona göre skor hesaplama
        float calculatedScore = playerTransform.position.x - scoreOffset;
        float totalScore = sceneData.baseScore + calculatedScore;

        // Skoru güncelle
        sceneData.currentScore = Mathf.Max(sceneData.baseScore, totalScore);

        // HighScore'u güncelle
        sceneData.sceneHighScore = Mathf.Max(sceneData.sceneHighScore, sceneData.currentScore);

        // UI'yi her frame güncelle
        UpdateScoreDisplay();
    }

    private void UpdateScoreDisplay()
    {
        if (scoreText != null)
        {
            int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
            if (scenesScoreData.ContainsKey(currentSceneIndex))
            {
                // Tam sayý formatýnda göster
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
                Debug.Log("Mutant öldürüldü! +" + mutantScore + " puan");
                break;
            default:
                Debug.LogWarning("Bilinmeyen düþman tipi: " + enemyType);
                return;
        }

        // Puaný ekle
        sceneData.baseScore += scoreToAdd;
        sceneData.currentScore += scoreToAdd;
        sceneData.sceneHighScore = Mathf.Max(sceneData.sceneHighScore, sceneData.currentScore);

        // UI'yi güncelle
        UpdateScoreDisplay();

        Debug.Log("Toplam skor: " + sceneData.currentScore);
    }

    // Oyuncu öldüðünde çaðrýlacak metod
    public void PlayerDied()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;

        // Respawn olduðunu iþaretle
        isRespawning = true;

        // Mevcut skoru, baþlangýç skoruna geri döndür
        if (scenesScoreData.ContainsKey(currentSceneIndex))
        {
            scenesScoreData[currentSceneIndex].currentScore = scenesScoreData[currentSceneIndex].baseScore;
            Debug.Log("Oyuncu öldü. Skor, sahne baþlangýç deðerine döndürüldü: " + scenesScoreData[currentSceneIndex].baseScore);
        }
    }

    // Tüm canlar bittiðinde çaðrýlacak metod (tam Game Over)
    public void GameOver()
    {
        // Tüm skor verilerini temizle
        scenesScoreData.Clear();
        Debug.Log("Game Over: Tüm skor verileri sýfýrlandý.");
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

    // Debug için tüm sahne skorlarýný yazdýr
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