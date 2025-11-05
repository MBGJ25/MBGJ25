using Cinemachine;
using UnityEngine;


namespace PhysicsCharacterController
{
    public class ThirdPersonCameraController : MonoBehaviour
    {
        [Header("Camera controls")]
        public Vector2 mouseSensivity = new Vector2(5f, 1f);
        public Vector2 analogSensivity = new Vector2(13f, 1.5f);
        public float smoothSpeed = 0.17f;

        [Header("References")]
        public InputReader inputReader;


        private CinemachineFreeLook cinemachineFreeLook;
        private Vector2 currentSensitivity;
        private Vector2 smoothVelocity;
        private Vector2 currentInputVector;
        private Vector2 input;


        /**/


        private void Awake()
        {
            cinemachineFreeLook = this.GetComponent<CinemachineFreeLook>();
            currentSensitivity = mouseSensivity;
        }


        private void Update()
        {
            input += inputReader.cameraDelta * currentSensitivity * new Vector2(0.01f, 0.001f);

            if (input.y > 1f) input.y = 1f;
            else if (input.y < 0f) input.y = 0f;

            currentInputVector = Vector2.SmoothDamp(currentInputVector, input, ref smoothVelocity, smoothSpeed);
            cinemachineFreeLook.m_XAxis.Value = currentInputVector.x;
            cinemachineFreeLook.m_YAxis.Value = currentInputVector.y;
        }


        public void SetInitialValue(float _valueX, float _valueY)
        {
            input = new Vector2(_valueX, _valueY);
            currentInputVector = input;

            cinemachineFreeLook.m_XAxis.Value = _valueX;
            cinemachineFreeLook.m_YAxis.Value = _valueY;
        }


        public void IsInputGamepad(bool _state)
        {
            if (!_state) currentSensitivity = mouseSensivity;
            else currentSensitivity = analogSensivity;
        }
    }
}