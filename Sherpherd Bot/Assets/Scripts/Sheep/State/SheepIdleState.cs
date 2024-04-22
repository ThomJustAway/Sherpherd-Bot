using Data_control;
using PGGE.Patterns;
using System.Collections;
using UnityEngine;

namespace Sheep
{
    public class SheepIdleState : SheepState
    {
        float eatTransitionTimer;
        float eatElapseTime;
        // dont need to have this reset every time the idle enter for faster growth
        float growElapseTime = 0f; 
        public SheepIdleState(FSM fsm, SheepFlock flock, SheepBehaviour sheep) : base(fsm, flock, sheep)
        {
            mId = (int)SheepStates.Idle;
        }

        public override void Enter()
        {
            eatTransitionTimer = Random.Range(flock.TimeToEnterEatStateMin , flock.TimeToEnterEatStateMax);
            eatElapseTime = 0f;
        }

        public override void Update()
        {
            Debug.Log($"{sheepBehaviour.name} "+ "Idle");
            SenseDog();
            SenseEating();
            DetermineFurGrowth();
        }

        //can improve this by making sure that the face is pointing at the grass
        private void SenseEating()
        {
            if(!Physics.CheckSphere(transform.position, flock.EatingRadius, LayerManager.GrassPatchLayer))
            {
                eatElapseTime = 0f;
                return;
            }

            while (eatElapseTime < eatTransitionTimer )
            {
                eatElapseTime += Time.deltaTime;
                return;
            }

            mFsm.SetCurrentState((int)SheepStates.Eat);
        }

        private void SenseDog()
        {
            if(Vector3.Distance(flock.Predator.transform.position, transform.position) <= flock.EscapeRadius) 
            { 
                mFsm.SetCurrentState((int)SheepStates.Movement);
            }
        }

        private void DetermineFurGrowth()
        {
            //add this && sheepBehaviour.Water >= flock.WaterCost
            if (!(sheepBehaviour.Food >= flock.FoodCost ))
            {
                growElapseTime = 0f;
                return;
            }

            while( growElapseTime < flock.TimeGrowth)
            {
                growElapseTime += Time.deltaTime;
                return;
            }
            //restart the timer
            growElapseTime = 0f;
            mFsm.SetCurrentState((int)SheepStates.GrowFur);
            
        }
    }
}