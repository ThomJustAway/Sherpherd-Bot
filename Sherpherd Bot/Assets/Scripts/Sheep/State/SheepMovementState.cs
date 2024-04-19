using PGGE.Patterns;
using System.Collections;
using UnityEngine;

namespace Sheep
{
    public class SheepState : FSMState
    {
        private SheepBehaviour sheep;

        public SheepState(FSM fsm , SheepBehaviour sheep) : base(fsm)
        {
            this.sheep = sheep;
        }
    }

    public class SheepMovementState : SheepState
    {
        public SheepMovementState(FSM fsm, SheepBehaviour sheep) : base(fsm, sheep)
        {
            ID = (int) SheepStates.Movement;
        }
    }
}