using System.Collections;
using UnityEngine;

namespace Sheep
{
    public class SheepFlock : MonoBehaviour
    {
        [SerializeField] private float rotationSpeed;

        #region radius
        [Header("for flocking")]
        [SerializeField] private float cohesionRadius;
        [SerializeField] private float seperationRadius;
        [SerializeField] private float alignmentRadius;
        [SerializeField] private float escapeRadius;
        [SerializeField] private float wallAvoidanceRadius;
        #endregion
        #region softness
        [SerializeField] private float seperationSoftness = 1f;
        [SerializeField] private float escapeSoftness = 10f;
        [SerializeField] private float wallSoftness = 3f;
        #endregion
        #region weights
        [Header("Weights")]
        //first weight
        [Range(0,1f)]
        [SerializeField] private float firstCohesionWeight =1f;
        [Range(0, 1f)]
        [SerializeField] private float firstSeperationWeight =1f;
        [Range(0, 1f)]
        [SerializeField] private float firstAlignmentWeight = 1f;
        [SerializeField] private float escapeWeight = 1f;
        [SerializeField] private float wallAvoidanceWeight = 4f;
        //second weights
        [SerializeField] private float secondCohesionWeight = 1f;
        [SerializeField] private float secondSeperationWeight = 1f;
        [SerializeField] private float secondAlignmentWeight = 1f;
        #endregion
        #region rules
        [Header("toggle Rules")]
        [SerializeField] private bool cohesionRule;
        [SerializeField] private bool seperationRule;
        [SerializeField] private bool alignmentRule;
        [SerializeField] private bool escapeRule;
        #endregion
        [SerializeField] private float maxVelocity = 6f;
        [SerializeField] private float minVelocityThreshold = 1f;

        [Header("Debugging")]
        [SerializeField] private Transform predator;
        #region sheep
        [Header("sheeps")]
        [SerializeField] private BoxCollider bound;
        [SerializeField] private float yOffset;
        [SerializeField] private float furModelGrowth;
        [SerializeField] private int numberOfSheep;
        [SerializeField] private GameObject sheepPrefab;
        #endregion
        [Header("Idle")]
        [SerializeField] private float timeToEnterIdle;

        [Header("Eating")]
        [SerializeField] private float timeToEnterEatStateMin;
        [SerializeField] private float timeToEnterEatStateMax;
        [SerializeField] private float timeToEatFinish;
        [SerializeField] private float eatingRadius;

        [Header("Growth")]
        [SerializeField] private int foodCost = 5;
        [SerializeField] private int waterCost = 5;
        [Range(0, 1)]
        [SerializeField] private float probabilityOfGrowth = 0.5f;
        [SerializeField] private float timeGrowth = 10f;

        //global information
        public SheepBehaviour[] Sheeps { get; private set; }
        public float flockFood { get; private set; }
        public float flockWater { get; private set; }
        #region getter
        public float RotationSpeed { get => rotationSpeed; set => rotationSpeed = value; }
        #region radius
        public float CohesionRadius { get => cohesionRadius; set => cohesionRadius = value; }
        public float SeperationRadius { get => seperationRadius; set => seperationRadius = value; }
        public float AlignmentRadius { get => alignmentRadius; set => alignmentRadius = value; }
        public float EscapeRadius { get => escapeRadius; set => escapeRadius = value; }
        public float WallAvoidanceRadius { get => wallAvoidanceRadius; set => wallAvoidanceRadius = value; }
        #endregion
        #region softness
        public float EscapeSoftness { get => escapeSoftness; set => escapeSoftness = value; }
        public float SeperationSoftness { get => seperationSoftness; set => seperationSoftness = value; }
        public float WallSoftness { get => wallSoftness; set => wallSoftness = value; }
        #endregion
        #region rule toggle
        public bool CohesionRule { get => cohesionRule; set => cohesionRule = value; }
        public bool SeperationRule { get => seperationRule; set => seperationRule = value; }
        public bool AlignmentRule { get => alignmentRule; set => alignmentRule = value; }
        public bool EscapeRule { get => escapeRule; set => escapeRule = value; }
        #endregion
        #region weights
        public float FirstCohesionWeight { get => firstCohesionWeight; set => firstCohesionWeight = value; }
        public float FirstSeperationWeight { get => firstSeperationWeight; set => firstSeperationWeight = value; }
        public float FirstAlignmentWeight { get => firstAlignmentWeight; set => firstAlignmentWeight = value; }
        public float WallAvoidanceWeight { get => wallAvoidanceWeight; set => wallAvoidanceWeight = value; }
        public float EscapeWeight { get => escapeWeight; set => escapeWeight = value; }
        public float SecondCohesionWeight { get => secondCohesionWeight; set => secondCohesionWeight = value; }
        public float SecondSeperationWeight { get => secondSeperationWeight; set => secondSeperationWeight = value; }
        public float SecondAlignmentWeight { get => secondAlignmentWeight; set => secondAlignmentWeight = value; }
        #endregion

