using UnityEngine;

public class PlayerSounds : MonoBehaviour
{
    [SerializeField] private FMODUnity.EventReference _footsteps;
    private FMOD.Studio.EventInstance footsteps;

    private void Awake()
    {
        if (!_footsteps.IsNull)
        {
            footsteps = FMODUnity.RuntimeManager.CreateInstance(_footsteps);
        }
    }

    public void PlayFootstep()
    {
        if (footsteps.isValid())
        {
            FMODUnity.RuntimeManager.AttachInstanceToGameObject(footsteps, transform);
            footsteps.start();
        }
    }

    private void OnDestroy()
    {
        // Clean up FMOD instance
        if (footsteps.isValid())
        {
            footsteps.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            footsteps.release();
        }
    }
}
