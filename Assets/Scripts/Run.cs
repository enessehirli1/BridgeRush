using UnityEngine;

public class Run : MonoBehaviour
{
    [SerializeField] public float moveSpeed = 5f;
    public Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        rb.linearVelocity = transform.forward * moveSpeed + Vector3.up * rb.linearVelocity.y;
    }
}
