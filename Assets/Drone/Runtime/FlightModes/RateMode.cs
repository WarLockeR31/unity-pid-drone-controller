using Drone.Runtime.CascadeControllers;
using UnityEngine;
using Drone.Runtime.PID;

namespace Drone.Runtime.FlightModes
{
    // ================= RATE MODE =================
    // Вход -> Target Rate
    public class RateMode : IFlightMode
    {
        private readonly float _maxRate;
        public string ModeName => "ACRO / RATE";
		public MixingStrategy Mixing => MixingStrategy.PrioritizeAttitude;

        public RateMode(float maxRate)
        {
            _maxRate = maxRate;
        }

        public FlightControlOutput Calculate(DroneInputs inputs, DroneState state, float dt)
        {
			float normalizedThrottle = Mathf.Clamp01(inputs.Throttle);
            return new FlightControlOutput
            {
                TargetRate = new Vector3(
                    inputs.Cyclic.y * _maxRate,
                    inputs.Yaw * _maxRate,
                    -inputs.Cyclic.x * _maxRate
                ),
                Throttle = normalizedThrottle
            };
        }

        public void Reset() { }
    }

    

    /*// ================= VELOCITY MODE =================
    // No Input: Target Vel=0 -> [VelToAngle] -> Target Angle -> [AngleToRate] -> Target Rate
    // Input: Acts like Angle Mode
    public class VelocityMode : IFlightMode
    {
        private readonly VelocityToAngleController _velCtrl;
        private readonly AngleToRateController _angleCtrl;
        private readonly AltitudeController _altCtrl;
        private readonly float _maxTilt;
        private readonly float _maxYawRate;
        private readonly float _inputDeadzone;

        public string ModeName => "VELOCITY STABILIZE";

        public VelocityMode(
            VelocityToAngleController velCtrl, 
            AngleToRateController angleCtrl, 
            AltitudeController altCtrl, 
            float maxTilt, float maxYawRate, float deadzone = 0.05f)
        {
            _velCtrl = velCtrl;
            _angleCtrl = angleCtrl;
            _altCtrl = altCtrl;
            _maxTilt = maxTilt;
            _maxYawRate = maxYawRate;
            _inputDeadzone = deadzone;
        }

        public FlightControlOutput Calculate(DroneInputs inputs, DroneState state, float dt)
        {
            Vector2 targetAngles;

            // Если стик отпущен -> Стабилизируем скорость (Braking)
            if (inputs.Cyclic.magnitude < _inputDeadzone)
            {
                // Целевая скорость 0. Текущая скорость (state.Velocity.x, state.Velocity.z)
                // Z - вперед/назад, X - влево/вправо
                targetAngles = _velCtrl.GetTargetAngles(Vector2.zero, new Vector2(state.Velocity.x, state.Velocity.z), dt);
            }
            else
            {
                // Если стик нажат -> Летим как в Angle Mode (перехват управления)
                _velCtrl.Reset(); // Сбрасываем PID скорости, чтобы не накапливалась ошибка
                targetAngles = inputs.Cyclic * _maxTilt;
            }

            // Angle -> Rate
            Vector2 rates = _angleCtrl.GetTargetRates(targetAngles, state.Rotation, dt);

            // Alt Hold
            float thr = _altCtrl.GetThrottle(inputs.Throttle, state.VerticalVelocity, dt);

            return new FlightControlOutput
            {
                TargetRate = new Vector3(rates.x, inputs.Yaw * _maxYawRate, -rates.y),
                Throttle = thr
            };
        }

        public void Reset()
        {
            _velCtrl.Reset();
            _angleCtrl.Reset();
            _altCtrl.Reset();
        }
    }*/
}