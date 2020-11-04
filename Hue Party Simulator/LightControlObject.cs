using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hue_Party_Simulator
{
    public class LightControlObject
    {
        //public bool On { get; set; }
        public int Sat { get; set; }
        public int Bri { get; set; }
        public int Hue { get; set; }
    }

    public class LightControlObjectHue
    {
        public int Hue { get; set; }
    }

    public class LightControlObjectOn
    {
        public bool On { get; set; }
    }
}
