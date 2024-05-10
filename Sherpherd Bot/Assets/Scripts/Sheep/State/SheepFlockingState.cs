using Data_control;
using PGGE.Patterns;
using System;
using System.Linq;
using UnityEngine;

namespace Sheep
{
}

namespace Sheep
{
    /// <summary>
    /// The sheep behaviour responsible of flocking with the other neighbours as well as 
    /// flee from shepherd dog.
    /// </summary>
    public class SheepFlockingState : SheepState
    {
        //this is all reference in http://www.diva-portal.org/smash/get/diva2:675990/FULLTEXT01.pdf
        private float elapseTime;
        public SheepFlockingState(FSM fsm, SheepFlock flock, SheepBehaviour sheep) : base(fsm, flock, sheep)
        {
            mId = (int)SheepStates.Flocking;
        }

        public override void Enter()
        {
            elapseTime = 0f;
        }

        public override void Update()
        {
            CalculateVelocity();
            MoveSheep();
            DecidedIfCanIdle();
        }

        /// <summary>
        /// Will decide the velocity of the sheep based on the environment it is surround with
        /// This includes 
        /// - Sheeps nearby
        /// - How close the shepherd dog is from said sheep
        /// - How close it is near to a wall
        /// </summary>
        private void CalculateVelocity()
        {
            //
            Vector3 resultantVector = Vector3.zero;
            //in the paper, there are two multipler that affects the weight
            //First one affects the overall strength of the flock and the other
            //one for fine tuneing the strength of the weight
            float secondWeight = CalculateSecondMultipler();

            //calculate the weight of cohesion, alignment and seperation 
            // the second weight in the code represent how much the attribute
            // (cohesion, alignment and seperation) should affect the sheep
            // the closer the shepherd dog is from the sheep, the more it would 
            // to stick together.

            float weightOfCohesion = (flock.FirstCohesionWeight * (1 + secondWeight * flock.SecondCohesionWeight));
            float weightOfAlignment = (flock.FirstAlignmentWeight * (1 + secondWeight * flock.SecondAlignmentWeight));
            float weightOfSeperation = (flock.FirstSeperationWeight * (1 + secondWeight * flock.SecondSeperationWeight));

            //Afterwards, add all the rules together to get the resultant behaviour
            resultantVector = weightOfCohesion * CohesionRule() +
                weightOfAlignment * AlignmentRule() +
                weightOfSeperation * SeperationRule() +
                flock.EscapeWeight * EscapeRule() +
                flock.WallAvoidanceWeight * WallAvoidanceRule();

            //for debugging
            //var cohesion = CohesionRule();
            //var alignment = AlignmentRule();
            //var seperation = SeperationRule();
            //var escape = EscapeRule();

            //resultantVector = weightOfCohesion * cohesion +
            // weightOfAlignment * alignment +
            // weightOfSeperation * seperation +
            // flock.EscapeWeight * escape;

            //if the resultant vector has a great amount of velocity, clamp it
            //else check if it falls a certain value. If it does, then stop it from moving.
            if (resultantVector.magnitude < flock.MinVelocityThreshold)
            {
                //if the velocity is small or not worth considering, stop the sheep
                resultantVector = Vector3.zero;
            }
            else
            {
                //Prevent the resultant velocity to be over a vertain magnitude.
                resultantVector = Vector3.ClampMagnitude(resultantVector, flock.MaxVelocity);
            }
            
            sheepBehaviour.ResultantVelocity = resultantVector;
            sheepBehaviour.Velocity = resultantVector;
        }
        /// <summary>
        /// Basic moving script that will move the sheep based on the velocity.
        /// </summary>
        private void MoveSheep()
        {
            //basic function to move the sheeps.
            if (sheepBehaviour.Velocity != Vector3.zero)
            {
                sheepBehaviour.Velocity.y = 0; //clamp the y value
                var targetRotation = Quaternion.LookRotation(sheepBehaviour.Velocity);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation,
                    Time.deltaTime * flock.RotationSpeed);
                //rotate the sheep to the intended target velocity

                //move the sheep
                transform.position += transform.forward * sheepBehaviour.Velocity.magnitude * Time.deltaTime;
            }
        }
        /// <summary>
        /// A simple function to see if the sheep can idle.
        /// </summary>
        private void DecidedIfCanIdle()
        {
            //if the sheep is not moving. it means it is now not in a threat.
            if(sheepBehaviour.Velocity == Vector3.zero)
            {
                //continue the timer
                elapseTime += Time.deltaTime;
                if(elapseTime > flock.TimeToEnterIdle)
                {
                    //switch to Idle
                    mFsm.SetCurrentState((int)SheepStates.Idle);
                }
            }
            else 
            {
                //reset the timer
                elapseTime = 0f;
            }
        }

