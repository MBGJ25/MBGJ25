using System;
using UnityEngine;

[CreateAssetMenu(fileName = "Player Stats", menuName = "Scriptable Objects/State/New Player Stats")]
public class PlayerStats : ScriptableObject 
{
    #region Events
    public event Action<int> OnDamageTaken;
    public event Action<int> OnHealed;
    public event Action OnDeath;
    public event Action OnHealthChanged;
    #endregion

    #region Serialized Fields
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int attackDamage = 10;
    [SerializeField] private float attackCooldown = 0.5f;
    #endregion

    #region Properties
    public int MaxHealth => maxHealth;
    public int CurrentHealth { get; private set; }
    public bool IsAlive => CurrentHealth > 0;
    public float HealthPercentage => (float)CurrentHealth / maxHealth;
    public int AttackDamage => attackDamage;
    public float AttackCooldown => attackCooldown;
    #endregion

    #region Initialization
    private void OnEnable()
    {
        ResetHealth();
    }

    public void ResetHealth()
    {
        CurrentHealth = maxHealth;
    }
    #endregion

    #region Health Management
    public void TakeDamage(int damage)
    {
        if (!IsAlive || damage <= 0) return;

        int actualDamage = Mathf.Min(damage, CurrentHealth);
        CurrentHealth -= actualDamage;
        
        OnDamageTaken?.Invoke(actualDamage);
        OnHealthChanged?.Invoke();

        if (CurrentHealth <= 0)
        {
            CurrentHealth = 0;
            OnDeath?.Invoke();
        }
    }

    public void Heal(int amount)
    {
        if (!IsAlive || amount <= 0) return;

        int actualHeal = Mathf.Min(amount, maxHealth - CurrentHealth);
        CurrentHealth += actualHeal;
        
        OnHealed?.Invoke(actualHeal);
        OnHealthChanged?.Invoke();
    }
    #endregion
}
