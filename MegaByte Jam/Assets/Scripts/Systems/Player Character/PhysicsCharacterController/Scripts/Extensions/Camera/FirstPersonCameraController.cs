using Cinemachine;
using UnityEngine;


namespace PhysicsCharacterController
{
    public class FirstPersonCameraController : MonoBehaviour
    {
        [Header("Camera controls")]
        public Vector2 mouseSensivity = new Vector2(5f, -50f);
        public Vector2 analogSensivity = new Vector2(25f, -150f);
        public float smoothSpeed = 0.05f;

        [Header("References")]
        public InputReader inputReader;


        private CinemachinePOV cinemachinePOV;
        private Vector2 currentSensitivity;
        private Vector2 smoothVelocity;
        private Vector2 currentInputVector;
        private Vector2 input;


        /**/


        private void Awake()
        {
            cinemachinePOV = this.GetComponent<CinemachineVirtualCamera>().GetCinemachineComponent<CinemachinePOV>();
            currentSensitivity = mouseSensivity;
        }


        private void Update()
        {
            input += inputReader.cameraDelta * currentSensitivity * new Vector2(0.01f, 0.001f);

            if (input.y > cinemachinePOV.m_VerticalAxis.m_MaxValue) input.y = cinemachinePOV.m_VerticalAxis.m_MaxValue;
            else if (input.y < cinemachinePOV.m_VerticalAxis.m_MinValue) input.y = cinemachinePOV.m_VerticalAxis.m_MinValue;

            currentInputVector = Vector2.SmoothDamp(currentInputVector, input, ref smoothVelocity, smoothSpeed);
            cinemachinePOV.m_HorizontalAxis.Value = currentInputVector.x;
            cinemachinePOV.m_VerticalAxis.Value = currentInputVector.y;
        }


        public void SetInitialValue(float _valueX, float _valueY)
        {
            input = new Vector2(_valueX, _valueY);
            currentInputVector = input;

            cinemachinePOV.m_HorizontalAxis.Value = _valueX;
            cinemachinePOV.m_VerticalAxis.Value = _valueY;
        }


        public void IsInputGamepad(bool _state)
        {
            if (!_state) currentSensitivity = mouseSensivity;
            else currentSensitivity = analogSensivity;
        }
    }
}