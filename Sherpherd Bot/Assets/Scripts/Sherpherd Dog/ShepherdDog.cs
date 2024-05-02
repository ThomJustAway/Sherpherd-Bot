using PGGE.Patterns;
using Sheep;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace sherpherdDog
{
    public class ShepherdDog : MonoBehaviour
    {
        // dog https://royalsocietypublishing.org/doi/epdf/10.1098/rsif.2014.0719

        //need to have like three behaviour
        [Header("Movement")]
        [SerializeField] private float minSpeed = 1f;
        [SerializeField] private float maxSpeed = 8f;
        [SerializeField] private float rotationSpeed = 10f;

        [Header("Collecting and gathering debugging")]
        [SerializeField] private SheepFlock flockData; //to know what the flock information
        [SerializeField] private float senseRadius = 30f;
        [SerializeField] private float collectionOffset = 2f;
        [SerializeField] private float gatheringOffset = 2f;
        [SerializeField] private float targetRadius = 6f;
        [Header("Debugging")]
        [SerializeField] private Vector3 targetPoint;

        //fsm state machine
        private FSM fsm;



    }

    public enum DogState
    {
        ListeningState,
        Driving,
        Gathering,
    }
}