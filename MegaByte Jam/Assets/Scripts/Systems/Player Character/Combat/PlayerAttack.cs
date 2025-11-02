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
    [SerializeField] private Transform attackPoint; // Assign a child object as attack origin
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
    public event Action<int> OnAttackPerformed; // Passes combo count
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
        
        // Update combo count
        if (Time.time - lastComboTime <= stats.ComboWindow)
        {
            currentComboCount = Mathf.Min(currentComboCount + 1, stats.MaxComboCount);
        }
        else
        {
            currentComboCount = 1; // Start new combo
        }
        lastComboTime = Time.time;
        
        Debug.Log($"Player attacking! Combo: {currentComboCount}");
        
        // Invoke event
        OnAttackPerformed?.Invoke(currentComboCount);
        
        // Visual/Audio feedback
        PlayAttackFeedback();
        
        // Detect and damage enemies
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
        
        foreach (Collider enemy in hitEnemies)
        {
            Zombie zombie = enemy.GetComponent<Zombie>();
            if (zombie != null && zombie.IsAlive)
            {
                // Calculate damage (could scale with combo in the future)
                int damage = CalculateDamage();
                
                zombie.TakeDamage(damage);
                OnEnemyHit?.Invoke(zombie);
                
                Debug.Log($"Hit {enemy.name} for {damage} damage! Combo: {currentComboCount}");
            }
        }
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
        if (attackSound != null && attackSound != null)
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