using System.Collections.Generic;
using UnityEngine;

// NOTE: This is only to test saving and loading player data and is not the actual player data
[CreateAssetMenu(fileName = "Save Test Player Data", menuName = "Game Data/Test Player Data")]
public class SaveTestPlayerData : ScriptableObject
{
    [Header("Player Info")]
    public string playerName = "Save Test Player";
    public int level = 1;
    public float health = 100f;
    public Vector3 position = Vector3.zero;

    [Header("Inventory")]
    public List<string> inventory = new List<string> { "Skates", "Spray Can" };

    public void ResetData()
    {
        playerName = "Test Player";
        level = 1;
        health = 100f;
        position = Vector3.zero;
        inventory = new List<string> { "Skates", "Spray Can" };
    }
}