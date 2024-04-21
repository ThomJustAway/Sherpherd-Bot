using PGGE.Patterns;
using System.Collections;
using UnityEngine;

namespace Sheep
{
    public class SheepBehaviour : MonoBehaviour
    {
        [SerializeField] private SheepFlock flock;

        public Vector3 Velocity;
        public Vector3 ResultantVelocity;
        private FSM fsm;

        public void Init(SheepFlock flock)
        {
            this.flock = flock;
            fsm = new FSM();
            fsm.Add(new SheepMovementState(fsm, flock, this));
            fsm.Add(new SheepIdleState(fsm, flock, this));
            fsm.SetCurrentState((int)SheepStates.Movement);
        }

        private void Update()
        {
            fsm.Update();       
        }

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
        }
    }
}