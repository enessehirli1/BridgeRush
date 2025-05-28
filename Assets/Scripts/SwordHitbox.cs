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

    [Header("Mutant Özel Efektleri")]
    [SerializeField] private GameObject mutantBloodEffect; // Mutant kan efekti prefab'ý
    [SerializeField] private int mutantFragmentCount = 15; // Mutant için parça sayýsý (normal objeler için 8)
    [SerializeField] private float mutantExplosionForce = 8f; // Mutant patlama kuvveti (normal için 5f)
    [SerializeField] private Color mutantBloodColor = new Color(0.8f, 0f, 0f, 1f); // Kan rengi
    [SerializeField] private float mutantFragmentLifetime = 4f; // Mutant parçalarýnýn yaþam süresi

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
                // Mutant kontrolü ekle
                else if (hitCollider.CompareTag("Mutant"))
                {
                    Debug.Log("Mutant " + hitCollider.name + " öldürüldü!");

                    // Mutant özel efekti
                    StartCoroutine(KillMutant(hitCollider.gameObject));
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
            CreateCubeFragments(objPosition, objColor, objectToBreak.transform.localScale, false);
        }

        // Kýsa bir gecikme
        yield return new WaitForSeconds(destroyDelay);

        // Objeyi yok et
        Destroy(objectToBreak);
    }

    // Mutant öldürme fonksiyonu
    private IEnumerator KillMutant(GameObject mutant)
    {
        Vector3 mutantPosition = mutant.transform.position;
        Vector3 mutantScale = mutant.transform.localScale;

        // Kan efekti oluþtur
        if (mutantBloodEffect != null)
        {
            GameObject bloodEffect = Instantiate(mutantBloodEffect, mutantPosition, Quaternion.identity);

            // Kan parçacýk sistemini ayarla
            ParticleSystem bloodPS = bloodEffect.GetComponent<ParticleSystem>();
            if (bloodPS != null)
            {
                var main = bloodPS.main;
                main.startColor = mutantBloodColor;
                main.maxParticles = 50; // Daha fazla kan parçacýðý
            }

            Destroy(bloodEffect, 3f);
        }

        // Mutant parçacýklarý oluþtur (daha fazla ve daha dramatik)
        CreateCubeFragments(mutantPosition, mutantBloodColor, mutantScale, true);

        // Ekstra kan damlalarý efekti
        CreateBloodDroplets(mutantPosition);

        // Kýsa gecikme
        yield return new WaitForSeconds(destroyDelay);

        // Mutant'ý oyundan kaldýr
        Destroy(mutant);
    }

    // Geliþtirilmiþ parça oluþturma fonksiyonu
    private void CreateCubeFragments(Vector3 position, Color color, Vector3 originalScale, bool isMutant)
    {
        int fragmentCount = isMutant ? mutantFragmentCount : 8;
        float fragmentSize = originalScale.x * (isMutant ? 0.2f : 0.3f); // Mutant parçalarý daha küçük
        float explosionForce = isMutant ? mutantExplosionForce : 5f;
        float lifetime = isMutant ? mutantFragmentLifetime : 2f;

        for (int i = 0; i < fragmentCount; i++)
        {
            // Küçük bir küp oluþtur
            GameObject fragment = GameObject.CreatePrimitive(PrimitiveType.Cube);
            fragment.transform.localScale = Vector3.one * fragmentSize;

            // Rastgele yakýn bir pozisyon (mutantlar için daha geniþ yayýlým)
            float spread = isMutant ? 0.6f : 0.3f;
            Vector3 randomOffset = new Vector3(
                Random.Range(-spread, spread),
                Random.Range(-spread, spread),
                Random.Range(-spread, spread)
            );
            fragment.transform.position = position + randomOffset;

            // Renk ayarla
            Renderer fragRenderer = fragment.GetComponent<Renderer>();
            if (fragRenderer != null)
            {
                fragRenderer.material = new Material(Shader.Find("Standard"));
                fragRenderer.material.color = color;

                // Mutant parçalarý için metalik görünüm
                if (isMutant)
                {
                    fragRenderer.material.SetFloat("_Metallic", 0.3f);
                    fragRenderer.material.SetFloat("_Glossiness", 0.7f);
                }
            }

            // Fizik ekle
            Rigidbody rb = fragment.AddComponent<Rigidbody>();
            rb.mass = 0.1f; // Daha hafif parçacýklar

            // Patlama kuvveti uygula
            rb.AddExplosionForce(explosionForce, position, 2f);

            // Rastgele dönüþ ekle
            rb.AddTorque(Random.insideUnitSphere * 10f);

            // Parçayý yok et
            Destroy(fragment, lifetime);
        }
    }

    // Kan damlalarý efekti
    private void CreateBloodDroplets(Vector3 position)
    {
        int dropletCount = 12; // Kan damla sayýsý

        for (int i = 0; i < dropletCount; i++)
        {
            // Küçük küreler oluþtur (kan damlalarý)
            GameObject droplet = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            droplet.transform.localScale = Vector3.one * Random.Range(0.05f, 0.15f);

            // Rastgele pozisyon
            Vector3 randomOffset = new Vector3(
                Random.Range(-0.5f, 0.5f),
                Random.Range(0f, 0.8f), // Yukarý doðru daha fazla
                Random.Range(-0.5f, 0.5f)
            );
            droplet.transform.position = position + randomOffset;

            // Kan rengi
            Renderer dropletRenderer = droplet.GetComponent<Renderer>();
            if (dropletRenderer != null)
            {
                dropletRenderer.material = new Material(Shader.Find("Standard"));
                dropletRenderer.material.color = mutantBloodColor;
                dropletRenderer.material.SetFloat("_Glossiness", 0.8f); // Parlak kan
            }

            // Fizik
            Rigidbody rb = droplet.AddComponent<Rigidbody>();
            rb.mass = 0.05f;

            // Yukarý doðru fýrlatma kuvveti
            Vector3 force = new Vector3(
                Random.Range(-3f, 3f),
                Random.Range(3f, 6f),
                Random.Range(-3f, 3f)
            );
            rb.AddForce(force, ForceMode.Impulse);

            // Kan damlasýný yok et
            Destroy(droplet, 3f);
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