using UnityEngine;
using Unity;
using System;


/// <summary>
/// This is an example of how to write a savable piece of state and does not
/// actually  need to be used at the moment.
/// </summary>
[CreateAssetMenu(fileName = "Player Progression",  menuName = "Scriptable Objects/State/Player Progression")]
public class PlayerProgression : ScriptableObject
{
    [SerializeField] private int currentLevel = 1;
}
