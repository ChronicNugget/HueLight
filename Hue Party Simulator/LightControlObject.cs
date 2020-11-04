using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hue_Party_Simulator
{   
    /// <summary>
    /// JSON convertable light control instance.
    /// </summary>
    public class LightControlObject
    {
        public bool On { get; set; }
        public int Sat { get; set; }
        public int Bri { get; set; }
        public int Hue { get; set; }
    }

    /// <summary>
    /// Light Control Enumerable. Contains index of the light a\
    /// Also has group index. WE CAN NOT SEND THIS AS A JSON OBJECT TO THE 
    /// BRIDGE!!
    /// 
    /// Inherited from base type of LightControlObject.
    /// 
    /// </summary>
    public class ListableLightObject : LightControlObject
    {
        public int LightIndex { get; set; }
        public int LightGroup { get; set; }
    }
}
