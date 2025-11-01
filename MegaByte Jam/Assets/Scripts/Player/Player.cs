using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private PlayerStats stats;
    public bool IsAlive => stats.IsAlive;
    public int CurrentHealth => stats.CurrentHealth;

    private void Awake()
    {
        stats.OnDeath += HandleDeath;
    }

    private void OnDestroy()
    {
        if (stats != null)
        {
            stats.OnDeath -= HandleDeath;
        }
    }

    #region Health Bridge Methods
    public void TakeDamage(int damage)
    {
        stats.TakeDamage(damage);
    }

    public void Heal(int amount)
    {
        stats.Heal(amount);
    }
    #endregion

    #region Death Handling
    private void HandleDeath()
    {
        // CS TODO: Add Player-specific death behavior
        // - Disable player controller
        // - Play death animation
        // - Trigger game over
        
        var controller = GetComponent<PlayerController>();
        if (controller != null)
        {
            controller.enabled = false;
        }
        
        Debug.Log("Player died!");
        // CS TODO: Add call to game manager or respawn here
    }
    #endregion

    #region Combat (Optional)
    // CS TODO: Fill out
    public void Attack()
    {
        // Use stats.AttackDamage
        // Use stats.AttackCooldown
    }
    #endregion
}
