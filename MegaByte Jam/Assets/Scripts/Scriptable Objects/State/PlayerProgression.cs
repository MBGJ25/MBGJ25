using UnityEngine;

[CreateAssetMenu(fileName = "Player Progression", menuName = "Scriptable Objects/State/New Player Progression")]
public class PlayerProgression : SavableScriptableObject
{
    [SerializeField] private int currentLevel = 1;
    
    // Unique key for saving/loading - make sure this is unique across all SOs
    public override string SaveKey => "PlayerProgression";
    
    // Public property to access the level
    public int CurrentLevel 
    { 
        get => currentLevel; 
        set => currentLevel = value; 
    }
}
