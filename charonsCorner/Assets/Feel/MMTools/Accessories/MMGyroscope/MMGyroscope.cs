using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem;
using ISGyroscope = UnityEngine.InputSystem.Gyroscope;
#endif

namespace MoreMountains.Tools
{
	/// <summary>
	/// A base gyroscope class, meant to be accessed statically or extended, that exposes a number of gyroscope values.
	/// CAREFUL: this will work fine on an iOS or Android build or any other device with a gyroscope, in build.
	/// Unity Remote won't send most of these gyro values (as of early May 2026, there's still hope it'll be fixed someday).
	/// You can look at the MMGyroParallax class for an example of implementation.
	/// </summary>
	[AddComponentMenu("More Mountains/Tools/Gyroscope/MM Gyroscope")]
	public class MMGyroscope : MonoBehaviour
	{
		public enum TimeScales { Scaled, Unscaled }

		/// whether or not the gyroscope is active
		public static bool GyroscopeActive = true;
		/// the timescale to use (scaled or unscaled)
		public static TimeScales TimeScale = TimeScales.Scaled;
		/// the min/max clamp values applied to all acceleration and gravity vectors
		public static Vector2 Clamps = new Vector2(-1f, 1f);
		/// the speed at which lerped values move towards their targets
		public static float LerpSpeed = 1f;
		/// whether or not to use test values instead of real sensor data
		public static bool TestMode;
        
		[Header("Debug")]
		/// turn this on if you want to use the inspector to test this camera
		public bool _TestMode = false;
		/// the rotation to apply on the x axiswhen in test mode
		[Range(-1f, 1f)]
		public float TestXAcceleration = 0f;
		/// the rotation to apply on the y axis while in test mode
		[Range(-1f, 1f)]
		public float TestYAcceleration = 0f;
		/// the rotation to apply on the y axis while in test mode
		[Range(-1f, 1f)]
		public float TestZAcceleration = 0f;

		/// the current device orientation as a quaternion (from AttitudeSensor / Input.gyro.attitude)
		public static Quaternion GyroscopeAttitude { get { GetValues(); return m_GyroscopeAttitude; } }
		/// the current unbiased rotation rate in radians per second around each axis (from Gyroscope / Input.gyro.rotationRateUnbiased)
		public static Vector3 GyroscopeRotationRate { get { GetValues(); return m_GyroscopeRotationRate; } }
		/// the current user-induced acceleration (gravity removed) in g-force units (from LinearAccelerationSensor / Input.gyro.userAcceleration)
		public static Vector3 GyroscopeAcceleration { get { GetValues(); return m_GyroscopeAcceleration; } }
		/// the current raw accelerometer reading including gravity, clamped to the Clamps range (from Accelerometer / Input.acceleration)
		public static Vector3 InputAcceleration { get { GetValues(); return m_InputAcceleration; } }
		/// the current gravity vector as reported by the sensor, clamped to the Clamps range (from GravitySensor / Input.gyro.gravity)
		public static Vector3 GyroscopeGravity { get { GetValues(); return m_GyroscopeGravity; } }

		/// the device orientation captured at auto-calibration time (~0.5 s after start)
		public static Quaternion InitialGyroscopeAttitude { get { GetValues(); return m_InitialGyroscopeAttitude; } }
		/// the rotation rate captured at auto-calibration time
		public static Vector3 InitialGyroscopeRotationRate { get { GetValues(); return m_InitialGyroscopeRotationRate; } }
		/// the user acceleration captured at auto-calibration time
		public static Vector3 InitialGyroscopeAcceleration { get { GetValues(); return m_InitialGyroscopeAcceleration; } }
		/// the raw accelerometer reading captured at auto-calibration time
		public static Vector3 InitialInputAcceleration { get { GetValues(); return m_InitialInputAcceleration; } }
		/// the gravity vector captured at auto-calibration time
		public static Vector3 InitialGyroscopeGravity { get { GetValues(); return m_InitialGyroscopeGravity; } }

		/// the raw accelerometer reading rotated into the calibration reference frame
		public static Vector3 CalibratedInputAcceleration { get { GetValues(); return m_CalibratedInputAcceleration; } }
		/// the gravity vector rotated into the calibration reference frame
		public static Vector3 CalibratedGyroscopeGravity { get { GetValues(); return m_CalibratedGyroscopeGravity; } }

		/// CalibratedInputAcceleration smoothed over time using LerpSpeed
		public static Vector3 LerpedCalibratedInputAcceleration { get { GetValues(); return m_LerpedCalibratedInputAcceleration; } }
		/// CalibratedGyroscopeGravity smoothed over time using LerpSpeed
		public static Vector3 LerpedCalibratedGyroscopeGravity { get { GetValues(); return m_LerpedCalibratedGyroscopeGravity; } }
        
