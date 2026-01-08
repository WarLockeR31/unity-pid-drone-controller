using Drone.Runtime.PID;
using UnityEngine;

namespace Drone.Runtime.CascadeControllers
{
	public class AltitudeController
	{
		private readonly PIDController _altPID;
		private readonly float _maxClimbSpeed;
		private readonly float _hoverThrottle;

		public AltitudeController(PIDConfig config, float maxClimbSpeed, float hoverThrottle = 0.3f)
		{
			_altPID = new PIDController(config);
			_maxClimbSpeed = maxClimbSpeed;
			_hoverThrottle = hoverThrottle;
		}

		public float GetThrottle(float inputThrottle, float currentVelY, float dt)
		{
			float targetVelY = inputThrottle * _maxClimbSpeed;
            
			float correction = _altPID.Compute(targetVelY - currentVelY, dt);
            
			return Mathf.Clamp01(_hoverThrottle + correction);
		}

		public void Reset() => _altPID.Reset();
	}
}