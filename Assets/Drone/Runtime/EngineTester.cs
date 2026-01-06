using UnityEngine;

namespace Drone.Runtime
{
	public class EngineTester : MonoBehaviour
	{
		public DroneController drone;
    
		[Range(0, 20)] public float testForce = 0f; 

		void FixedUpdate()
		{
			drone.ApplyMotorForces(testForce, testForce, testForce, testForce);
		}
	}
}