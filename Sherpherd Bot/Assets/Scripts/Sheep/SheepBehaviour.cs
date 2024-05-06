using PGGE.Patterns;
using System.Collections;
using UnityEngine;

namespace Sheep
{
    /// <summary>
    /// The behaviour of how the sheep might behave in the simulation
    /// The behaviour is simplfied for the simulation 
    /// </summary>
    public class SheepBehaviour : MonoBehaviour
    {
        [SerializeField] private SheepFlock flock;
        [SerializeField] private Transform furModel;
        public Vector3 Velocity;
        public Vector3 ResultantVelocity;

        private FSM fsm;

        //will need to eat and drink to grow fur
        //stats
        private int fur;
        private int food;
        private int water;

        private const float furOriginalSize = 1.1768f;
        public int Fur
        {
            get { return fur; } 
            set {
                //when setting the value of the fur
                //automatically adjust the fur model size.
                fur = value;
                furModel.localScale = new Vector3(
                    furOriginalSize + fur * flock.FurModelGrowth,
                    furOriginalSize + fur * flock.FurModelGrowth,
                    furOriginalSize);
            }
        }

        public int Food { get => food; set => food = value; }
        public int Water { get => water; set => water = value; }

        public void Init(SheepFlock flock)
        {
            this.flock = flock;
            //set up the states for the FSM.
            fsm = new FSM();
            fsm.Add(new SheepFlockingState(fsm, flock, this));
            fsm.Add(new SheepIdleState(fsm, flock, this));
            fsm.Add(new SheepEatState(fsm, flock, this));
            fsm.Add(new SheepGrowFur(fsm, flock, this));
            fsm.SetCurrentState((int)SheepStates.Flocking);
        }

        private void Start()
        {
            Food = 0;
            Water = 0;
            Fur = 0;
        }

        private void Update()
        {
            fsm.Update();       
        }

        //For debugging puposes
        private void OnDrawGizmosSelected()
        {
            Vector3 centre = transform.position;
            Gizmos.color = Color.black;
            Gizmos.DrawWireSphere(centre , flock.AlignmentRadius);
            Gizmos.color = Color.gray;
            Gizmos.DrawWireSphere(centre, flock.SeperationRadius);
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(centre, flock.CohesionRadius);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(centre, flock.EscapeRadius);
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(centre, flock.EatingRadius);
        }
    }
}