using System;
using System.Collections.Generic;
using Drone.Runtime.CascadeControllers;
using Drone.Runtime.FlightModes;
using Drone.Runtime.PID;
using UnityEngine;

namespace Drone.Runtime
{
    [RequireComponent(typeof(DroneHardware), typeof(DroneInputs))]
    public class FlightControlSystem : MonoBehaviour
    {
        [Header("Configurations")]
        public PIDConfig pitchRateSettings;
        public PIDConfig rollRateSettings;
        public PIDConfig yawRateSettings;
        [Space]
        public PIDConfig pitchAngleSettings;
        public PIDConfig rollAngleSettings;
        [Space]
        public PIDConfig altitudeSettings;
        public PIDConfig velocitySettings;

        [Header("Characteristics")] 
        public DroneSettings droneSettings;
		
		public event Action<string> OnModeChanged;

        // Dependencies
        private DroneHardware _hardware;
        private DroneInputs _inputs;
        private Rigidbody _rb;

        // Controllers
        private RateController _rateController;

        // Strategies
        private List<IFlightMode> _flightModes;
        private int _currentModeIndex = 0;
		
		private float _maxMotorForce;
		
		public float MaxMotorForce => _maxMotorForce;

        private void Awake()
        {
            _hardware = GetComponent<DroneHardware>();
            _inputs = GetComponent<DroneInputs>();
            _rb = GetComponent<Rigidbody>();
			
			float hoverForce = _hardware.mass * 9.81f;
			float maxThrust = hoverForce * droneSettings.thrustToWeightRatio;
			_maxMotorForce = maxThrust / 4f;

            InitializeControllers();
        }

		private void Start()
		{
			OnModeChanged?.Invoke(_flightModes[_currentModeIndex].ModeName);
		}

		private void InitializeControllers()
		{
			_rateController = new RateController(pitchRateSettings, rollRateSettings, yawRateSettings);

			var angleCtrl = new AngleToRateController(pitchAngleSettings, rollAngleSettings, droneSettings.maxTiltAngle);
			var altCtrl = new AltitudeController(altitudeSettings, droneSettings.maxClimbSpeed);
			var velCtrl = new VelocityToAngleController(velocitySettings, droneSettings.maxTiltAngle);

			_flightModes = new List<IFlightMode>
			{
				new RateMode(droneSettings.maxYawSpeed), 
				new AngleAltHoldMode(angleCtrl, altCtrl, velCtrl, droneSettings.maxTiltAngle, droneSettings.maxYawSpeed),
				new VelocityMode(velCtrl, angleCtrl, altCtrl, droneSettings.maxTiltAngle, droneSettings.maxHorizontalSpeed, droneSettings.maxYawSpeed)
			};
		}

        private void Update()
        {
            // TODO: Add hotkeys
            if (_inputs.changeModeAction.action.WasPressedThisFrame())
            {
                CycleMode();
            }
        }

        private void CycleMode()
        {
            _currentModeIndex = (_currentModeIndex + 1) % _flightModes.Count;
            _flightModes[_currentModeIndex].Reset();
			OnModeChanged?.Invoke(_flightModes[_currentModeIndex].ModeName);
#if UNITY_EDITOR
            Debug.Log($"Switched to Mode: {_flightModes[_currentModeIndex].ModeName}");
#endif
        }

        private void FixedUpdate()
        {
            float dt = Time.fixedDeltaTime;

            DroneState state = new DroneState
            {
                Rotation = NormalizeAngles(transform.localEulerAngles),
                AngularVelocity = transform.InverseTransformDirection(_rb.angularVelocity) * Mathf.Rad2Deg, 
                Velocity = transform.InverseTransformDirection(_rb.linearVelocity),
                VerticalVelocity = _rb.linearVelocity.y
            };

            IFlightMode currentMode = _flightModes[_currentModeIndex];
            FlightControlOutput output = currentMode.Calculate(_inputs, state, dt);

            Vector3 pidCorrections = _rateController.ComputeCorrections(output.TargetRate, state.AngularVelocity, dt);

			ApplyMotorMixing(output.Throttle, pidCorrections, currentMode.Mixing);;
        }

		private void ApplyMotorMixing(float requestedThrottle, Vector3 correction, MixingStrategy strategy)
        {
            float throttlePercent = Mathf.Max(requestedThrottle, droneSettings.idleThrottle);
            float throttleForce = throttlePercent * _maxMotorForce;

            float pitch = correction.x;
            float yaw   = correction.y;
            float roll  = correction.z;

            float mixFL = -pitch - roll - yaw;
            float mixFR = -pitch + roll + yaw;
            float mixBL =  pitch - roll + yaw;
            float mixBR =  pitch + roll - yaw;
            
            float minMix = Mathf.Min(mixFL, Mathf.Min(mixFR, Mathf.Min(mixBL, mixBR)));
            float maxMix = Mathf.Max(mixFL, Mathf.Max(mixFR, Mathf.Max(mixBL, mixBR)));
            
            if (strategy == MixingStrategy.PrioritizeThrottle)
            {
                // Scaling
                
                if (throttleForce + minMix < 0)
                {
                    float availableRoom = throttleForce; 
                    float requiredRoom = -minMix;           
                    float scale = availableRoom / requiredRoom;
        
                    pitch *= scale;
                    roll  *= scale;
                    yaw   *= scale;
                    
                    // TODO: Optimize
                    mixFL = -pitch - roll - yaw;
                    mixFR = -pitch + roll + yaw;
                    mixBL =  pitch - roll + yaw;
                    mixBR =  pitch + roll - yaw;
                }
            }
            else 
            {
				// AirMode
                
                if (throttleForce + minMix < 0)
                {
                    float offset = -(throttleForce + minMix);
                    throttleForce += offset; 
                }
                // TODO: Clipping max
            }

            float fl = throttleForce + mixFL;
            float fr = throttleForce + mixFR;
            float bl = throttleForce + mixBL;
            float br = throttleForce + mixBR;

			// TODO: Check if needed
            fl = Mathf.Max(0, fl);
            fr = Mathf.Max(0, fr);
            bl = Mathf.Max(0, bl);
            br = Mathf.Max(0, br);

            _hardware.ApplyMotorForces(fl, fr, bl, br);
        }

		private Vector3 NormalizeAngles(Vector3 angles)
        {
            angles.x = (angles.x > 180) ? angles.x - 360 : angles.x;
            angles.y = (angles.y > 180) ? angles.y - 360 : angles.y;
            angles.z = (angles.z > 180) ? angles.z - 360 : angles.z;
            return angles;
        }
    }
}