using System.Collections;
using UnityEngine;

namespace Data_control
{
    public class LayerManager 
    {
        public static int SheepLayer {get { return 1 << 6; } }
        public static int PredatorLayer { get { return 1 << 7; } }
    }
}