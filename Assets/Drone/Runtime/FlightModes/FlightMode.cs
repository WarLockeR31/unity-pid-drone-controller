using UnityEngine;

namespace Drone.Runtime.FlightModes
{
	public enum MixingStrategy
	{
		PrioritizeThrottle,
		PrioritizeAttitude
	}
	
	public struct DroneState
	{
		public Vector3 Rotation;        
		public Vector3 AngularVelocity; 
		public Vector3 Velocity;        
		public float VerticalVelocity;  
	}

	public struct FlightControlOutput
	{
		public Vector3 TargetRate; 
		public float Throttle;     
	}

	public interface IFlightMode
	{
		FlightControlOutput Calculate(DroneInputs inputs, DroneState state, float dt);
		void Reset();
		string ModeName { get; }
		MixingStrategy Mixing { get; }
	}
}