		private static Quaternion m_GyroscopeAttitude;
		private static Vector3 m_GyroscopeRotationRate;
		private static Vector3 m_GyroscopeAcceleration;
		private static Vector3 m_InputAcceleration;
		private static Vector3 m_GyroscopeGravity;
		private static Quaternion m_InitialGyroscopeAttitude;
		private static Vector3 m_InitialGyroscopeRotationRate;
		private static Vector3 m_InitialGyroscopeAcceleration;
		private static Vector3 m_InitialInputAcceleration;
		private static Vector3 m_InitialGyroscopeGravity;
		private static Vector3 m_CalibratedInputAcceleration;
		private static Vector3 m_CalibratedGyroscopeGravity;
		private static Vector3 m_LerpedCalibratedInputAcceleration;
		private static Vector3 m_LerpedCalibratedGyroscopeGravity;

		[Header("Settings")]
		/// whether this rig should move in scaled or unscaled time
		[SerializeField]
		private TimeScales _TimeScale = TimeScales.Scaled;
		/// the clamps to apply to the values
		[SerializeField]
		private Vector2 _Clamps = new Vector2(-1f, 1f);
		/// the speed at which to move towards the new position
		[SerializeField]
		private float _LerpSpeed = 1f;

		[Header("Raw Values")]
		[MMReadOnly]
		[SerializeField]
		private Quaternion _GyroscopeAttitude;
		[MMReadOnly]
		[SerializeField]
		private Vector3 _GyroscopeRotationRate;
		[MMReadOnly]
		[SerializeField]
		private Vector3 _GyroscopeAcceleration;
		[MMReadOnly]
		[SerializeField]
		private Vector3 _InputAcceleration;
		[MMReadOnly]
		[SerializeField]
		private Vector3 _GyroscopeGravity;

		[Header("AutoCalibration Values")]
		[MMReadOnly]
		[SerializeField]
		private Quaternion _InitialGyroscopeAttitude;
		[MMReadOnly]
		[SerializeField]
		private Vector3 _InitialGyroscopeRotationRate;
		[MMReadOnly]
		[SerializeField]
		private Vector3 _InitialGyroscopeAcceleration;
		[MMReadOnly]
		[SerializeField]
		private Vector3 _InitialInputAcceleration;
		[MMReadOnly]
		[SerializeField]
		private Vector3 _InitialGyroscopeGravity;

		[Header("Relative Values")]
		[MMReadOnly]
		[SerializeField]
		private Vector3 _CalibratedInputAcceleration;
		[MMReadOnly]
		[SerializeField]
		private Vector3 _CalibratedGyroscopeGravity;

		[Header("Lerped Values")]
		[MMReadOnly]
		[SerializeField]
		private Vector3 _LerpedCalibratedInputAcceleration;
		[MMReadOnly]
		[SerializeField]
		private Vector3 _LerpedCalibratedGyroscopeGravity;

		[MMInspectorButton("CalibrateDebug")]
		public bool CalibrateButton;
		[MMInspectorButton("GyroscopeDebugInitialization")]
		public bool GyroscopeInitializationBtn;

		#if !ENABLE_INPUT_SYSTEM || ENABLE_LEGACY_INPUT_MANAGER
		private static Gyroscope _gyroscope;
		#endif
		protected static Vector3 _testVector = Vector3.zero;
		private static bool _initialized = false;
		private static Matrix4x4 _accelerationMatrix;
		private static Matrix4x4 _gravityMatrix;
		private static float _lastGetValuesAt = 0f;
        
		protected virtual void OnEnable()
		{
			#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
			InputSystem.onDeviceChange += OnDeviceChange;
			#endif
		}

		protected virtual void OnDisable()
		{
			#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
			InputSystem.onDeviceChange -= OnDeviceChange;
			#endif
		}

		#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
		private static void OnDeviceChange(InputDevice device, InputDeviceChange change)
		{
			if (change == InputDeviceChange.Added || change == InputDeviceChange.Enabled || change == InputDeviceChange.Reconnected)
			{
				// Enable the device directly — .current may still be null for other sensors at this point
				if (device is Sensor sensor)
				{
					InputSystem.EnableDevice(sensor);
					// Reset calibration so it re-runs once real data is flowing from the new device
					_initialized = false;
				}
			}
		}
		#endif

