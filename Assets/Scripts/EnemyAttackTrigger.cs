using UnityEngine;

public class EnemyAttackTrigger : MonoBehaviour
{
    [Header("Attack Settings")]
    public float attackRange = 3f;  // Saldýrý mesafesi
    public Transform player;        // Ana karakter referansý

    private Animator animator;
    private bool playerInRange = false;

    void Start()
    {
        // Animator bileþenini al
        animator = GetComponent<Animator>();

        // Eðer player referansý atanmamýþsa, "Player" tag'i ile bul
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
        }
    }

    void Update()
    {
        // Player referansý varsa mesafeyi kontrol et
        if (player != null)
        {
            float distance = Vector3.Distance(transform.position, player.position);

            // Mesafe kontrolü ve animator parametresi güncelleme
            if (distance <= attackRange)
            {
                if (!playerInRange)
                {
                    playerInRange = true;
                    SetAttackState(true);
                }
            }
            else
            {
                if (playerInRange)
                {
                    playerInRange = false;
                    SetAttackState(false);
                }
            }
        }
    }

    void SetAttackState(bool isAttacking)
    {
        if (animator != null)
        {
            // Bool parametresi kullan - sürekli animasyon için
            animator.SetBool("attackCondition", isAttacking);

            // Eðer trigger kullanmak istersen (her defasýnda tetikler)
            // if (isAttacking)
            // {
            //     animator.SetTrigger("attackCondition");
            // }
        }
    }

    // Saldýrý menzilini görselleþtirmek için (Scene view'da görünür)
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}