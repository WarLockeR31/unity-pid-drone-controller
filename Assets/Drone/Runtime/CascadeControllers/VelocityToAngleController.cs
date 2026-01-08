using Drone.Runtime.PID;
using UnityEngine;

namespace Drone.Runtime.CascadeControllers
{
	public class VelocityToAngleController
	{
		private readonly PIDController _velPID;
		private readonly float _maxTilt;

		public VelocityToAngleController(PIDConfig config, float maxTilt)
		{
			_velPID = new PIDController(config);
			_maxTilt = maxTilt;
		}

		public Vector2 GetTargetAngles(Vector2 targetVelocity, Vector2 currentVelocity, float dt)
		{
			float pitchAngle = -_velPID.Compute(currentVelocity.y - targetVelocity.y, dt);
			float rollAngle = _velPID.Compute(targetVelocity.x - currentVelocity.x, dt);

			pitchAngle = Mathf.Clamp(pitchAngle, -_maxTilt, _maxTilt);
			rollAngle = Mathf.Clamp(rollAngle, -_maxTilt, _maxTilt);

			return new Vector2(rollAngle, pitchAngle);
		}

		public void Reset() => _velPID.Reset();
	}
}