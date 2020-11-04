using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using System.Windows.Documents;

namespace Hue_Party_Simulator
{
    /// <summary>
    /// Class used for controlling Hue Lights
    /// </summary>
    public class LightControl
    {
        // Changed to private and assigned these accourding to dev using the program.
        private static string URL_Jack = "http://192.168.42.3/api/7XzuDmSNw0US1gcx0sI7QiFfFnBOttyu8d0LX3kY/";
        private static string URL_Zack = "http://192.168.1.19/api/fLBUxXi967wpwVkGHgrCmM315v5f8pPElJMJer-v/";

        // The public object is assigned on class construction now. Makes it easier for me/jack to debug shit.
        public static string URL;

        // List of all Light control instances/objects.
        public List<int> LightIndexValues = new List<int>();
        public List<LightControlObject> AllLightInstances = new List<LightControlObject>();
        
        // Light instance that we need for the controller.
        public LightControlObject LightInstance;

        private int SetHue = 0;       // Start Hue Value (Begin at 0)      
        private int MaxHue = 65535;   // MAX Hue Value (Begin from 0)
        private int HueStep = 5000;   // Hue to increase each loop.

        /// <summary>
        /// Constructor for LightControl
        /// </summary>
        public LightControl()
        {
            // ZW - 11/04
            //   Added Dynamic machine name swapping for URL of Bridge. 
            //   We ideally want this to end up being pullled from the users UI/some automatic
            //   config script in the end of the day. But for now using this in debug mode setting 
            //   based on the machine name works fine.

            if (Environment.MachineName == "DESKTOP-1IAH9GR") { URL = URL_Jack; }
            if (Environment.MachineName == "DESKTOP-S3H8UTA") { URL = URL_Zack; }

            // ZW - 11/04
            //   Were gonna just remove this for now and let a JSON thingy do some magic to make working 
            //   on this project together easier.

            // LightInstance = new LightControlObject { On = true, Bri = 0, Sat = 0, Hue = 0 };
            
            // ZW - 11/04 
            //   Yep im officially putting too much work into this but oh fuckin well.
            //   Pull back the result from the GET call of "lights" and split it/parse it 
            //   out into seperate light object strings. For each one of those, populate a new light instance
            //   into the Listable LightControl Object.

            // Pull down light objects and regex match each entry value.
            string AllHueBridgeJson = new WebClient().DownloadString(URL + "lights");
            MatchCollection allLightMatches = Regex.Matches(AllHueBridgeJson, "\"(\\d+)\":{");

            // Make sure we have some lights to fuck around with. If not throw an error and return.
            if (allLightMatches.Count == 0)
            {
                Console.WriteLine("COULD NOT FIND ANY LIGHT OBJECTS");
                throw new Exception("NO LIGHTS WERE FOUND ON THE API CALL. ENSURE THAT YOU HAVE A REAL API KEY");
            }

            // If we got lights:
            // Loop each match and make a light instance based on it's properties.
            // Then add that object to list of light objs

            int Counter = 1;
            Console.WriteLine("FOUND: " + allLightMatches.Count + " LIGHT OBJECTS!");
            foreach (Match LightMatch in allLightMatches)
            {
                // Substring the main repsonse to only have values from our current light match.
                // Make sure we dont overstep the length of the match collection.
                int LenOfString = (AllHueBridgeJson.Length - LightMatch.Index) - 10;
                if (Counter != allLightMatches.Count) { LenOfString = allLightMatches[Counter].Index; }

                // Pull the substring with the new length obj we made. Try Catch since this was somehow breaking on the last item.
                // Im lazy and dont wanna debug it correctly so fuck it this will do.
                string TempLightString = "";
                try { TempLightString = AllHueBridgeJson.Substring(LightMatch.Index, LenOfString); }
                catch { TempLightString = AllHueBridgeJson.Substring(LightMatch.Index); }

                // Make a bunch of matches on the new substring using a Regex ZW wrote to extract easy values.
                // The regex is: {  \\\"state\\\":{\\\"on\\\":(\w+),\\\"bri\\\":(\d+),\\\"hue\\\":(\d+),\\"sat\\\":(\d+)  } without the {}
                var MatchesForVals = Regex.Match(TempLightString, "\\\"state\\\":{\\\"on\\\":(\\w+),\\\"bri\\\":(\\d+),\\\"hue\\\":(\\d+),\\\"sat\\\":(\\d+)");

                // IF we dont have a match for all vals skip. 
                if (MatchesForVals.Groups.Count != 5)
                {
                    Counter++;
                    continue;
                }

                // I/O = Group[1]
                // Bri = Group[2]
                // Hue = Group[3]
                // Sat = Group[4]

                // Print out the number of the light we're dealing with at the moment.
                Console.WriteLine("LIGHT FOUND! --> INDEX: " + LightMatch.Groups[1].Value);
                Console.WriteLine("STATE: " + MatchesForVals.Groups[1].Value);
                Console.WriteLine("BRI:   " + MatchesForVals.Groups[2].Value);
                Console.WriteLine("HUE:   " + MatchesForVals.Groups[3].Value);
                Console.WriteLine("SAT:   " + MatchesForVals.Groups[4].Value);

                // Make a new light object and add it to the list of all objects. 
                LightIndexValues.Add(int.Parse(LightMatch.Groups[1].Value));
                AllLightInstances.Add(new LightControlObject
                {
                    On = bool.Parse(MatchesForVals.Groups[1].Value),
                    Bri = int.Parse(MatchesForVals.Groups[2].Value),
                    Hue = int.Parse(MatchesForVals.Groups[3].Value),
                    Sat = int.Parse(MatchesForVals.Groups[4].Value)
                });

                Console.WriteLine("\nADDED LIGHT TO LIST OF ALL OBJECTS!\n");
                Counter++;
            }

            // Make the light control object for the main object the first item in the list of 
            // All Light objects.
            LightInstance = AllLightInstances[0];
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

                int IndexCounter = 0;
                foreach (var CurrentInstance in AllLightInstances)
                {
                    SetHue = CurrentInstance.Hue + HueStep;
                    if (SetHue > MaxHue) { SetHue -= MaxHue; }

                    CurrentInstance.Hue = SetHue;

                    string ApiStirng = "lights/" + LightIndexValues[IndexCounter] + "/state";
                    HttpClientObject.PutAsJsonAsync(ApiStirng, CurrentInstance);

                    IndexCounter++;
                }

                /* Commented Out. Try the code ive got up above^
                _ = HttpClientObject.PutAsJsonAsync("lights/1/state", LightInstance).Result;
                _ = HttpClientObject.PutAsJsonAsync("lights/2/state", LightInstance).Result;
                _ = HttpClientObject.PutAsJsonAsync("lights/4/state", LightInstance).Result;
                _ = HttpClientObject.PutAsJsonAsync("lights/5/state", LightInstance).Result;
                */
            }            
        }