        public Transform Predator { get => predator; set => predator = value; }
        public float MaxVelocity { get => maxVelocity; set => maxVelocity = value; }
        public float MinVelocityThreshold { get => minVelocityThreshold; set => minVelocityThreshold = value; }
        public float TimeToEnterIdle { get => timeToEnterIdle; set => timeToEnterIdle = value; }
        #region sheep eating
        public float TimeToEatFinish { get => timeToEatFinish; set => timeToEatFinish = value; }
        public float TimeToEnterEatStateMin { get => timeToEnterEatStateMin; set => timeToEnterEatStateMin = value; }
        public float TimeToEnterEatStateMax { get => timeToEnterEatStateMax; set => timeToEnterEatStateMax = value; }
        public float EatingRadius { get => eatingRadius; set => eatingRadius = value; }
        public int FoodCost { get => foodCost; set => foodCost = value; }
        #endregion
        public int WaterCost { get => waterCost; set => waterCost = value; }
        #region fur growth 
        public float ProbabilityOfGrowth { get => probabilityOfGrowth; set => probabilityOfGrowth = value; }
        public float TimeGrowth { get => timeGrowth; set => timeGrowth = value; }
        public float FurModelGrowth { get => furModelGrowth; set => furModelGrowth = value; }
        #endregion
        

        public Vector3 CG { get; private set; }
        #endregion

        private void Start()
        {
            Predator = GameObject.FindGameObjectWithTag("Predator").transform;
            Sheeps = new SheepBehaviour[numberOfSheep];
            for(int i = 0; i < numberOfSheep ; i++)
            {
                float randX = Random.Range(bound.bounds.min.x, bound.bounds.max.x);
                float randZ = Random.Range(bound.bounds.min.z, bound.bounds.max.z);
                var sheep = Instantiate(sheepPrefab, 
                    new Vector3(randX,yOffset,randZ) , 
                    Quaternion.identity,
                    transform);
                SheepBehaviour component = sheep.GetComponent<SheepBehaviour>();
                component.Init(this);
                component.name = $"sheep {i}";
                //populating the sheeps
                Sheeps[i] = component;
            }
        }

        private void Update()
        {
            CalculateFlockData();
        }

        private void CalculateFlockData()
        {
            float saturationLevel = 0f;
            float hydrationLevel = 0f;
            Vector3 val = Vector3.zero;
            foreach(var t in Sheeps)
            {
                val += t.transform.position;
                saturationLevel += t.Food;
                hydrationLevel += t.Water;
            }
            CG = val / Sheeps.Length;
            flockFood = saturationLevel / Sheeps.Length;
            flockWater = hydrationLevel / Sheeps.Length;
        }
    }

    

    public enum SheepStates
    {
        Drink,
        Eat,
        Sleep,
        Idle,
        GrowFur,
        Movement
    }
}