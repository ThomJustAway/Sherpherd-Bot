using Data_control;
using GOAPTHOM;
using Sheep;
using sherpherdDog;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Assets.Scripts.Shepherd.GOAP
{

    //
    public class Shepherd : MonoBehaviour
    {
        public GoapAgent agent;

        //values
        [SerializeField] SheepFlock flock;
        [SerializeField] ShepherdDog dog;
        [SerializeField] private float movingSpeed;
        [SerializeField] private float rotationSpeed;
        [SerializeField] private float senseRadius = 5f;
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

            CreatingBelief();
            CreatingActions();
            CreatingGoals();
            //agent.SetupBeliefs(CreatingBelief());
            //agent.SetupActions(CreatingActions());
            //agent.SetupGoals(CreatingGoals());
        }

        private void Update()
        {
            SenseCollision();
            agent.updateFunction();
        }

        private bool InWithinLocation(Vector3 targetPos, Vector3 posToCheck, float radius)
        {
            return Vector3.Distance(targetPos, posToCheck) < radius;
        }
        private void CreatingBelief()
        {
            var beliefs = new Dictionary<string , AgentBelief>();
            var beliefsFactory = new BeliefFactory(transform, beliefs);
            print($"created belief {beliefs != null}");
            //the values that needs to be change
            beliefsFactory.AddBelief(Beliefs.Nothing, () => false);//can be done repeatedly
            beliefsFactory.AddBelief(Beliefs.FoundFoodSource, () => !GrassPosition.IsUnityNull());
            beliefsFactory.AddBelief(Beliefs.FoundWatersource, () => !WaterPosition.IsUnityNull());
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


            agent.SetupBeliefs(beliefs);
        }

        private void CreatingActions()
        {
            var actions = new HashSet<AgentAction>();
            var beliefs = agent.beliefs;

            actions.Add(new AgentAction
                .Builder(Actions.Idle)
                .AddEffect(Beliefs.Nothing, beliefs)
                .WithStrategy(new IdleStrategy(6f))
                .Build());

            actions.Add(new AgentAction
                .Builder(Actions.WanderAround)
                .AddEffect(Beliefs.Nothing, beliefs)
                .WithStrategy(new WanderStrategy(this, 5f))
                .Build());


            actions.Add(new AgentAction
                .Builder(Actions.FindingGrassPatch)
                .AddEffect(Beliefs.SheepAtFoodSource, beliefs)
                .WithStrategy(new SearchStrategy(this, () => GrassPosition != null ))
                .Build()
                );

            actions.Add(new AgentAction
                .Builder(Actions.FindingWaterPatch)
                .AddEffect(Beliefs.SheepAtWaterSource, beliefs)
                .WithStrategy(new SearchStrategy(this, () => WaterPosition != null))
                .Build()
                );

            actions.Add(new AgentAction
                .Builder(Actions.CommandSheeptoGrassLocation)
                .AddEffect(Beliefs.SheepAtFoodSource, beliefs)
                .AddPrecondition(Beliefs.FoundFoodSource, beliefs)
                .WithStrategy(new DogCommandMoveSheepStrategy(flock,
                () => GrassPosition.position,
                dog ))
                .Build()
                );

            actions.Add(new AgentAction
                .Builder(Actions.WaitForSheepToEat)
                .AddEffect(Beliefs.SheepEaten,beliefs)
                .AddPrecondition(Beliefs.SheepAtFoodSource, beliefs)
                .WithStrategy(new WaitTillStrategy(()=> flock.flockFood >= acceptableFoodLevel))
                .Build()
                );

            agent.SetupActions(actions);

        }

        private void CreatingGoals()
        {
            var goals = new HashSet<AgentGoal>();
            var beliefs = agent.beliefs;

            goals.Add(new AgentGoal.Builder(Goal.Relax)
                .WithDesiredEffect(Beliefs.Nothing, beliefs)
                .WithPriority(1)
                .Build());

            goals.Add(new AgentGoal.Builder(Goal.FindGrassSource)
                .WithDesiredEffect(Beliefs.FoundFoodSource , beliefs)
                .WithPriority(2)
                .Build());

            goals.Add(new AgentGoal.Builder(Goal.FeedSheep)
                .WithDesiredEffect(Beliefs.SheepEaten, beliefs)
                .WithPriority(3)
                .Build());

            agent.SetupGoals(goals);

        }

        private void SenseCollision()
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, senseRadius);
            
            foreach(var hit in hits)
            {
                print($"has hit {hit.transform.name}");
                if (hit.gameObject.layer == LayerManager.GrassPatchLayer)
                {
                    print("found grass patch");
                    //that means that it is a grass patch
                    GrassPosition = hit.transform;
                }
                else if (hit.gameObject.layer == LayerManager.WaterPatchLayer)
                {
                    WaterPosition = hit.transform;
                }
            }

        }

    }
}