using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Jump : MonoBehaviour
{
    private Animator mAnimator;
    public Transform player;
    private Rigidbody rb;
    private CapsuleCollider characterCollider;
    private bool isJumping = false;

    // Zıplama kuvveti
    public float jumpForce = 5.0f;
    // Yer çekimi kuvveti (düşerken daha hızlı düşmesi için)
    public float gravityMultiplier = 2.5f;

    // Default layer'ı kullanacağız
    private int groundLayerMask;
    private bool isGrounded = true;
    private float groundCheckDistance = 0.2f;

    void Start()
    {
        mAnimator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        characterCollider = GetComponent<CapsuleCollider>();

        // Default layer'ı kullan (0. layer)
        groundLayerMask = 1 << 0;

        // Rigidbody ayarlarını yapalım
        if (rb != null)
        {
            rb.constraints = RigidbodyConstraints.FreezeRotation; // Dönmeyi engelle
        }
        else
        {
            Debug.LogError("Rigidbody bulunamadı!");
            // Eğer rigidbody yoksa ekleyelim
            rb = gameObject.AddComponent<Rigidbody>();
            rb.constraints = RigidbodyConstraints.FreezeRotation;
        }
    }

    void Update()
    {
        // Yerde olup olmadığımızı kontrol et
        CheckGrounded();

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded && !isJumping)
        {
            mAnimator.SetTrigger("Jump");
            JumpCharacter();
        }



        // Havadayken daha hızlı düşmesini sağla
        if (!isGrounded && rb.linearVelocity.y < 0)
        {
            rb.AddForce(Vector3.down * gravityMultiplier, ForceMode.Acceleration);
        }
    }

    // Karakterin yerde olup olmadığını kontrol et
    void CheckGrounded()
    {
        if (characterCollider != null)
        {
            // Karakterin altından kısa bir mesafeye kadar raycast gönder
            Vector3 rayStart = transform.position + characterCollider.center;
            rayStart.y -= (characterCollider.height / 2) - characterCollider.radius + 0.05f;

            // Debug için görsel çizgi
            // Debug.DrawRay(rayStart, Vector3.down * groundCheckDistance, Color.red);

            // Küre şeklinde raycast gönder
            isGrounded = Physics.SphereCast(
                rayStart,
                characterCollider.radius * 0.9f,
                Vector3.down,
                out RaycastHit hit,
                groundCheckDistance,
                groundLayerMask
            );

            // Alternatif yöntem - basit raycast
            if (!isGrounded)
            {
                isGrounded = Physics.Raycast(
                    rayStart,
                    Vector3.down,
                    groundCheckDistance + 0.1f,
                    groundLayerMask
                );
            }
        }
    }

    // Zıplama fonksiyonu
    void JumpCharacter()
    {
        if (rb != null)
        {
            isJumping = true;

            // Yukarı doğru kuvvet uygula
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z); // Y hızını sıfırla
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);

            // Zıplama animasyonu süresince bekle
            StartCoroutine(ResetJumpState());
        }
    }

    // Zıplama durumunu sıfırla
    IEnumerator ResetJumpState()
    {
        // Zıplama animasyonunun tamamlanmasını bekle
        yield return new WaitForSeconds(0.5f);

        // Yere düşmeyi bekle
        while (!isGrounded)
        {
            yield return null;
        }

        // Zıplama durumunu sıfırla
        isJumping = false;
    }

    // Visual debugging için
    void OnDrawGizmos()
    {
        if (characterCollider != null)
        {
            // Yerle temas noktasını görselleştir
            Gizmos.color = Color.green;
            Vector3 rayStart = transform.position + characterCollider.center;
            rayStart.y -= (characterCollider.height / 2) - characterCollider.radius + 0.05f;
            Gizmos.DrawWireSphere(rayStart, characterCollider.radius * 0.9f);
            Gizmos.DrawLine(rayStart, rayStart + Vector3.down * groundCheckDistance);
        }
    }
}