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

		public Vector2 Cyclic { get; private set; } 
		public float Yaw { get; private set; }
		public float Throttle { get; private set; }

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
		}
		
		private void Update()
		{
			Cyclic = new Vector2(rollAction.action.ReadValue<float>(), pitchAction.action.ReadValue<float>());
			Yaw = yawAction.action.ReadValue<float>();
			Throttle = throttleAction.action.ReadValue<float>();
		}

		private void OnEnable()
		{
			pitchAction?.action.Enable();
			rollAction?.action.Enable();
			yawAction?.action.Enable();
			throttleAction?.action.Enable();
		}
    
		private void OnDisable()
		{
			pitchAction?.action.Disable();
			rollAction?.action.Disable();
			yawAction?.action.Disable();
			throttleAction?.action.Disable();
		}
	}
}