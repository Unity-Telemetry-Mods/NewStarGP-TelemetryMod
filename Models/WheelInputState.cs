namespace com.drowhunter.NewStarGPTelemetryMod
{
    /// <summary>
    /// Snapshot of the physical wheel's current input state, normalised to -1..1 or 0..1 where applicable.
    /// </summary>
    internal struct WheelInputState
    {
        /// <summary>Steering angle normalised to -1..1 (left to right).</summary>
        public float SteerAngle;

        /// <summary>Steering velocity in normalised units per second.</summary>
        public float SteerVelocity;

        /// <summary>Throttle axis 0..1 (0 = released, 1 = fully pressed).</summary>
        public float Throttle;

        /// <summary>Brake axis 0..1.</summary>
        public float Brake;

        /// <summary>Clutch axis 0..1.</summary>
        public float Clutch;

        /// <summary>Raw 128-element button state array from the HID report. May be null if no data available.</summary>
        public bool[] Buttons;
    }
}
