using UnityEngine;
using System.Collections;

public class OpenDoorScript : MonoBehaviour
{

    public GameObject[] lighttrifficObjects;

    public Animator doorAnimator;
    bool doorOpened = false;

    public AudioSource doorAudioSource;

    public static OpenDoorScript Instance;

    void Awake()
    {
        // setup singleton-like access so other scripts can call PlayLighttriffic()
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Debug.LogWarning("Multiple instances of OpenDoorScript detected. Using the first one.");
        }
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }
    
    void Update()
    {
        if (doorOpened) return;

        if (lighttrifficObjects == null || lighttrifficObjects.Length == 0)
            return;

        // Check that every referenced object has a lighttrifficAction with lightactive == true
        bool allActive = true;
        foreach (var go in lighttrifficObjects)
        {
            if (go == null)
            {
                allActive = false;
                break;
            }

            var l = go.GetComponent<lighttrifficAction>();
            if (l == null || !l.lightactive)
            {
                allActive = false;
                break;
            }
        }

        if (allActive && doorAnimator != null)
        {
            doorAnimator.SetTrigger("opendoor");
            doorOpened = true;

            StartCoroutine(PlayDoorOpenSound());
        }
       
    }


    private IEnumerator PlayDoorOpenSound()
    {
        yield return new WaitForSeconds(4f);
        if (doorAudioSource != null)
        {
            doorAudioSource.Play();
        }
        yield return null;
    }
}

