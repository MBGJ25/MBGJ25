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
    #endregion

    private void Awake()
    {
        playerObject = GameObject.Find("Player Character").transform;
        agent = GetComponent<NavMeshAgent>();
        player = playerObject.GetComponent<Player>();
        
        // Initialize health
        CurrentHealth = stats.MaxHealth;
    }

    private void Update()
    {
        if (!IsAlive) return; // Don't update AI if dead

        //Check for sight and attack range
        playerInSightRange = Physics.CheckSphere(transform.position, sightRange, whatIsPlayer);
        playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, whatIsPlayer);

        if (!playerInSightRange && !playerInAttackRange) Patroling();
        if (playerInSightRange && !playerInAttackRange) ChasePlayer();
        if (playerInAttackRange && playerInSightRange) AttackPlayer();
    }

    #region AI Behavior
    private void Patroling()
    {
        if (!walkPointSet) SearchWalkPoint();

        if (walkPointSet)
            agent.SetDestination(walkPoint);

        Vector3 distanceToWalkPoint = transform.position - walkPoint;

        //Walkpoint reached
        if (distanceToWalkPoint.magnitude < 1f)
            walkPointSet = false;
    }

    private void SearchWalkPoint()
    {
        //Calculate random point in range
        float randomZ = UnityEngine.Random.Range(-walkPointRange, walkPointRange);
        float randomX = UnityEngine.Random.Range(-walkPointRange, walkPointRange);

        walkPoint = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);

        if (Physics.Raycast(walkPoint, -transform.up, 2f, whatIsGround))
            walkPointSet = true;
    }

    private void ChasePlayer()
    {
        agent.SetDestination(playerObject.position);
    }

    // CS TODO: Use this one if we want projectiles
    private void ProjectileAttackPlayer()
    {
        //Make sure enemy doesn't move
        agent.SetDestination(transform.position);

        transform.LookAt(playerObject);

        if (!alreadyAttacked)
        {
            // CS TODO: Add call to take damage here and use Stats.AttackDamage
            ///Attack code here
            Rigidbody rb = Instantiate(projectile, transform.position, Quaternion.identity).GetComponent<Rigidbody>();
            rb.AddForce(transform.forward * 32f, ForceMode.Impulse);
            rb.AddForce(transform.up * 8f, ForceMode.Impulse);
            ///End of attack code

            alreadyAttacked = true;
            Invoke(nameof(ResetAttack), timeBetweenAttacks);
        }
    }
    
    private void AttackPlayer()
    {
        agent.SetDestination(transform.position);
        transform.LookAt(playerObject);

        if (!alreadyAttacked)
        {
            // CS TODO: Add any attack effects we'd like
            // Deal damage directly to player
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
        
        // Disable AI
        agent.enabled = false;
        
        // CS TODO: Add death animation and any eventing we need here
        // animator.SetTrigger("Death");
        
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
    }
}