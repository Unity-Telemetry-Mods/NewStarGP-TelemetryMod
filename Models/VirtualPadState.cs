namespace com.drowhunter.NewStarGPTelemetryMod
{
    /// <summary>
    /// Represents the desired state of the virtual Xbox 360 gamepad to be submitted to ViGEm.
    /// </summary>
    internal struct VirtualPadState
    {
        /// <summary>Left stick X axis: -1..1 (left/right). Mapped from wheel steer angle.</summary>
        public float LeftStickX;

        /// <summary>Left stick Y axis: -1..1 (down/up). Reserved for future use.</summary>
        public float LeftStickY;

        /// <summary>Right stick X axis: -1..1. Reserved for future use.</summary>
        public float RightStickX;

        /// <summary>Right stick Y axis: -1..1. Reserved for future use.</summary>
        public float RightStickY;

        /// <summary>Left trigger (gas/throttle): 0..1.</summary>
        public float LeftTrigger;

        /// <summary>Right trigger (brake): 0..1.</summary>
        public float RightTrigger;

        /// <summary>Xbox button bitmask. See Nefarius.ViGEm.Client.Targets.Xbox360.Xbox360Button.</summary>
        public ushort Buttons;
    }
}
