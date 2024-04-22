using PGGE.Patterns;
using System.Collections;
using UnityEngine;

namespace Sheep
{
    public class SheepGrowFur : SheepState
    {
        public SheepGrowFur(FSM fsm, SheepFlock flock, SheepBehaviour sheep) : base(fsm, flock, sheep)
        {
            mId = (int) SheepStates.GrowFur;
        }

        public override void Enter()
        {
            float randNumber = Random.value;
            if( randNumber < flock.ProbabilityOfGrowth )
            {//means that it can grow
                sheepBehaviour.Fur += 1;
                sheepBehaviour.Food -= flock.FoodCost;
                sheepBehaviour.Water -= flock.WaterCost;
                Debug.Log($"{sheepBehaviour.name} Grow!");
            }
            mFsm.SetCurrentState((int)SheepStates.Idle);
        }
    }
}