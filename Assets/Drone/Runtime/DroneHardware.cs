using UnityEngine;

namespace Drone.Runtime
{
	[RequireComponent(typeof(Rigidbody))]
	public class DroneHardware : MonoBehaviour
	{
		[Header("Chassis")] 
		public float mass = 1.0f;
		public float armLength = 0.5f;
		public float motorHeight = 0.1f;
		public float torqueFactor = 0.02f; // TODO: Change to more physics correct calculation
		
		[Header("Visualization")]
		public Vector3 bodySize = new Vector3(0.3f, 0.1f, 0.15f);
		public Vector3 motorSize = new Vector3(0.05f, 0.05f, 0.05f);
		public bool	showDebugVectors = false;
		
		public float[] CurrentForces => _currentForces;

		private Transform[] _motorTransforms;
		private float[] _currentForces;
		private Rigidbody _rb;
		
		private void Awake()
		{
			_rb = GetComponent<Rigidbody>();
			_rb.mass = mass;
			
			_currentForces = new float[4];
			BuildChassis();
		}

		private void BuildChassis()
		{
			GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
			body.name = "BodyVisual";
			body.transform.SetParent(transform);
			body.transform.localPosition = Vector3.zero;
			body.transform.localScale = bodySize;
			Destroy(body.GetComponent<Collider>());

			float dist = armLength * 0.7071f; // 0.7071 ~= sin(45)

			Vector3[] positions = new Vector3[]
			{
				new Vector3(-dist, motorHeight, dist),  // Front Left
				new Vector3(dist, motorHeight, dist),   // Front Right
				new Vector3(-dist, motorHeight, -dist), // Back Left
				new Vector3(dist, motorHeight, -dist)   // Back Right
			};

			_motorTransforms = new Transform[4];

			for (int i = 0; i < 4; i++)
			{
				GameObject motor = GameObject.CreatePrimitive(PrimitiveType.Cube);
				motor.name = $"Motor_{i}";
				motor.transform.SetParent(transform);
				motor.transform.localPosition = positions[i];
				motor.transform.localScale = motorSize;
				Destroy(motor.GetComponent<Collider>());
            
				var motorRenderer = motor.GetComponent<Renderer>();
				motorRenderer.material.color = (i < 2) ? Color.red : Color.gray;

				_motorTransforms[i] = motor.transform;
			}
		}
		
		public void ApplyMotorForces(float fl, float fr, float bl, float br)
		{
			_currentForces[0] = fl;
			_currentForces[1] = fr;
			_currentForces[2] = bl;
			_currentForces[3] = br;

			Vector3 forceDir = transform.up;

			Debug.Log($"FL: {fl}, FR: {fr}, BL: {bl}, BR: {br}");
			_rb.AddForceAtPosition(forceDir * fl, _motorTransforms[0].position);
			_rb.AddForceAtPosition(forceDir * fr, _motorTransforms[1].position);
			_rb.AddForceAtPosition(forceDir * bl, _motorTransforms[2].position);
			_rb.AddForceAtPosition(forceDir * br, _motorTransforms[3].position);
			
			float torqueFL = -fl * torqueFactor;
			float torqueFR =  fr * torqueFactor;
			float torqueBL =  bl * torqueFactor;
			float torqueBR = -br * torqueFactor;

			float totalYawTorque = torqueFL + torqueFR + torqueBL + torqueBR;
			_rb.AddRelativeTorque(transform.up * totalYawTorque);
		}
		
		private void OnDrawGizmos()
		{
			if (!showDebugVectors || _motorTransforms == null || _rb == null) return;

			Gizmos.color = Color.green;
			Vector3 velocity = Application.isPlaying ? _rb.linearVelocity : Vector3.zero;
        
			Gizmos.DrawRay(transform.position, velocity);
        
			if (velocity.magnitude > 0.1f)
				Gizmos.DrawSphere(transform.position + velocity, 0.05f);

			Gizmos.color = Color.yellow;
			float forceScale = 0.05f; 

			for (int i = 0; i < 4; i++)
			{
				if (_motorTransforms[i] == null) continue;

				Vector3 pos = _motorTransforms[i].position;
				Vector3 forceVector = transform.up * _currentForces[i] * forceScale;

				Gizmos.DrawRay(pos, forceVector);
            
				if (_currentForces[i] > 0)
					Gizmos.DrawWireSphere(pos + forceVector, 0.02f);
			}
		}
	}
}