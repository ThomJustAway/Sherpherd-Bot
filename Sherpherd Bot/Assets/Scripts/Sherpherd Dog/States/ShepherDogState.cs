using PGGE.Patterns;
using System.Collections;
using UnityEngine;

namespace sherpherdDog
{
    /// <summary>
    /// Base state that every shepherd dog should have.
    /// </summary>
    public class ShepherDogState : FSMState
    {
        protected ShepherdDog dog;
        protected Transform transform;
        public ShepherDogState(FSM fsm , ShepherdDog dog) : base(fsm)
        {
            this.dog = dog;
            transform = dog.transform;
        }
    }
}