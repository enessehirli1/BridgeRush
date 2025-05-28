using UnityEngine;
using TMPro;
using System.Collections;

public class MultiTriggerInfo : MonoBehaviour
{
    [Header("Bu Trigger'a �zel Ayarlar")]
    public string triggerID = "Trigger1";
    [TextArea(3, 5)]
    public string bilgilendirmeMetni = "Buraya bu trigger'a �zel mesaj� yaz�n";
    public float gosterimSuresi = 3f;
    public bool sadeceBirKezGoster = true;
    public bool respawnSonrasiReset = true; // Yeni eklenen �zellik

    // Static de�i�kenler
    private static GameObject bilgilendirmePanel;
    private static TextMeshProUGUI bilgilendirmeText;
    private static bool sistemBaslatildi = false;
    private static Coroutine aktifCoroutine;

    private bool zatenGosterildi = false;

    void Start()
    {
        // UI sistemini ba�lat
        if (!sistemBaslatildi)
        {
            UISysteminiBaslat();
            sistemBaslatildi = true;
        }

        // Player respawn oldu�unda trigger'lar� resetle
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
        // Debug i�in player kontrol�
        Debug.Log($"Trigger'a giren obje: {other.name}, Tag: {other.tag}");

        if (other.CompareTag("Player"))
        {
            if (sadeceBirKezGoster && zatenGosterildi)
            {
                Debug.Log($"{triggerID} zaten g�sterildi, tekrar g�sterilmiyor");
                return;
            }

            BilgilendirmeGoster();
            zatenGosterildi = true;

            Debug.Log($"{triggerID} tetiklendi: {bilgilendirmeMetni}");
        }
        else
        {
            Debug.Log($"Trigger'a giren obje Player de�il: {other.tag}");
        }
    }

    void BilgilendirmeGoster()
    {
        // UI referanslar�n� tekrar kontrol et
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
            Debug.LogError("UI referanslar� bulunamad�! BilgilendirmePanel ve TextMeshPro kontrol� yap�n.");
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

    // Trigger'� resetle (respawn sonras� �al��mas� i�in)
    public void ResetTrigger()
    {
        zatenGosterildi = false;
        Debug.Log($"{triggerID} trigger'� resetlendi");
    }

    // Manuel reset (Inspector'dan test i�in)
    [ContextMenu("Reset Trigger")]
    public void ManuelReset()
    {
        ResetTrigger();
    }

    // T�m trigger'lar� resetle (static method)
    public static void TumTriggerlariResetlte()
    {
        MultiTriggerInfo[] tumTriggerlar = FindObjectsOfType<MultiTriggerInfo>();
        foreach (MultiTriggerInfo trigger in tumTriggerlar)
        {
            trigger.ResetTrigger();
        }
        Debug.Log("T�m trigger'lar resetlendi");
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