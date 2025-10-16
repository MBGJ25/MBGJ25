using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Whenever you have a value that you want to be able to finetune 
/// at runtime, instantiate a IntVariable, then attach a IntReference
/// with this attached to it to the GameObject you need the value on.
/// </summary>
[CreateAssetMenu(fileName = "IntVariable")]
public class IntVariable : ScriptableObject
{
    public int Value;
}
