using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;

public class FMODOneShotPlayer : MonoBehaviour
{
    [Header("FMOD Event")]
    [SerializeField] private EventReference eventToPlay;

    // Chame esta função para tocar o som
    public void PlayOneShot()
    {
        if (eventToPlay.IsNull)
        {
            Debug.LogWarning("FMODOneShotPlayer: EventReference está vazio!");
            return;
        }

        RuntimeManager.PlayOneShot(eventToPlay, transform.position);
    }
}

