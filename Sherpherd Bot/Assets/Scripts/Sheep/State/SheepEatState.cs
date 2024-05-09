using PGGE.Patterns;
using System.Collections;
using UnityEditor.Search;
using UnityEngine;

namespace Sheep
{
    public class SheepEatState : SheepState
    {
        private float elapseTime;

        public SheepEatState(FSM fsm, SheepFlock flock, SheepBehaviour sheep) : base(fsm, flock, sheep)
        {
            mId = (int)SheepStates.Eat;
        }

        public override void Update()
        {
            SenseDog();
            while(elapseTime < flock.TimeToEatFinish)
            {
                //wait for the sheep to eat finish
                elapseTime += Time.deltaTime;
                return;
            }
            Debug.Log($"{sheepBehaviour.name} "+"eat");
            sheepBehaviour.Saturation += 1; //add one food every time it eats.
            mFsm.SetCurrentState((int)SheepStates.Idle);
        }

        public override void Exit()
        {
            elapseTime = 0;
        }

        private void SenseDog()
        {
            if (Vector3.Distance(flock.Predator.transform.position, transform.position) <= flock.EscapeRadius)
            {
                mFsm.SetCurrentState((int)SheepStates.Flocking);
            }
        }

    }
}