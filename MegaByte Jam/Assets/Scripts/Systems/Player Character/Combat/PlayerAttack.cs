using System;
using UnityEngine;
using PhysicsCharacterController;

public class PlayerAttack : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputReader inputReader;
    
    [Header("Stats")]
    [SerializeField] private PlayerStats stats;
    
    [Header("Attack Settings")]
    [SerializeField] private Transform attackPoint;
    [SerializeField] private LayerMask enemyLayer;
    
    [Header("Visual Feedback (Optional)")]
    [SerializeField] private ParticleSystem attackEffect;
    [SerializeField] private AudioClip attackSound;
    
    // Attack state
    private bool canAttack = true;
    private float lastAttackTime;
    
    // Combo tracking (for future extension)
    private int currentComboCount = 0;
    private float lastComboTime;
    
    // Events
    public event Action<int> OnAttackPerformed;
    public event Action<Zombie> OnEnemyHit;
    
    // Public properties
    public int CurrentComboCount => currentComboCount;
    public bool CanAttack => canAttack;

    #region Lifecycle Methods

    private void OnEnable()
    {
        if (inputReader != null)
        {
            inputReader.AttackEvent += HandleOnAttackEvent;
        }
    }

    private void OnDisable()
    {
        if (inputReader != null)
        {
            inputReader.AttackEvent -= HandleOnAttackEvent;
        }
    }
    
    private void Update()
    {
        UpdateComboTimer();
    }

    #endregion

    #region Event Handlers

    private void HandleOnAttackEvent()
    {
        if (canAttack)
        {
            PerformAttack();
        }
        else
        {
            Debug.Log("Attack on cooldown!");
        }
    }

    #endregion
    
    #region Attack Logic
    
    private void PerformAttack()
    {
        canAttack = false;
        lastAttackTime = Time.time;
        
        Debug.Log($"Player attacking!");
        
        // Visual/Audio feedback
        PlayAttackFeedback();
        
        // Detect and damage enemies (this will handle combo incrementing)
        DetectAndDamageEnemies();
        
        // Reset attack cooldown
        Invoke(nameof(ResetAttack), stats.AttackCooldown);
    }
    
    private void DetectAndDamageEnemies()
    {
        if (attackPoint == null)
        {
            Debug.LogWarning("Attack Point not assigned!");
            return;
        }
        
        // Use OverlapSphere for melee detection
        Collider[] hitEnemies = Physics.OverlapSphere(
            attackPoint.position, 
            stats.AttackRange, 
            enemyLayer
        );
        
        if (hitEnemies.Length > 0)
        {
            Debug.Log($"Detected {hitEnemies.Length} potential targets");
        }
        
        bool hitAnyEnemy = false;
        
        foreach (Collider enemy in hitEnemies)
        {
            Zombie zombie = enemy.GetComponent<Zombie>();
            if (zombie != null && zombie.IsAlive)
            {
                int damage = CalculateDamage();
                zombie.TakeDamage(damage);
                OnEnemyHit?.Invoke(zombie);
                
                hitAnyEnemy = true;
                
                #if UNITY_EDITOR
                Debug.Log($"Hit {enemy.name} for {damage} damage!");
                #endif
            }
        }
        
        // Only increment combo if we actually hit an enemy
        if (hitAnyEnemy)
        {
            IncrementCombo();
        }
        else
        {
            Debug.Log("Attack missed - no combo increment");
        }
    }
    
    private void IncrementCombo()
    {
        if (Time.time - lastComboTime <= stats.ComboWindow)
        {
            currentComboCount = Mathf.Min(currentComboCount + 1, stats.MaxComboCount);
        }
        else
        {
            currentComboCount = 1;
        }
        lastComboTime = Time.time;
        
        #if UNITY_EDITOR
        Debug.Log($"Combo: {currentComboCount}");
        #endif
        
        // Invoke event with updated combo count
        OnAttackPerformed?.Invoke(currentComboCount);
    }
    
    private int CalculateDamage()
    {
        // Base damage for now
        int damage = stats.AttackDamage;
        
        // Example combo scaling (uncomment to enable):
        // damage += (currentComboCount - 1) * 5;
        
        return damage;
    }
    
    private void PlayAttackFeedback()
    {
        // Play particle effect
        if (attackEffect != null)
        {
            attackEffect.Play();
        }
        
        // Play sound
        if (attackSound != null)
        {
            AudioSource.PlayClipAtPoint(attackSound, transform.position);
        }
    }
    
    private void ResetAttack()
    {
        canAttack = true;
    }
    
    private void UpdateComboTimer()
    {
        // Reset combo if window expires
        if (Time.time - lastComboTime > stats.ComboWindow && currentComboCount > 0)
        {
            Debug.Log("Combo reset!");
            currentComboCount = 0;
        }
    }
    
    #endregion
    
    #region Debug Visualization
    
    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, stats.AttackRange);
    }
    
    #endregion
}