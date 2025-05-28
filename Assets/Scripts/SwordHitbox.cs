using UnityEngine;
using System.Collections;

public class SwordHitbox : MonoBehaviour
{
    [Header("Hitbox Ayarlar�")]
    [SerializeField] private Vector3 hitboxSize = new Vector3(1f, 0.5f, 0.5f); // Hitbox boyutu
    [SerializeField] private Vector3 hitboxOffset = new Vector3(0f, 0f, 1f); // Karakterin �n�nde ne kadar mesafede olu�acak
    [SerializeField] private float hitboxDuration = 0.3f; // Hitbox'�n aktif kalma s�resi

    [Header("G�rselle�tirme")]
    [SerializeField] private bool showDebugVisuals = true; // Geli�tirme s�ras�nda hitbox'� g�stermek i�in
    [SerializeField] private Color hitboxColor = new Color(1f, 0f, 0f, 0.5f); // Debug rengi

    [Header("Patlama Efekti")]
    [SerializeField] private GameObject breakEffect; // �ste�e ba�l�: Par�ac�k efekti prefab'�
    [SerializeField] private float destroyDelay = 0.1f; // Efekt ba�lad�ktan sonra yok olma gecikmesi
    [SerializeField] private bool useSimpleDestroy = false; // Basit destroy mu yoksa efekt mi kullan�ls�n

    [Header("Mutant �zel Efektleri")]
    [SerializeField] private GameObject mutantBloodEffect; // Mutant kan efekti prefab'�
    [SerializeField] private int mutantFragmentCount = 15; // Mutant i�in par�a say�s� (normal objeler i�in 8)
    [SerializeField] private float mutantExplosionForce = 8f; // Mutant patlama kuvveti (normal i�in 5f)
    [SerializeField] private Color mutantBloodColor = new Color(0.8f, 0f, 0f, 1f); // Kan rengi
    [SerializeField] private float mutantFragmentLifetime = 4f; // Mutant par�alar�n�n ya�am s�resi

    private Animator animator; // Karakterin animat�r�
    private bool hitboxActive = false;

    void Start()
    {
        animator = GetComponent<Animator>();
        // Animasyon event'i animasyon dosyas�na eklenmeli
        // Animasyon edit�r�nde k�l�� animasyonuna "ActivateHitbox" adl� event eklenmeli
    }

    // Animasyon event'inden �a�r�lacak (Animator pencersinden ayarlanmal�)
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

        // Hitbox olu�tur ve kontrol et
        CheckHitbox();

        // Hitbox s�resini bekle
        yield return new WaitForSeconds(hitboxDuration);

