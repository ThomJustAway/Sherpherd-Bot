using Data_control;
using PGGE.Patterns;
using System.Collections;
using UnityEngine;

namespace Sheep
{
    /// <summary>
    /// what the sheep would do if there is not threaten by shepherd dog.
    /// </summary>
    public class SheepIdleState : SheepState
    {
        //TODO: convert the water and eating transtion timing to timer
        //the timer for counting when the sheep will go eat state.
        float eatTransitionTimer;
        float eatElapseTime;

        float drinkTransitionTimer;
        float drinkElapseTime;
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

            drinkTransitionTimer = Random.Range(flock.TimeToEnterDrinkStateMin, flock.TimeToEnterDrinkStateMax);
            drinkElapseTime = 0f;
        }

        public override void Update()
        {
            //Debug.Log($"{sheepBehaviour.name} "+ "Idle");
            SenseDog();
            SenseEating();
            SenseDrink();
            DetermineFurGrowth();
        }

        //can improve this by making sure that the face is pointing at the grass
        private void SenseEating()
        {
            if(!Physics.CheckSphere(transform.position, flock.MouthRadius, LayerManager.GrassPatchLayer))
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

        private void SenseDrink()
        {
            if (!Physics.CheckSphere(transform.position, flock.MouthRadius, LayerManager.WaterPatchLayer))
            {
                drinkElapseTime = 0f;
                return;
            }

            while (drinkElapseTime < drinkTransitionTimer)
            {
                drinkElapseTime += Time.deltaTime;
                return;
            }

            mFsm.SetCurrentState((int)SheepStates.Drink);
        }

        private void SenseDog()
        {
            if(Vector3.Distance(flock.Predator.transform.position, transform.position) <= flock.EscapeRadius) 
            { 
                mFsm.SetCurrentState((int)SheepStates.Flocking);
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