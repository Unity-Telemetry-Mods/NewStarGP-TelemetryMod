using System.Runtime.InteropServices;

namespace com.drowhunter.NewStarGPTelemetryMod
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct NewStarTelemetryData
    {
        //public Quaternion Orientation;

        public float Pitch;
        public float Yaw;
        public float Roll;

        public float AngularVelocityX;
        public float AngularVelocityY;
        public float AngularVelocityZ;

        public float cForce;

        public float VelocityX;
        public float VelocityY;
        public float VelocityZ;

        public float AccelX;
        public float AccelY;
        public float AccelZ;

        /// <summary>
        /// Speed in Meter per Second
        /// </summary>
        public float Speed;       
        public float RPM;        
        public int CurrentGear;
        
        public float TireFL;
        public float TireFR;
        public float TireBL;
        public float TireBR;

        public int WheelsOnTrack;
        public bool IsBoosting;
        
        internal bool AllowDriving;
        internal bool IsRacing;
        internal bool IsEventOver;

        //public float WheelFL;
        //public float WheelFR;
        //public float WheelBL;
        //public float WheelBR;
        //public bool GamePaused;
        //public bool IsRacing;
        //public bool Boost;




    }


}