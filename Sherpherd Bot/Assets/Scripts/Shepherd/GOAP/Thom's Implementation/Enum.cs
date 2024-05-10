using System.Collections;
using UnityEngine;

namespace GOAPTHOM
{
    public enum Beliefs
    {
        FoundWatersource,
        FoundFoodSource,
        SheepEaten,
        SheepHydrated,
        SheepNotEaten,
        SheepNotHydrate,
        SheepAtFoodSource,
        SheepAtWaterSource,

        NearSheeps,
        SheepHasWool,
        FinishShearing,
        WagonNearby,
        FinishCollectingWool,
        SheepAtShearingPosition,
        SellWool,
        Nothing,
    }

    public enum Actions
    {
        Idle,
        WaitForSheepToEat,
        WaitForSheepToDrink,
        FindingGrassPatch,
        FindingWaterPatch,
        SheerSheep,
        CollectWool,
        PutWoolInWagon,
        PullWagon,
        SellWool,
        MoveToOriginalPlace,
        WanderAround,
        CommandSheeptoGrassLocation,
        CommandSheeptoWaterLocation,

        //collecting wool,
        CommandSheeptoShearingLocation,
        MoveToSheeps,
        ShearSheeps
    }

    public enum Goal
    {
        FeedSheep,
        HydrateSheep,
        SheerSheep,
        Sellfur,
        FindWaterSource,
        FindGrassSource,
        Relax,
        ShearSheep
    }
}