using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections;
using UnityEngine.InputSystem;

public class startGameeffect : MonoBehaviour
{

    // Volume global et paramètres de film grain
    
    public float filmBase = 0.2f;
    public float filmAmplitude = 0.1f;
    public float filmSpeed = 2f;

    private FilmGrain filmGrain;

    public GameObject apparence;

    public Animator cameraAnimator;
  
    public static startGameeffect Instance;

    public GameObject explication;
    public GameObject textinteraction;

    public GameObject fonduNoir;

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
            Debug.LogWarning("Multiple instances of startGameeffect detected. Using the first one.");
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        isStarting = false;
       
 
    }

    // Update is called once per frame
    void Update()
    {
       
        // Osciller l'intensité du film grain si trouvé
        if (filmGrain != null)
        {
            float intensity = filmBase + filmAmplitude * Mathf.Sin(Time.time * filmSpeed);
            intensity = Mathf.Clamp01(intensity);
            filmGrain.intensity.value = intensity;
        }

        // Détecter la touche E avec le nouveau Input System
        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame && isStarting==true)
        {
            
            StartCoroutine(startContinue());
        }
    }


    public void ButtonStart()
    {
        StartCoroutine(LoadMainScene());
    }


    IEnumerator LoadMainScene()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        // Attendre un court instant avant de charger la scène principale
        apparence.SetActive(false);
        cameraAnimator.SetTrigger("start");
        yield return new WaitForSeconds(1.5f);
        explication.SetActive(true);
        isStarting = true;
        yield return new WaitForSeconds(10f);
        textinteraction.SetActive(true);
        StartCoroutine(BlinkTextInteraction());

        
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


    public void QuitGame()
    {
        Application.Quit();
    }

    private IEnumerator startContinue()
    {
        if (fonduNoir != null)
        {
            fonduNoir.SetActive(true);
            
            // Récupérer le composant Image ou CanvasGroup
            var image = fonduNoir.GetComponent<UnityEngine.UI.Image>();
            var canvasGroup = fonduNoir.GetComponent<CanvasGroup>();
            
            // Animer l'opacité de 0 à 255 progressivement
            float duration = 3f;
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Clamp01(elapsed / duration);
                
                if (image != null)
                {
                    Color c = image.color;
                    c.a = alpha;
                    image.color = c;
                }
                
                if (canvasGroup != null)
                {
                    canvasGroup.alpha = alpha;
                }
                
                yield return null;
            }
            
            // S'assurer que l'opacité est à 100% à la fin
            if (image != null)
            {
                Color c = image.color;
                c.a = 1f;
                image.color = c;
            }
            
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
            }
        }
        else
        {
            yield return new WaitForSeconds(3f);
        }
        
        UnityEngine.SceneManagement.SceneManager.LoadScene("Scene_A");
    }
}
