using BehaviourTreeImplementation;
using Data_control;
using GOAPTHOM;
using Sheep;
using sherpherdDog;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

namespace Assets.Scripts.Shepherd.GOAP
{
    /// <summary>
    /// The behaviour that contain the shepherd. The AI uses GOAP to help
    /// the shepherd decide which is the best decision it should make. 
    /// </summary>
    public class Shepherd : MonoBehaviour
    {
        //the goap logic that will control the shepherd.
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
        public float HandRadius { get => handRadius; set => handRadius = value; }

        [Header("feeding")]
        [SerializeField] private float grassAndWaterAcceptableRange;
        [SerializeField] private float acceptableSaturationLevel;
        [SerializeField] private float acceptableHydrationLevel;
        [SerializeField] private float wanderingRadius;

        [Header("wool")]
        [SerializeField] private float acceptableWoolSize;
        [SerializeField] private Transform shearingPosition;
        [SerializeField] private float handRadius;
        [ContextMenuItem("Add wool",nameof(AddWool))]
        [SerializeField] private Transform wool;
        [SerializeField]private int woolAmount;
        public int WoolAmount
        {
            get => woolAmount; private set
            {
                woolAmount = value;
                wool.localScale = new Vector3(woolAmount, woolAmount , woolAmount ) * 0.4f;
                wool.localPosition = new Vector3(0, woolAmount / 2, 0) + 
                    new Vector3(0, 4.5f); //the original height
            }
        } //how much wool the shepherd retrieve
        //How the shepherd will sense the world.
        public float SenseRadius { get => senseRadius; set => senseRadius = value; }

        [Header("Selling")]
        [SerializeField] private Barn Barn;

        [Header("player interaction")]
        [SerializeField] private float interactionRange;
        [SerializeField] private PlayerInteractions player;
        [SerializeField] private ShepherdUI UI;
        private bool haveRecieveFood;
        [Header("Behaviour Tree")]
        [SerializeField] private bool useBehaviourTree;
        public bool UseBehaviourTree { get => useBehaviourTree; set => useBehaviourTree = value; }
        public bool HaveRecieveFood { get => haveRecieveFood; set => haveRecieveFood = value; }
        public float InteractionRange { get => interactionRange; set => interactionRange = value; }

        private BehaviourTree behaviourTree;

        private void AddWool()
        {
            WoolAmount++;
        }

