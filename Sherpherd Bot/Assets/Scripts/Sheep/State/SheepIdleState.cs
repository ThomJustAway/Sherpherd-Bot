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
            //will randomly determine how many seconds to transition to eating or drinking state if the condition is fuillied
            eatTransitionTimer = Random.Range(flock.TimeToEnterEatStateMin , flock.TimeToEnterEatStateMax);
            eatElapseTime = 0f;

            drinkTransitionTimer = Random.Range(flock.TimeToEnterDrinkStateMin, flock.TimeToEnterDrinkStateMax);
            drinkElapseTime = 0f;
        }

        public override void Update()
        {
            //check for the condition if it is fuilled and if it is, transition to the respective state.
            //Debug.Log($"{sheepBehaviour.name} "+ "Idle");
            SenseDog();
            SenseEating();
            SenseDrink();
            DetermineWoolGrowth();
        }

        /// <summary>
        /// Check if it is near a grass patch. if it is, it will start a countdown 
        /// to let the sheep decide if it needs to eat. once the count down is finish,
        /// transition to eating state.
        /// </summary>
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
        /// <summary>
        /// Check if it is near a drinking area. if it is, it will start a countdown 
        /// to let the sheep decide if it needs to drink. once the count down is finish,
        /// transition to drinking state.
        /// </summary>
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
        /// <summary>
        /// Transition to escape mode if the sheep is near the shepherd dog.
        /// </summary>
        private void SenseDog()
        {
            if(Vector3.Distance(flock.Predator.transform.position, transform.position) <= flock.EscapeRadius) 
            { 
                mFsm.SetCurrentState((int)SheepStates.Flocking);
            }
        }
        /// <summary>
        /// Will decide if it can grow wool if it has eaten enough food
        /// and water
        /// </summary>
        private void DetermineWoolGrowth()
        {
            
            if (!((sheepBehaviour.Saturation >= flock.FoodCost ) &&
                (sheepBehaviour.Hydration >= flock.WaterCost))
                )
            {
                growElapseTime = 0f;
            }

            while( growElapseTime < flock.TimeGrowth)
            {
                growElapseTime += Time.deltaTime;
                return;
            }
            //restart the timer
            growElapseTime = 0f;
            mFsm.SetCurrentState((int)SheepStates.GrowWool);
            
        }
    }
}