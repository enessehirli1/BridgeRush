using UnityEngine;
using System;

public class PlayerRespawnHandler : MonoBehaviour
{
    [Header("Respawn Ayarlar�")]
    public Transform spawnPoint;
    public float respawnDelay = 2f;

    // Event - trigger'lar bunu dinleyecek
    public event Action OnPlayerRespawn;

    // Player �ld���nde �a�r�lacak method
    public void PlayerOldu()
    {
        Debug.Log("Player �ld�, respawn ba�l�yor...");
        Invoke("RespawnPlayer", respawnDelay);
    }

    void RespawnPlayer()
    {
        // Player'� spawn noktas�na ta��
        if (spawnPoint != null)
        {
            transform.position = spawnPoint.position;
            transform.rotation = spawnPoint.rotation;
        }

        // Tag'in do�ru oldu�undan emin ol
        if (!CompareTag("Player"))
        {
            tag = "Player";
            Debug.Log("Player tag'i yeniden atand�");
        }

        // Collider'� kontrol et
        Collider playerCollider = GetComponent<Collider>();
        if (playerCollider != null && !playerCollider.enabled)
        {
            playerCollider.enabled = true;
            Debug.Log("Player collider aktif edildi");
        }

        // Rigidbody'yi resetle (varsa)
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // Trigger'lar� resetle
        OnPlayerRespawn?.Invoke();

        Debug.Log("Player respawn tamamland�");
    }

    // Test i�in - Inspector'dan �a��rabilirsiniz
    [ContextMenu("Test Respawn")]
    public void TestRespawn()
    {
        PlayerOldu();
    }

    // Alternatif: T�m trigger'lar� manuel resetle
    [ContextMenu("Reset All Triggers")]
    public void ResetAllTriggers()
    {
        MultiTriggerInfo.TumTriggerlariResetlte();
    }
}