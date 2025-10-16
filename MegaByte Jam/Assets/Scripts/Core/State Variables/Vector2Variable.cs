using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Whenever you have a value that you want to be able to finetune 
/// at runtime, instantiate a Vector2Variable, then attach a Vector2Reference
/// with this attached to it to the GameObject you need the value on.
/// </summary>

[CreateAssetMenu(fileName = "Vector2Variable")]
public class Vector2Variable : ScriptableObject
{
    public Vector2 CurrentValue;
    public Vector2 PreviousValue;
}
