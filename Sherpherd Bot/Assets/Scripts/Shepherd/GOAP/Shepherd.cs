using Data_control;
using GOAPTHOM;
using Sheep;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Assets.Scripts.Shepherd.GOAP
{
    public class Shepherd : MonoBehaviour
    {
        GoapAgent agent;

        //values
        [SerializeField] SheepFlock flock;

        [SerializeField] private float movingSpeed;
        [SerializeField] private float rotationSpeed;
        public Transform GrassPosition { get; private set; }
        public Transform WaterPosition {get; private set; }
        public float WanderingRadius { get => wanderingRadius; set => wanderingRadius = value; }
        public float MovingSpeed { get => movingSpeed; set => movingSpeed = value; }
        public float RotationSpeed { get => rotationSpeed; set => rotationSpeed = value; }

        [Header("feeding")]
        [SerializeField] private float grassAndWaterAcceptableRange;
        [SerializeField] private float acceptableFoodLevel;
        [SerializeField] private float acceptableWaterLevel;

        [SerializeField] private float wanderingRadius;

        private void Start ()
        {
            agent = new GoapAgent();
            //need to set up the belief, actions, goals of the GOAP
            agent.CreatePlanner();
            agent.SetUpdate();
            agent.SetupBeliefs(CreatingBelief());
            agent.SetupActions(CreatingActions());
            agent.SetupGoals(CreatingGoals());
        }

        private void Update()
        {
            agent.updateFunction();
        }

        private Dictionary<string,AgentBelief> CreatingBelief()
        {
            var beliefs = new Dictionary<string,AgentBelief>();
            var beliefsFactory = new BeliefFactory(transform, beliefs);

            //the values that needs to be change
            beliefsFactory.AddBelief(Beliefs.Nothing, () => false);//can be done repeatedly
            beliefsFactory.AddBelief(Beliefs.FoundFoodSource, () => GrassPosition.IsUnityNull());
            beliefsFactory.AddBelief(Beliefs.FoundWatersource, () => WaterPosition.IsUnityNull());
            beliefsFactory.AddBelief(Beliefs.SheepAtFoodSource, () => InWithinLocation(flock.CG, 
                GrassPosition?.position ?? Vector3.positiveInfinity, //can nvr be reach
                grassAndWaterAcceptableRange
                ));
            beliefsFactory.AddBelief(Beliefs.SheepAtWaterSource, () => InWithinLocation(
                WaterPosition?.position ?? Vector3.positiveInfinity, 
                flock.CG,
                grassAndWaterAcceptableRange
                ));
            beliefsFactory.AddBelief(Beliefs.SheepEaten, () => flock.flockFood > acceptableFoodLevel);
            beliefsFactory.AddBelief(Beliefs.SheepHydrated, () => flock.flockWater > acceptableWaterLevel);
            return beliefs;
        }

        private bool InWithinLocation(Vector3 targetPos, Vector3 posToCheck, float radius)
        {
            return Vector3.Distance(targetPos, posToCheck) < radius;
        }

        private HashSet<AgentAction> CreatingActions()
        {
            var actions = new HashSet<AgentAction>();
            var beliefs = agent.beliefs;
            actions.Add(new AgentAction
                .Builder(Actions.Idle)
                .AddEffect(Beliefs.Nothing,beliefs)
                .WithStrategy(new IdleStrategy(6f))
                .Build());

            actions.Add(new AgentAction
                .Builder(Actions.WanderAround)
                .AddEffect(Beliefs.FoundFoodSource, beliefs)
                .AddEffect(Beliefs.FoundWatersource ,beliefs)
                .WithStrategy(new SearchStrategy(this))
                .Build());
            


            return actions;
        }

        private HashSet<AgentGoal> CreatingGoals()
        {
            var goals = new HashSet<AgentGoal>();
            return goals;
        }

        //will check if it is a water or a food
        private void OnCollisionEnter(Collision collision)
        {
            //sense for collision for water and food.
            if(collision.collider.gameObject.layer == LayerManager.GrassPatchLayer)
            {
                //that means that it is a grass patch
                GrassPosition = collision.transform;
            }
            else if(collision.collider.gameObject.layer == LayerManager.WaterPatchLayer)
            {
                WaterPosition = collision.transform;
            }
        }
    }
}