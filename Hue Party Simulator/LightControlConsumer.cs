using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace Hue_Party_Simulator
{
    /// <summary>
    /// Class used for controlling Hue Lights
    /// </summary>
    public class LightControlConsumer
    {
        // Hue Controller Vals.
        public LightController ControllerBase;

        /// <summary>
        /// Constructor for LightControl
        /// </summary>
        public LightControlConsumer()
        {
            // Init our controller first.
            ControllerBase = new LightController();
        }

        /// <summary>
        /// Cycle light colors moron, obviously its called CycleColors
        /// </summary>
        public void CycleColorsOrReset(bool ToDefault = false)
        {
            var AllLightInstances = ControllerBase.LightObjects.ToList();
            if (ToDefault) { AllLightInstances = ControllerBase.DefaultLightValues.ToList(); }

            Parallel.ForEach(AllLightInstances, (CurrentLightObject) =>
            {
                int Indexer = AllLightInstances.IndexOf(CurrentLightObject);
                int LightName = AllLightInstances[Indexer].LightIndex;
                var CurrentInstance = AllLightInstances[Indexer].LightObject;

                if (!ToDefault)
                {
                    int SetHue = CurrentInstance.Hue + ControllerBase.HueStep;
                    if (SetHue > ControllerBase.MaxHue) { SetHue -= ControllerBase.MaxHue; }

                    CurrentInstance.Hue = SetHue;
                }

                string ApiStirng = "lights/" + LightName + "/state";
                ControllerBase.HttpWebClient.PutAsJsonAsync(ApiStirng, CurrentInstance);
            });

            #region Split Cmds
            /* Commented Out. Try the code ive got up above^
            _ = HttpClientObject.PutAsJsonAsync("lights/1/state", LightInstance).Result;
            _ = HttpClientObject.PutAsJsonAsync("lights/2/state", LightInstance).Result;
            _ = HttpClientObject.PutAsJsonAsync("lights/4/state", LightInstance).Result;
            _ = HttpClientObject.PutAsJsonAsync("lights/5/state", LightInstance).Result;
            */
            #endregion
        }

        /// <summary>
        /// Disco toggle lights on and off 
        /// </summary>
        /// <param name="ToDefault">Set if we turn lights back to normal or not</param>
        public void DiscoOrReset(bool ToDefault = false)
        {
            var AllLightInstances = ControllerBase.LightObjects.ToList();
            if (ToDefault) { AllLightInstances = ControllerBase.DefaultLightValues.ToList(); }

            Parallel.ForEach(AllLightInstances, (CurrentLightObject) =>
            {
                int Indexer = AllLightInstances.IndexOf(CurrentLightObject);
                int LightName = AllLightInstances[Indexer].LightIndex;

                var CurrentInstance = CurrentLightObject.LightObject;
                string ApiStirng = "lights/" + LightName + "/state";

                if (ToDefault) { ControllerBase.HttpWebClient.PutAsJsonAsync(ApiStirng, CurrentLightObject); }
                else
                {
                    CurrentInstance.On = true;
                    ControllerBase.HttpWebClient.PutAsJsonAsync(ApiStirng, CurrentLightObject);

                    // Wait 100ms
                    System.Threading.Thread.Sleep(100);

                    CurrentInstance.On = false;
                    ControllerBase.HttpWebClient.PutAsJsonAsync(ApiStirng, CurrentLightObject);
                }
            });
        }
    }
}
