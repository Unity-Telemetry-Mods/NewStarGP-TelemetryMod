using BepInEx;
using BepInEx.Logging;

using NSGP;

using System.Net;
using System.Runtime.InteropServices;

using TelemetryLib.Telemetry;

using TelemetryLib;

using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine.UIElements;
using BepInEx.Configuration;

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

        private ConfigEntry<float> configAccelSmoothing;

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

        static bool paused = false;


        //[HarmonyPatch(typeof(PauseMenu), nameof(PauseMenu.Update))]
        //class Patch
        //{
        //    static void Postfix(PauseMenu __instance)
        //    {
        //        paused = __instance.paused;

                

        //    }
        //}


        private void Awake()
        {
            //Harmony.CreateAndPatchAll(typeof(NewStarTelemetryPlugin));
            //var harmony = new Harmony("com.drowhunter.NewStarTelemetryPlugin");
            //harmony.PatchAll();


            // Plugin startup logic
            Logger = base.Logger;
            


            configAccelSmoothing = Config.Bind("Telemetry", "AccelSmoothing", 0.1f, "Smoothing factor for AccelZ");



            _udp = new UdpTelemetry<NewStarTelemetryData>(new UdpTelemetryConfig
            {
                SendAddress = new IPEndPoint(IPAddress.Loopback, 12345)
            });

            Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
        }

        

        private void Start()
        {
            telemetryExtractor = new TelemetryExtractor();
        }

        

        
        private Delta[] deltas = Enumerable.Range(0, 4).Select(_ => new Delta()).ToArray();

        enum wheel { rr, fr, rl, fl };
        void GetWheels(Vehicle vehicle)
        {
            var localwheels = vehicle.wheels.Select(w => w.transform.position.y).ToArray();

            var avg = localwheels.Average();
            for (var i = 0; i < localwheels.Length; i++)
            {
                deltas[i].Update(localwheels[i] - avg);                
            }
        }

        
        
        

        private void FixedUpdate()
        {
            bool doTelemetry = true;

            if (!_carControl)
            {
                return;
            }
            var isRacing = TWK.RaceState == "Racing";
            var isRaceOver = TWK.ChallengeEventOver == 1;

            var vehicle = _carControl.vehicle;
            var allowDriving = vehicle.allowDriving;
            

            doTelemetry = (allowDriving && !isRaceOver && isRacing);

            if(!doTelemetry)
            {
                return;
            }

            var data = new NewStarTelemetryData();


            var cRigidbody = vehicle.rigid;
            telemetryExtractor.Update(cRigidbody);

            if (!cRigidbody)
            {
                Logger.LogInfo("Rigidbody is null");
                return;
            }

            var basic = telemetryExtractor.ExtractTelemetry();

            GetWheels(vehicle);
            
            data = new NewStarTelemetryData
            {
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
                RPM = vehicle.engine.maxRPM != 0 ? vehicle.engine.RPM / vehicle.engine.maxRPM : 0,
                CurrentGear = _carControl.vehicle.gearbox.targetGear,

                TireFL = deltas[(int)wheel.fl] / Time.fixedDeltaTime,
                TireFR = deltas[(int)wheel.fr] / Time.fixedDeltaTime,
                TireBL = deltas[(int)wheel.rl] / Time.fixedDeltaTime,
                TireBR = deltas[(int)wheel.rr] / Time.fixedDeltaTime,
               

                WheelsOnTrack = vehicle.wheelsOnTrack,
                IsBoosting = vehicle.slipstream.boosting,
                AllowDriving = allowDriving,
                IsRacing = isRacing,
                IsEventOver = isRaceOver,

            };
            
            _udp.Send(data);

        }
    }

    public class Delta : MonoBehaviour
    {
        private float _lastValue;

        public float Value { get; private set; }

        public float Update(float currentValue)
        {
            Value = currentValue - _lastValue;
            _lastValue = currentValue;
            return Value;
        }

        public static implicit operator float(Delta d) => d.Value;
    }
}