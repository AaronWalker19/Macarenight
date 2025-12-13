using UnityEngine;
using System.Collections;

public class lighttrifficAction : MonoBehaviour
{

    public static lighttrifficAction Instance;

    public Animator lighttrifficAnimator;


    public bool lightactive;

    public AudioSource lighttrifficAudioSource;

    public GameObject ApparenceGame;
    private CanvasGroup apparenceCanvasGroup;

    void Awake()
    {
        // setup singleton-like access so other scripts can call PlayLighttriffic()
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Debug.LogWarning("Multiple instances of lighttrifficAction detected. Using the first one.");
        }
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        lightactive = false;
        
        // Ajouter ou récupérer le CanvasGroup sur ApparenceGame
        if (ApparenceGame != null)
        {
            apparenceCanvasGroup = ApparenceGame.GetComponent<CanvasGroup>();
            if (apparenceCanvasGroup == null)
            {
                apparenceCanvasGroup = ApparenceGame.AddComponent<CanvasGroup>();
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void PlayLighttriffic()
    {
       StartCoroutine(PlayLighttrifficCoroutine());
    }
    private IEnumerator PlayLighttrifficCoroutine()
    {
        // Disable player movement during cutscene

        // Désactiver ApparenceGame via CanvasGroup (plus fiable pour UI)
        if (apparenceCanvasGroup != null)
        {
            Debug.Log($"Désactivation de {ApparenceGame.name} via CanvasGroup");
            apparenceCanvasGroup.alpha = 0f;
            apparenceCanvasGroup.interactable = false;
            apparenceCanvasGroup.blocksRaycasts = false;
        }
        else
        {
            Debug.LogError("CanvasGroup non trouvé sur ApparenceGame!");
        }

        Movesplayer2 player = FindObjectOfType<Movesplayer2>();
        if (player != null)
            player.enabled = false;


        // Switch to dedicated camera
        if (CameraLightScript.Instance != null)
            CameraLightScript.Instance.ActiveCamera(true);

        // Play animation
        if (lighttrifficAnimator != null)
            lighttrifficAnimator.SetTrigger("go");
            lightactive = true;


         enemieMovement.StopAllEnemies(true);

         yield return new WaitForSeconds(0.5f); // Attendre un peu avant de jouer le son
            lighttrifficAudioSource.Play();

        // Arrêter TOUS les ennemis
       

        // Wait for animation to complete
        yield return new WaitForSeconds(5f);

        // Restore cameras
        if (CameraLightScript.Instance != null)
            CameraLightScript.Instance.ActiveCamera(false);
         
        
        // Re-enable player movement
        if (player != null)
            player.enabled = true;

        // Redémarrer TOUS les ennemis
        enemieMovement.StopAllEnemies(false); 
        // Réactiver ApparenceGame via CanvasGroup
        if (apparenceCanvasGroup != null)
        {
            Debug.Log($"Réactivation de {ApparenceGame.name} via CanvasGroup");
            apparenceCanvasGroup.alpha = 1f;
            apparenceCanvasGroup.interactable = true;
            apparenceCanvasGroup.blocksRaycasts = true;
        }   Debug.Log($"État après réactivation: {ApparenceGame.activeSelf}");
        }
    }


