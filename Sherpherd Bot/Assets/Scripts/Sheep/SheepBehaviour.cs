using System.Collections;
using UnityEngine;

namespace Sheep
{
    public class SheepBehaviour : MonoBehaviour
    {
        [SerializeField] private float walkingState;
        [SerializeField] private float rotationSpeed;


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