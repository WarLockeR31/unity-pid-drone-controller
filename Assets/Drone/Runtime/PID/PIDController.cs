using UnityEngine;

namespace Drone.Runtime.PID
{
	[System.Serializable]
	public class PIDController
	{
		private PIDConfig _settings;
    
		private float _integral;
		private float _prevError;

		public PIDController(PIDConfig settings)
		{
			this._settings = settings;
		}

		public float Compute(float error, float dt)
		{
			float p = error * _settings.Kp;

			_integral += error * dt;
			// TODO: Change to different anti-windup realization?
			_integral = Mathf.Clamp(_integral, -_settings.integralSaturation, _settings.integralSaturation);
			float i = _integral * _settings.Ki;

			float d = (error - _prevError) * _settings.Kd / dt;
			_prevError = error;

			float output = p + i + d;
			return Mathf.Clamp(output, -_settings.maxOutput, _settings.maxOutput);
		}
    
		public void Reset()
		{
			_integral = 0f;
			_prevError = 0f;
		}
	}
}