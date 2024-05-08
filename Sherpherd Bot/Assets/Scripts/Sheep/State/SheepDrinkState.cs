using PGGE.Patterns;
using Sheep;
using System.Collections;
using UnityEngine;

namespace Sheep
{
    public class SheepDrinkState : SheepState
    {
        private float elapseTime;

        public SheepDrinkState(FSM fsm, SheepFlock flock, SheepBehaviour sheep) : base(fsm, flock, sheep)
        {
            mId = (int)SheepStates.Drink;
        }

        public override void Update()
        {
            SenseDog();
            while (elapseTime < flock.TimeToDrinkFinish)
            {
                //wait for the sheep to eat finish
                elapseTime += Time.deltaTime;
                return;
            }
            //Debug.Log($"{sheepBehaviour.name} " + "Drink");
            sheepBehaviour.Water += 1; //add one food every time it eats.
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