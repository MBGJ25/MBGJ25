using UnityEngine;
 

/// <summary>
/// Whenever you have a value that you want to be able to finetune 
/// at runtime, instantiate a FloatVariable, then attach a FloatReference
/// with this attached to it to the GameObject you need the value on.
/// </summary>
[CreateAssetMenu(fileName = "FloatVariable")]
public class FloatVariable : ScriptableObject
{
    public float Value;
}
