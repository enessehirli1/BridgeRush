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

    // Player'ın orijinal rigidbody ayarlarını saklamak için
    private bool originalIsKinematic;
    private RigidbodyInterpolation originalInterpolation;
    private CollisionDetectionMode originalCollisionDetection;

    void Awake()
    {
        // Singleton deseni
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

    void Start()
    {
        mAnimator = GetComponent<Animator>();
        FindPlayerReference();
        FindAndSetupTextMesh();
        UpdateLivesText();

        // Player'ın orijinal rigidbody ayarlarını kaydet
        SaveOriginalPlayerSettings();
    }

    // Player'ın orijinal ayarlarını kaydetme
    void SaveOriginalPlayerSettings()
    {
        if (playerRigidbody != null)
        {
            originalIsKinematic = playerRigidbody.isKinematic;
            originalInterpolation = playerRigidbody.interpolation;
            originalCollisionDetection = playerRigidbody.collisionDetectionMode;
        }
    }

    // Player referansını bulan metod
    void FindPlayerReference()
    {
        if (playerTransform == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                playerTransform = playerObj.transform;
                playerAnimator = playerObj.GetComponent<Animator>();
                playerRigidbody = playerObj.GetComponent<Rigidbody>();

                playerMovementScript = playerObj.GetComponent("PlayerController") as MonoBehaviour;
                if (playerMovementScript == null)
                    playerMovementScript = playerObj.GetComponent("PlayerMovement") as MonoBehaviour;
                if (playerMovementScript == null)
                    playerMovementScript = playerObj.GetComponent("Movement") as MonoBehaviour;

                // Ground check setup
                SetupGroundCheck(playerObj);
            }
            else
            {
                playerObj = GameObject.Find("Player");
                if (playerObj != null)
                {
                    playerTransform = playerObj.transform;
                    playerAnimator = playerObj.GetComponent<Animator>();
                    playerRigidbody = playerObj.GetComponent<Rigidbody>();

                    playerMovementScript = playerObj.GetComponent("PlayerController") as MonoBehaviour;
                    if (playerMovementScript == null)
                        playerMovementScript = playerObj.GetComponent("PlayerMovement") as MonoBehaviour;
                    if (playerMovementScript == null)
                        playerMovementScript = playerObj.GetComponent("Movement") as MonoBehaviour;

                    SetupGroundCheck(playerObj);
                }
            }
        }
        else
        {
            playerAnimator = playerTransform.GetComponent<Animator>();
            playerRigidbody = playerTransform.GetComponent<Rigidbody>();

            playerMovementScript = playerTransform.GetComponent("PlayerController") as MonoBehaviour;
            if (playerMovementScript == null)
                playerMovementScript = playerTransform.GetComponent("PlayerMovement") as MonoBehaviour;
            if (playerMovementScript == null)
                playerMovementScript = playerTransform.GetComponent("Movement") as MonoBehaviour;

            SetupGroundCheck(playerTransform.gameObject);
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
        // Sahne yüklendiğinde gameHasEnded'i sıfırla
        gameHasEnded = false;

        // Player referansını tekrar bul
        FindPlayerReference();
        FindAndSetupTextMesh();
        UpdateLivesText();

        // Player ayarlarını orijinal haline getir
        RestoreOriginalPlayerSettings();

        // Player'ın orijinal ayarlarını yeniden kaydet
        SaveOriginalPlayerSettings();

        Debug.Log("Scene loaded, player settings restored");
    }

    // Player ayarlarını orijinal haline getiren metod
    void RestoreOriginalPlayerSettings()
    {
        if (playerRigidbody != null)
        {
            playerRigidbody.isKinematic = originalIsKinematic;
            playerRigidbody.interpolation = originalInterpolation;
            playerRigidbody.collisionDetectionMode = originalCollisionDetection;
            playerRigidbody.linearVelocity = Vector3.zero;
            playerRigidbody.angularVelocity = Vector3.zero;
        }

        if (playerMovementScript != null)
        {
            playerMovementScript.enabled = true;
        }

        CharacterController characterController = playerTransform?.GetComponent<CharacterController>();
        if (characterController != null)
        {
            characterController.enabled = true;
        }
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

    void Update()
    {
        if (textRemainedLives != null && textRemainedLives.text != remainingLives.ToString())
        {
            UpdateLivesText();
        }
    }

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
            Transform rightToeBase = FindDeepChild(playerObj.transform, "mixamorig:RightToeBase");
            if (rightToeBase != null)
            {
                groundCheckTransform = rightToeBase;
                Debug.Log("GroundCheckTransform found at mixamorig:RightToeBase");
            }
        }
    }

    Transform FindDeepChild(Transform parent, string childName)
    {
        foreach (Transform child in parent)
        {
            if (child.name == childName)
                return child;

            Transform result = FindDeepChild(child, childName);
            if (result != null)
                return result;
        }
        return null;
    }

    bool IsPlayerGrounded()
    {
        if (groundCheckTransform == null || playerTransform == null)
        {
            Debug.LogWarning("GroundCheck or PlayerTransform is null!");
            return false;
        }

        // Daha güvenilir ground check için birden fazla raycast
        Vector3 raycastOrigin = groundCheckTransform.position;
        bool isGrounded = false;

        // Merkez raycast
        isGrounded = Physics.Raycast(raycastOrigin, Vector3.down, groundCheckDistance, groundLayerMask);

        // Eğer merkez raycast başarısız olursa, çevresini de kontrol et
        if (!isGrounded)
        {
            Vector3[] offsets = {
                new Vector3(0.1f, 0, 0),
                new Vector3(-0.1f, 0, 0),
                new Vector3(0, 0, 0.1f),
                new Vector3(0, 0, -0.1f)
            };

            foreach (Vector3 offset in offsets)
            {
                if (Physics.Raycast(raycastOrigin + offset, Vector3.down, groundCheckDistance, groundLayerMask))
                {
                    isGrounded = true;
                    break;
                }
            }
        }

        // Debug için ray çiz
        Debug.DrawRay(raycastOrigin, Vector3.down * groundCheckDistance, isGrounded ? Color.green : Color.red);

        return isGrounded;
    }

    public void endGame()
    {
        if (gameHasEnded == false)
        {
            remainingLives--;
            Debug.Log("Life Lost - Remaining Lives: " + remainingLives);

            gameHasEnded = true; // Önce bunu set et ki tekrar çağırılmasın

            bool isGrounded = IsPlayerGrounded();
            Debug.Log("Player is grounded: " + isGrounded);

            // Ölüm animasyonunu oynat
            PlayDeathAnimation(isGrounded);

            if (remainingLives <= 0)
            {
                remainingLives = initialLives;

                if (ScoreManager.instance != null)
                {
                    ScoreManager.instance.GameOver();
                }

                Debug.Log("Game Over - No Lives Left. Restarting from first level with " + remainingLives + " lives");

                float delay = isGrounded ? restartDelay : 1f;
                Invoke("RestartFromFirstLevel", delay);
            }
            else
            {
                if (ScoreManager.instance != null)
                {
                    ScoreManager.instance.PlayerDied();
                }

                float delay = isGrounded ? restartDelay : 1f;
                Invoke("Restart", delay);
            }
        }
    }

    void PlayDeathAnimation(bool isGrounded)
    {
        if (playerAnimator != null)
        {
            if (isGrounded)
            {
                playerAnimator.SetTrigger("Death"); // Yerde ölme animasyonu
                StopPlayerMovement();
            }
            else
            {
                playerAnimator.SetTrigger("FallDeath"); // Düşme ölme animasyonu (kendi trigger isminizi kullanın)
            }
            Debug.Log("Death animation triggered! Grounded: " + isGrounded);
        }
        else
        {
            Debug.LogWarning("Player Animator not found! Trying to find player again...");
            FindPlayerReference();

            if (playerAnimator != null)
            {
                if (isGrounded)
                {
                    playerAnimator.SetTrigger("Death");
                }
                else
                {
                    playerAnimator.SetTrigger("FallDeath");
                }
                Debug.Log("Death animation triggered after refinding player! Grounded: " + isGrounded);
            }

            StopPlayerMovement();
        }

        
    }

    void StopPlayerMovement()
    {
        if (playerTransform == null) return;

        if (playerRigidbody != null)
        {
            playerRigidbody.linearVelocity = Vector3.zero;
            playerRigidbody.angularVelocity = Vector3.zero;
            playerRigidbody.isKinematic = true;
            Debug.Log("Player rigidbody movement stopped!");
        }

        if (playerMovementScript != null)
        {
            playerMovementScript.enabled = false;
            Debug.Log("Player movement script disabled!");
        }

        CharacterController characterController = playerTransform.GetComponent<CharacterController>();
        if (characterController != null)
        {
            characterController.enabled = false;
            Debug.Log("CharacterController disabled!");
        }

        if (playerAnimator != null)
        {
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

    bool HasParameter(Animator animator, string paramName)
    {
        foreach (AnimatorControllerParameter param in animator.parameters)
        {
            if (param.name == paramName)
                return true;
        }
        return false;
    }

    void EnablePlayerMovement()
    {
        if (playerTransform == null) return;

        if (playerRigidbody != null)
        {
            playerRigidbody.isKinematic = originalIsKinematic;
        }

        if (playerMovementScript != null)
        {
            playerMovementScript.enabled = true;
        }

        CharacterController characterController = playerTransform.GetComponent<CharacterController>();
        if (characterController != null)
        {
            characterController.enabled = true;
        }
    }

    void Restart()
    {
        Debug.Log(SceneManager.GetActiveScene().name);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void RestartFromFirstLevel()
    {
        SceneManager.LoadScene(firstLevelIndex);
    }

    public void GoToNextLevel()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        int nextSceneIndex = currentSceneIndex + 1;

        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextSceneIndex);
        }
        else
        {
            Debug.Log("No more levels! Going back to first level.");
            RestartFromFirstLevel();
        }
    }
}