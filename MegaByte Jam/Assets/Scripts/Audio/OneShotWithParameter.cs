using System.Collections;
using System.Collections.Generic;
using FMODUnity;
using FMOD.Studio;
using UnityEngine;

public class OneShotWithParameter : MonoBehaviour
{
    [SerializeField] private EventReference eventToPlay; // Assign your FMOD event path in the Inspector

    public string parameterName = "Is Idle"; // Name of your FMOD parameter
    public float parameterValue = 0.5f; // Value to set for the parameter

    public void PlayOneShotWithParameter()
    {
        // 1. Create an EventInstance
        EventInstance instance = RuntimeManager.CreateInstance(eventToPlay);

        // 3. Start the event
        instance.start();

        // 4. Release the instance (important to avoid memory leaks)
        instance.release();
    }
}

