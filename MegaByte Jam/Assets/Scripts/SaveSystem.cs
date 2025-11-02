using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SaveSystem : SingletonMonoBehavior<SaveSystem>
{

    float health;
    int level;
    string playerName = "Test Player";
    Vector3 position = Vector3.zero;
    public List<string> inventory = new List<string> { "Skates", "Spray Can" };

    [Header("Player Info")]
    public string reset_playerName = "";
    public int reset_level = 1;
    public float reset_health = 100f;
    public Vector3 reset_position = Vector3.zero;

    [Header("Inventory")]
    public List<string> reset_inventory = new List<string> { "Skates", "Spray Can" };
    void SaveGame( float current_health , int current_level ,Vector3 current_position , List <string> current_inventory)
    {
        health = current_health;
        level = current_level;
        position = current_position;
        inventory = current_inventory;
    }

    void LoadGame(float load_health, int load_level, Vector3 load_position, List<string> load_inventory)
    {
        health = load_health;
        level = load_level;
        position = load_position;
        inventory = load_inventory;
    }

    void ResetGame()
    {
        health = reset_health;
        level = reset_level;
        position = reset_position;
        inventory = reset_inventory;
    }
}
