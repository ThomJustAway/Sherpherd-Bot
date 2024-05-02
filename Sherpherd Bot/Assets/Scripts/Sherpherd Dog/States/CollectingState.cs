using PGGE.Patterns;
using System.Collections;
using UnityEngine;

namespace sherpherdDog
{
    //needs to do the following, 
    public class CollectingState : SherpherDogState
    {
        //Driving is a state where the shepherd dog would collect wandering
        //sheeps to group them up to the global center of the herd.

        public CollectingState(FSM fsm, ShepherdDog dog) : base(fsm, dog)
        {
            mId = (int)DogState.Driving;
        }


    }

    public class DrivingState : SherpherDogState
    {
        //Gathering is when the dog would push all the 
        //herd to the specified location

        public DrivingState(FSM fsm, ShepherdDog dog) : base(fsm, dog)
        {
            mId = (int)DogState.Driving;
        }


    }
}