		protected virtual void Start()
		{
			TimeScale = _TimeScale;
			Clamps = _Clamps;
			LerpSpeed = _LerpSpeed;
			GyroscopeInitialization();
		}


		public virtual void CalibrateDebug()
		{
			Calibrate();
		}

		public virtual void GyroscopeDebugInitialization()
		{
			GyroscopeInitialization();
		}

		public static void GyroscopeInitialization()
		{
			#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
			foreach (var device in InputSystem.devices)
			{
				if (device is Sensor sensor)
				{
					InputSystem.EnableDevice(sensor);
				}
			}
			InputSystem.EnableDevice(UnityEngine.InputSystem.Gyroscope.current);
			InputSystem.EnableDevice(GravitySensor.current);
			UnityEngine.InputSystem.Gyroscope.current.angularVelocity.ReadValue();
			Vector3 grav = GravitySensor.current.gravity.ReadValue();
			
			#else
			_gyroscope = Input.gyro;
			_gyroscope.enabled = true;
			#endif
		}

		protected virtual void Update()
		{
			TestMode = _TestMode;
			
			if (!GyroscopeActive)
			{
				return;
			}
			HandleTestMode();
			GetValues();
			_GyroscopeAttitude = m_GyroscopeAttitude;
			_GyroscopeRotationRate = m_GyroscopeRotationRate;
			_GyroscopeAcceleration = m_GyroscopeAcceleration;
			_InputAcceleration = m_InputAcceleration;
			_GyroscopeGravity = m_GyroscopeGravity;
			_InitialGyroscopeAttitude = m_InitialGyroscopeAttitude;
			_InitialGyroscopeRotationRate = m_InitialGyroscopeRotationRate;
			_InitialGyroscopeAcceleration = m_InitialGyroscopeAcceleration;
			_InitialInputAcceleration = m_InitialInputAcceleration;
			_InitialGyroscopeGravity = m_InitialGyroscopeGravity;
			_CalibratedInputAcceleration = m_CalibratedInputAcceleration;
			_CalibratedGyroscopeGravity = m_CalibratedGyroscopeGravity;
			_LerpedCalibratedInputAcceleration = m_LerpedCalibratedInputAcceleration;
			_LerpedCalibratedGyroscopeGravity = m_LerpedCalibratedGyroscopeGravity;
		}

		public static void GetValues()
		{
			if (Time.frameCount == _lastGetValuesAt)
			{
				return;
			}
			AutoCalibration();
			GetGyroValues();
			_lastGetValuesAt = Time.frameCount;
		}

		private static void GetGyroValues()
		{
			float deltaTime = (TimeScale == TimeScales.Scaled) ? Time.deltaTime : Time.unscaledDeltaTime;

			#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
			var attitudeSensor = GetSensor<AttitudeSensor>();
			var gyroSensor = GetSensor<ISGyroscope>();
			var linearAccelSensor = GetSensor<LinearAccelerationSensor>();

			m_GyroscopeAttitude = attitudeSensor != null
				? attitudeSensor.attitude.ReadValue()
				: Quaternion.identity;
			m_GyroscopeRotationRate = gyroSensor != null
				? gyroSensor.angularVelocity.ReadValue()
				: Vector3.zero;
			m_GyroscopeAcceleration = linearAccelSensor != null
				? linearAccelSensor.acceleration.ReadValue()
				: Vector3.zero;
			#else
			m_GyroscopeAttitude = GyroscopeToUnity(Input.gyro.attitude);
			m_GyroscopeRotationRate = Input.gyro.rotationRateUnbiased;
			m_GyroscopeAcceleration = Input.gyro.userAcceleration;
			#endif
            
			GetAccelerationAndGravity();
			ClampAcceleration();

			m_CalibratedInputAcceleration = CalibratedAcceleration(m_InputAcceleration, _accelerationMatrix);
			m_CalibratedGyroscopeGravity = CalibratedAcceleration(m_GyroscopeGravity, _gravityMatrix);

			m_LerpedCalibratedInputAcceleration = Vector3.Lerp(m_LerpedCalibratedInputAcceleration, m_CalibratedInputAcceleration, deltaTime * LerpSpeed);
			m_LerpedCalibratedGyroscopeGravity = Vector3.Lerp(m_LerpedCalibratedGyroscopeGravity, m_CalibratedGyroscopeGravity, deltaTime * LerpSpeed);
		}

