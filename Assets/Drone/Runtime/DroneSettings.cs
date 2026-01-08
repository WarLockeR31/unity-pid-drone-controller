using UnityEngine;

namespace Drone.Runtime
{
	[CreateAssetMenu(fileName = "DroneSettings", menuName = "Scriptable Objects/Settings")]
	public class DroneSettings : ScriptableObject
	{
		[Header("Limits")]
		public float maxTiltAngle = 30f;
		public float maxYawSpeed = 180f;
		public float maxClimbSpeed = 5f;
		public float maxHorizontalSpeed = 10f;
		[Range(0, 1)] public float idleThrottle = 0.05f;
		public float thrustToWeightRatio = 2.0f;
	}
}