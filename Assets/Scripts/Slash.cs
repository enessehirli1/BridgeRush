using UnityEngine;
using UnityEngine.InputSystem;

public class Slash : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private Animator mAnimator;
    private Rigidbody rb;
    void Start()
    {
        mAnimator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            mAnimator.SetTrigger("Slash");
        }

    }
}
