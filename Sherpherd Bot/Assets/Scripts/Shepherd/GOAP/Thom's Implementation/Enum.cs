using System.Collections;
using UnityEngine;

namespace GOAPTHOM
{
    public enum Beliefs
    {
        FoundWatersource,
        FoundFoodSource,
        HasWool,
        NearSheep,
        WoolOnWagon,
        SheepEaten,
        SheepHydrated,
        SheepNotEaten,
        SheepNotHydrate,
        SheepAtFoodSource,
        SheepAtWaterSource,
        SheepHasWool,
        FurOnGround,
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
        sellWool,
        MoveToOriginalPlace,
        WanderAround,
        CommandSheeptoGrassLocation,
        CommandSheeptowaterLocation,

    }

    public enum Goal
    {
        FeedSheep,
        HydrateSheep,
        SheerSheep,
        Sellfur,
        FindWaterSource,
        FindGrassSource,
        Relax
    }
}