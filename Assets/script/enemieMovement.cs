using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class enemieMovement : MonoBehaviour
{
    public Transform[] goals; // Points de passage assignables dans l'inspecteur
    public Transform player;
    public Animator animator; // À assigner dans l'inspecteur

    private NavMeshAgent agent;
    private int currentGoalIndex = 0;
    public bool chasingPlayer = false;
    private bool isStopped = false; // Flag pour arrêter complètement l'ennemi

    [Header("Combat")]
    public float attackRange = 1.5f; // distance à laquelle l'ennemi s'arrête pour attaquer

    public float distanceDetectPlayer = 30f;
    private float attackCooldown = 1.0f; // 1 seconde entre attaques
    private float lastAttackTime = -999f;

    public static enemieMovement Instance;
    private static List<enemieMovement> allEnemies = new List<enemieMovement>();


    void Awake()
    {
        // setup singleton-like access so other scripts can call StopEnnemie()
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Debug.LogWarning("Multiple instances of enemieMovement detected. Using the first one.");
        }
        
        // Ajouter cet ennemi à la liste globale
        if (!allEnemies.Contains(this))
        {
            allEnemies.Add(this);
        }
    }
    
    void OnDestroy()
    {
        // Retirer cet ennemi de la liste quand il est détruit
        if (allEnemies.Contains(this))
        {
            allEnemies.Remove(this);
        }
    }
    
    // Méthode statique pour arrêter TOUS les ennemis
    public static void StopAllEnemies(bool stop)
    {
        foreach (var enemy in allEnemies)
        {
            if (enemy != null)
            {
                enemy.StopEnnemie(stop);
            }
        }
    }

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        if (goals != null && goals.Length > 0)
        {
            agent.destination = goals[currentGoalIndex].position;
        }
        // Configurer la distance d'arrêt pour éviter de traverser le joueur
        if (agent != null)
        {
            agent.stoppingDistance = attackRange;
        }
    }

    void Update()
    {
        // Si l'ennemi est arrêté, ne rien faire
        if (isStopped) return;
        
        if (goals == null || goals.Length == 0 || player == null) return;

        healt playerHealth = player.GetComponent<healt>();
        if (playerHealth == null || playerHealth == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // Si le joueur n'a plus de PV, l'ennemi arrête la poursuite
        if (playerHealth != null && playerHealth.GetCurrentHealth() <= 0)
        {
            if (chasingPlayer)
            {
                chasingPlayer = false;
                agent.destination = goals[currentGoalIndex].position;
            }
            return;
        }

            // Attaque si à portée d'attaque (attackRange)
            if (distanceToPlayer <= attackRange && Time.time - lastAttackTime > attackCooldown)
            {
                lastAttackTime = Time.time;
                // Stopper l'agent pour éviter qu'il n'entre dans le joueur pendant l'animation
                if (agent != null)
                {
                    agent.isStopped = true;
                }

                if (animator != null)
                {
                    animator.SetTrigger("attack");
                }
                // Lance la coroutine pour appliquer le hit après un délai
                StartCoroutine(AttackWithDelay(playerHealth));
            }

            if (!chasingPlayer && distanceToPlayer <= distanceDetectPlayer)
            {
                // Commence à poursuivre le joueur
                chasingPlayer = true;
                agent.destination = player.position;
            }
            else if (chasingPlayer)
            {
                if (distanceToPlayer > 30f)
                {
                    // Reprend le parcours
                    chasingPlayer = false;
                    agent.destination = goals[currentGoalIndex].position;
                }
                else
                {
                    // Continue à poursuivre le joueur
                    agent.destination = player.position;
                }
            }
            else
            {
                // Parcours normal des goals
                if (!agent.pathPending && agent.remainingDistance < 0.5f)
                {
                    currentGoalIndex = (currentGoalIndex + 1) % goals.Length;
                    agent.destination = goals[currentGoalIndex].position;
                }
            }
    }

    // Coroutine pour synchroniser le hit avec l'animation
    private System.Collections.IEnumerator AttackWithDelay(healt playerHealth)
    {
        // Attendre la fenêtre d'impact de l'animation
        yield return new WaitForSeconds(0.3f); // Ajuster selon l'animation
        if (playerHealth != null && playerHealth.GetCurrentHealth() > 0)
        {
            playerHealth.Damage(10);
        }

        // Rester arrêté pendant 4 secondes après l'attaque
        float stopDuration = 4f;
        float elapsed = 0f;
        while (elapsed < stopDuration)
        {
            // Si l'ennemi a été globalement stoppé via StopEnnemie(), on sort et ne redémarre pas
            if (isStopped)
                yield break;

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Après le délai, redémarrer l'agent si l'ennemi n'est pas globalement arrêté
        if (!isStopped && agent != null)
        {
            agent.isStopped = false;
            if (player != null)
                agent.destination = player.position;
        }
    }

    public void StopEnnemie(bool stop)
    {
        isStopped = stop;
        
        if (stop)
        {
            // Arrêter complètement l'agent
            if (agent != null)
            {
                agent.isStopped = true;
                agent.ResetPath(); // Efface le chemin actuel
            }
            
            
            chasingPlayer = false;
            
            if (animator != null)
            {
                animator.SetBool("isWalking", false);
            }
        }
        else
        {

            // Redémarrer l'ennemi
            if (agent != null)
            {
                agent.isStopped = false;
                // Redéfinir la destination vers le goal actuel
                if (goals != null && goals.Length > 0)
                {
                    agent.destination = goals[currentGoalIndex].position;
                }
            }
            
            if (animator != null)
            {
                animator.SetBool("isWalking", true);
            }
        }
    }

}
    
    