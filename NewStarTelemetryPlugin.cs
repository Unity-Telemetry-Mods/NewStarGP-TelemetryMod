using BepInEx;
using BepInEx.Logging;

using NSGP;

using System.Net;
using System.Runtime.InteropServices;

using TelemetryLib.Telemetry;

using TelemetryLib;

using UnityEngine;

namespace com.drowhunter.NewStarGPTelemetryMod
{

    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    public class NewStarTelemetryPlugin : BaseUnityPlugin
    {
        internal static new ManualLogSource Logger;

        TelemetryExtractor telemetryExtractor;

        //GameManager gameManager;


        internal NewStarTelemetryData data;

        UdpTelemetry<NewStarTelemetryData> _udp;

        RacingContextManager _racingContextManager;


        CarControl _carControl
        {
            get
            {
                if (!_racingContextManager) 
                { 
                    _racingContextManager = RacingContextManager.inst;
                }

                return _racingContextManager?.SinglePlayer?.Control;
            }
        }




        private void Awake()
        {


            // Plugin startup logic
            Logger = base.Logger;
            Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");

            //gameManager = NSGP.GameManager.inst;


            _udp = new UdpTelemetry<NewStarTelemetryData>(new UdpTelemetryConfig
            {
                SendAddress = new IPEndPoint(IPAddress.Loopback, 12345)
            });

        }

        private void Start()
        {
            telemetryExtractor = new TelemetryExtractor();
        }

        private GameObject fl, fr, rl, rr, chassis;
        
        void GetWheels()
        {
            if(fl != null && fr != null)
            {
                return;
            }

            fl = GameObject.Find("RacingContexts/SinglePlayerRacingContext/PlayerCarTemplate/Wheels/frontleft");
            fr = GameObject.Find("RacingContexts/SinglePlayerRacingContext/PlayerCarTemplate/Wheels/frontright");
            rl = GameObject.Find("RacingContexts/SinglePlayerRacingContext/PlayerCarTemplate/Wheels/rearleft");
            rr = GameObject.Find("RacingContexts/SinglePlayerRacingContext/PlayerCarTemplate/Wheels/rearright");
            chassis = GameObject.Find("RacingContexts/SinglePlayerRacingContext/PlayerCarTemplate/Car_chasis/Car_FrontWing");

            Logger.LogInfo($"Found Wheels: {fl?.name ?? "nope"} {fr?.name ?? "nope"}");
        }
        private void FixedUpdate()
        {
            if (_carControl == null)
            {
                return;
            }
            

            var vehicle = _carControl.vehicle;

            var gearbox = _carControl.vehicle.gearbox;

            var cRigidbody = vehicle.rigid;// _carControl.rb;
            telemetryExtractor.Update(cRigidbody);

            if (cRigidbody == null)
            {
                Logger.LogInfo("Rigidbody is null");
                return;
            }
            //_carControl.GetAcceleration();
            
            

            var basic = telemetryExtractor.ExtractTelemetry();
            GetWheels();

            var data = new NewStarTelemetryData
            {
                //Orientation = basic.Rotation,
                Pitch = basic.EulerAngles.x,
                Yaw = basic.EulerAngles.y,
                Roll = basic.EulerAngles.z,
                AngularVelocityX = basic.LocalAngularVelocity.x,
                AngularVelocityY = basic.LocalAngularVelocity.y,
                AngularVelocityZ = basic.LocalAngularVelocity.z,
                cForce = basic.CentripetalForce,
                VelocityX = basic.LocalVelocity.x,
                VelocityY = basic.LocalVelocity.y,
                VelocityZ = basic.LocalVelocity.z,
                AccelX = basic.Accel.x,
                AccelY = basic.Accel.y,
                AccelZ = basic.Accel.z,
                Speed = vehicle.speed,
                RPM = vehicle.engine.maxRPM != 0 ? vehicle.engine.RPM / vehicle.engine.maxRPM: 0,
                CurrentGear = gearbox.targetGear,
                WheelsOnTrack = vehicle.wheelsOnTrack,
                Boosting = vehicle.slipstream.boosting,
                TireFL = fl.transform.position.y,
                TireFR = fr.transform.position.y,
                TireBL = rl.transform.position.y,
                TireBR = rr.transform.position.y


            };

            _udp.Send(data);

        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct NewStarTelemetryData
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
        public int WheelsOnTrack;
        public bool Boosting;
        public float TireFL;
        public float TireFR;
        public float TireBL;
        public float TireBR;
        //public bool GamePaused;
        //public bool IsRacing;
        //public bool Boost;




    }


}