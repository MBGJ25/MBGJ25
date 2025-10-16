using System;
using UnityEngine;

/// <summary>
/// This is a template for how any state objects should be written.
/// To use them, just add a Serialized Field reference in the game object that
/// needs to reference it and subscribe to the `OnHealthChanged` event.
/// </summary>

[CreateAssetMenu(fileName = "Player Stats", menuName = "Scriptable Objects/State/PlayerStats")]
public class PlayerStats : ScriptableObject 
{

    #region Events
    public event Action OnHealthChanged;
    #endregion

    #region Serialized Fields
    [SerializeField] private int health;
    #endregion

    #region Getters and Setters
    public int Health
    {
        get => health;
        set
        {
            if (health == value) return;
            health = value;
            OnHealthChanged?.Invoke();
        }
    }
    #endregion
}