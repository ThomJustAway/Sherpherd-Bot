using PGGE.Patterns;
using System.Collections;
using UnityEngine;

namespace sherpherdDog
{
    public class SherpherDogState : FSMState
    {
        protected ShepherdDog dog;
        protected Transform transform;
        public SherpherDogState(FSM fsm , ShepherdDog dog) : base(fsm)
        {
            this.dog = dog;
            transform = dog.transform;
        }
    }
}