		private static void AutoCalibration()
		{
			if (!_initialized && Time.time > 0.5f)
			{
				#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
				var accelSensor = GetSensor<Accelerometer>();
				var gravitySensor = GetSensor<GravitySensor>();
				var attitudeSensor = GetSensor<AttitudeSensor>();
				var gyroSensor = GetSensor<ISGyroscope>();
				var linearAccelSensor = GetSensor<LinearAccelerationSensor>();

				Vector3 currentAccel = accelSensor != null
					? accelSensor.acceleration.ReadValue()
					: Vector3.zero;
				if (currentAccel == Vector3.zero)
				{
					return;
				}

				m_InitialGyroscopeAttitude = attitudeSensor != null
					? attitudeSensor.attitude.ReadValue()
					: Quaternion.identity;
				m_InitialGyroscopeRotationRate = gyroSensor != null
					? gyroSensor.angularVelocity.ReadValue()
					: Vector3.zero;
				m_InitialGyroscopeAcceleration = linearAccelSensor != null
					? linearAccelSensor.acceleration.ReadValue()
					: Vector3.zero;
				m_InitialInputAcceleration = currentAccel;
				m_InitialGyroscopeGravity = gravitySensor != null
					? gravitySensor.gravity.ReadValue()
					: Vector3.zero;
				#else
				m_InitialGyroscopeAttitude = GyroscopeToUnity(Input.gyro.attitude);
				m_InitialGyroscopeRotationRate = Input.gyro.rotationRateUnbiased;
				m_InitialGyroscopeAcceleration = Input.gyro.userAcceleration;
				m_InitialInputAcceleration = Input.acceleration;
				m_InitialGyroscopeGravity = Input.gyro.gravity;
				#endif

				Calibrate();

				_initialized = true;
			}
		}

		protected static Quaternion GyroscopeToUnity(Quaternion q)
		{
			return new Quaternion(q.x, q.y, -q.z, -q.w);
		}

		#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
		private static T GetSensor<T>() where T : InputDevice
		{
			return InputSystem.GetDevice<T>() ?? (T)InputSystem.devices.FirstOrDefault(d => d is T);
		}
		#endif

		private static void ClampAcceleration()
		{
			m_InputAcceleration.x = Mathf.Clamp(m_InputAcceleration.x, Clamps.x, Clamps.y);
			m_InputAcceleration.y = Mathf.Clamp(m_InputAcceleration.y, Clamps.x, Clamps.y);
			m_InputAcceleration.z = Mathf.Clamp(m_InputAcceleration.z, Clamps.x, Clamps.y);

			m_GyroscopeGravity.x = Mathf.Clamp(m_GyroscopeGravity.x, Clamps.x, Clamps.y);
			m_GyroscopeGravity.y = Mathf.Clamp(m_GyroscopeGravity.y, Clamps.x, Clamps.y);
			m_GyroscopeGravity.z = Mathf.Clamp(m_GyroscopeGravity.z, Clamps.x, Clamps.y);
		}

		protected virtual void HandleTestMode()
		{
			if (TestMode)
			{
				_testVector.x = TestXAcceleration;
				_testVector.y = TestYAcceleration;
				_testVector.z = TestZAcceleration;
				m_InputAcceleration = _testVector;
				m_GyroscopeGravity = _testVector;
			}
			else
			{
				GetAccelerationAndGravity();
			}
		}

		private static void GetAccelerationAndGravity()
		{
			if (!TestMode)
			{
				#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
				var accelSensor = GetSensor<Accelerometer>();
				var gravitySensor = GetSensor<GravitySensor>();
				m_InputAcceleration = accelSensor != null
					? accelSensor.acceleration.ReadValue()
					: Vector3.zero;
				m_GyroscopeGravity = gravitySensor != null
					? gravitySensor.gravity.ReadValue()
					: Vector3.zero;
				#else
				m_InputAcceleration = Input.acceleration;
				m_GyroscopeGravity = Input.gyro.gravity;
				#endif
			}
		}

		private static void Calibrate()
		{
			_accelerationMatrix = CalibrateAcceleration(m_InputAcceleration);
			_gravityMatrix = CalibrateAcceleration(m_GyroscopeGravity);
		}

		private static Matrix4x4 CalibrateAcceleration(Vector3 initialAcceleration)
		{
			Quaternion rotationQuaternion = Quaternion.FromToRotation(-Vector3.forward, initialAcceleration);
			Matrix4x4 newMatrix = Matrix4x4.TRS(Vector3.zero, rotationQuaternion, Vector3.one);
			return newMatrix.inverse;
		}

		private static Vector3 CalibratedAcceleration(Vector3 accelerator, Matrix4x4 matrix)
		{
			Vector3 fixedAcceleration = matrix.MultiplyVector(accelerator);
			return fixedAcceleration;
		}
	}
}