using UnityEngine;
using System.Collections;

public class CharacterSlideController : MonoBehaviour
{
    [Header("Slide Settings")]
    [SerializeField] private float slideDuration = 1f;    // Duration of the slide in seconds
    [SerializeField] private float slideSpeed = 10f;      // Additional force applied during sliding
    [SerializeField] private float slideCooldown = 0.5f;  // Time before player can slide again

    [Header("Collider Settings")]
    [SerializeField] private float colliderHeightMultiplier = 0.5f;  // The collider height will be reduced to this percentage of original

    // Component references
    private Animator animator;
    private Rigidbody rb;
    private CapsuleCollider characterCollider;

    // Original collider values
    private float originalColliderHeight;
    private Vector3 originalColliderCenter;

    // State tracking
    private bool isSliding = false;
    private bool canSlide = true;
    private bool isGrounded = true;  // This should be updated by your character controller

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Get component references
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        characterCollider = GetComponent<CapsuleCollider>();

        // Store original collider values
        originalColliderHeight = characterCollider.height;
        originalColliderCenter = characterCollider.center;
    }

    // Update is called once per frame
    void Update()
    {
        // Check if player can and wants to slide
        if (Input.GetKeyDown(KeyCode.LeftShift) && CanInitiateSlide())
        {
            StartSlide();
        }
    }

    // Custom method to check ground state - integrate with your existing ground check system
    void CheckGrounded()
    {
        // Simple ground check example - replace with your existing ground detection
        float rayLength = 0.1f;
        Vector3 rayStart = transform.position + Vector3.up * 0.1f; // Small offset to avoid collision with ground
        isGrounded = Physics.Raycast(rayStart, Vector3.down, rayLength);
    }

    bool CanInitiateSlide()
    {
        // Update grounded status
        CheckGrounded();

        // Can only slide if grounded, not already sliding, and not in cooldown
        return isGrounded && canSlide && !isSliding;
    }

    void StartSlide()
    {
        // Set sliding state
        isSliding = true;
        canSlide = false;

        // Trigger animation
        animator.SetTrigger("Slide");

        // Apply sliding physics
        ApplySlideColliderChanges();
        ApplySlideForce();

        // Start slide timing coroutines
        StartCoroutine(EndSlideAfterDuration());
        StartCoroutine(SlideCooldown());
    }

    void ApplySlideColliderChanges()
    {
        // Calculate new height keeping the minimum point unchanged
        float newHeight = originalColliderHeight * colliderHeightMultiplier;

        // Calculate how much height is being reduced
        float heightReduction = originalColliderHeight - newHeight;

        // Calculate new center - THIS IS THE KEY CHANGE!
        // We're keeping the bottom of the collider at EXACTLY the same position
        Vector3 newCenter = originalColliderCenter;

        // Move the center down by half the height reduction so bottom stays fixed
        newCenter.y = originalColliderCenter.y - (heightReduction / 2f);

        // Apply new collider values
        characterCollider.height = newHeight;
        characterCollider.center = newCenter;

        // NO position adjustment - we're only changing the collider
    }

    void ApplySlideForce()
    {
        // Apply force in the direction the character is facing
        Vector3 slideDirection = transform.forward;
        rb.AddForce(slideDirection * slideSpeed, ForceMode.Impulse);
    }

    void ResetCollider()
    {
        // Reset collider to original values
        characterCollider.height = originalColliderHeight;
        characterCollider.center = originalColliderCenter;
    }

    IEnumerator EndSlideAfterDuration()
    {
        // Wait for the slide duration
        yield return new WaitForSeconds(slideDuration);

        // End the slide
        isSliding = false;

        // Reset collider to original values - this is all we need to do
        ResetCollider();

        // Notify animation system (optional - if you have an "EndSlide" trigger)
        // animator.SetTrigger("EndSlide");
    }

    IEnumerator SlideCooldown()
    {
        // Wait for cooldown
        yield return new WaitForSeconds(slideCooldown);

        // Allow sliding again
        canSlide = true;
    }

    // Public accessor for other scripts
    public bool IsSliding()
    {
        return isSliding;
    }
}