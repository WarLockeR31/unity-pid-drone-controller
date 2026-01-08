using Drone.Runtime.PID;
using UnityEngine;

namespace Drone.Runtime.CascadeControllers
{
	public class AngleToRateController
	{
		private readonly PIDController _pitchPID;
		private readonly PIDController _rollPID;
		private float _maxTilt;

		public AngleToRateController(PIDConfig pitch, PIDConfig roll, float maxTilt)
		{
			_pitchPID = new PIDController(pitch);
			_rollPID = new PIDController(roll);
			_maxTilt = maxTilt;
		}

		public Vector2 GetTargetRates(Vector2 targetAngles, Vector3 currentAngles, float dt)
		{
			float pitchRate = _pitchPID.Compute(targetAngles.y - currentAngles.x, dt);
			float rollRate = _rollPID.Compute(- targetAngles.x - currentAngles.z, dt);
			return new Vector2(pitchRate, rollRate);
		}

		public void Reset()
		{
			_pitchPID.Reset();
			_rollPID.Reset();
		}
	}
}