        private void Start ()
        {
            WoolAmount = 0;

            //if specify to use behavior tree, it will create the
            //behaviour tree else the GOAP
            if (useBehaviourTree)
            {
                ImplementBehaviourTree();
            }
            else
            {
                ImplementGOAP();
            }

            void ImplementGOAP()
            {
                agent = new GoapAgent();
                //need to set up the belief, actions, goals of the GOAP
                //agent set up planner and update loop create the base line logic.
                agent.CreatePlanner();
                agent.SetUpdate();

                //setting up beliefs, actions and goals for the agent to know.
                CreatingBelief();
                CreatingActions();
                CreatingGoals();
            }
            void ImplementBehaviourTree()
            {
                behaviourTree = new BehaviourTree(); //create a new behaviour tree
                behaviourTree.SetUpSelection(); 

                //currently interact player is the most important node in the sequence.
                //so it will check if the player is nearby. if it is, then it will do the interact player action
                var interactPlayerNode = new SequenceNode("Interact player node");
                interactPlayerNode.AddChild(new Leaf("Is Player nearby",
                    new Condition(() =>
                    InWithinLocation(player.transform.position, transform.position, interactionRange))
                    ));
                #region human interaction

                //will make sure to greet the player when the player is nearby
                var interactPlayerAction = new SequenceNode("interact player action");
                interactPlayerAction.AddChild(new Leaf("Greetings",new CustomFunc(() =>
                {
                    UI.SetTemporaryMessage(5f, "Howdy! How are U doing!");
                    return Status.Success;
                })));

                //After greeting, it will just wait for the player to leave. This also contain 
                //the code that recieve food and thank the player for helping them give them food
                interactPlayerAction.AddChild(new Leaf("Waiting",
                    new WaitingForPlayer(player, UI,this , interactPlayerAction)));
                interactPlayerNode.AddChild(interactPlayerAction);
                #endregion

                //sell wool an important action that the shepherd has too take.
                var sellWool = new SequenceNode("Sell wool");
                //will check if the action can be perform by checking the surrounding condition
                sellWool.AddChild(new Leaf("complete shearing" ,
                    new Condition(()=>flock.flockWool == 0 &&
                    Physics.CheckSphere(transform.position, SenseRadius, LayerManager.WoolLayer))
                    ));
                var sellWoolAction = new SequenceNode("Sell wool");
                sellWool.AddChild(sellWoolAction);
                #region sell wool

                //if can then the shepherd will run the action if all the flock has been sheared
                sellWoolAction.AddChild(new Leaf("collect wool",
                    new CollectingWool(this)));
                //after that, it would move to the barn and sell it.
                sellWoolAction.AddChild(new Leaf("move to Barn" , 
                    new MoveTo(()=> InWithinLocation2D(Barn.transform.position, 
                    transform.position, 
                    handRadius),
                    this,
                    () => Barn.transform.position.With(y:0)
                    )));
                //After the player manage to move to the barn, it can then sell it.
                sellWoolAction.AddChild(new Leaf("sell wool",
                    new CustomFunc(() =>
                    {
                        Barn.SellFur(this);
                        return Status.Success;
                    })));
                #endregion

                // shearing wool once the flock manage to reach a acceptable wool size.
                var shearSheep = new SequenceNode("Shear wool");
                shearSheep.AddChild(new Leaf("Sheep has wool",
                    new Condition(() => flock.flockWool > acceptableWoolSize)));

                #region shearing sheep
                var shearSheepAction = new SequenceNode("shear wool action");

                //To shear all the sheep, flock needs to go to the shearing location
                shearSheepAction.AddChild(
                   new Leaf("Command dog to shearing location",
                   new CustomFunc(() =>
                   {
                       dog.ChaseSheeps(shearingPosition.position);
                       return Status.Success;
                   })
                   ));

                //Afterward, the shepherd must be at the shearing position to shear all the sheeps.
                shearSheepAction.AddChild(
                    new Leaf("Move to Shearing Position" , 
                    new MoveTo(()=> InWithinLocation(shearingPosition.position, transform.position, handRadius),
                    this,
                    () => shearingPosition.position
                    )
                    ));

                //It would then check if the sheeps are at the shearing location. else it would just be doing nothing
                //would be cool to have a idle tree here to make the shepherd more interactive.
                shearSheepAction.AddChild(
                    new Leaf("Sheeps at location",
                    new Condition(()=> InWithinLocation(flock.CG, shearingPosition.position, dog.TargetRadius) , 
                    Status.Running)
                    ));

                //After the sheeps are together, the shear all the sheeps.
                shearSheepAction.AddChild(
                    new Leaf("Shear sheep",
                    new ShearingSheeps(flock, this)
                    ));
                shearSheep.AddChild(shearSheepAction);
                #endregion

                //Hydrate
                var hydrateSheep = new SequenceNode("Hydrate Sheep");
                hydrateSheep.AddChild(new Leaf("Is sheep not hydrated",
                    new Condition(
                        () => !(flock.flockHydration > acceptableHydrationLevel)
                    )));

                #region hydrate sheep
                //if the sheeps aren't hydrated, then make sure that the sheeps are hydrated
                var hydrateSheepActions = new SequenceNode("Hydrate sheep action");
                hydrateSheepActions.AddChild(
                    new Leaf("find water location",
                    new Search(
                        () => WaterPosition != null,
                        this
                        )
                    ));
                //make sure that the shepherd found a water position before commanding
                //the dog to move the said location
                hydrateSheepActions.AddChild(
                    new Leaf("Command dog",
                    new CustomFunc(() =>
                    {
                        dog.ChaseSheeps(WaterPosition.position);
                        return Status.Success;
                    })
                    ));

                //afterwards, just wait for the sheep to reach the destine position.
                hydrateSheepActions.AddChild(
                    new Leaf("Wait for sheep to reach",
                    new Condition
                    (() => InWithinLocation(flock.CG, WaterPosition.position , dog.TargetRadius) , 
                    Status.Running)
                    ));

                //finally just wait for the sheep to finish drinking.
                hydrateSheepActions.AddChild(
                    new Leaf("Wait for sheep to hydrate",
                    new Condition(()=> flock.flockHydration > acceptableHydrationLevel , Status.Running)
                    ));
                hydrateSheep.AddChild(hydrateSheepActions);
                #endregion

                //feeding the sheeps
                var feedSheep = new SequenceNode("Feed Sheep");
                feedSheep.AddChild(new Leaf("Is sheep not fed",
                    new Condition(
                        () => !(flock.flocksaturation > acceptableSaturationLevel)
                    )));
                //same thing as hydrating sheep but for the grass patch location.
                #region Feed sheep
                var FeedSheepActions = new SequenceNode("feed sheep action");
                FeedSheepActions.AddChild(
                    new Leaf("find food location",
                    new Search(
                        () => GrassPosition != null,
                        this
                        )
                    ));
                FeedSheepActions.AddChild(
                    new Leaf("Command dog to location",
                    new CustomFunc(() =>
                    {
                        dog.ChaseSheeps(GrassPosition.position);
                        return Status.Success;
                    })
                    ));
                FeedSheepActions.AddChild(
                    new Leaf("Wait for sheep to reach food location",
                    new Condition
                    (() => InWithinLocation(flock.CG, GrassPosition.position, dog.TargetRadius),
                    Status.Running)
                    ));
                FeedSheepActions.AddChild(
                    new Leaf("Wait for sheep to eat",
                    new Condition(() => flock.flocksaturation > acceptableSaturationLevel, Status.Running)
                    ));

                feedSheep.AddChild(FeedSheepActions);
                #endregion
                //the behavior tree is sort out by importance. the 
                //first branch that is added will have more importance than the following 
                //branch. 
                behaviourTree.AddBranch(interactPlayerNode);
                behaviourTree.AddBranch(sellWool);
                behaviourTree.AddBranch(shearSheep);
                behaviourTree.AddBranch(hydrateSheep);
                behaviourTree.AddBranch(feedSheep);
                //If there is nothing the shepherd can do, then it will just idle
                behaviourTree.AddBranch(new Leaf("Idle",
                    new WaitFor(5f)
                    ));
            }
        }

