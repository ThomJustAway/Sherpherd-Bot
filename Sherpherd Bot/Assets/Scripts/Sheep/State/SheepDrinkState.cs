using PGGE.Patterns;
using Sheep;
using System.Collections;
using UnityEngine;

namespace Sheep
{
    /// <summary>
    /// The sheep drinking state for it to drink water.
    /// </summary>
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
            //will wait for a certain amount of time to drink finish
            while (elapseTime < flock.TimeToDrinkFinish)
            {
                //wait for the sheep to eat finish
                elapseTime += Time.deltaTime;
                return;
            }
            //once the timer is done, add one water to the sheep and return back to Idle
            //Debug.Log($"{sheepBehaviour.name} " + "Drink");
            sheepBehaviour.Hydration += 1; //add one food every time it eats.
            mFsm.SetCurrentState((int)SheepStates.Idle);
        }

        public override void Exit()
        {
            elapseTime = 0;
        }

        /// <summary>
        /// Sense if the sheep is in danger. if it is, then run away from the dog.
        /// </summary>
        private void SenseDog()
        {
            if (Vector3.Distance(flock.Predator.transform.position, transform.position) <= flock.EscapeRadius)
            {
                mFsm.SetCurrentState((int)SheepStates.Flocking);
            }
        }
    }
}