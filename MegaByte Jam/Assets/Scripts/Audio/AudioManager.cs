using FMOD.Studio;
using FMODUnity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance { get; private set; }



    [Header("Volume")]
    [Range(0, 1)]
    public float masterVolume = 1;
    [Range(0, 1)]
    public float musicVolume = 1;
    [Range(0, 1)]
    public float soundeffectsVolume = 1;
    [Range(0, 1)]
    public float ambienceVolume = 1;

    private Bus masterBus;

    private Bus musicBus;

    private Bus soundeffectsBus;

    private Bus ambienceBus;

    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogError("Found more than one Audio Manager in the scene.");
        }
        instance = this;

        masterBus = RuntimeManager.GetBus("bus:/");
        musicBus = RuntimeManager.GetBus("bus:/Music");
        soundeffectsBus = RuntimeManager.GetBus("bus:/SoundEffects");
        ambienceBus = RuntimeManager.GetBus("bus:/Ambience");
    }

    private void Update()
    {
        masterBus.setVolume(masterVolume);
        musicBus.setVolume(musicVolume);
        soundeffectsBus.setVolume(soundeffectsVolume);
        ambienceBus.setVolume(ambienceVolume);
    }
}