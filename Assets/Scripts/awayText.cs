using UnityEngine;

public class awayText : MonoBehaviour
{
    public Transform player; // Oyuncu veya kamera transformu
    public float parallaxFactor = 0.5f; // Ne kadar yavaþ/ters hareket edecek

    private Vector3 initialOffset;

    void Start()
    {
        if (player == null)
            player = Camera.main.transform;

        // Baþlangýçta text'in oyuncuya olan offset'ini kaydet
        initialOffset = transform.position - player.position;
    }

    void Update()
    {
        Vector3 playerMovement = player.position;

        // Text'i oyuncunun pozisyonuna göre ters yönde kaydýr
        Vector3 targetPosition = playerMovement + initialOffset * parallaxFactor;
        transform.position = targetPosition;
    }
}
