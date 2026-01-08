using UnityEngine;
using UnityEngine.InputSystem;

namespace Drone.Runtime
{
	public class DroneInputs : MonoBehaviour
	{
		[Header("Input Action References")]
		public InputActionReference pitchAction; 
		public InputActionReference rollAction;
		public InputActionReference yawAction;     
		public InputActionReference throttleAction;
		public InputActionReference toggleStabilizationAction;
		public InputActionReference changeModeAction;

		public Vector2 Cyclic { get; private set; } 
		public float Yaw { get; private set; }
		public float Throttle { get; private set; }
		public bool IsStabilizationActive { get; private set; }

		private void Awake()
		{
			if (pitchAction == null)
				Debug.LogError("Pitch action is not set!");
			if (rollAction == null)
				Debug.LogError("Roll action is not set!");
			if (yawAction == null)
				Debug.LogError("Yaw action is not set!");
			if (throttleAction == null)
				Debug.LogError("Throttle action is not set!");
			if (toggleStabilizationAction == null)
				Debug.LogError("Toggle stabilization action is not set!");
			if (changeModeAction == null)
				Debug.LogError("Change mode action is not set!");
		}
		
		private void Update()
		{
			Cyclic = new Vector2(rollAction.action.ReadValue<float>(), pitchAction.action.ReadValue<float>());
			Yaw = yawAction.action.ReadValue<float>();
			Throttle = throttleAction.action.ReadValue<float>();
			if (toggleStabilizationAction.action.WasPressedThisFrame())
			{
				IsStabilizationActive = !IsStabilizationActive; 
#if UNITY_EDITOR
				Debug.Log($"Stabilization Mode: {IsStabilizationActive}"); 
#endif
			}
		}

		private void OnEnable()
		{
			pitchAction?.action.Enable();
			rollAction?.action.Enable();
			yawAction?.action.Enable();
			throttleAction?.action.Enable();
			toggleStabilizationAction?.action.Enable();
		}
    
		private void OnDisable()
		{
			pitchAction?.action.Disable();
			rollAction?.action.Disable();
			yawAction?.action.Disable();
			throttleAction?.action.Disable();
			toggleStabilizationAction?.action.Disable();
		}
	}
}