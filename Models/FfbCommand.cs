namespace com.drowhunter.NewStarGPTelemetryMod
{
    /// <summary>
    /// The computed force-feedback command for one physics frame.
    /// </summary>
    internal struct FfbCommand
    {
        /// <summary>
        /// Lateral damper coefficient derived from centripetal force. Range -1..1.
        /// Positive values push the wheel right; negative push left.
        /// Sent to the ETDamper effect as positive/negative coefficient.
        /// </summary>
        public float DamperCoefficient;

        /// <summary>
        /// Whether a short bump impulse should fire this frame.
        /// </summary>
        public bool HasImpulse;

        /// <summary>
        /// Magnitude of the bump impulse. Range -1..1.
        /// Positive = right-side bump, negative = left-side bump.
        /// Sent to the ETConstantForce effect.
        /// </summary>
        public float ImpulseMagnitude;
    }
}
