using PGGE.Patterns;
using System.Collections;
using UnityEngine;

namespace Sheep
{
    /// <summary>
    /// The behaviour of how the sheep might behave in the simulation
    /// The behaviour is simplfied for the simulation and does not contain a lot of 
    /// complexity as much of it is within the FSM.
    /// </summary>
    public class SheepBehaviour : MonoBehaviour
    {
        private SheepFlock flock;
        [SerializeField] private Transform furModel;
        public Vector3 Velocity;
        public Vector3 ResultantVelocity;

        private FSM fsm;

        //will need to eat and drink to grow wool
        //stats
        private int wool;
        private int food;
        private int water;

        public int Wool
        {
            get { return wool; } 
            set {
                //when setting the value of the wool
                //automatically adjust the wool model size.
                wool = value;
                furModel.localScale = new Vector3(
                    flock.furOriginalSize + wool * flock.FurModelGrowth,
                    flock.furOriginalSize + wool * flock.FurModelGrowth,
                    flock.furOriginalSize);
            }
        }
        public int Saturation { get => food; set => food = value; }
        public int Hydration { get => water; set => water = value; }

        public void Init(SheepFlock flock)
        {
            this.flock = flock;
            //set up the states for the FSM.
            fsm = new FSM();
            fsm.Add(new SheepFlockingState(fsm, flock, this));
            fsm.Add(new SheepIdleState(fsm, flock, this));
            fsm.Add(new SheepEatState(fsm, flock, this));
            fsm.Add(new SheepGrowWool(fsm, flock, this));
            fsm.Add(new SheepDrinkState(fsm, flock, this));
            fsm.SetCurrentState((int)SheepStates.Flocking);
        }

        private void Start()
        {
            Saturation = 0;
            Hydration = 0;
            Wool = 0;
        }

        private void Update()
        {
            fsm.Update();       
        }

        public void ShearWool()
        {
            if (Wool == 0) return;
            Transform woolObject =  Instantiate(flock.WoolPrefab, flock.WoolContainer).transform;
            float scale = 1 + (Wool * 0.1f);
            //create a wool prefab
            woolObject.localScale = new Vector3(scale, scale, scale);
            woolObject.transform.position = transform.position;

            Wool = 0;
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
            Gizmos.DrawWireSphere(centre, flock.MouthRadius);
        }
    }
}