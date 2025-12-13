using UnityEngine;
using UnityEngine.UI;

public class RunActionScript : MonoBehaviour
{
    public Image runimage;
    public Slider runslider;
    
    [Header("Stamina Settings")]
    public float maxStamina = 100f;
    public float staminaDrainRate = 30f; 
    public float staminaRegenRate = 10f; 
    public float regenDelay = 3f; 
    
    public float currentStamina;
    private float timeSinceLastSprint = 0f;
    private bool isRegenerating = false;
    private bool isSprinting = false; // Track si on est en train de sprinter
    
    public static RunActionScript Instance;
    
    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
            Debug.LogWarning("Multiple RunActionScript instances detected.");
    }
    
    void Start()
    {
        currentStamina = maxStamina;
        UpdateUI();
    }
    
    void Update()
    {
        // Gérer la régénération de stamina
        if (!isSprinting && currentStamina < maxStamina)
        {
            timeSinceLastSprint += Time.deltaTime;
            
            // Si le délai est passé, commencer la régénération
            if (timeSinceLastSprint >= regenDelay)
            {
                isRegenerating = true;
                currentStamina += staminaRegenRate * Time.deltaTime;
                currentStamina = Mathf.Min(currentStamina, maxStamina);
                UpdateUI();
            }
        }
    }
    
    // Appelé par Movesplayer2 pour consommer la stamina pendant le sprint
    public bool TryConsumeStamina(float deltaTime)
    {
        if (currentStamina <= 0f)
        {
            isSprinting = false;
            return false; // Pas assez de stamina pour sprinter
        }
        
        isSprinting = true;
        currentStamina -= staminaDrainRate * deltaTime;
        currentStamina = Mathf.Max(currentStamina, 0f);
        
        // Réinitialiser le timer de régénération
        timeSinceLastSprint = 0f;
        isRegenerating = false;
        
        UpdateUI();
        return true;
    }
    
    // Appelé quand le joueur arrête de sprinter
    public void StopSprinting()
    {
        isSprinting = false;
    }
    
    // Vérifie si le joueur est en train de sprinter (appelé depuis Movesplayer2)
    private bool IsRunning()
    {
        return isSprinting;
    }
    
    public bool CanSprint()
    {
        return currentStamina > 0f;
    }
    
    void UpdateUI()
    {
        float staminaPercent = currentStamina / maxStamina;
        
        // Mettre à jour le slider
        if (runslider != null)
        {
            runslider.value = staminaPercent;
        }
        
        // Mettre à jour l'opacité de l'image (max 70/255 = 0.275)
        if (runimage != null)
        {
            Color c = runimage.color;
            c.a = staminaPercent * (70f / 255f); // Opacité limitée à 70/255
            runimage.color = c;
        }
    }
    
    // Méthode publique pour obtenir le pourcentage de stamina (optionnel)
    public float GetStaminaPercent()
    {
        return currentStamina / maxStamina;
    }
}
