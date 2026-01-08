using Drone.Runtime.CascadeControllers;
using UnityEngine;

namespace Drone.Runtime.FlightModes
{
	// ================= VELOCITY MODE =================
	// Input (0..1) -> Target Velocity (0..MaxSpeed) -> [VelToAngle] -> Target Angle
	public class VelocityMode : IFlightMode
	{
		private readonly VelocityToAngleController _velCtrl;
		private readonly AngleToRateController _angleCtrl;
		private readonly AltitudeController _altCtrl;
	
		private readonly float _maxTilt;
		private readonly float _maxSpeed;
		private readonly float _maxYawRate;

		public string ModeName => "VELOCITY CRUISE";
		public MixingStrategy Mixing => MixingStrategy.PrioritizeThrottle;

		public VelocityMode(
			VelocityToAngleController velCtrl, 
			AngleToRateController angleCtrl, 
			AltitudeController altCtrl, 
			float maxTilt, float maxSpeed, float maxYawRate)
		{
			_velCtrl = velCtrl;
			_angleCtrl = angleCtrl;
			_altCtrl = altCtrl;
			_maxTilt = maxTilt;
			_maxSpeed = maxSpeed;
			_maxYawRate = maxYawRate;
		}

		public FlightControlOutput Calculate(DroneInputs inputs, DroneState state, float dt)
		{
			Vector2 targetVelocity = new Vector2(inputs.Cyclic.x, inputs.Cyclic.y) * _maxSpeed;

			Vector2 targetAngles = _velCtrl.GetTargetAngles(targetVelocity, new Vector2(state.Velocity.x, state.Velocity.z), dt);
			Vector2 rates = _angleCtrl.GetTargetRates(targetAngles, state.Rotation, dt);
			float thr = _altCtrl.GetThrottle(inputs.Throttle, state.VerticalVelocity, dt);

			return new FlightControlOutput
			{
				TargetRate = new Vector3(rates.x, inputs.Yaw * _maxYawRate, rates.y),
				Throttle = thr
			};
		}
	
		public void Reset()
		{
			_velCtrl.Reset();
			_angleCtrl.Reset();
			_altCtrl.Reset();
		}
	}
}