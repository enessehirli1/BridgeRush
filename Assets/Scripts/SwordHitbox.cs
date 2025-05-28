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
            CreateCubeFragments(objPosition, objColor, objectToBreak.transform.localScale);
        }

        // K�sa bir gecikme
        yield return new WaitForSeconds(destroyDelay);

        // Objeyi yok et
        Destroy(objectToBreak);
    }

    // Basit k�p par�ac�klar� olu�turan fonksiyon (e�er haz�r efekt yoksa)
    private void CreateCubeFragments(Vector3 position, Color color, Vector3 originalScale)
    {
        int fragmentCount = 8; // Ka� par�a olaca��
        float fragmentSize = originalScale.x * 0.3f; // K���k k�plerin boyutu, orijinal boyuttan oran olarak hesapla
        float explosionForce = 5f; // Ne kadar g��l� patlayacak

        for (int i = 0; i < fragmentCount; i++)
        {
            // K���k bir k�p olu�tur
            GameObject fragment = GameObject.CreatePrimitive(PrimitiveType.Cube);
            fragment.transform.localScale = Vector3.one * fragmentSize;

            // Rastgele yak�n bir pozisyon
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

            // Birka� saniye sonra par�ay� da yok et
            Destroy(fragment, 2f);
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