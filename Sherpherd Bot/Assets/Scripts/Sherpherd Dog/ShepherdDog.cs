using PGGE.Patterns;
using Sheep;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace sherpherdDog
{
    /// <summary>
    /// The script that controls the dog behaviour. The sheep dog
    /// purpose is to make sure the sheeps are in a certain position. that
    /// the shepherd ask it should go.
    /// </summary>
    public class ShepherdDog : MonoBehaviour
    {
        //This is the paper reference to implement the controlling sheep.
        //https://royalsocietypublishing.org/doi/epdf/10.1098/rsif.2014.0719

        [Header("Movement")]
        [SerializeField] private float minSpeed = 1f;
        [SerializeField] private float maxSpeed = 8f;
        [SerializeField] private float rotationSpeed = 10f;

        [Header("Collecting and gathering debugging")]
        [SerializeField] private SheepFlock flockData; //to know what the flock information
        [Header("Dog sense radius to know where the environment")]
        [SerializeField] private float senseRadius = 30f;
        [Header("Collection offset is how much distance the dog should " +
            "be away from the shepherd back in order to bring it to the flock.CG")]
        [SerializeField] private float collectionOffset = 2f;
        [Header("driving offset is how much distance the dog should " +
            "be away from the flock.cg back in order to bring it to the target position")]
        [SerializeField] private float drivingOffset = 2f;
        [SerializeField] private float targetRadius = 6f;
        [SerializeField] private float gatheringMultiplier = 1.3f;

        [SerializeField] private float collectingAvoidanceSheep = 1f;
        [Range(0,25)]
        [SerializeField] private float angleOfAvoidance = 5;
        [SerializeField] private float weightOfTarget = 1f;
        [SerializeField] private float weightOfAvoidanceOfOtherSheep = 1f;
        [SerializeField] private float weightOfAvoidanceFromChosenSheep = 1f;

        [Header("Debugging")]
        [ContextMenuItem("Move Sheeps to target",nameof(ChaseSheeps))]
        [SerializeField]private Transform target;
        private Vector3 targetPoint;

        //fsm state machine
        private FSM fsm;
        #region getter
        public float SenseRadius { get => senseRadius; set => senseRadius = value; }
        public float CollectionOffset { get => collectionOffset; set => collectionOffset = value; }
        public float DrivingOffset { get => drivingOffset; set => drivingOffset = value; }
        public float TargetRadius { get => targetRadius; set => targetRadius = value; }
        public float MinSpeed { get => minSpeed; set => minSpeed = value; }
        public float MaxSpeed { get => maxSpeed; set => maxSpeed = value; }
        public float RotationSpeed { get => rotationSpeed; set => rotationSpeed = value; }
        public Vector3 TargetPoint { get => targetPoint; set => targetPoint = value.With(y: flockData.YOffset); }

        public float GatheringRadius { get { return flockData.CohesionRadius * gatheringMultiplier; } }

        public float CollectingAvoidanceSheepValue { get => collectingAvoidanceSheep; set => collectingAvoidanceSheep = value; }
        public float AngleOfAvoidance { get => angleOfAvoidance; set => angleOfAvoidance = value; }
        public float WeightOfTarget { get => weightOfTarget; set => weightOfTarget = value; }
        public float WeightOfAvoidanceOfOtherSheep { get => weightOfAvoidanceOfOtherSheep; set => weightOfAvoidanceOfOtherSheep = value; }
        public float WeightOfAvoidanceFromChosenSheep { get => weightOfAvoidanceFromChosenSheep; set => weightOfAvoidanceFromChosenSheep = value; }
        #endregion

        private void Start()
        {
            fsm = new FSM();
            fsm.Add(new CollectingState(fsm, this, flockData));
            fsm.Add(new DrivingState(fsm, this, flockData));
            fsm.Add(new ListeningState(fsm, this));
            fsm.SetCurrentState((int)DogState.ListeningState);
        }

        private void Update()
        {
            //print($"dog at {(DogState)fsm.GetCurrentState().ID}");
            fsm.Update();
        }

        //experimental
        public void ChaseSheeps()
        {
            TargetPoint = target.position;
            fsm.SetCurrentState((int)DogState.Collecting);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(transform.position, SenseRadius);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(targetPoint, targetRadius);
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(flockData.CG, GatheringRadius);
        }
        
        /// <summary>
        /// A instruction given by the shepherd to the dog to 
        /// make the sheeps move to the intended target point.
        /// </summary>
        /// <param name="targetPoint">
        /// Where the flock should go.
        /// </param>
        public void ChaseSheeps(Vector3 targetPoint)
        {
            TargetPoint = targetPoint;
            fsm.SetCurrentState((int)DogState.Collecting);
        }
    }

    public enum DogState
    {
        ListeningState,
        Driving,
        Collecting,
    }
}