using UnityEngine;


namespace PhysicsCharacterController
{
    public class VFXManager : MonoBehaviour
    {
        [Header("Particle references")]
        public CharacterManager characterManager;
        [Space(10)]

        public GameObject particleGrind;
        public GameObject particleLand;
        public GameObject particleFast;
        [Space(10)]

        public bool enableVFX = false;

        private CapsuleCollider collider;
        private Quaternion gqt;
        private GameObject characterModel;


        /**/


        private void Awake()
        {
            collider = characterManager.GetComponent<CapsuleCollider>();
            characterModel = characterManager.characterModel;
        }


        #region VFX 

        public void ParticleGrind()
        {
            if (enableVFX)
            {
                Quaternion newQuaternion = new Quaternion(0, 0, characterManager.transform.rotation.z, characterManager.transform.rotation.w);
                GameObject tmpObj = GameObject.Instantiate(particleGrind, characterManager.transform.position - new Vector3(0f, collider.height / 2f, 0f), characterManager.transform.rotation);
                tmpObj.transform.parent = this.transform;
            }
        }

        public void ParticleLand()
        {
            if (enableVFX)
            {
                GameObject tmpObj = GameObject.Instantiate(particleLand, characterManager.transform.position - new Vector3(0f, collider.height / 2f, 0f), Quaternion.identity);
                tmpObj.transform.parent = this.transform; 
            }
        }

        public void ParticleFast()
        {
            if (enableVFX)
            {
                GameObject tmpObj = GameObject.Instantiate(particleFast, characterManager.transform.position, characterModel.transform.rotation);
                tmpObj.transform.parent = characterManager.transform;
            }
        }

        #endregion
    }
}