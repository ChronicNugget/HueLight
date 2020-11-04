using System;
using System.Net.Http;
using System.Net.Http.Json;

namespace Hue_Party_Simulator
{
    /// <summary>
    /// Class used for controlling Hue Lights
    /// </summary>
    public class LightControl
    {
        public static string URL = "http://192.168.42.3/api/7XzuDmSNw0US1gcx0sI7QiFfFnBOttyu8d0LX3kY/";

        public LightControlObject LightInstance;
        public LightControlObjectHue LightInstanceHue;
        public LightControlObjectOn LightInstanceOn;

        private int SetHue = 0;       // Start Hue Value (Begin at 0)      
        private int MaxHue = 65535;   // MAX Hue Value (Begin from 0)
        private int HueStep = 5000;   // Hue to increase each loop.

        /// <summary>
        /// Constructor for LightControl
        /// </summary>
        public LightControl()
        {
            LightInstance = new LightControlObject
            {
                //On = true,
                Bri = 0,
                Sat = 0,
                Hue = 0
            };

            LightInstanceHue = new LightControlObjectHue
            {
                Hue = 0
            };

            LightInstanceOn = new LightControlObjectOn
            {
                On = true
            };
        }
        
        /// <summary>
        /// Cycle light colors moron, obviously its called CycleColors
        /// </summary>
        public async void CycleColors()
        {
            LightInstance.Bri = 254;
            LightInstance.Sat = 254;

            using (var HttpClientObject = new HttpClient())
            {
                // URL Base
                HttpClientObject.BaseAddress = new Uri(URL);

                SetHue = LightInstanceHue.Hue + HueStep;
                if (SetHue > MaxHue) { SetHue -= MaxHue; }

                LightInstanceHue.Hue = SetHue;

                _ = HttpClientObject.PutAsJsonAsync("lights/1/state", LightInstanceHue).Result;
                _ = HttpClientObject.PutAsJsonAsync("lights/2/state", LightInstanceHue).Result;
                _ = HttpClientObject.PutAsJsonAsync("lights/4/state", LightInstanceHue).Result;
                _ = HttpClientObject.PutAsJsonAsync("lights/5/state", LightInstanceHue).Result;
            }            
        }
        public async void Disco()
        {
            LightInstance.Bri = 254;
            LightInstance.Sat = 254;

            using (var HttpClientObject = new HttpClient())
            {
                // URL Base
                HttpClientObject.BaseAddress = new Uri(URL);

                LightInstanceOn.On = true;
                _ = HttpClientObject.PutAsJsonAsync("lights/1/state", LightInstanceOn).Result;
                
                LightInstanceOn.On = false;
                _ = HttpClientObject.PutAsJsonAsync("lights/1/state", LightInstanceOn).Result;

                //SetHue = LightInstanceHue.Hue + HueStep;
                //if (SetHue > MaxHue) { SetHue -= MaxHue; }
                //
                //LightInstanceHue.Hue = SetHue;
                //
                //if (LightInstanceOn.On == true) { LightInstanceOn.On = false; }                
                //_ = HttpClientObject.PutAsJsonAsync("lights/1/state", LightInstanceHue).Result;
                //_ = HttpClientObject.PutAsJsonAsync("lights/1/state", LightInstanceOn).Result;
                //
                //LightInstanceHue.Hue += 10000;
                //if (LightInstanceOn.On == false) { LightInstanceOn.On = true; }
                //if (LightInstanceHue.Hue > MaxHue) { LightInstanceHue.Hue -= MaxHue; }
                //
                //_ = HttpClientObject.PutAsJsonAsync("lights/2/state", LightInstanceHue).Result;
                //_ = HttpClientObject.PutAsJsonAsync("lights/2/state", LightInstanceOn).Result;
                //
                //LightInstanceHue.Hue -= 10000;
                //if (LightInstanceOn.On == true) { LightInstanceOn.On = false; }
                //if (LightInstanceHue.Hue < 0) { LightInstanceHue.Hue += MaxHue; }
                //
                //_ = HttpClientObject.PutAsJsonAsync("lights/4/state", LightInstanceHue).Result;
                //_ = HttpClientObject.PutAsJsonAsync("lights/4/state", LightInstanceOn).Result;
                //
                //LightInstanceHue.Hue += 20000;
                //if (LightInstanceOn.On == false) { LightInstanceOn.On = true; }
                //if (LightInstanceHue.Hue > MaxHue) { LightInstanceHue.Hue -= MaxHue; }
                //
                //_ = HttpClientObject.PutAsJsonAsync("lights/5/state", LightInstanceHue).Result;
                //_ = HttpClientObject.PutAsJsonAsync("lights/5/state", LightInstanceOn).Result;
                //if (LightInstanceOn.On == true) { LightInstanceOn.On = false; }
            }
        }

        /// <summary>
        /// Stores baseline light values
        /// </summary>
        public async void LightDefault()
        {
            using (var HttpClientObject = new HttpClient())
            {
                //URL Base
                HttpClientObject.BaseAddress = new Uri(URL);

                //GET call for light
                HttpResponseMessage response = await HttpClientObject.GetAsync("lights/1/state");

                if (response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    Console.WriteLine(responseBody);
                }
                else;
            }
        }
    }
}
