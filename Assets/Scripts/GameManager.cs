using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // Singleton deseni uygulayalım ki her sahnede tek bir GameManager olsun
    public static GameManager instance;
    private static int remainingLives = 3; // Statik değişken olarak kalan canlar

    bool gameHasEnded = false;
    public float restartDelay = 2f;
    public int initialLives = 3; // Başlangıç can sayısı
    public TextMeshPro textRemainedLives; // TextMesh Pro referansı

    // İlk level'ın sahne indeksi (genelde 0 veya 1 olur)
    public int firstLevelIndex = 0;

    // Animator referansı
    private Animator mAnimator;

    // Player referansı (ana karakter)
    public Transform playerTransform; // Inspector'dan atayın veya otomatik bulacak
    private Animator playerAnimator;
    private Rigidbody playerRigidbody;
    private MonoBehaviour playerMovementScript; // Player hareket scripti (varsa)

    [Header("Ground Check Settings")]
    public LayerMask groundLayerMask = 1; // Ground layer mask
    public float groundCheckDistance = 0.1f; // Yere olan mesafe kontrolü
    public Transform groundCheckTransform; // Ground check pozisyonu (ayaklar)

    void Awake()
    {
        // Singleton deseni
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // Sahneler arası geçişte nesneyi koru
        }
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        mAnimator = GetComponent<Animator>();

        // Player referansını bul
        FindPlayerReference();

        // TextMeshPro referanslarını bul (eğer Inspector'da atanmamışsa)
        FindAndSetupTextMesh();

        // Oyun başladığında can sayısını TextMesh Pro'ya yaz
        UpdateLivesText();
    }

    // Player referansını bulan metod
    void FindPlayerReference()
    {
        if (playerTransform == null)
        {
            // "Player" tag'ine sahip objeyi bul
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                playerTransform = playerObj.transform;
                playerAnimator = playerObj.GetComponent<Animator>();
                playerRigidbody = playerObj.GetComponent<Rigidbody>();

                // Yaygın hareket script isimlerini dene
                playerMovementScript = playerObj.GetComponent("PlayerController") as MonoBehaviour;
                if (playerMovementScript == null)
                    playerMovementScript = playerObj.GetComponent("PlayerMovement") as MonoBehaviour;
                if (playerMovementScript == null)
                    playerMovementScript = playerObj.GetComponent("Movement") as MonoBehaviour;
            }
            else
            {
                // Tag yoksa isimle ara
                playerObj = GameObject.Find("Player");
                if (playerObj != null)
                {
                    playerTransform = playerObj.transform;
                    playerAnimator = playerObj.GetComponent<Animator>();
                    playerRigidbody = playerObj.GetComponent<Rigidbody>();

                    // Yaygın hareket script isimlerini dene
                    playerMovementScript = playerObj.GetComponent("PlayerController") as MonoBehaviour;
                    if (playerMovementScript == null)
                        playerMovementScript = playerObj.GetComponent("PlayerMovement") as MonoBehaviour;
                    if (playerMovementScript == null)
                        playerMovementScript = playerObj.GetComponent("Movement") as MonoBehaviour;
                }
            }
        }
        else
        {
            // Transform referansı varsa diğer componentleri al
            playerAnimator = playerTransform.GetComponent<Animator>();
            playerRigidbody = playerTransform.GetComponent<Rigidbody>();

            // Yaygın hareket script isimlerini dene
            playerMovementScript = playerTransform.GetComponent("PlayerController") as MonoBehaviour;
            if (playerMovementScript == null)
                playerMovementScript = playerTransform.GetComponent("PlayerMovement") as MonoBehaviour;
            if (playerMovementScript == null)
                playerMovementScript = playerTransform.GetComponent("Movement") as MonoBehaviour;
        }

        if (playerAnimator == null)
        {
            Debug.LogWarning("Player Animator not found! Death animation won't play.");
        }

        if (playerRigidbody == null)
        {
            Debug.LogWarning("Player Rigidbody not found! Physics movement won't be stopped.");
        }
    }

    // Her sahne yüklendiğinde çalışacak metod
    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Yeni sahnede player referansını tekrar bul
        FindPlayerReference();

        // Yeni sahnede TextMeshPro referansını tekrar bul
        FindAndSetupTextMesh();

        // Can sayısını güncelle
        UpdateLivesText();
    }

    void FindAndSetupTextMesh()
    {
        if (textRemainedLives == null)
        {
            GameObject textObj = GameObject.Find("textRemainedLives");
            if (textObj != null)
            {
                textRemainedLives = textObj.GetComponent<TextMeshPro>();
            }
        }
    }

    // Her karede kontrol et (başlangıçta güncellemeyi garantilemek için)
    void Update()
    {
        // Eğer textRemainedLives içindeki değer remainingLives ile eşleşmiyorsa güncelle
        if (textRemainedLives != null && textRemainedLives.text != remainingLives.ToString())
        {
            UpdateLivesText();
        }
    }

    // Can sayısını güncelleyen metod
    void UpdateLivesText()
    {
        if (textRemainedLives != null)
        {
            textRemainedLives.text = remainingLives.ToString();
            Debug.Log("Lives text updated to: " + remainingLives);
        }
        else
        {
            Debug.LogError("textRemainedLives reference lost! Trying to find it again.");
            FindAndSetupTextMesh();

            if (textRemainedLives != null)
            {
                textRemainedLives.text = remainingLives.ToString();
            }
        }
    }

    void SetupGroundCheck(GameObject playerObj)
    {
        if (groundCheckTransform == null)
        {
            // "GroundCheck" isimli child obje var mı kontrol et
            Transform existingGroundCheck = playerObj.transform.Find("GroundCheck");
            if (existingGroundCheck != null)
            {
                groundCheckTransform = existingGroundCheck;
            }
            else
            {
                // Yoksa player'ın kendisini kullan
                groundCheckTransform = playerObj.transform;
                Debug.Log("GroundCheck child object not found, using player transform for ground checking.");
            }
        }
    }

    bool IsPlayerGrounded()
    {
        if (groundCheckTransform == null || playerTransform == null)
            return false;

        // Raycast ile yere olan mesafeyi kontrol et
        Vector3 raycastOrigin = groundCheckTransform.position;
        bool isGrounded = Physics.Raycast(raycastOrigin, Vector3.down, groundCheckDistance, groundLayerMask);

        // Debug için ray çiz (Scene view'da görünür)
        Debug.DrawRay(raycastOrigin, Vector3.down * groundCheckDistance, isGrounded ? Color.green : Color.red);

        return isGrounded;
    }
    public void endGame()
    {
        if (gameHasEnded == false)
        {
            // Can sayısını azalt
            remainingLives--;
            Debug.Log("Life Lost - Remaining Lives: " + remainingLives);

            // Ölüm animasyonunu oynat
            
            if (IsPlayerGrounded())
            {
                PlayDeathAnimation();
            }
            
            
            

            if (remainingLives <= 0)
            {
                // Canlar bittiyse:
                // 1. Ölme animasyonu oynat (yukarıda oynatıldı)
                // 2. Canları sıfırla (3'e geri getir)
                // 3. Skoru da sıfırla
                // 4. İlk level'a geri dön
                remainingLives = initialLives;

                // ScoreManager'a GameOver bilgisi gönder
                if (ScoreManager.instance != null)
                {
                    ScoreManager.instance.GameOver();
                }

                Debug.Log("Game Over - No Lives Left. Restarting from first level with " + remainingLives + " lives");
                gameHasEnded = true;
                if (IsPlayerGrounded())
                {
                    Invoke("RestartFromFirstLevel", restartDelay);
                }

                else if (!IsPlayerGrounded())
                {
                    Invoke("RestartFromFirstLevel", 1f);
                }

            }
            else
            {
                // Hala can varsa mevcut sahneyi yeniden başlat
                // ScoreManager'a ölüm bilgisi gönder
                if (ScoreManager.instance != null)
                {
                    ScoreManager.instance.PlayerDied();
                }

                gameHasEnded = true;
                if (IsPlayerGrounded())
                {
                    Invoke("Restart", restartDelay);
                }
                else if (!IsPlayerGrounded())
                {
                    Invoke("Restart", 1f);
                }

            }
        }
    }

    // Ölüm animasyonunu oynatma ve hareketi durdurma metodu
    void PlayDeathAnimation()
    {
        if (playerAnimator != null)
        {
            playerAnimator.SetTrigger("Death");
            Debug.Log("Death animation triggered!");
        }
        else
        {
            Debug.LogWarning("Player Animator not found! Trying to find player again...");
            FindPlayerReference();

            // Tekrar dene
            if (playerAnimator != null)
            {
                playerAnimator.SetTrigger("Death");
                Debug.Log("Death animation triggered after refinding player!");
            }
        }

        // Player hareketini durdur
        StopPlayerMovement();
    }

    // Player hareketini durduran metod
    void StopPlayerMovement()
    {
        if (playerTransform == null) return;

        // 1. Rigidbody varsa fizik hareketini durdur
        if (playerRigidbody != null)
        {
            playerRigidbody.linearVelocity = Vector3.zero;
            playerRigidbody.angularVelocity = Vector3.zero;
            // Rigidbody'yi kinematic yap (fizik etkilerini durdur)
            playerRigidbody.isKinematic = true;
            Debug.Log("Player rigidbody movement stopped!");
        }

        // 2. Hareket scriptini devre dışı bırak (varsa)
        if (playerMovementScript != null)
        {
            playerMovementScript.enabled = false;
            Debug.Log("Player movement script disabled!");
        }

        // 3. CharacterController varsa devre dışı bırak
        CharacterController characterController = playerTransform.GetComponent<CharacterController>();
        if (characterController != null)
        {
            characterController.enabled = false;
            Debug.Log("CharacterController disabled!");
        }

        // 4. Animator'daki hareket parametrelerini sıfırla (eğer hareket animasyonları varsa)
        if (playerAnimator != null)
        {
            // Yaygın hareket parametreleri (kendi animator parametrelerinize göre ayarlayın)
            if (HasParameter(playerAnimator, "Speed"))
                playerAnimator.SetFloat("Speed", 0f);
            if (HasParameter(playerAnimator, "Horizontal"))
                playerAnimator.SetFloat("Horizontal", 0f);
            if (HasParameter(playerAnimator, "Vertical"))
                playerAnimator.SetFloat("Vertical", 0f);
            if (HasParameter(playerAnimator, "IsMoving"))
                playerAnimator.SetBool("IsMoving", false);
            if (HasParameter(playerAnimator, "IsGrounded"))
                playerAnimator.SetBool("IsGrounded", true);
        }
    }

    // Animator'da parametre var mı kontrol eden yardımcı metod
    bool HasParameter(Animator animator, string paramName)
    {
        foreach (AnimatorControllerParameter param in animator.parameters)
        {
            if (param.name == paramName)
                return true;
        }
        return false;
    }

    // Player hareketini yeniden aktifleştiren metod (sahne yeniden yüklendiğinde otomatik olur ama elle de çağırabilirsiniz)
    void EnablePlayerMovement()
    {
        if (playerTransform == null) return;

        // Rigidbody'yi tekrar kinematic olmaktan çıkar
        if (playerRigidbody != null)
        {
            playerRigidbody.isKinematic = false;
        }

        // Hareket scriptini aktifleştir
        if (playerMovementScript != null)
        {
            playerMovementScript.enabled = true;
        }

        // CharacterController'ı aktifleştir
        CharacterController characterController = playerTransform.GetComponent<CharacterController>();
        if (characterController != null)
        {
            characterController.enabled = true;
        }
    }

    // Mevcut sahneyi yeniden yükler
    void Restart()
    {
        gameHasEnded = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // İlk level'a geri döner ve canları sıfırlar
    void RestartFromFirstLevel()
    {
        gameHasEnded = false;
        SceneManager.LoadScene(firstLevelIndex);
    }

    // Sonraki seviyeye geçiş metodu
    public void GoToNextLevel()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        int nextSceneIndex = currentSceneIndex + 1;

        // Eğer bir sonraki sahne varsa yükle
        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            gameHasEnded = false;
            SceneManager.LoadScene(nextSceneIndex);
        }
        else
        {
            Debug.Log("No more levels! Going back to first level.");
            RestartFromFirstLevel();
        }
    }
}