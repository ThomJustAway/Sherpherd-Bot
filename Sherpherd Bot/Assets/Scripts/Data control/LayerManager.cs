using System.Collections;
using UnityEngine;

namespace Data_control
{
    public class LayerManager 
    {
        public static int GrassPatchLayer {  get { return 1 << 9; } }
        public static int SheepLayer {get { return 1 << 6; } }
        public static int PredatorLayer { get { return 1 << 7; } }
        public static int WallLayer { get { return 1 << 8; } }
        public static int WaterPatchLayer { get { return 1 << 10; } }
    }
}