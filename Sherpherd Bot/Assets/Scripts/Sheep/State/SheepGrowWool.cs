using PGGE.Patterns;
using System.Collections;
using UnityEngine;

namespace Sheep
{
    /// <summary>
    /// The state if the sheep needs to grow wool. It would do a probability check to see
    /// if it can grow the wool. If it can, then grow the wool.
    /// </summary>
    public class SheepGrowWool : SheepState
    {
        public SheepGrowWool(FSM fsm, SheepFlock flock, SheepBehaviour sheep) : base(fsm, flock, sheep)
        {
            mId = (int) SheepStates.GrowWool;
        }

        public override void Enter()
        {
            float randNumber = Random.value;
            if( randNumber < flock.ProbabilityOfGrowth )
            {//means that it can grow
                sheepBehaviour.Wool += 1;
                //reduce the saturation and hydration of the sheep as a result of growth.
                sheepBehaviour.Saturation -= flock.FoodCost;
                sheepBehaviour.Hydration -= flock.WaterCost;
                Debug.Log($"{sheepBehaviour.name} Grow!");
            }
            mFsm.SetCurrentState((int)SheepStates.Idle);
        }
    }
}