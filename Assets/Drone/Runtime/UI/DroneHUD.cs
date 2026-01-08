using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Drone.Runtime.UI
{
    public class DroneHUD : MonoBehaviour
    {
        [Header("References")]
        public DroneHardware droneHardware;
        public FlightControlSystem flightSystem;
        public Rigidbody droneRb;

        [Header("UI Text Elements")]
        public TextMeshProUGUI telemetryText; 
        public TextMeshProUGUI modeText;      
        public TextMeshProUGUI motorsText;    

        [Header("Artificial Horizon")]
        public RectTransform horizonTransform;
        public float pitchSensitivity = 5f;	

        [Header("Motor Bars")]
        public Slider[] motorSliders;		
		public Image[] motorFills;

		private float _maxMotorForceDisplay;
		private Color _safeColor = Color.green;
		private Color _warnColor = Color.yellow;
		private Color _dangerColor = Color.red;

		private void OnEnable()
		{
			if (flightSystem != null)
			{
				flightSystem.OnModeChanged += UpdateModeUI;
			}
			
			if (telemetryText != null) telemetryText.color = Color.green;
			if (modeText != null) modeText.color = Color.green;
		}

		private void OnDisable()
		{
			if (flightSystem != null)
			{
				flightSystem.OnModeChanged -= UpdateModeUI;
			}
		}

		private void Start()
		{
			_maxMotorForceDisplay = flightSystem.MaxMotorForce;	
		}
		
        private void Update()
        {
            UpdateTelemetry();
            UpdateHorizon();
            UpdateMotors();
        }

		private void UpdateTelemetry()
		{
			Vector3 localVel = droneHardware.transform.InverseTransformDirection(droneRb.linearVelocity);
			float speedHor = new Vector2(localVel.x, localVel.z).magnitude;
            
			Vector3 angVel = droneHardware.transform.InverseTransformDirection(droneRb.angularVelocity) * Mathf.Rad2Deg;
			float alt = droneHardware.transform.position.y;

			telemetryText.SetText(
				"ALT: {0:1} m\nSPD H: {1:1} m/s\nSPD V: {2:1} m/s\n----------------\nANG P: {3:0}°/s\nANG R: {4:0}°/s\nANG Y: {5:0}°/s",
				alt, speedHor, localVel.y, angVel.x, angVel.z, angVel.y
			);
		}
		
        private void UpdateHorizon()
        {
            if (horizonTransform == null) return;

            Vector3 angles = droneHardware.transform.eulerAngles;
            float roll = -angles.z; 
            float pitch = angles.x;

            if (pitch > 180) pitch -= 360;

            horizonTransform.localRotation = Quaternion.Euler(0, 0, roll);
            horizonTransform.anchoredPosition = new Vector2(0, pitch * pitchSensitivity);
        }

		private void UpdateMotors()
		{
			float[] forces = droneHardware.CurrentForces; 

			for (int i = 0; i < motorSliders.Length; i++)
			{
				if (i >= forces.Length) break;

				float normalizedValue = Mathf.Clamp01(forces[i] / _maxMotorForceDisplay);
                
				motorSliders[i].value = normalizedValue;

				if (motorFills != null && i < motorFills.Length && motorFills[i] != null)
				{
					if (normalizedValue < 0.5f)
					{
						motorFills[i].color = Color.Lerp(_safeColor, _warnColor, normalizedValue * 2f);
					}
					else
					{
						motorFills[i].color = Color.Lerp(_warnColor, _dangerColor, (normalizedValue - 0.5f) * 2f);
					}
				}
			}
		}
		
		private void UpdateModeUI(string modeName)
		{
			modeText.text = $"MODE: <color=yellow>{modeName}</color>";
		}
    }
}