        private void Update()
        {
            SenseCollision();
            if (useBehaviourTree)
            {
                behaviourTree.Update();
            }
            else
            {
                agent.updateFunction();
            }
        }

        #region Goap
        /// <summary>
        /// create the belief for the GOAP to work with.
        /// </summary>
        private void CreatingBelief()
        {
            var beliefs = new Dictionary<string , AgentBelief>();
            var beliefsFactory = new BeliefFactory(transform, beliefs);
            beliefsFactory.AddBelief(Beliefs.Nothing, () => false); // can be done repeatedly
            #region food and water
            //For eating and drinking
            beliefsFactory.AddBelief(Beliefs.FoundFoodSource, 
                () => !GrassPosition.IsUnityNull());//check if there is an existing grass patch.
            beliefsFactory.AddBelief(Beliefs.FoundWatersource, 
                () => !WaterPosition.IsUnityNull()); //check if there is an existing watet patch
            beliefsFactory.AddBelief(Beliefs.SheepAtFoodSource, () => InWithinLocation(flock.CG, 
                GrassPosition?.position ?? Vector3.positiveInfinity, 
                grassAndWaterAcceptableRange
                ));//check if the flock is within the parameter of the grass patch.
            beliefsFactory.AddBelief(Beliefs.SheepAtWaterSource, () => InWithinLocation(
                WaterPosition?.position ?? Vector3.positiveInfinity, 
                flock.CG,
                grassAndWaterAcceptableRange
                ));//check if the flock is within the parameter of the water patch.
            beliefsFactory.AddBelief(Beliefs.SheepEaten, 
                () => flock.flocksaturation > acceptableSaturationLevel); //determine whether the flock is well fed 
            beliefsFactory.AddBelief(Beliefs.SheepHydrated, 
                () => flock.flockHydration > acceptableHydrationLevel); //determine whether the flock is well hydrated.
            #endregion
            #region shearing
            //for shearing
            beliefsFactory.AddBelief(Beliefs.SheepAtShearingPosition, () => InWithinLocation(flock.CG,
                shearingPosition?.position ?? Vector3.positiveInfinity, 
                dog.TargetRadius
                ));//check if the flock is within the shearing position.
            beliefsFactory.AddBelief(Beliefs.SheepHasWool, 
                () => flock.flockWool > acceptableWoolSize); //check if the flock can be sheared if they have enough wool
            beliefsFactory.AddBelief(Beliefs.NearSheeps, () => InWithinLocation(
                flock.CG,
                transform.position,
                SenseRadius
                )); //check if the shepherd is near the flock.
            beliefsFactory.AddBelief(Beliefs.FinishShearing, () => flock.flockWool == 0f 
            && Physics.CheckSphere(transform.position, SenseRadius, LayerManager.WoolLayer)
            ); //check if it has finish shearing the sheeps.
            #endregion
            #region selling
            //selling
            beliefsFactory.AddBelief(Beliefs.FinishCollectingWool, () => 0.3f > flock.flockWool 
            && !Physics.CheckSphere(transform.position, SenseRadius, LayerManager.WoolLayer) 
            && WoolAmount > 0
            ); //see if the shepherd has completed collecting the wool to get maximum profit

            beliefsFactory.AddBelief(Beliefs.SellWool, 
                () => InWithinLocation2D(
                Barn.transform.position, 
                transform.position, 
                handRadius));//check if the shepherd has sold the wool.
            #endregion

            agent.SetupBeliefs(beliefs);
        }
        /// <summary>
        /// create the Action for the GOAP to work with.
        /// </summary>
        private void CreatingActions()
        {
            var actions = new HashSet<AgentAction>();
            var beliefs = agent.beliefs;

            #region chill behaviour
            //if there is nothing to do then do this actions
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

            //either can idle or wander around 
            #endregion

            #region finding patch
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
            //the shepherd will try to find the grass/water patch until the he sense the 
            //grass/water patch.
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

            actions.Add(new AgentAction
                .Builder(Actions.CommandSheeptoShearingLocation)
                .AddEffect(Beliefs.SheepAtShearingPosition , beliefs)
                .WithStrategy(new DogCommandMoveSheepStrategy(flock, 
                ()=> shearingPosition.position, 
                dog ))
                .Build()
                );
            //this is for moving the flock around different location.
            #endregion

            #region waiting actions
            //this actions is meant to wait for the sheeps to eat and drink finish so that
            //it has enough to create wool.
            actions.Add(new AgentAction
                .Builder(Actions.WaitForSheepToDrink)
                .AddEffect(Beliefs.SheepHydrated,beliefs)
                .AddPrecondition(Beliefs.SheepAtWaterSource, beliefs)
                .WithStrategy(new WaitTillStrategy(()=> flock.flockHydration >= acceptableHydrationLevel))
                .Build()
                );//wait until the flock is well hydrated

            actions.Add(new AgentAction
                .Builder(Actions.WaitForSheepToEat)
                .AddEffect(Beliefs.SheepEaten, beliefs)
                .AddPrecondition(Beliefs.SheepAtFoodSource, beliefs)
                .WithStrategy(new WaitTillStrategy(() => flock.flocksaturation>= acceptableSaturationLevel))
                .Build()
                );//wait until the flock is well fed.
            #endregion

            #region shearing
            //actions that is related to moving
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
            // to shear the sheep it needs the sheep to have enough
            // wool and in a suitable position in order to shear it.
            #endregion

            #region selling
            //actions related to selling.
            actions.Add(new AgentAction
                .Builder(Actions.CollectWool)
                .AddEffect(Beliefs.FinishCollectingWool, beliefs)
                .AddPrecondition(Beliefs.FinishShearing, beliefs)
                .WithStrategy(new CollectingStrategy(this))
                .Build()
                );
            //the shepherd can collect the wool once it has finish shearing.

            actions.Add(new AgentAction
                .Builder(Actions.SellWool)
                .AddEffect(Beliefs.SellWool, beliefs)
                .AddPrecondition(Beliefs.FinishCollectingWool, beliefs)
                .WithStrategy(new SellingStrategy(this,Barn))
                .Build()
                );
            //the shepherd can then sell all the wool once shepherd finish collecting the wool
            #endregion
            agent.SetupActions(actions);
        }
        /// <summary>
        /// create the GOAL for the GOAP to work with.
        /// </summary>
        private void CreatingGoals()
        {
            var goals = new HashSet<AgentGoal>();
            var beliefs = agent.beliefs;

            goals.Add(new AgentGoal.Builder(Goal.Relax)
                .WithDesiredEffect(Beliefs.Nothing, beliefs)
                .WithPriority(1)
                .Build());
            //relaxing has the lowest priority

            goals.Add(new AgentGoal.Builder(Goal.FindGrassSource)
                .WithDesiredEffect(Beliefs.FoundFoodSource , beliefs)
                .WithPriority(2)
                .Build());

            goals.Add(new AgentGoal.Builder(Goal.FindWaterSource)
                .WithDesiredEffect(Beliefs.FoundWatersource, beliefs)
                .WithPriority(2)
                .Build()); 
            //finding water and food source is some what important so just
            //make sure that the shepherd find those area if the sheep are well
            //hydrated, full.

            goals.Add(new AgentGoal.Builder(Goal.FeedSheep)
                .WithDesiredEffect(Beliefs.SheepEaten, beliefs)
                .WithPriority(3)
                .Build());

            goals.Add(new AgentGoal.Builder(Goal.HydrateSheep)
                .WithDesiredEffect(Beliefs.SheepHydrated, beliefs)
                .WithPriority(3)
                .Build());
            //quite important to make sure that the sheep are well fed and hydrated.

            goals.Add(new AgentGoal.Builder(Goal.ShearSheep)
                .WithDesiredEffect(Beliefs.FinishShearing, beliefs)
                .WithPriority(4)
                .Build());

            goals.Add(new AgentGoal.Builder(Goal.Sellfur)
                .WithDesiredEffect(Beliefs.SellWool, beliefs)
                .WithPriority(4)
                .Build());
            //most important which is the goal of the shepherd.

            agent.SetupGoals(goals);

        }
        #endregion

