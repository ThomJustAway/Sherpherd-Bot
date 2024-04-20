﻿using Data_control;
using PGGE.Patterns;
using System;
using System.Linq;
using UnityEngine;

namespace Sheep
{
    public class SheepState : FSMState
    {
        protected SheepFlock flock;
        protected SheepBehaviour sheepBehaviour;
        protected Transform transform;
        public SheepState(FSM fsm, SheepFlock flock, SheepBehaviour sheep) : base(fsm)
        {
            this.flock = flock;
            sheepBehaviour = sheep;
            this.transform = sheep.transform;
        }
    }

    public class SheepMovementState : SheepState
    {
        public SheepMovementState(FSM fsm, SheepFlock flock, SheepBehaviour sheep) : base(fsm, flock, sheep)
        {
            mId = (int)SheepStates.Movement;
        }

        //this is all reference in http://www.diva-portal.org/smash/get/diva2:675990/FULLTEXT01.pdf

        //public SheepMovementState(FSM fsm, SheepFlock sheep) : base(fsm, sheep)
        //{
        //    mId = (int) SheepStates.Movement;
        //}

        //will do wondering or roaming around.

        public override void Enter()
        {
            base.Enter();
        }

        public override void Update()
        {
            Vector3 resultantVector = Vector3.zero;
            float secondWeight = CalculateSecondMultipler();

            float weightOfCohesion = (flock.FirstCohesionWeight * (1 + secondWeight * flock.SecondCohesionWeight));
            float weightOfAlignment = (flock.FirstAlignmentWeight * (1 + secondWeight * flock.SecondAlignmentWeight));
            float weightOfSeperation = (flock.FirstSeperationWeight * (1 + secondWeight * flock.SecondSeperationWeight));

            var cohesion = CohesionRule();
            var alignment = AlignmentRule();
            var seperation = SeperationRule();
            var escape = EscapeRule();

            //resultantVector = weightOfCohesion * CohesionRule() +
            //    weightOfAlignment * AlignmentRule() +
            //    weightOfSeperation * SeperationRule() +
            //    flock.EscapeWeight * EscapeRule();

            resultantVector = weightOfCohesion * cohesion +
             weightOfAlignment * alignment +
             weightOfSeperation * seperation +
             flock.EscapeWeight * escape;

            if (resultantVector.magnitude < flock.MinVelocityThreshold)
            {
                resultantVector = Vector3.zero;
            }
            else
            {
                resultantVector = Vector3.ClampMagnitude(resultantVector, flock.MaxVelocity);
            }
            sheepBehaviour.ResultantVelocity = resultantVector;
            sheepBehaviour.Velocity = resultantVector;

            MoveSheep();
        }

        private void MoveSheep()
        {
            Debug.Log("moving");
            if (sheepBehaviour.Velocity != Vector3.zero)
            {
                sheepBehaviour.Velocity.y = 0; //clamp the y value
                var targetRotation = Quaternion.LookRotation(sheepBehaviour.Velocity);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation,
                    Time.deltaTime * flock.RotationSpeed);

                //move the sheep
                transform.position += transform.forward * sheepBehaviour.Velocity.magnitude * Time.deltaTime;
            }
        }

        private Vector3 CohesionRule()
        {
            if (!flock.CohesionRule) return Vector3.zero;
            Vector3 result = Vector3.zero;

            //layer not working
            var sheeps = Physics.OverlapSphere(transform.position, flock.CohesionRadius, LayerManager.SheepLayer);
            if (sheeps.Length == 0) return result;

            foreach (var collider in sheeps)
            {
                result += collider.transform.position;
            }
            //find avg
            result = result / sheeps.Length;
            //return the directional vector of the cohesion rule
            return (result - transform.position).normalized;
        }

        private Vector3 SeperationRule()
        {
            Vector3 result = Vector3.zero;
            if (!flock.SeperationRule) return result;

            var sheeps = Physics.OverlapSphere(transform.position, flock.SeperationRadius, LayerManager.SheepLayer);
            if (sheeps.Length == 0) return result;
            //No check when no sheeps detected
            foreach (var collider in sheeps)
            {
                Vector3 direction = (transform.position - collider.transform.position);
                result += direction.normalized * InvSqrt(direction.magnitude * flock.SeperationSoftness);
            }

            return result;
        }

        private Vector3 AlignmentRule()
        {
            Vector3 result = Vector3.zero;
            if (!flock.AlignmentRule) return result;
            var sheeps = Physics.OverlapSphere(transform.position, flock.AlignmentRadius, LayerManager.SheepLayer)
                .Select(collider => collider.GetComponent<SheepBehaviour>())
                .ToArray();
            if (sheeps.Length == 0) return result;

            foreach (var lamp in sheeps)
            {
                result += lamp.Velocity;
            }

            return result / sheeps.Length;
        }

        private Vector3 EscapeRule()
        {
            Vector3 result = Vector3.zero;
            if (!flock.EscapeRule) return result;

            var predator = Physics.OverlapSphere(transform.position, flock.EscapeRadius, LayerManager.PredatorLayer);
            if (predator.Length == 0) return result;

            Vector3 direction = (transform.position - predator[0].transform.position);
            return direction.normalized * InvSqrt(direction.magnitude * flock.EscapeSoftness);
        }

        private float CalculateSecondMultipler()
        {
            float distance = Vector3.Distance(flock.Predator.position, transform.position);
            return (float)((1 / Math.PI) * Math.Atan((flock.EscapeRadius - distance) / 20) + 0.5f);
        }

        private float InvSqrt(float number)
        {
            const float threehalfs = 1.5F;

            float x2 = number * 0.5F;
            float y = number;

            // evil floating point bit level hacking
            uint i = BitConverter.ToUInt32(BitConverter.GetBytes(y), 0);

            // value is pre-assumed
            i = 0x5f3759df - (i >> 1);
            y = BitConverter.ToSingle(BitConverter.GetBytes(i), 0);

            // 1st iteration
            y = y * (threehalfs - (x2 * y * y));

            // 2nd iteration, this can be removed
            // y = y * ( threehalfs - ( x2 * y * y ) );

            return y;
        }

    }
}