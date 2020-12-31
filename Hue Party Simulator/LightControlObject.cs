using System;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;

namespace Hue_Party_Simulator
{
    /// <summary>
    /// Base light controller object.
    /// </summary>
    public class LightController
    {    
        /// <summary>
        /// Does JSON URI and light input parsing.
        /// </summary>
        internal class LightControllerSetup
        {
            // URLS to control
            private string URL_Jack = "http://192.168.42.3/api/7XzuDmSNw0US1gcx0sI7QiFfFnBOttyu8d0LX3kY/";
            private string URL_Zack = "http://192.168.1.3/api/sqWWsmo0Y0YHF3tDfeltJukkQTtJt4By50QGfMvY/";

            // JSON From api and URL to base.
            public string JSONString;
            public string URLString;

            // Light Count
            public int LightCount;

            public LightControllerSetup()
            {
                if (Environment.MachineName == "DESKTOP-1IAH9GR") { URLString = URL_Jack; }
                if (Environment.MachineName == "DESKTOP-S3H8UTA") { URLString = URL_Zack; }

                // Pull down light objects and regex match each entry value.
                JSONString = new WebClient().DownloadString(URLString + "lights");
            }

            // Get matches. Return if matches found.
            public bool GetRegexMatches(out MatchCollection AllLightMatches)
            {
                AllLightMatches = Regex.Matches(JSONString, "\"(\\d+)\":{");
                LightCount = AllLightMatches.Count;

                return (AllLightMatches.Count > 0);
            }
        }

        // HttpClient to send info
        public HttpClient HttpWebClient;

        // Internal Help.
        LightControllerSetup SetupController;
        MatchCollection MatchesForLights;

        // Helper Ints.
        public int MaxHue = 65535;   // Default Max Hue
        public int HueStep = 5000;   // Hue Step Value

        // Light object lists. Sets of light control objs listable.
        public ListableLightObject[] DefaultLightValues;    // List of default vals.
        public ListableLightObject[] LightObjects;          // List of current Vals.

        // Useful vals.
        public int NumLights;
        private string JSONString;

        /// <summary>
        /// CTOR for light controller base.
        /// </summary>
        public LightController()
        {
            // Client int.
            HttpWebClient = new HttpClient();
            HttpWebClient.BaseAddress = new Uri(SetupController.URLString);

            // Do the URL And JSON pulling here.
            SetupController = new LightControllerSetup();

            // Find JSON and light count.
            SetupController.GetRegexMatches(out MatchesForLights); 
            NumLights = SetupController.LightCount;

            // Make default list values.
            DefaultLightValues = new ListableLightObject[NumLights];
            LightObjects = new ListableLightObject[NumLights];

            // Add lights to our controller.
            AddLightsToController();
        }

        /// <summary>
        /// Adds lights to the controller.
        /// </summary>
        /// <param name="Matches">MatchCol of results from JSON pars . Or null</param>
        public void AddLightsToController(MatchCollection Matches = null)
        {
            // If we passed in a new set of match vals here.
            if (Matches != null) 
            {
                MatchesForLights = Matches;
                NumLights = Matches.Count;

                DefaultLightValues = new ListableLightObject[NumLights];
                LightObjects = new ListableLightObject[NumLights];
            }

            for (int Count = 0; Count < MatchesForLights.Count; Count++)
            {
                // Store temp vals here.
                var LightMatch = MatchesForLights[Count];
                int IndexOfLight = int.Parse(LightMatch.Groups[1].Value);

                // Substring the main repsonse to only have values from our current light match.
                // Make sure we dont overstep the length of the match collection.
                int LenOfString = (JSONString.Length - LightMatch.Index) - 10;
                if (Count != MatchesForLights.Count) { LenOfString = MatchesForLights[Count].Index; }

                // Pull the substring with the new length obj we made. Try Catch since this was somehow breaking on the last item.
                // Im lazy and dont wanna debug it correctly so fuck it this will do.
                string TempLightString = "";
                try { TempLightString = JSONString.Substring(LightMatch.Index, LenOfString); }
                catch { TempLightString = JSONString.Substring(LightMatch.Index); }

                // Make a bunch of matches on the new substring using a Regex ZW wrote to extract easy values.
                // The regex is: {  \\\"state\\\":{\\\"on\\\":(\w+),\\\"bri\\\":(\d+),\\\"hue\\\":(\d+),\\"sat\\\":(\d+)  } without the {}
                var MatchesForVals = Regex.Match(TempLightString, "\\\"state\\\":{\\\"on\\\":(\\w+),\\\"bri\\\":(\\d+),\\\"hue\\\":(\\d+),\\\"sat\\\":(\\d+)");

                // Add to the light list.
                AddLightToLists(IndexOfLight, MatchesForVals, Matches == null);
            }
        }

