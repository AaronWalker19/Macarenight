
using UnityEngine;
using UnityEngine.UI;

public class healt : MonoBehaviour
{
    [Header("Mesh à placer au sol lors de la mort")]
    public GameObject meshObject; // À assigner dans l'inspecteur
    public Slider healthBar; // À assigner dans l'inspecteur
    public int maxHealth = 100;
    private int currentHealth;
    private Animator animator;
    private Movesplayer2 movesplayer;
    private bool isDead = false;

    public Image firstbatterie;

    void Start()
    {
        currentHealth = maxHealth;
        animator = GetComponent<Animator>();
        movesplayer = GetComponent<Movesplayer2>();
        if (healthBar != null)
        {
            healthBar.maxValue = maxHealth;
            healthBar.value = currentHealth;
        }
        if (firstbatterie != null)
            firstbatterie.gameObject.SetActive(true);
    }

    // Fonction pour soigner
    public void Heal(int amount)
    {
        if (isDead) return;
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        UpdateHealthBar();
    }

    // Fonction pour infliger des dégâts
    public void Damage(int amount)
    {
        if (isDead) return;
        int oldHealth = currentHealth;
        currentHealth = Mathf.Max(currentHealth - amount, 0);
        UpdateHealthBar();
        if (currentHealth < oldHealth && currentHealth > 0)
        {
            
            // Effet de secousse de caméra si Movesplayer existe
            if (movesplayer != null)
                movesplayer.CameraShake();
        }
        if (currentHealth == 0 && !isDead)
        {
            isDead = true;
            if (animator != null)
                animator.SetBool("death", true);
            if (movesplayer != null)
                movesplayer.enabled = false;

            // Déverrouiller et afficher le curseur pour permettre l'interaction avec l'UI de mort
            

            if(canvaEffect.Instance != null)
            {
                canvaEffect.Instance.SetDeathCanva();
            }
            // Rend le mesh invisible
            if (meshObject != null)
            {
                Renderer meshRenderer = meshObject.GetComponent<Renderer>();
                if (meshRenderer != null)
                    meshRenderer.enabled = false;
            }

            // Ajuste le CapsuleCollider pour coller au sol
            CapsuleCollider capsule = GetComponent<CapsuleCollider>();
            if (capsule != null)
            {
                capsule.height = 0.5f; // Hauteur réduite
                capsule.center = new Vector3(0, 0.25f, 0); // Centre plus bas
                capsule.radius = 0.5f; // Rayon ajusté si besoin
            }

            // Place le mesh au niveau du sol
            if (meshObject != null)
            {
                RaycastHit hit;
                Vector3 meshPos = meshObject.transform.position;
                if (Physics.Raycast(meshPos + Vector3.up, Vector3.down, out hit, 10f))
                {
                    meshPos.y = hit.point.y;
                    meshObject.transform.position = meshPos;
                }
            }

            // Oriente la caméra vers le haut au ras du sol
                // Effet de chute de la caméra au sol
                if (movesplayer != null && movesplayer.playerCamera != null)
                {
                    StartCoroutine(FallCameraToGround(movesplayer.playerCamera));
                }
        }
    }

    private void UpdateHealthBar()
    {
        if (healthBar != null)
        {
            healthBar.value = currentHealth;
        }
        if (firstbatterie != null)
        {
            // show batterie UI only when at full health, hide otherwise
            if (currentHealth < maxHealth)
                firstbatterie.gameObject.SetActive(false);
            else
                firstbatterie.gameObject.SetActive(true);
        }

    }
    

    // Coroutine pour animer la chute de la caméra au sol
    private System.Collections.IEnumerator FallCameraToGround(Camera cam)
    {
        Vector3 startPos = cam.transform.position;
        Vector3 endPos = transform.position;
        endPos.y += 0.1f; // ras du sol
        Quaternion startRot = cam.transform.localRotation;
        Quaternion endRot = Quaternion.Euler(-30f, 0f, 0f); // regarde vers le haut
        float duration = 0.5f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            cam.transform.position = Vector3.Lerp(startPos, endPos, elapsed / duration);
            cam.transform.localRotation = Quaternion.Slerp(startRot, endRot, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        cam.transform.position = endPos;
        cam.transform.localRotation = endRot;
    }

    // Permet de lire les PV actuels depuis un autre script
    public int GetCurrentHealth()
    {
        return currentHealth;
    }
}