        public async void Disco(int LightIndex = 1) // Whenever you see int PARAM = VALUE it means you dont have to pass in a value when calling it.
        {
            // Assign a temp local object to modify here. 
            var CurrentLight = LightInstance;

            // If we have an object in the list of all light objects that matches the current light index we requested:
            if (LightIndexValues.Contains(LightIndex)) 
            {
                // Find the index of where the index tracker has that value.
                int CurrentIndex = LightIndexValues.IndexOf(LightIndex);

                // Make the current light the INDEX OF THE INDEX WE REQUESTED. THIS IS CONFUSING
                // INDEX OF CURRENT INDEX GIVES INDEX OF THE LIGHT WE WANT. THINK OF IT LIKE THAT.
                CurrentLight = AllLightInstances[CurrentIndex];
            }

            // Set some shit.
            CurrentLight.Bri = 254;
            CurrentLight.Sat = 254;

            using (var HttpClientObject = new HttpClient())
            {
                // URL Base
                HttpClientObject.BaseAddress = new Uri(URL);

                // Set some more shit.
                CurrentLight.On = true;
                _ = HttpClientObject.PutAsJsonAsync("lights/" + LightIndex + "/state", CurrentLight).Result;

                // Set some more shit.
                CurrentLight.On = false;
                _ = HttpClientObject.PutAsJsonAsync("lights/" + LightIndex + "/state", CurrentLight).Result;

                // Donezo.

                #region Some Shit right here that takes up too much fuckin room. 
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
                #endregion
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
