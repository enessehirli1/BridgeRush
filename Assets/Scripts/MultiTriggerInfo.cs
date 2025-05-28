using UnityEngine;
using TMPro;
using System.Collections;

public class MultiTriggerInfo : MonoBehaviour
{
    [Header("Bu Trigger'a Özel Ayarlar")]
    public string triggerID = "Trigger1";
    [TextArea(3, 5)]
    public string bilgilendirmeMetni = "Buraya bu trigger'a özel mesajý yazýn";
    public float gosterimSuresi = 3f;
    public bool sadeceBirKezGoster = true;
    public bool respawnSonrasiReset = true; // Yeni eklenen özellik

    // Static deðiþkenler
    private static GameObject bilgilendirmePanel;
    private static TextMeshProUGUI bilgilendirmeText;
    private static bool sistemBaslatildi = false;
    private static Coroutine aktifCoroutine;

    private bool zatenGosterildi = false;

    void Start()
    {
        // UI sistemini baþlat
        if (!sistemBaslatildi)
        {
            UISysteminiBaslat();
            sistemBaslatildi = true;
        }

        // Player respawn olduðunda trigger'larý resetle
        if (respawnSonrasiReset)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                PlayerRespawnHandler respawnHandler = player.GetComponent<PlayerRespawnHandler>();
                if (respawnHandler != null)
                {
                    respawnHandler.OnPlayerRespawn += ResetTrigger;
                }
            }
        }
    }

    void UISysteminiBaslat()
    {
        if (bilgilendirmePanel == null)
        {
            bilgilendirmePanel = GameObject.Find("BilgilendirmePanel");
        }

        if (bilgilendirmeText == null && bilgilendirmePanel != null)
        {
            bilgilendirmeText = bilgilendirmePanel.GetComponentInChildren<TextMeshProUGUI>();
        }

        if (bilgilendirmePanel != null)
        {
            bilgilendirmePanel.SetActive(false);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // Debug için player kontrolü
        Debug.Log($"Trigger'a giren obje: {other.name}, Tag: {other.tag}");

        if (other.CompareTag("Player"))
        {
            if (sadeceBirKezGoster && zatenGosterildi)
            {
                Debug.Log($"{triggerID} zaten gösterildi, tekrar gösterilmiyor");
                return;
            }

            BilgilendirmeGoster();
            zatenGosterildi = true;

            Debug.Log($"{triggerID} tetiklendi: {bilgilendirmeMetni}");
        }
        else
        {
            Debug.Log($"Trigger'a giren obje Player deðil: {other.tag}");
        }
    }

    void BilgilendirmeGoster()
    {
        // UI referanslarýný tekrar kontrol et
        if (bilgilendirmePanel == null || bilgilendirmeText == null)
        {
            UISysteminiBaslat();
        }

        if (bilgilendirmePanel != null && bilgilendirmeText != null)
        {
            if (aktifCoroutine != null)
            {
                StopCoroutine(aktifCoroutine);
            }

            bilgilendirmeText.text = bilgilendirmeMetni;
            bilgilendirmePanel.SetActive(true);

            aktifCoroutine = StartCoroutine(BilgilendirmeGizle());
        }
        else
        {
            Debug.LogError("UI referanslarý bulunamadý! BilgilendirmePanel ve TextMeshPro kontrolü yapýn.");
        }
    }

    IEnumerator BilgilendirmeGizle()
    {
        yield return new WaitForSeconds(gosterimSuresi);

        if (bilgilendirmePanel != null)
        {
            bilgilendirmePanel.SetActive(false);
        }

        aktifCoroutine = null;
    }

    // Trigger'ý resetle (respawn sonrasý çalýþmasý için)
    public void ResetTrigger()
    {
        zatenGosterildi = false;
        Debug.Log($"{triggerID} trigger'ý resetlendi");
    }

    // Manuel reset (Inspector'dan test için)
    [ContextMenu("Reset Trigger")]
    public void ManuelReset()
    {
        ResetTrigger();
    }

    // Tüm trigger'larý resetle (static method)
    public static void TumTriggerlariResetlte()
    {
        MultiTriggerInfo[] tumTriggerlar = FindObjectsOfType<MultiTriggerInfo>();
        foreach (MultiTriggerInfo trigger in tumTriggerlar)
        {
            trigger.ResetTrigger();
        }
        Debug.Log("Tüm trigger'lar resetlendi");
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        if (GetComponent<BoxCollider>() != null)
        {
            BoxCollider box = GetComponent<BoxCollider>();
            Gizmos.DrawWireCube(transform.position + box.center, box.size);
        }
        else if (GetComponent<SphereCollider>() != null)
        {
            SphereCollider sphere = GetComponent<SphereCollider>();
            Gizmos.DrawWireSphere(transform.position + sphere.center, sphere.radius);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        if (GetComponent<BoxCollider>() != null)
        {
            BoxCollider box = GetComponent<BoxCollider>();
            Gizmos.DrawCube(transform.position + box.center, box.size);
        }
    }
}