using System.Collections;
using UnityEngine;

namespace Sheep
{
    public class SheepFlock : MonoBehaviour
    {
        [SerializeField] private float rotationSpeed;

        [Header("for flocking")]
        [SerializeField] private float cohesionRadius;
        [SerializeField] private float seperationRadius;
        [SerializeField] private float alignmentRadius;
        [SerializeField] private float escapeRadius;

        [SerializeField] private float seperationSoftness = 1f;
        [SerializeField] private float escapeSoftness = 10f;

        [Header("Weights")]
        //first weight
        [Range(0,1f)]
        [SerializeField] private float firstCohesionWeight =1f;
        [Range(0, 1f)]
        [SerializeField] private float firstSeperationWeight =1f;
        [Range(0, 1f)]
        [SerializeField] private float firstAlignmentWeight = 1f;
        [SerializeField] private float escapeWeight = 1f;
        //second weights
        [SerializeField] private float secondCohesionWeight = 1f;
        [SerializeField] private float secondSeperationWeight = 1f;
        [SerializeField] private float secondAlignmentWeight = 1f;

        [Header("toggle Rules")]
        [SerializeField] private bool cohesionRule;
        [SerializeField] private bool seperationRule;
        [SerializeField] private bool alignmentRule;
        [SerializeField] private bool escapeRule;

        [SerializeField] private float maxVelocity = 6f;
        [SerializeField] private float minVelocityThreshold = 1f;

        [Header("Debugging")]
        [SerializeField] private Transform predator;

        [Header("sheeps")]
        [SerializeField] private BoxCollider bound;
        [SerializeField] private float yOffset;
        [SerializeField] private int numberOfSheep;
        [SerializeField] private GameObject sheepPrefab;
        #region getter
        public float RotationSpeed { get => rotationSpeed; set => rotationSpeed = value; }
        #region radius
        public float CohesionRadius { get => cohesionRadius; set => cohesionRadius = value; }
        public float SeperationRadius { get => seperationRadius; set => seperationRadius = value; }
        public float AlignmentRadius { get => alignmentRadius; set => alignmentRadius = value; }
        public float EscapeRadius { get => escapeRadius; set => escapeRadius = value; }
        #endregion
        #region softness
        public float EscapeSoftness { get => escapeSoftness; set => escapeSoftness = value; }
        public float SeperationSoftness { get => seperationSoftness; set => seperationSoftness = value; }
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
        public float EscapeWeight { get => escapeWeight; set => escapeWeight = value; }
        public float SecondCohesionWeight { get => secondCohesionWeight; set => secondCohesionWeight = value; }
        public float SecondSeperationWeight { get => secondSeperationWeight; set => secondSeperationWeight = value; }
        public float SecondAlignmentWeight { get => secondAlignmentWeight; set => secondAlignmentWeight = value; }
        #endregion

        public Transform Predator { get => predator; set => predator = value; }
        public float MaxVelocity { get => maxVelocity; set => maxVelocity = value; }
        public float MinVelocityThreshold { get => minVelocityThreshold; set => minVelocityThreshold = value; }
        #endregion

        private void Start()
        {
            Predator = GameObject.FindGameObjectWithTag("Predator").transform;

            for(int i = 0; i < numberOfSheep ; i++)
            {
                float randX = Random.Range(bound.bounds.min.x, bound.bounds.max.x);
                float randZ = Random.Range(bound.bounds.min.z, bound.bounds.max.z);
                var sheep = Instantiate(sheepPrefab, 
                    new Vector3(randX,yOffset,randZ) , 
                    Quaternion.identity,
                    transform);
                sheep.GetComponent<SheepBehaviour>().Init(this);
            }
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