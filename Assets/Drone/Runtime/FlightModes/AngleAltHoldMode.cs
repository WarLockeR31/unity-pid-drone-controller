using Drone.Runtime.CascadeControllers;
using UnityEngine;

namespace Drone.Runtime.FlightModes
{
	// ================= ANGLE + ALT HOLD =================
	// Вход -> Target Angle -> [AngleToRate] -> Target Rate
	public class AngleAltHoldMode : IFlightMode
	{
		private readonly AngleToRateController _angleCtrl;
		private readonly AltitudeController _altCtrl;
		private readonly float _maxTilt;
		private readonly float _maxYawRate;
        
		public string ModeName => "ANGLE + ALT";
		public MixingStrategy Mixing => MixingStrategy.PrioritizeThrottle;

		public AngleAltHoldMode(AngleToRateController angleCtrl, AltitudeController altCtrl, float maxTilt, float maxYawRate)
		{
			_angleCtrl = angleCtrl;
			_altCtrl = altCtrl;
			_maxTilt = maxTilt;
			_maxYawRate = maxYawRate;
		}

		public FlightControlOutput Calculate(DroneInputs inputs, DroneState state, float dt)
		{
			Vector2 targetAngles = new Vector2(inputs.Cyclic.x, inputs.Cyclic.y) * _maxTilt;
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
			_angleCtrl.Reset();
			_altCtrl.Reset();
		}
	}
}