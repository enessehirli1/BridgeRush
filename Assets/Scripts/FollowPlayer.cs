using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public Transform player; // Player GameObject'u
    public Vector3 offset;


    // Update is called once per frame
    void Update()
    {
        transform.position = player.position + offset;

    }
}
