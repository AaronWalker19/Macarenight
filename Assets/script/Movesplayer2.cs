using UnityEngine;
using UnityEngine.InputSystem;

public class Movesplayer2 : MonoBehaviour
{
    private Rigidbody rb;
    private Animator animator;
    
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float runSpeed = 8f;
    public float jumpForce = 5f;
    
    private Vector2 moveInput;
    private bool isRunning = false;
    private bool isGrounded = false;
    private bool isDead = false;
    
    [Header("Camera Settings")]
    public Camera playerCamera;
    public float eyeHeight = 1.6f; // Hauteur des yeux du personnage
    public float sprintNearClip = 2f; // near clip plane when sprinting
    public float normalNearClip = 0.9f; // normal near clip plane
    
    [Header("Mouse Look Settings")]
    public float mouseSensitivity = 2f;
    private Vector2 lookInput;
    private float xRotation = 0f; // Rotation verticale de la caméra
    private float yRotation = 0f; // Rotation horizontale du joueur
    
    // Actions Input System
    private InputAction moveAction;
    private InputAction lookAction;
    private InputAction jumpAction;
    private InputAction sprintAction;

    [Header("Audio Settings")]
    public AudioSource sounderPlayer;
    
    [Header("Ground Detection")]
    public float groundCheckDistance = 0.3f;
    public LayerMask groundLayer = -1; // -1 = tous les layers par défaut

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
    }

    void Start()
    {
        // Configurer le son de marche en boucle
        if (sounderPlayer != null)
        {
            sounderPlayer.loop = true;
            sounderPlayer.Stop(); // S'assurer qu'il ne joue pas au départ
        }
        
        // Verrouiller et cacher le curseur
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        // Configuration du Rigidbody
        if (rb != null)
        {
            rb.freezeRotation = true; // Empêcher le joueur de basculer
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            rb.interpolation = RigidbodyInterpolation.Interpolate; // Lissage du mouvement
            rb.linearDamping = 0f; // Pas de friction linéaire (on gère manuellement)
            rb.angularDamping = 0.05f; // Faible friction angulaire
            rb.mass = 80f; // Masse suffisante pour ne pas être propulsé facilement
            
        }
        else
        {
            Debug.LogError("ERREUR: Pas de Rigidbody sur le joueur!");
        }
        
        // Positionner la caméra si elle existe
        if (playerCamera != null)
        {
            playerCamera.transform.SetParent(transform);
            playerCamera.transform.localPosition = new Vector3(0, eyeHeight, 0);
            playerCamera.transform.localRotation = Quaternion.identity;
            
        }
        else
        {
            Debug.LogWarning("ATTENTION: Pas de caméra assignée!");
        }
        
        // Configuration de l'Input System via le nouveau système
        SetupInputActions();
       


    }
    
    void SetupInputActions()
    {
        // Récupérer l'asset d'actions
        var playerInput = GetComponent<PlayerInput>();
        if (playerInput != null)
        {
            var actionMap = playerInput.actions.FindActionMap("Player");
            
            if (actionMap != null)
            {
                // Move
                moveAction = actionMap.FindAction("Move");
                if (moveAction != null)
                {
                    moveAction.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
                    moveAction.canceled += ctx => moveInput = Vector2.zero;
                }
                
                // Look
                lookAction = actionMap.FindAction("Look");
                if (lookAction != null)
                {
                    lookAction.performed += ctx => lookInput = ctx.ReadValue<Vector2>();
                    lookAction.canceled += ctx => lookInput = Vector2.zero;
                }
                
                // Jump
                jumpAction = actionMap.FindAction("Jump");
                if (jumpAction != null)
                {
                    jumpAction.performed += OnJumpPerformed;
                }
                
                // Sprint
                sprintAction = actionMap.FindAction("Sprint");
                if (sprintAction != null)
                {
                    sprintAction.performed += ctx => 
                    {
                        // Vérifier si on peut sprinter (stamina disponible)
                        if (RunActionScript.Instance != null && RunActionScript.Instance.CanSprint())
                            isRunning = true;
                    };
                    sprintAction.canceled += ctx => 
                    {
                        isRunning = false;
                        // Notifier le script de stamina qu'on a arrêté de sprinter
                        if (RunActionScript.Instance != null)
                            RunActionScript.Instance.StopSprinting();
                    };
                }
                
                
            }
            else
            {
                Debug.LogError("Action Map 'Player' introuvable!");
            }
        }
        else
        {
            Debug.LogError("ERREUR: Pas de Player Input Component!");
        }
    }
    
    void OnJumpPerformed(InputAction.CallbackContext context)
    {
        if (isDead || !isGrounded || rb == null) return;
        
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        Debug.Log("SAUT!");
    }

    void Update()
    {
        // Ne rien faire si le joueur est mort
        if (isDead) return;
        
        // Ajuste le near clip plane de la caméra selon l'état de sprint
        if (playerCamera != null)
        {
            playerCamera.nearClipPlane = isRunning ? sprintNearClip : normalNearClip;
        }
        
        // Gestion de la rotation de la caméra avec la souris
        HandleMouseLook();
        
        // Vérifier si le joueur est au sol
        CheckGrounded();
    }
    
    void FixedUpdate()
    {
        // Gestion du mouvement
        HandleMovement();
    }
    
    void HandleMouseLook()
    {
        if (playerCamera == null) return;
        
        // Si mort, ignorer complètement les inputs de rotation
        if (isDead)
        {
            lookInput = Vector2.zero;
            return;
        }
        
        // Rotation horizontale (gauche/droite) - appliquée au joueur
        yRotation += lookInput.x * mouseSensitivity;
        
        // Rotation verticale (haut/bas) - appliquée à la caméra
        xRotation -= lookInput.y * mouseSensitivity;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f); // Limiter la rotation verticale
        
        // Appliquer la rotation au joueur (axe Y)
        transform.rotation = Quaternion.Euler(0f, yRotation, 0f);
        
        // Appliquer la rotation à la caméra (axe X)
        playerCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }
    
    void HandleMovement()
    {
        if (rb == null || isDead) return;
        
        // Détecter si le joueur est en train de marcher (avec deadzone pour éviter les faux positifs)
        bool isWalking = moveInput.magnitude > 0.1f;
        
        // Calculer la direction de déplacement basée sur l'orientation du joueur
        Vector3 forward = transform.forward;
        Vector3 right = transform.right;
        
        // Créer le vecteur de mouvement (avant/arrière + gauche/droite)
        Vector3 moveDirection = (forward * moveInput.y + right * moveInput.x).normalized;
        
        // Vérifier si on peut sprinter et consommer la stamina
        bool canActuallySprint = false;
        if (isRunning && RunActionScript.Instance != null)
        {
            canActuallySprint = RunActionScript.Instance.TryConsumeStamina(Time.fixedDeltaTime);
            if (!canActuallySprint)
            {
                // Pas assez de stamina, forcer l'arrêt du sprint
                isRunning = false;
                RunActionScript.Instance.StopSprinting();
            }
        }
        else if (!isRunning && RunActionScript.Instance != null)
        {
            // S'assurer que le script sait qu'on ne sprinte pas
            RunActionScript.Instance.StopSprinting();
        }
        
        // Déterminer la vitesse actuelle
        float currentSpeed = (isRunning && canActuallySprint) ? runSpeed : moveSpeed;
        
        // Appliquer le mouvement (conserver la vélocité Y pour la gravité/saut)
        Vector3 targetVelocity = moveDirection * currentSpeed;
        
        // Limiter la vélocité verticale pour éviter les propulsions parasites
        float clampedY = rb.linearVelocity.y;
        // Si on est au sol et qu'on ne saute pas, plafonner la vitesse verticale
        if (isGrounded && clampedY > 0f)
        {
            clampedY = Mathf.Min(clampedY, 0.5f); // Limite les rebonds/propulsions
        }
        
        rb.linearVelocity = new Vector3(targetVelocity.x, clampedY, targetVelocity.z);
        
        // Gestion des animations de mouvement
        if (animator != null)
        {
            animator.SetBool("walk", isWalking);
            animator.SetBool("run", (isRunning && canActuallySprint) && isWalking);
        }
        
        // Gestion du son de marche
        if (sounderPlayer != null)
        {
            if (isWalking && !sounderPlayer.isPlaying)
            {
                sounderPlayer.Play();
            }
            else if (!isWalking && sounderPlayer.isPlaying)
            {
                sounderPlayer.Stop();
            }
        }
    }
    
    void CheckGrounded()
    {
        // Obtenir la hauteur du collider pour partir du bas du personnage
        float yOffset = 0.1f; // Petit offset pour partir légèrement du bas
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            yOffset = col.bounds.extents.y + 0.1f; // Rayon du collider + marge
        }
        
        // Point de départ au bas du personnage
        Vector3 rayStart = transform.position - new Vector3(0, yOffset, 0);
        
        // Raycast pour vérifier si le joueur touche le sol
        isGrounded = Physics.Raycast(rayStart, Vector3.down, groundCheckDistance, groundLayer);
        
        // Debug visuel (optionnel - commentez si vous ne voulez pas voir le ray)
        Debug.DrawRay(rayStart, Vector3.down * groundCheckDistance, isGrounded ? Color.green : Color.red);
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
    
    // Méthode à appeler quand le joueur meurt (0 PV)
    public void OnPlayerDeath()
    {
        isDead = true;
        
        // Arrêter le son de marche
        if (sounderPlayer != null && sounderPlayer.isPlaying)
        {
            sounderPlayer.Stop();
        }
        
        // Bloquer le mouvement
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
        }
        
        // Réinitialiser les inputs pour arrêter tout mouvement
        moveInput = Vector2.zero;
        lookInput = Vector2.zero;
        
        // Désactiver les actions pour empêcher tout nouveau input
        if (moveAction != null) moveAction.Disable();
        if (lookAction != null) lookAction.Disable();
        if (jumpAction != null) jumpAction.Disable();
        if (sprintAction != null) sprintAction.Disable();
        
        // Figer la caméra dans sa position actuelle
        // (la rotation est déjà bloquée par le check isDead dans HandleMouseLook)
        
        // Animation de mort si elle existe
        if (animator != null)
        {
            animator.SetBool("walk", false);
            animator.SetBool("run", false);
            animator.SetTrigger("death"); // Trigger optionnel pour animation de mort
        }
        
        Debug.Log("Joueur mort - Caméra et mouvement bloqués");
    }
    
    // Méthode optionnelle pour réanimer le joueur
    public void OnPlayerRespawn()
    {
        isDead = false;
        
        // Réactiver les actions
        if (moveAction != null) moveAction.Enable();
        if (lookAction != null) lookAction.Enable();
        if (jumpAction != null) jumpAction.Enable();
        if (sprintAction != null) sprintAction.Enable();
        
        Debug.Log("Joueur réanimé - Contrôles réactivés");
    }
    
    void OnDestroy()
    {
        // Nettoyer les abonnements aux événements
        if (moveAction != null)
        {
            moveAction.performed -= ctx => moveInput = ctx.ReadValue<Vector2>();
            moveAction.canceled -= ctx => moveInput = Vector2.zero;
        }
        
        if (lookAction != null)
        {
            lookAction.performed -= ctx => lookInput = ctx.ReadValue<Vector2>();
            lookAction.canceled -= ctx => lookInput = Vector2.zero;
        }
        
        if (jumpAction != null)
        {
            jumpAction.performed -= OnJumpPerformed;
        }
        
        if (sprintAction != null)
        {
            sprintAction.performed -= ctx => isRunning = true;
            sprintAction.canceled -= ctx => isRunning = false;
        }
    }
}
