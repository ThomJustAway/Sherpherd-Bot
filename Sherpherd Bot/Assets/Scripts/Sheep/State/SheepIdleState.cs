using PGGE.Patterns;
using System.Collections;
using UnityEngine;

namespace Sheep
{
    public class SheepIdleState : SheepState
    {
        public SheepIdleState(FSM fsm, SheepFlock flock, SheepBehaviour sheep) : base(fsm, flock, sheep)
        {
            mId = (int)SheepStates.Idle;
        }

        public override void Update()
        {
            Debug.Log("Idle");
            SenseDog();
        }

        private void SenseDog()
        {
            if(Vector3.Distance(flock.Predator.transform.position, transform.position) <= flock.EscapeRadius) 
            { 
                mFsm.SetCurrentState((int)SheepStates.Movement);
            }
        }
    }
}