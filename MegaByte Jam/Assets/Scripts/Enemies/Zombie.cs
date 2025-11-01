using System;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

public class Zombie : MonoBehaviour
{
    #region Health System
    [Header("Stats")]
    [SerializeField] private EnemyStats stats;
    
    public event Action<int> OnDamageTaken;
    public event Action OnDeath;

    public int CurrentHealth { get; private set; }
    public bool IsAlive => CurrentHealth > 0;
    public float HealthPercentage => (float)CurrentHealth / stats.MaxHealth;
    #endregion

    #region AI Components
    [Header("AI Components")]
    public NavMeshAgent agent;
    [FormerlySerializedAs("player")]
    public Transform playerObject;
    public LayerMask whatIsGround, whatIsPlayer;
    private Player player;
    private PlayerInteraction playerInteraction; // ✨ NEW
    #endregion

    #region Patrolling
    [Header("Patrolling")]
    public Vector3 walkPoint;
    bool walkPointSet;
    public float walkPointRange;
    #endregion

    #region Attacking
    [Header("Attacking")]
    public float timeBetweenAttacks;
    bool alreadyAttacked;
    public GameObject projectile;
    #endregion

    #region States
    [Header("Detection")]
    public float sightRange, attackRange;
    public bool playerInSightRange, playerInAttackRange;
    
    // ✨ NEW: Lantern fear settings
    [Header("Lantern Avoidance")]
    [SerializeField] private float fleeDistance = 15f; // How far to flee from player
    [SerializeField] private float fleeSpeed = 5f; // Speed when fleeing (can be faster than normal)
    private float normalSpeed; // Store original speed
    #endregion

    private void Awake()
    {
        playerObject = GameObject.Find("Player Character").transform;
        agent = GetComponent<NavMeshAgent>();
        player = playerObject.GetComponent<Player>();
        playerInteraction = playerObject.GetComponent<PlayerInteraction>(); // ✨ NEW
        
        // ✨ NEW: Store original speed
        normalSpeed = agent.speed;
        
        // Initialize health
        CurrentHealth = stats.MaxHealth;
    }

    private void Update()
    {
        if (!IsAlive) return;

        // Check for sight and attack range
        playerInSightRange = Physics.CheckSphere(transform.position, sightRange, whatIsPlayer);
        playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, whatIsPlayer);

        // ✨ NEW: Check if player has lit lantern
        bool playerHasLitLantern = playerInteraction != null && playerInteraction.HasLitLantern;

        // ✨ MODIFIED: AI behavior based on lantern status
        if (playerHasLitLantern && playerInSightRange)
        {
            // Zombie is afraid! Flee from player
            FleeFromPlayer();
        }
        else
        {
            // Normal behavior - not afraid
            if (!playerInSightRange && !playerInAttackRange) Patroling();
            if (playerInSightRange && !playerInAttackRange) ChasePlayer();
            if (playerInAttackRange && playerInSightRange) AttackPlayer();
        }
    }

    #region AI Behavior
    private void Patroling()
    {
        agent.speed = normalSpeed; // Ensure normal speed
        
        if (!walkPointSet) SearchWalkPoint();

        if (walkPointSet)
            agent.SetDestination(walkPoint);

        Vector3 distanceToWalkPoint = transform.position - walkPoint;

        if (distanceToWalkPoint.magnitude < 1f)
            walkPointSet = false;
    }

    private void SearchWalkPoint()
    {
        float randomZ = UnityEngine.Random.Range(-walkPointRange, walkPointRange);
        float randomX = UnityEngine.Random.Range(-walkPointRange, walkPointRange);

        walkPoint = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);

        if (Physics.Raycast(walkPoint, -transform.up, 2f, whatIsGround))
            walkPointSet = true;
    }

    private void ChasePlayer()
    {
        agent.speed = normalSpeed; // Ensure normal speed
        agent.SetDestination(playerObject.position);
    }

    // ✨ NEW: Flee behavior when player has lit lantern
    private void FleeFromPlayer()
    {
        agent.speed = fleeSpeed; // Can flee faster if you want
        
        // Calculate direction away from player
        Vector3 directionAwayFromPlayer = (transform.position - playerObject.position).normalized;
        
        // Calculate flee destination
        Vector3 fleeDestination = transform.position + directionAwayFromPlayer * fleeDistance;
        
        // Make sure flee point is on the ground
        NavMeshHit hit;
        if (NavMesh.SamplePosition(fleeDestination, out hit, fleeDistance, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
        else
        {
            // If can't find valid flee point, just move away in any direction
            agent.SetDestination(transform.position + directionAwayFromPlayer * 5f);
        }
        
        // Optional: Look at player while fleeing (shows fear/awareness)
        transform.LookAt(new Vector3(playerObject.position.x, transform.position.y, playerObject.position.z));
    }

    private void AttackPlayer()
    {
        agent.speed = normalSpeed; // Ensure normal speed
        agent.SetDestination(transform.position);
        transform.LookAt(playerObject);

        if (!alreadyAttacked)
        {
            if (player != null)
            {
                player.TakeDamage(stats.AttackDamage);
            }

            alreadyAttacked = true;
            Invoke(nameof(ResetAttack), timeBetweenAttacks);
        }
    }

    private void ResetAttack()
    {
        alreadyAttacked = false;
    }
    #endregion

    #region Health System
    public void TakeDamage(int damage)
    {
        if (!IsAlive || damage <= 0) return;

        int actualDamage = Mathf.Min(damage, CurrentHealth);
        CurrentHealth -= actualDamage;
        
        OnDamageTaken?.Invoke(actualDamage);

        if (CurrentHealth <= 0)
        {
            CurrentHealth = 0;
            Die();
        }
    }

    private void Die()
    {
        OnDeath?.Invoke();
        agent.enabled = false;
        Invoke(nameof(DestroyEnemy), 0.5f);
    }

    private void DestroyEnemy()
    {
        Destroy(gameObject);
    }
    #endregion

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);
        
        // ✨ NEW: Show flee distance
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, fleeDistance);
    }
}