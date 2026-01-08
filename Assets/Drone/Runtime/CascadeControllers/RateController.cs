using UnityEngine;
using Drone.Runtime.PID;

namespace Drone.Runtime.FlightModes
{
    public class RateController
    {
        private readonly PIDController _pitchPID;
        private readonly PIDController _rollPID;
        private readonly PIDController _yawPID;

        public RateController(PIDConfig pitch, PIDConfig roll, PIDConfig yaw)
        {
            _pitchPID = new PIDController(pitch);
            _rollPID = new PIDController(roll);
            _yawPID = new PIDController(yaw);
        }

        public Vector3 ComputeCorrections(Vector3 targetRate, Vector3 currentRate, float dt)
        {
            return new Vector3(
                _pitchPID.Compute(targetRate.x - currentRate.x, dt),
                _yawPID.Compute(targetRate.y - currentRate.y, dt),
                _rollPID.Compute(targetRate.z - currentRate.z, dt)
            );
        }

        public void Reset()
        {
            _pitchPID.Reset();
            _rollPID.Reset();
            _yawPID.Reset();
        }
    }
}