        private void SenseCollision()
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, SenseRadius);
            
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

        #region helpful function
        //function used for call back
        //Check if the two point in space are within a certain distance (3D)
        private bool InWithinLocation(Vector3 targetPos, Vector3 posToCheck, float radius)
        {
            return Vector3.Distance(targetPos, posToCheck) < radius;
        }
        //Check if the two point in space are within a certain distance (2D)
        private bool InWithinLocation2D(Vector3 targetPos, Vector3 posToCheck, float radius)
        {
            Vector2 pos1 = new Vector2(targetPos.x, targetPos.z);
            Vector2 pos2 = new Vector2(posToCheck.x, posToCheck.z);
            return Vector2.Distance(pos1,pos2) < radius;
        }
        #endregion
        /// <summary>
        /// Will Collect wool in the form of a transform
        /// The wool data is stored within the size of the 
        /// wool game.
        /// </summary>
        /// <param name="wool"></param>
        public void CollectWool(Transform wool)
        {
            WoolAmount += (int)((wool.localScale.x - 1) / 0.1f);
            wool.gameObject.GetComponent<Collider>().enabled = false;
            //make sure that the wool cant be detected again.
            Destroy(wool.gameObject);
        }
        public void ResetWool()
        {
            WoolAmount = 0;
        }
        public void Move(Vector3 direction)
        {
            ///move the shepherd and rotate the shepherd
            transform.position += direction * MovingSpeed * Time.deltaTime;
            var targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation,
                Time.deltaTime * RotationSpeed);
        }
        public void RecieveFood()
        {
            haveRecieveFood = true;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, handRadius);
        }
    }
}