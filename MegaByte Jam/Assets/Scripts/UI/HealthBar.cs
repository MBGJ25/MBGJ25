using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private Slider slider;
    [SerializeField] private PlayerStats playerStats;
    private void OnEnable()
    {
        if (playerStats == null || slider == null)
            return;

        // Subscribe to health change event
        playerStats.OnHealthChanged += HandleHealthChanged;

        // Initialize the slider to current health
        slider.maxValue = playerStats.MaxHealth;
        slider.value = playerStats.CurrentHealth;
    }

    private void OnDisable()
    {
        if (playerStats != null)
            playerStats.OnHealthChanged -= HandleHealthChanged;
    }

    // Update slider value when health changes
    private void HandleHealthChanged()
    {
        slider.value = playerStats.CurrentHealth;
    }
}
