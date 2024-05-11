using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Wagon
{
    //ignore this. for assignment 2 :)
    public class Wagon : MonoBehaviour
    {
        [ContextMenuItem("Add fur",nameof(Add))]
        [SerializeField] private int maxAmountOfFur;
        [SerializeField] private Transform wool;
        private int furAmount;
        public int FurAmount
        { get => furAmount; private set
            {
                furAmount = value;
                var globalSize = transform.lossyScale;
                wool.localScale = new Vector3(furAmount / globalSize.x, furAmount / globalSize.y, furAmount / globalSize.z);
                wool.localPosition = new Vector3(0,furAmount/2 ,0);
            }
        }
        public bool IsWagonEmpty { get { return FurAmount == 0; } }
        public bool IsFull {get { return FurAmount >= maxAmountOfFur; } }

        private void Start()
        {
            FurAmount = 0;    
        }


        public void AddFur(Transform wool)
        {
            if (IsFull) return;
            int fur = (int)((wool.localScale.x - 1) / 0.1f);
            furAmount += fur;
            Destroy(wool);
        }

        private void Add()
        {
            if (IsFull) return;
            FurAmount++;
        }
    }
}
