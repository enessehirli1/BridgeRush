using UnityEngine;

public class EnemyAttackTrigger : MonoBehaviour
{
    [Header("Attack Settings")]
    public float attackRange = 3f;  // Sald�r� mesafesi
    public Transform player;        // Ana karakter referans�

    private Animator animator;
    private bool playerInRange = false;

    void Start()
    {
        // Animator bile�enini al
        animator = GetComponent<Animator>();

        // E�er player referans� atanmam��sa, "Player" tag'i ile bul
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
        // Player referans� varsa mesafeyi kontrol et
        if (player != null)
        {
            float distance = Vector3.Distance(transform.position, player.position);

            // Mesafe kontrol� ve animator parametresi g�ncelleme
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
            // Bool parametresi kullan - s�rekli animasyon i�in
            animator.SetBool("attackCondition", isAttacking);

            // E�er trigger kullanmak istersen (her defas�nda tetikler)
            // if (isAttacking)
            // {
            //     animator.SetTrigger("attackCondition");
            // }
        }
    }

    // Sald�r� menzilini g�rselle�tirmek i�in (Scene view'da g�r�n�r)
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}