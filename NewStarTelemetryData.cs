using System.Runtime.InteropServices;

namespace com.drowhunter.NewStarGPTelemetryMod
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct NewStarTelemetryData
    {
        /// <summary>
        /// Vehicle pitch angle.
        /// Unity Euler angle component.
        /// Unit: degrees.
        /// </summary>
        public float Pitch;

        /// <summary>
        /// Vehicle yaw/heading angle.
        /// Unity Euler angle component.
        /// Unit: degrees.
        /// </summary>
        public float Yaw;

        /// <summary>
        /// Vehicle roll/bank angle.
        /// Unity Euler angle component.
        /// Unit: degrees.
        /// </summary>
        public float Roll;

        /// <summary>
        /// Local angular velocity around X axis.
        /// Unit: radians per second (Unity Rigidbody angular velocity convention unless converted upstream).
        /// </summary>
        public float AngularVelocityX;

        /// <summary>
        /// Local angular velocity around Y axis.
        /// Unit: radians per second (Unity Rigidbody angular velocity convention unless converted upstream).
        /// </summary>
        public float AngularVelocityY;

        /// <summary>
        /// Local angular velocity around Z axis.
        /// Unit: radians per second (Unity Rigidbody angular velocity convention unless converted upstream).
        /// </summary>
        public float AngularVelocityZ;

        /// <summary>
        /// Centripetal-related value from telemetry extractor.
        /// Unit depends on extractor formula:
        /// - v^2 / r            => m/s^2
        /// - (v^2 / r) / 9.81   => g
        /// - m * v^2 / r        => N
        /// </summary>
        public float cForce;

        /// <summary>
        /// Local velocity X component.
        /// Unit: meters per second (m/s).
        /// </summary>
        public float VelocityX;

        /// <summary>
        /// Local velocity Y component.
        /// Unit: meters per second (m/s).
        /// </summary>
        public float VelocityY;

        /// <summary>
        /// Local velocity Z component.
        /// Unit: meters per second (m/s).
        /// </summary>
        public float VelocityZ;

        /// <summary>
        /// Local acceleration X component.
        /// Unit: meters per second squared (m/s^2), unless normalized upstream.
        /// </summary>
        public float AccelX;

        /// <summary>
        /// Local acceleration Y component.
        /// Unit: meters per second squared (m/s^2), unless normalized upstream.
        /// </summary>
        public float AccelY;

        /// <summary>
        /// Local acceleration Z component.
        /// Unit: meters per second squared (m/s^2), unless normalized upstream.
        /// </summary>
        public float AccelZ;

        /// <summary>
        /// Scalar speed.
        /// Unit: meters per second (m/s).
        /// </summary>
        public float Speed;

        /// <summary>
        /// Normalized engine RPM ratio.
        /// Computed as engine.RPM / engine.maxRPM.
        /// Unit: unitless ratio (typically 0..1), not raw RPM.
        /// </summary>
        public float RPM;

        /// <summary>
        /// Current gearbox target gear.
        /// Unit: unitless integer.
        /// </summary>
        public int CurrentGear;

        /// <summary>
        /// Front-left wheel vertical relative motion rate.
        /// Derived from wheel Y delta (relative to average wheel Y) divided by fixedDeltaTime.
        /// Unit: meters per second (m/s).
        /// </summary>
        public float TireFL;

        /// <summary>
        /// Front-right wheel vertical relative motion rate.
        /// Derived from wheel Y delta (relative to average wheel Y) divided by fixedDeltaTime.
        /// Unit: meters per second (m/s).
        /// </summary>
        public float TireFR;

        /// <summary>
        /// Back-left wheel vertical relative motion rate.
        /// Derived from wheel Y delta (relative to average wheel Y) divided by fixedDeltaTime.
        /// Unit: meters per second (m/s).
        /// </summary>
        public float TireBL;

        /// <summary>
        /// Back-right wheel vertical relative motion rate.
        /// Derived from wheel Y delta (relative to average wheel Y) divided by fixedDeltaTime.
        /// Unit: meters per second (m/s).
        /// </summary>
        public float TireBR;

        /// <summary>
        /// Number of wheels currently on track.
        /// Unit: count.
        /// </summary>
        public int WheelsOnTrack;

        /// <summary>
        /// Whether slipstream boost is active.
        /// </summary>
        public bool IsBoosting;

        /// <summary>
        /// Whether driving input/state allows active driving.
        /// </summary>
        internal bool AllowDriving;

        /// <summary>
        /// Whether race state is currently "Racing".
        /// </summary>
        internal bool IsRacing;

        /// <summary>
        /// Whether the current challenge event is over.
        /// </summary>
        internal bool IsEventOver;
    }
}
