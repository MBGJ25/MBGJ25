using System;
using UnityEngine;

[CreateAssetMenu(fileName = "Enemy Stats", menuName = "Scriptable Objects/State/New Enemy Stats")]
public class EnemyStats : ScriptableObject
{
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int attackDamage = 1;
    public int MaxHealth => maxHealth;
}