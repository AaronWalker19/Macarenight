using UnityEngine;
using UnityEngine.InputSystem;

public class Movesplayer : MonoBehaviour
{
    private Rigidbody rb;
    private Animator animator;
    public float moveSpeed = 10f;
    public float runSpeed = 10f;
    public float rotationSpeed = 150f;
    public float jumpForce = 5f;
    private Vector2 moveInput;
    private bool isRunning = false;
    
    
    [Header("Camera Settings")]
    public Camera playerCamera; // Référence à la caméra
    // Mode première personne toujours actif
    public float eyeHeight = 5f; // Hauteur des yeux du personnage
    public float sprintNearClip = 2f; // near clip plane when sprinting
    public float normalNearClip = 0.9f; // normal near clip plane
    
    [Header("Mouse Look Settings")]
    public float mouseSensitivity = 2f;
    private Vector2 lookInput;
    private float xRotation = 0f;
    private float yRotation = 0f;
    private bool prevIsWalking = false;



    void Start()
    {
        rb = GetComponent<Rigidbody>();
        // N'écrase pas une référence assignée via l'inspector
        if (animator == null)
            animator = GetComponent<Animator>();

        // Mode première personne toujours actif
        if (playerCamera != null)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            UpdateFirstPersonCamera();
            
        }

        // Configure le Rigidbody pour un personnage
        rb.freezeRotation = true; // Empêche toutes les rotations physiques
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        
        // Désactive le NavMeshObstacle s'il existe (conflit avec Rigidbody)
        UnityEngine.AI.NavMeshObstacle obstacle = GetComponent<UnityEngine.AI.NavMeshObstacle>();
        if (obstacle != null)
        {
            obstacle.enabled = false;
        }
    }
    // Effet de secousse de caméra lors d'un coup reçu
    public void CameraShake(float duration = 0.2f, float magnitude = 0.2f)
    {
        if (playerCamera != null)
            StartCoroutine(ShakeCoroutine(duration, magnitude));
    }

    private System.Collections.IEnumerator ShakeCoroutine(float duration, float magnitude)
    {
        Vector3 originalPos = playerCamera.transform.localPosition;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;
            playerCamera.transform.localPosition = originalPos + new Vector3(x, y, 0);
            elapsed += Time.deltaTime;
            yield return null;
        }
        playerCamera.transform.localPosition = originalPos;
    }

    void Update()
    {
        // Détecte si Ctrl gauche est maintenu
        isRunning = Keyboard.current.leftCtrlKey.isPressed;
        // Ajuste le near clip plane de la caméra selon l'état de sprint
        if (playerCamera != null)
        {
            playerCamera.nearClipPlane = isRunning ? sprintNearClip : normalNearClip;
        }
        
        // Gestion de la souris et rotation caméra en première personne
        if (playerCamera != null)
        {
            // Rotation horizontale (gauche/droite) - appliquée au joueur
            yRotation += lookInput.x * mouseSensitivity;
            
            // Rotation verticale (haut/bas) - appliquée à la caméra
            xRotation -= lookInput.y * mouseSensitivity;
            xRotation = Mathf.Clamp(xRotation, -75f, 75f); // Limite verticale
            
            // Appliquer les rotations
            transform.rotation = Quaternion.Euler(0f, yRotation, 0f);
            playerCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        }
        
        // Met à jour l'animation de course
        if (animator != null)
            animator.SetBool("run", isRunning && moveInput.y != 0);
    }
    
    void UpdateFirstPersonCamera()
    {
        // La caméra est enfant du joueur : on utilise uniquement localPosition
        playerCamera.transform.localPosition = Vector3.up * eyeHeight + Vector3.right * -0.1f;
        // La rotation Y est héritée automatiquement du parent, pas besoin de la forcer
    }
    
    // Suppression de ToggleFirstPerson et de toute référence à isFirstPerson

    void FixedUpdate()
    {
        // (ground check supprimé)
        
        // Déplacement sur les axes locaux (avant/arrière/gauche/droite)
        float currentSpeed = isRunning ? runSpeed : moveSpeed;
        Vector3 moveDirection = (transform.forward * moveInput.y + transform.right * moveInput.x).normalized;
        Vector3 movement = moveDirection * currentSpeed;
        
        // Applique le mouvement en conservant la vélocité verticale
        rb.linearVelocity = new Vector3(movement.x, rb.linearVelocity.y, movement.z);

        // Active l'animation de marche si on bouge
        if (animator != null)
        {
            bool isWalking = (moveInput.x != 0 || moveInput.y != 0);
            animator.SetBool("walk", isWalking);
            if (isWalking)
                animator.SetBool("dance", false);

            // Debug: log quand l'état de marche change
            if (isWalking != prevIsWalking)
            {
                prevIsWalking = isWalking;
            }
        }
    }
    // ground check and collision handlers removed (not needed)

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
        // Debug: vérifier que l'input arrive bien
        // (enlève ou commente cette ligne quand tu as fini de déboguer)
    }

    public void OnLook(InputValue value)
    {
        lookInput = value.Get<Vector2>();
        // Note: L'Input System envoie déjà le delta, pas besoin de Time.deltaTime
    }

    public void OnJump(InputValue value)
    {
        if (value.isPressed)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);

            // Déclenche l'animation de saut
            if (animator != null)
            {
                animator.SetTrigger("jump");
                animator.SetBool("dance", false);
            }
        }
    }

    



    
    

}
