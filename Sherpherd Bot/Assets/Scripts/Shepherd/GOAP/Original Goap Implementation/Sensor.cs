using System;
using UnityEngine;

namespace OriginalGOAP
{

    //For the GOAP agent to sense things
    [RequireComponent(typeof(SphereCollider))]
    public class Sensor : MonoBehaviour
    {
        [SerializeField] float detectionRadius = 5f;
        [SerializeField] float timerInterval = 1f;

        SphereCollider detectionRange;

        public event Action OnTargetChanged = delegate { };

        public Vector3 TargetPosition => target ? target.transform.position : Vector3.zero;
        public bool IsTargetInRange => TargetPosition != Vector3.zero;

        GameObject target;
        Vector3 lastKnownPosition;
        CountdownTimer timer;

        void Awake()
        {
            detectionRange = GetComponent<SphereCollider>();
            detectionRange.isTrigger = true;
            detectionRange.radius = detectionRadius;
        }

        void Start()
        {
            timer = new CountdownTimer(timerInterval);
            timer.OnTimerStop += () =>
            {
                UpdateTargetPosition(target.OrNull());
                timer.Start();
            };
            timer.Start();
        }

        void Update()
        {
            timer.Tick(Time.deltaTime);
        }

        //this will update the game object "target if it has move or it ever existed.
        void UpdateTargetPosition(GameObject target = null)
        {
            this.target = target;
            if (IsTargetInRange && (lastKnownPosition != TargetPosition || lastKnownPosition != Vector3.zero))
            {
                lastKnownPosition = TargetPosition;
                OnTargetChanged.Invoke();
            }
        }

        void OnTriggerEnter(Collider other)
        {
            //if not the player, then make sure that the player knows
            if (!other.CompareTag("Player")) return;
            UpdateTargetPosition(other.gameObject);
        }

        void OnTriggerExit(Collider other)
        {
            //if not the player, then make sure to remove it from update target position.
            if (!other.CompareTag("Player")) return;
            UpdateTargetPosition();
        }

        void OnDrawGizmos()
        {
            Gizmos.color = IsTargetInRange ? Color.red : Color.green;
            Gizmos.DrawWireSphere(transform.position, detectionRadius);
        }
    }
}