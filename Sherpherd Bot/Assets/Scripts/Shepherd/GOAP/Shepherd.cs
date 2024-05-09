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
        public float ShearingRadius { get => shearingRadius; set => shearingRadius = value; }

        [Header("feeding")]
        [SerializeField] private float grassAndWaterAcceptableRange;
        [SerializeField] private float acceptableSaturationLevel;
        [SerializeField] private float acceptableHydrationLevel;
        [SerializeField] private float wanderingRadius;

        [Header("wool")]
        [SerializeField] private float acceptableWoolSize;
        [SerializeField] private Transform sheeringPosition;
        [SerializeField] private float shearingRadius;
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
            //For eating and drinking
            beliefsFactory.AddBelief(Beliefs.Nothing, () => false);//can be done repeatedly
            #region food and water
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
            beliefsFactory.AddBelief(Beliefs.SheepEaten, () => flock.flocksaturation > acceptableSaturationLevel);
            beliefsFactory.AddBelief(Beliefs.SheepHydrated, () => flock.flockHydration > acceptableHydrationLevel);
            #endregion
            //for shearing
            beliefsFactory.AddBelief(Beliefs.SheepAtShearingPosition, () => InWithinLocation(flock.CG,
                sheeringPosition?.position ?? Vector3.positiveInfinity, //can nvr be reach
                dog.TargetRadius
                ));
            beliefsFactory.AddBelief(Beliefs.SheepHasWool, () => flock.flockWool > acceptableWoolSize);
            beliefsFactory.AddBelief(Beliefs.NearSheeps, () => InWithinLocation(
                flock.CG,
                transform.position,
                senseRadius
                ));
            beliefsFactory.AddBelief(Beliefs.FinishShearing, () => flock.flockWool == 0f 
            && Physics.CheckSphere(transform.position, senseRadius, LayerManager.WoolLayer)
            );

            agent.SetupBeliefs(beliefs);
        }

        private void CreatingActions()
        {
            var actions = new HashSet<AgentAction>();
            var beliefs = agent.beliefs;

            #region chill behaviour
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
            #endregion

            #region finding water patch
            actions.Add(new AgentAction
                .Builder(Actions.FindingGrassPatch)
                .AddEffect(Beliefs.FoundFoodSource, beliefs)
                .WithStrategy(new SearchStrategy(this, () => GrassPosition != null ))
                .Build()
                );

            actions.Add(new AgentAction
                .Builder(Actions.FindingWaterPatch)
                .AddEffect(Beliefs.FoundWatersource, beliefs)
                .WithStrategy(new SearchStrategy(this, () => WaterPosition != null))
                .Build()
                );
            #endregion

            #region moving sheep
            actions.Add(new AgentAction
                .Builder(Actions.CommandSheeptoGrassLocation)
                .AddEffect(Beliefs.SheepAtFoodSource, beliefs)
                .AddPrecondition(Beliefs.FoundFoodSource, beliefs)
                .WithStrategy(
                new DogCommandMoveSheepStrategy(flock,
                () => GrassPosition.position,
                dog ))
                .Build()
                );

            actions.Add(new AgentAction
                .Builder(Actions.CommandSheeptoWaterLocation)
                .AddEffect(Beliefs.SheepAtWaterSource, beliefs)
                .AddPrecondition(Beliefs.FoundWatersource, beliefs)
                .WithStrategy(
                new DogCommandMoveSheepStrategy(flock,
                () => WaterPosition.position,
                dog))
                .Build()
                );

            #endregion

            #region waiting actions
            actions.Add(new AgentAction
                .Builder(Actions.WaitForSheepToDrink)
                .AddEffect(Beliefs.SheepHydrated,beliefs)
                .AddPrecondition(Beliefs.SheepAtWaterSource, beliefs)
                .WithStrategy(new WaitTillStrategy(()=> flock.flockHydration >= acceptableHydrationLevel))
                .Build()
                );

            actions.Add(new AgentAction
                .Builder(Actions.WaitForSheepToEat)
                .AddEffect(Beliefs.SheepEaten, beliefs)
                .AddPrecondition(Beliefs.SheepAtFoodSource, beliefs)
                .WithStrategy(new WaitTillStrategy(() => flock.flocksaturation>= acceptableSaturationLevel))
                .Build()
                );
            #endregion

            actions.Add(new AgentAction
                .Builder(Actions.CommandSheeptoShearingLocation)
                .AddEffect(Beliefs.SheepAtShearingPosition , beliefs)
                .WithStrategy(new DogCommandMoveSheepStrategy(flock, 
                ()=> sheeringPosition.position, 
                dog ))
                .Build()
                );

            actions.Add(new AgentAction
                .Builder(Actions.MoveToSheeps)
                .AddEffect(Beliefs.NearSheeps, beliefs)
                .WithStrategy(new MoveStrategy(this,() => flock.CG))
                .Build()
                );

            actions.Add(new AgentAction
                .Builder(Actions.ShearSheeps)
                .AddEffect(Beliefs.FinishShearing, beliefs)
                .AddPrecondition(Beliefs.SheepHasWool, beliefs)
                .AddPrecondition(Beliefs.SheepAtShearingPosition,beliefs)
                .WithStrategy(new ShearingStrategy(flock,this))
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

            goals.Add(new AgentGoal.Builder(Goal.FindWaterSource)
                .WithDesiredEffect(Beliefs.FoundWatersource, beliefs)
                .WithPriority(2)
                .Build());

            goals.Add(new AgentGoal.Builder(Goal.FeedSheep)
                .WithDesiredEffect(Beliefs.SheepEaten, beliefs)
                .WithPriority(3)
                .Build());

            goals.Add(new AgentGoal.Builder(Goal.HydrateSheep)
                .WithDesiredEffect(Beliefs.SheepHydrated, beliefs)
                .WithPriority(3)
                .Build());

            goals.Add(new AgentGoal.Builder(Goal.ShearSheep)
                .WithDesiredEffect(Beliefs.FinishShearing, beliefs)
                .WithPriority(4)
                .Build());

            agent.SetupGoals(goals);

        }

        private void SenseCollision()
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, senseRadius);
            
            foreach(var hit in hits)
            {
                //print($"has hit {hit.transform.name}");
                string name = LayerMask.LayerToName(hit.gameObject.layer);
                if (name == "Grass patch")
                {
                    //print("found grass patch");
                    //that means that it is a grass patch
                    GrassPosition = hit.transform;
                }
                else if (name == "Water patch")
                {
                    WaterPosition = hit.transform;
                }
            }

        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, shearingRadius);
        }
    }
}