        hitboxActive = false;
    }

    private void CheckHitbox()
    {
        // Karakterin �n�nde bir hitbox olu�tur
        Vector3 hitboxCenter = transform.position + transform.TransformDirection(hitboxOffset);

        // Hitbox i�indeki t�m Collider'lar� al
        Collider[] hitColliders = Physics.OverlapBox(hitboxCenter, hitboxSize / 2, transform.rotation);

        // Her bir collider i�in i�lem yap
        foreach (var hitCollider in hitColliders)
        {
            // E�er kendimiz de�ilse
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
                // Mutant kontrol� ekle
                else if (hitCollider.CompareTag("Mutant"))
                {
                    Debug.Log("Mutant " + hitCollider.name + " �ld�r�ld�!");

                    // Mutant �zel efekti
                    StartCoroutine(KillMutant(hitCollider.gameObject));
                }
            }
        }
    }

    private IEnumerator BreakObject(GameObject objectToBreak)
    {
        // Objenin pozisyonunu ve rengini al
        Vector3 objPosition = objectToBreak.transform.position;
        Color objColor = Color.white; // Varsay�lan renk

        // E�er objenin renderer'� varsa rengini al
        Renderer renderer = objectToBreak.GetComponent<Renderer>();
        if (renderer != null && renderer.material != null)
        {
            objColor = renderer.material.color;
        }

        // Efekt olu�tur
        if (breakEffect != null)
        {
            // Haz�r efekt prefab�n� kullan
            GameObject effect = Instantiate(breakEffect, objPosition, Quaternion.identity);

            // Par�ac�k sistemini ayarla (e�er varsa)
            ParticleSystem ps = effect.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                var main = ps.main;
                main.startColor = objColor;
            }

            // Efektin kendi kendini yok etmesi i�in
            Destroy(effect, 2f);
        }
        else
        {
            // Basit k�p par�ac�klar� olu�tur (efekt prefab� yoksa)
            CreateCubeFragments(objPosition, objColor, objectToBreak.transform.localScale, false);
        }

        // K�sa bir gecikme
        yield return new WaitForSeconds(destroyDelay);

        // Objeyi yok et
        Destroy(objectToBreak);
    }

    // Mutant �ld�rme fonksiyonu
    private IEnumerator KillMutant(GameObject mutant)
    {
        Vector3 mutantPosition = mutant.transform.position;
        Vector3 mutantScale = mutant.transform.localScale;

        // Kan efekti olu�tur
        if (mutantBloodEffect != null)
        {
            GameObject bloodEffect = Instantiate(mutantBloodEffect, mutantPosition, Quaternion.identity);

            // Kan par�ac�k sistemini ayarla
            ParticleSystem bloodPS = bloodEffect.GetComponent<ParticleSystem>();
            if (bloodPS != null)
            {
                var main = bloodPS.main;
                main.startColor = mutantBloodColor;
                main.maxParticles = 50; // Daha fazla kan par�ac���
            }

            Destroy(bloodEffect, 3f);
        }

        // Mutant par�ac�klar� olu�tur (daha fazla ve daha dramatik)
        CreateCubeFragments(mutantPosition, mutantBloodColor, mutantScale, true);

        // Ekstra kan damlalar� efekti
        CreateBloodDroplets(mutantPosition);

        // K�sa gecikme
        yield return new WaitForSeconds(destroyDelay);

        // Mutant'� oyundan kald�r
        Destroy(mutant);
    }

    // Geli�tirilmi� par�a olu�turma fonksiyonu
    private void CreateCubeFragments(Vector3 position, Color color, Vector3 originalScale, bool isMutant)
    {
        int fragmentCount = isMutant ? mutantFragmentCount : 8;
        float fragmentSize = originalScale.x * (isMutant ? 0.2f : 0.3f); // Mutant par�alar� daha k���k
        float explosionForce = isMutant ? mutantExplosionForce : 5f;
        float lifetime = isMutant ? mutantFragmentLifetime : 2f;

        for (int i = 0; i < fragmentCount; i++)
        {
            // K���k bir k�p olu�tur
            GameObject fragment = GameObject.CreatePrimitive(PrimitiveType.Cube);
            fragment.transform.localScale = Vector3.one * fragmentSize;

            // Rastgele yak�n bir pozisyon (mutantlar i�in daha geni� yay�l�m)
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

                // Mutant par�alar� i�in metalik g�r�n�m
                if (isMutant)
                {
                    fragRenderer.material.SetFloat("_Metallic", 0.3f);
                    fragRenderer.material.SetFloat("_Glossiness", 0.7f);
                }
            }

            // Fizik ekle
            Rigidbody rb = fragment.AddComponent<Rigidbody>();
            rb.mass = 0.1f; // Daha hafif par�ac�klar

            // Patlama kuvveti uygula
            rb.AddExplosionForce(explosionForce, position, 2f);

            // Rastgele d�n�� ekle
            rb.AddTorque(Random.insideUnitSphere * 10f);

            // Par�ay� yok et
            Destroy(fragment, lifetime);
        }
    }

    // Kan damlalar� efekti
    private void CreateBloodDroplets(Vector3 position)
    {
        int dropletCount = 12; // Kan damla say�s�

        for (int i = 0; i < dropletCount; i++)
        {
            // K���k k�reler olu�tur (kan damlalar�)
            GameObject droplet = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            droplet.transform.localScale = Vector3.one * Random.Range(0.05f, 0.15f);

            // Rastgele pozisyon
            Vector3 randomOffset = new Vector3(
                Random.Range(-0.5f, 0.5f),
                Random.Range(0f, 0.8f), // Yukar� do�ru daha fazla
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

            // Yukar� do�ru f�rlatma kuvveti
            Vector3 force = new Vector3(
                Random.Range(-3f, 3f),
                Random.Range(3f, 6f),
                Random.Range(-3f, 3f)
            );
            rb.AddForce(force, ForceMode.Impulse);

            // Kan damlas�n� yok et
            Destroy(droplet, 3f);
        }
    }

    // Hitbox'� g�rselle�tirmek i�in (sadece editor'da)
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

    // Test i�in manual olarak kullanabilece�iniz bir fonksiyon
    public void ManuallyActivateHitbox()
    {
        ActivateHitbox();
    }
}