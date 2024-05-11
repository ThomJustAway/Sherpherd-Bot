using PGGE.Patterns;

namespace sherpherdDog
{
    /// <summary>
    /// Idle state where the dog does nothing and weird
    /// </summary>
    public class IdleState : ShepherDogState
    {
        public IdleState(FSM fsm, ShepherdDog dog) : base(fsm, dog)
        {
            mId = (int)DogState.ListeningState;
        }

    }
}