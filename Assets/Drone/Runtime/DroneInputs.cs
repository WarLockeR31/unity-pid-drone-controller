using UnityEngine;
using UnityEngine.InputSystem;

namespace Drone.Runtime
{
	public class DroneInputs : MonoBehaviour
	{
		[Header("Input Action References")]
		public InputActionReference moveAction;    
		public InputActionReference yawAction;     
		public InputActionReference throttleAction;

		public Vector2 Cyclic { get; private set; } 
		public float Yaw { get; private set; }
		public float Throttle { get; private set; }

		private void Awake()
		{
			if (moveAction == null)
				Debug.LogError("Move action is not set!");
			if (yawAction == null)
				Debug.LogError("Yaw action is not set!");
			if (throttleAction == null)
				Debug.LogError("Throttle action is not set!");
		}
		
		private void Update()
		{
			Cyclic = moveAction.action.ReadValue<Vector2>();
			Yaw = yawAction.action.ReadValue<float>();
			Throttle = throttleAction.action.ReadValue<float>();
		}

		private void OnEnable()
		{
			moveAction?.action.Enable();
			yawAction?.action.Enable();
			throttleAction?.action.Enable();
		}
    
		private void OnDisable()
		{
			moveAction?.action.Disable();
			yawAction?.action.Disable();
			throttleAction?.action.Disable();
		}
	}
}