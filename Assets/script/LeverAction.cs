using UnityEngine;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem;
#endif

public class LeverAction : MonoBehaviour
{
    [Header("References")]
    public Animator leverAnimator;
    public GameObject interact; // UI message to show when player is in range (shared between all levers)
    public GameObject lighttrifficObject; // Reference to the lighttriffic object

    [Header("Settings")]
    public string playerTag = "Player";
    public string animatorTriggerName = "play";
    public bool leverActive = false; // Once true, lever can't be re-activated
    
    [Header("Debug")]
    public bool playerInRange = false; // Public bool to confirm player detection
    
    // Static reference for shared interact UI and counter
    private static GameObject sharedInteract;
    private static int activeInteractCount = 0;

    void Start()
    {
        // Store the shared interact reference from the first lever
        if (interact != null && sharedInteract == null)
        {
            sharedInteract = interact;
        }
    }

    void Update()
    {
        // If lever already active, block interaction
        if (leverActive)
        {
            return;
        }

        if (playerInRange)
        {
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
            // Use new Input System - E key
            if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            {
                ActivateLever();
            }
#else
            // Use legacy Input System - E key
            if (Input.GetKeyDown(KeyCode.E))
            {
                ActivateLever();
            }
#endif
        }
    }

    void ActivateLever()
    {
        // Prevent re-activation if already active
        if (leverActive) return;

        leverActive = true;

        if (leverAnimator != null)
            leverAnimator.SetTrigger(animatorTriggerName);

        if (lighttrifficObject != null)
        {
            var lt = lighttrifficObject.GetComponent<lighttrifficAction>();
            if (lt != null)
                lt.PlayLighttriffic();
            else
                Debug.LogWarning("Assigned lighttrifficObject has no lighttrifficAction component.", lighttrifficObject);
        }

        // Disable collider to prevent further triggers
        var col = GetComponent<Collider>();
        if (col != null)
            col.enabled = false;

        // Decrement counter and hide UI if no other lever needs it
        if (playerInRange)
        {
            playerInRange = false;
            DecrementInteractCounter();
        }
    }
    
    void IncrementInteractCounter()
    {
        activeInteractCount++;
        if (sharedInteract != null && !sharedInteract.activeSelf)
        {
            sharedInteract.SetActive(true);
        }
    }
    
    void DecrementInteractCounter()
    {
        activeInteractCount--;
        if (activeInteractCount <= 0)
        {
            activeInteractCount = 0;
            if (sharedInteract != null && sharedInteract.activeSelf)
            {
                sharedInteract.SetActive(false);
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!leverActive && other.CompareTag(playerTag))
        {
            playerInRange = true;
            IncrementInteractCounter();
            Debug.Log("Player in range of lever.");
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag) && playerInRange)
        {
            playerInRange = false;
            DecrementInteractCounter();
        }
    }
}
