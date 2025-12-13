using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections;
using UnityEngine.InputSystem;

public class canvaEffect : MonoBehaviour
{
    // Référence à l'image à faire vaciller
    public Image img;
    public Image imgdeath;

    // Volume global et paramètres de film grain
    public Volume globalVolume;
    public float filmBase = 0.2f;
    public float filmAmplitude = 0.1f;
    public float filmSpeed = 2f;

    private FilmGrain filmGrain;
    private healt playerHealth;

    public GameObject textinteraction;
    



    public static canvaEffect Instance;

    public Animator deathAnimator;


    public GameObject Player;

    public GameObject Playerfake;

    public GameObject cameraEnd;

    public GameObject controlInterface;

    public GameObject pauseCanva;

    private bool isStarting;

     void Awake()
    {
        // setup singleton-like access so other scripts can call PlayLighttriffic()
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Debug.LogWarning("Multiple instances of canvaEffect detected. Using the first one.");
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        controlInterface.SetActive(true);
        textinteraction.SetActive(true);

        StartCoroutine(controlInterfaceActive());

        RenderSettings.fogStartDistance = 0f;
        RenderSettings.fogEndDistance = 12f;

        isStarting = false;


        if (img == null)
        {
            img = GetComponent<Image>();
        }

        // Si aucun Volume n'est assigné, on recherche un Volume global dans la scène
        if (globalVolume == null)
        {
            var vols = FindObjectsOfType<Volume>();
            foreach (var v in vols)
            {
                if (v.isGlobal)
                {
                    globalVolume = v;
                    break;
                }
            }
        }

        if (globalVolume != null && globalVolume.profile != null)
        {
            // Instancier le profile pour éviter de modifier l'asset shared (préserve l'original)
            globalVolume.profile = Instantiate(globalVolume.profile);

            // Récupérer le FilmGrain s'il existe
            if (!globalVolume.profile.TryGet<FilmGrain>(out filmGrain))
            {
                Debug.LogWarning("Aucun FilmGrain trouvé dans le VolumeProfile du Volume global.");
            }
            else
            {
                // On force l'override pour s'assurer que la valeur est prise en compte
                filmGrain.intensity.overrideState = true;
            }
        }
        else
        {
            Debug.LogWarning("Volume global introuvable ou profile manquant. Le film grain ne sera pas modifié.");
        }

        // Trouve le composant de vie du joueur (si présent dans la scène)
        playerHealth = FindObjectOfType<healt>();

        // Assure que l'image de la mort est invisible au départ
        if (imgdeath != null)
            imgdeath.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (img != null)
        {
            // Oscille l'alpha entre 0.1 et 0.4 avec une fonction sinus
            float alpha = Mathf.Lerp(0.1f, 0.2f, (Mathf.Sin(Time.time * 2f) + 1f) / 2f);
            Color c = img.color;
            c.a = alpha;
            img.color = c;
        }

        // Si la vie du joueur est à 0, afficher l'image de mort
        if (playerHealth != null && imgdeath != null)
        {
            if (playerHealth.GetCurrentHealth() <= 0)
                imgdeath.gameObject.SetActive(true);
            else
                imgdeath.gameObject.SetActive(false);
        }
        // Osciller l'intensité du film grain si trouvé
        if (filmGrain != null)
        {
            float intensity = filmBase + filmAmplitude * Mathf.Sin(Time.time * filmSpeed);
            intensity = Mathf.Clamp01(intensity);
            filmGrain.intensity.value = intensity;
        }


        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (pauseCanva != null && pauseCanva.activeSelf)
            {
                resumeGame();
            }
            else
            {
                pauseGame();
            }
        }

        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame && isStarting==false)
        {
            
            if (controlInterface != null)
        {
            controlInterface.SetActive(false);
        }
        textinteraction.SetActive(false);
            isStarting = true;
        }
    }


    private IEnumerator controlInterfaceActive()
    {
        StartCoroutine(BlinkTextInteraction());
        
        yield return new WaitForSeconds(10f);
        if (controlInterface != null)
        {
            controlInterface.SetActive(false);
        }
        textinteraction.SetActive(false);
        
        
    }

   



    public void SetDeathCanva()
    {
        StartCoroutine(startCourtienneDeath());
       
    }

    public IEnumerator startCourtienneDeath()
    {
        yield return new WaitForSeconds(2f);
        if (deathAnimator != null)
            deathAnimator.SetTrigger("death");
        else
            Debug.LogWarning("deathAnimator not assigned on canvaEffect.");

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
    }

    public void SetWinCanva(){
        StartCoroutine(startWinCanva());
    }

    public IEnumerator startWinCanva()
    {
        // Augmenter la distance du fog pour meilleure visibilité
        RenderSettings.fogStartDistance = 0f;
        RenderSettings.fogEndDistance = 30f;
        
        // Désactiver le joueur si assigné
        if (Player != null)
            Player.SetActive(false);

        

        if (Playerfake != null)
            Playerfake.SetActive(true);

        // Activer la caméra de fin et déclencher son animator si présents
        if (cameraEnd != null)
        {
            cameraEnd.SetActive(true);
            var cam = cameraEnd.GetComponent<Camera>();
            if (cam != null)
                cam.enabled = true;

            var camAnim = cameraEnd.GetComponent<Animator>();
            if (camAnim != null)
                camAnim.SetTrigger("end");
        }

        yield return new WaitForSeconds(5f);
        if (deathAnimator != null)
            deathAnimator.SetTrigger("win");
        else
            Debug.LogWarning("deathAnimator not assigned on canvaEffect.");

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
    }


      IEnumerator BlinkTextInteraction()
    {
        var image = textinteraction.GetComponent<UnityEngine.UI.Image>();
        var text = textinteraction.GetComponent<UnityEngine.UI.Text>();
        var textMeshPro = textinteraction.GetComponent<TMPro.TextMeshProUGUI>();
        
        while (textinteraction.activeInHierarchy)
        {
            // Varier l'opacité entre 70/255 et 255/255
            float alpha = Mathf.Lerp(70f / 255f, 1f, (Mathf.Sin(Time.time * 2f) + 1f) / 2f);
            
            if (image != null)
            {
                Color c = image.color;
                c.a = alpha;
                image.color = c;
            }
            
            if (text != null)
            {
                Color c = text.color;
                c.a = alpha;
                text.color = c;
            }
            
            if (textMeshPro != null)
            {
                Color c = textMeshPro.color;
                c.a = alpha;
                textMeshPro.color = c;
            }
            
            yield return null;
        }
    }


    public void RestartLevel()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }

    public void MainMenu()
    {
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene("start");
    }


    public void pauseGame()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        pauseCanva.SetActive(true);
        Time.timeScale = 0f;

    }

    public void resumeGame()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Time.timeScale = 1f;
        pauseCanva.SetActive(false);
    }
}
