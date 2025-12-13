using UnityEngine;
using UnityEngine.Events;

public class ActiveGoodEnding : MonoBehaviour
{
    public BoxCollider triggerZone;
    public string playerTag = "Player";
    public bool triggerOnce = true; // si vrai, ne s'active qu'une seule fois

    public UnityEvent onEnter;
    public UnityEvent onExit;

    bool hasTriggered = false;

    void Start()
    {
        if (triggerZone == null)
        {
            triggerZone = GetComponent<BoxCollider>();
            if (triggerZone == null)
                Debug.LogWarning("ActiveGoodEnding: aucun BoxCollider assigné ni trouvé sur l'objet.");
        }

        if (triggerZone != null && !triggerZone.isTrigger)
            Debug.LogWarning("ActiveGoodEnding: le BoxCollider n'est pas en mode Trigger. Activez 'Is Trigger' pour que les événements fonctionnent.");
    }

    void OnTriggerEnter(Collider other)
    {
        if (hasTriggered && triggerOnce) return;
        if (!other.CompareTag(playerTag)) return;

        Debug.Log($"ActiveGoodEnding: player entered trigger on {gameObject.name}");
        onEnter?.Invoke();

        if (triggerOnce)
        {
            hasTriggered = true;
            // Optionnel: désactiver le collider pour éviter d'autres déclenchements
            if (triggerZone != null) triggerZone.enabled = false;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        Debug.Log($"ActiveGoodEnding: player exited trigger on {gameObject.name}");
        onExit?.Invoke();
    }
}