        #region rules
        /// <summary>
        /// Calculate the sheeps that is under the cohesion radius. it would calculate all the nearby
        /// neighbours position and the center of all the local neighbour
        /// </summary>
        /// <returns>The vector that will lead the sheep to the closest local neighbour</returns>
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
        /// <summary>
        /// find the nearby sheep in the seperation radius and try to avoid them.
        /// </summary>
        /// <returns>The seperation vector to avoid the sheeps.</returns>
        private Vector3 SeperationRule()
        {
            Vector3 result = Vector3.zero;
            if (!flock.SeperationRule) return result;

            var sheeps = Physics.OverlapSphere(transform.position, flock.SeperationRadius, LayerManager.SheepLayer);
            //No check when no sheeps detected
            if (sheeps.Length == 0) return result;

            foreach (var collider in sheeps)
            {
                //get all the sheeps nearby and try to go to the opposite direction from them 
                Vector3 direction = (transform.position - collider.transform.position);
                //the strength of the seperation increases as the sheep gets too close using the INVSQRT
                result += direction.normalized * InvSqrt(direction.magnitude * flock.SeperationSoftness);
            }

            return result;
        }

        /// <summary>
        /// Will try to align the movement direciton with the neighbour flock. 
        /// </summary>
        /// <returns>A resultant vector that give the overall direction of the flock.</returns>
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
            //find the sum of all the sheeps and get the average to find the overall 
            //velocity of the sheeps.
            return result / sheeps.Length;
        }

        /// <summary>
        /// The rule to escape from the shepherd dog. it will try to sense
        /// if the dog is nearby and would try to flee from it as much as possible.
        /// </summary>
        /// <returns>a velocity that will try to avoid the shepherd dog.</returns>
        private Vector3 EscapeRule()
        {
            Vector3 result = Vector3.zero;
            if (!flock.EscapeRule) return result;

            var predator = Physics.OverlapSphere(transform.position, flock.EscapeRadius, LayerManager.PredatorLayer);
            if (predator.Length == 0) return result;

            //will find the predator dogs and would try
            //and run away from the dog in the opposite direction
            //invsqrt to make sure it would run faster the closer the dog is.
            Vector3 direction = (transform.position - predator[0].transform.position);
            return direction.normalized * InvSqrt(direction.magnitude * flock.EscapeSoftness);
        }
        /// <summary>
        /// A simple rule to avoid crashing from the wall.
        /// </summary>
        /// <returns>A velocity that will avoid nearby walls</returns>
        private Vector3 WallAvoidanceRule()
        {
            Vector3 result = Vector3.zero;

            //try to find the nearest wall
            var wall = Physics.OverlapSphere(transform.position, flock.WallAvoidanceRadius, LayerManager.WallLayer);
            if (wall.Length == 0) return result;

            foreach(var collider in wall)
            {//if there is a wall, go to the opposite direction of the wall to avoid collision.
                var point = collider.ClosestPoint(transform.position);
                result += transform.position - point;
            }
            
            return result.normalized * InvSqrt(result.magnitude * flock.WallSoftness);
        }
        #endregion

        #region functions
        /// <summary>
        /// A special weight that will make the sheep alignment, seperation and cohesion 
        /// rules be more in effect as the shepherd dog gets closer to the sheeps.
        /// </summary>
        /// <returns>A weight that affect the collision,alignment and seperation attributes of the sheep</returns>
        private float CalculateSecondMultipler()
        {
            float distance = Vector3.Distance(flock.Predator.position, transform.position);
            return (float)((1 / Math.PI) * Math.Atan((flock.EscapeRadius - distance) / 20) + 0.5f);
        }

        //inverse sqrt function to calculate certain rules and weights
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
        #endregion
    }
}