        /// <summary>
        /// Adds light objects to the lists of lights in the controller.
        /// </summary>
        /// <param name="LightIndex"></param>
        /// <param name="JSONMatches"></param>
        private void AddLightToLists(int LightIndex, Match JSONMatches, bool IsDefault = false)
        {
            // Print out the number of the light we're dealing with at the moment.
            Console.WriteLine("LIGHT FOUND! --> INDEX: " + LightIndex);
            Console.WriteLine("STATE: " + JSONMatches.Groups[1].Value);
            Console.WriteLine("BRI:   " + JSONMatches.Groups[2].Value);
            Console.WriteLine("HUE:   " + JSONMatches.Groups[3].Value);
            Console.WriteLine("SAT:   " + JSONMatches.Groups[4].Value);

            // Index to add at.
            int IndexOfList = DefaultLightValues.Length - 1;
            if (IndexOfList < 0) { IndexOfList = 0; }

            // Make light object
            LightControlObject LightObject = new LightControlObject
            {
                On = bool.Parse(JSONMatches.Groups[1].Value),
                Bri = int.Parse(JSONMatches.Groups[2].Value),
                Hue = int.Parse(JSONMatches.Groups[3].Value),
                Sat = int.Parse(JSONMatches.Groups[4].Value)
            };

            // Add to controller base now.
            if (IsDefault) DefaultLightValues[IndexOfList] = new ListableLightObject { LightIndex = LightIndex, LightObject = LightObject };
            LightObjects[IndexOfList] = new ListableLightObject { LightIndex = LightIndex, LightObject = LightObject };

            Console.WriteLine("\nADDED LIGHT TO LIST OF ALL OBJECTS!\n");
        }
    }

    #region Light Objects

    /// <summary>
    /// Light Control Enumerable. Contains index of the light a\
    /// Also has group index. WE CAN NOT SEND THIS AS A JSON OBJECT TO THE 
    /// BRIDGE!!
    /// 
    /// Inherited from base type of LightControlObject.
    /// 
    /// </summary>
    public class ListableLightObject
    {
        public int LightIndex { get; set; }
        public LightControlObject LightObject { get; set; }
    }

    /// <summary>
    /// JSON convertable light control instance.
    /// </summary>
    public class LightControlObject
    {
        private bool _on;
        public bool On 
        {
            get { return _on; }
            set 
            {
                _on = value;
                OnString = @"{""on"":" + value.ToString().ToLower() +"}"; 
            }
        }

        private int _sat;
        public int Sat 
        {
            get { return _sat; } 
            set
            {
                _sat = value;
                SatString = @"{""sat"":" + value.ToString().ToLower() + "}";
            }
        }

        private int _bri;
        public int Bri 
        {
            get { return _bri; }
            set
            {
                _bri = value;
                BriString = @"{""bri"":" + value.ToString().ToLower() + "}";
            }
        }

        private int _hue;
        public int Hue 
        { 
            get { return _hue; }
            set
            {
                _hue = value;
                HueString = @"{""hue"":" + value.ToString().ToLower() + "}";
            }
        }

        // JSON FORMATTED STRING VALUES!
        public string OnString { get; set; }
        public string SatString { get; set; }
        public string BriString { get; set; }
        public string HueString { get; set; }

    }

    #endregion
}
