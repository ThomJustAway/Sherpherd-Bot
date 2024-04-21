using PGGE.Patterns;
using UnityEngine;

namespace Sheep
{
    public class SheepState : FSMState
    {
        protected SheepFlock flock;
        protected SheepBehaviour sheepBehaviour;
        protected Transform transform;
        public SheepState(FSM fsm, SheepFlock flock, SheepBehaviour sheep) : base(fsm)
        {
            this.flock = flock;
            sheepBehaviour = sheep;
            this.transform = sheep.transform;
        }
    }
}
