using Drone.Runtime.PID;
using UnityEngine;

namespace Drone.Runtime
{
	[RequireComponent(typeof(DroneHardware))]
	[RequireComponent(typeof(DroneInputs))]
	public class FlightControlSystem : MonoBehaviour
	{
	    [Header("PID Configuration")]
	    public PIDConfig pitchSettings;
	    public PIDConfig rollSettings;
	    public PIDConfig yawSettings;
	    public PIDConfig altitudeSettings; 
	
	    [Header("Flight Characteristics")]
	    public float maxTiltAngle = 30f;
	    public float maxYawSpeed = 90f;
	    public float maxClimbSpeed = 5f;
	
	    private DroneHardware _hardware;
	    private DroneInputs _inputs;
	    private Rigidbody _rb;
	
	    private PIDController _pitchPID;
	    private PIDController _rollPID;
	    private PIDController _yawPID;
	    private PIDController _altPID;
	
	    private void Awake()
	    {
	        _hardware = GetComponent<DroneHardware>();
	        _inputs = GetComponent<DroneInputs>();
	        _rb = GetComponent<Rigidbody>();
	
	        _pitchPID = new PIDController(pitchSettings);
	        _rollPID = new PIDController(rollSettings);
	        _yawPID = new PIDController(yawSettings);
	        _altPID = new PIDController(altitudeSettings);
	    }
	
	    private void FixedUpdate()
	    {
	        float dt = Time.fixedDeltaTime;
	
	        float targetPitch = _inputs.Cyclic.y * maxTiltAngle;
	        float targetRoll = -_inputs.Cyclic.x * maxTiltAngle;
	        float targetYawRate = _inputs.Yaw * maxYawSpeed;
	        float targetVelY = _inputs.Throttle * maxClimbSpeed;
	
	        Vector3 currentRotation = NormalizeAngles(transform.localEulerAngles);
			Vector3 localVelocity = transform.InverseTransformDirection(_rb.linearVelocity);
	        Vector3 localAngularVel = transform.InverseTransformDirection(_rb.angularVelocity);
	
	        float errorPitch = targetPitch - currentRotation.x;
	        float errorRoll = targetRoll - currentRotation.z;
	        float errorYaw = targetYawRate - (localAngularVel.y * Mathf.Rad2Deg);
	        float errorAlt = targetVelY - _rb.linearVelocity.y;
	        
	        float pitchCorrection = _pitchPID.Compute(errorPitch, dt);
	        float rollCorrection = _rollPID.Compute(errorRoll, dt);
	        float yawCorrection = _yawPID.Compute(errorYaw, dt);
	        float altCorrection = _altPID.Compute(errorAlt, dt);
	        
	        float gravityCompensation = 9.81f * _rb.mass;
	        float totalThrottle = /*gravityCompensation + */altCorrection;
	        
	        // Front Left (CW)
	        float fl = totalThrottle - pitchCorrection - rollCorrection - yawCorrection;
	        // Front Right (CCW)
	        float fr = totalThrottle - pitchCorrection + rollCorrection + yawCorrection;
	        // Back Left (CCW)
	        float bl = totalThrottle + pitchCorrection - rollCorrection + yawCorrection;
	        // Back Right (CW)
	        float br = totalThrottle + pitchCorrection + rollCorrection - yawCorrection;
	
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