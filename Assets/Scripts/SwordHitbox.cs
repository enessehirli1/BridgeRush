using UnityEngine;
using System.Collections;

public class SwordHitbox : MonoBehaviour
{
    [Header("Hitbox Ayarlarý")]
    [SerializeField] private Vector3 hitboxSize = new Vector3(1f, 0.5f, 0.5f); // Hitbox boyutu
    [SerializeField] private Vector3 hitboxOffset = new Vector3(0f, 0f, 1f); // Karakterin önünde ne kadar mesafede oluþacak
    [SerializeField] private float hitboxDuration = 0.3f; // Hitbox'ýn aktif kalma süresi

    [Header("Görselleþtirme")]
    [SerializeField] private bool showDebugVisuals = true; // Geliþtirme sýrasýnda hitbox'ý göstermek için
    [SerializeField] private Color hitboxColor = new Color(1f, 0f, 0f, 0.5f); // Debug rengi

    [Header("Patlama Efekti")]
    [SerializeField] private GameObject breakEffect; // Ýsteðe baðlý: Parçacýk efekti prefab'ý
    [SerializeField] private float destroyDelay = 0.1f; // Efekt baþladýktan sonra yok olma gecikmesi
    [SerializeField] private bool useSimpleDestroy = false; // Basit destroy mu yoksa efekt mi kullanýlsýn

    private Animator animator; // Karakterin animatörü
    private bool hitboxActive = false;

    void Start()
    {
        animator = GetComponent<Animator>();
        // Animasyon event'i animasyon dosyasýna eklenmeli
        // Animasyon editöründe kýlýç animasyonuna "ActivateHitbox" adlý event eklenmeli
    }

    // Animasyon event'inden çaðrýlacak (Animator pencersinden ayarlanmalý)
    public void ActivateHitbox()
    {
        if (!hitboxActive)
        {
            StartCoroutine(CreateHitbox());
        }
    }

    private IEnumerator CreateHitbox()
    {
        hitboxActive = true;

        // Hitbox oluþtur ve kontrol et
        CheckHitbox();

        // Hitbox süresini bekle
        yield return new WaitForSeconds(hitboxDuration);

        hitboxActive = false;
    }

    private void CheckHitbox()
    {
        // Karakterin önünde bir hitbox oluþtur
        Vector3 hitboxCenter = transform.position + transform.TransformDirection(hitboxOffset);

        // Hitbox içindeki tüm Collider'larý al
        Collider[] hitColliders = Physics.OverlapBox(hitboxCenter, hitboxSize / 2, transform.rotation);

        // Her bir collider için iþlem yap
        foreach (var hitCollider in hitColliders)
        {
            // Eðer kendimiz deðilse
            if (hitCollider.transform != transform)
            {
                // Breakable objesi mi kontrol et
                if (hitCollider.CompareTag("Breakable"))
                {
                    Debug.Log(hitCollider.name + " objesine vuruldu!");

                    if (useSimpleDestroy)
                    {
                        // Basit destroy
                        Destroy(hitCollider.gameObject);
                    }
                    else
                    {
                        // Efektli destroy
                        StartCoroutine(BreakObject(hitCollider.gameObject));
                    }
                }
            }
        }
    }

    private IEnumerator BreakObject(GameObject objectToBreak)
    {
        // Objenin pozisyonunu ve rengini al
        Vector3 objPosition = objectToBreak.transform.position;
        Color objColor = Color.white; // Varsayýlan renk

        // Eðer objenin renderer'ý varsa rengini al
        Renderer renderer = objectToBreak.GetComponent<Renderer>();
        if (renderer != null && renderer.material != null)
        {
            objColor = renderer.material.color;
        }

        // Efekt oluþtur
        if (breakEffect != null)
        {
            // Hazýr efekt prefabýný kullan
            GameObject effect = Instantiate(breakEffect, objPosition, Quaternion.identity);

            // Parçacýk sistemini ayarla (eðer varsa)
            ParticleSystem ps = effect.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                var main = ps.main;
                main.startColor = objColor;
            }

            // Efektin kendi kendini yok etmesi için
            Destroy(effect, 2f);
        }
        else
        {
            // Basit küp parçacýklarý oluþtur (efekt prefabý yoksa)
            CreateCubeFragments(objPosition, objColor, objectToBreak.transform.localScale);
        }

        // Kýsa bir gecikme
        yield return new WaitForSeconds(destroyDelay);

        // Objeyi yok et
        Destroy(objectToBreak);
    }

    // Basit küp parçacýklarý oluþturan fonksiyon (eðer hazýr efekt yoksa)
    private void CreateCubeFragments(Vector3 position, Color color, Vector3 originalScale)
    {
        int fragmentCount = 8; // Kaç parça olacaðý
        float fragmentSize = originalScale.x * 0.3f; // Küçük küplerin boyutu, orijinal boyuttan oran olarak hesapla
        float explosionForce = 5f; // Ne kadar güçlü patlayacak

        for (int i = 0; i < fragmentCount; i++)
        {
            // Küçük bir küp oluþtur
            GameObject fragment = GameObject.CreatePrimitive(PrimitiveType.Cube);
            fragment.transform.localScale = Vector3.one * fragmentSize;

            // Rastgele yakýn bir pozisyon
            Vector3 randomOffset = new Vector3(
                Random.Range(-0.3f, 0.3f),
                Random.Range(-0.3f, 0.3f),
                Random.Range(-0.3f, 0.3f)
            );
            fragment.transform.position = position + randomOffset;

            // Renk ayarla
            Renderer fragRenderer = fragment.GetComponent<Renderer>();
            if (fragRenderer != null)
            {
                fragRenderer.material = new Material(Shader.Find("Standard"));
                fragRenderer.material.color = color;
            }

            // Fizik ekle
            Rigidbody rb = fragment.AddComponent<Rigidbody>();
            rb.AddExplosionForce(explosionForce, position, 1f);

            // Birkaç saniye sonra parçayý da yok et
            Destroy(fragment, 2f);
        }
    }

    // Hitbox'ý görselleþtirmek için (sadece editor'da)
    private void OnDrawGizmos()
    {
        if (showDebugVisuals && hitboxActive)
        {
            Gizmos.color = hitboxColor;
            Vector3 hitboxCenter = transform.position + transform.TransformDirection(hitboxOffset);
            Gizmos.matrix = Matrix4x4.TRS(hitboxCenter, transform.rotation, Vector3.one);
            Gizmos.DrawCube(Vector3.zero, hitboxSize);
        }
    }

    // Test için manual olarak kullanabileceðiniz bir fonksiyon
    public void ManuallyActivateHitbox()
    {
        ActivateHitbox();
    }
}