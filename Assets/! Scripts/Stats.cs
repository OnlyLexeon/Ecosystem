using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

public enum StatType
{
    //stats
    MaxHealth,
    MaxHunger,
    MaxThirst,
    Regen,

    //eatng
    NeedsInterval,
    FoodEatPerSecond,
    DrinkPerSecond,
    HungerDepletionRate,
    ThirstDepletionRate,

    //look around
    LookWhileEatingInterval,
    LookAngleMin,
    LookAngleMax,
    LeaveBurrowWaitTime,

    //detect
    DetectionDistance,
    DetectionAngle,

    //Movement
    BaseSpeed,
    RunSpeed,
    WanderDistanceMin,
    WanderDistanceMax,
    WanderInterval,
    
    //seggs
    Fertile,
    BaseOffspringCount,
    MaxAdditionalOffspring,
    MinPositiveGenesPrefered,
    MaxNegativeGenesPrefered,
    ReproduceCooldownDays,

    //Eep
    SleepHours,
}

[System.Serializable]
public class StatModifier
{
    public StatType statType;
    public float value;

    public StatModifier(StatType statType, float value)
    {
        this.statType = statType;
        this.value = value;
    }
}

public class Stats : MonoBehaviour
{
    [Header("About")]
    public Gender gender;
    public int agedDays = 3;

    [Header("Current Stats")]
    public float health = 20f;
    public float hunger = 100f;
    public float thirst = 100f;

    [Header("Stats")]
    public float maxHealth = 20f;
    public float maxHunger = 100f;
    public float maxThirst = 100f;
    public float regenAmount = 1f;
    public int reproduceDaysLeft = 3;
    public int isAdultDays = 3;

    [Header("Eat Settings")]
    public float needsInterval = 1f;
    public float foodEatPerSecond = 2f;
    public float drinkPerSecond = 4f;
    public float lookWhileEatingInterval = 5f;
    public float lookAngleMin = 30f;
    public float lookAngleMax = 90f;

    [Header("Hiding Settings")]
    public float waitBeforeLeavingBurrow = 10f;

    [Header("Detect Settings")]
    public float detectionDistance = 6f;
    public float detectionAngle = 90f;

    [Header("Move Settings")]
    public float baseSpeed = 1.5f;
    public float runSpeed = 3.5f;
    public float wanderDistanceMin = 2f;
    public float wanderDistanceMax = 5f;
    public float wanderInterval = 2f;

    [Header("Deplete Settings")]
    public float hungerDepletionRate = 0.15f;
    public float thirstDepletionRate = 0.2f;

    [Header("Sleep Settings")]
    public float additionalSleepHours = 0f;

    [Header("Seggs Settings")]
    public int fertile = 1; // 0 - false 1 - true
    public int baseOffSpringCount = 2;
    public int maxAdditionalOffSpring = 4;
    public int minPositiveGenesPrefered = 2;
    public int maxNegativeGenesPrefered = 2;
    public int reproduceCooldownDays = 3;

    public List<Genes> genes = new List<Genes>();

    public void SetStats()
    {
        gender = (Gender)Random.Range(0, System.Enum.GetValues(typeof(Gender)).Length);
        health = maxHealth;
        hunger = maxHunger / 2;
        thirst = maxThirst / 2;
        reproduceDaysLeft = reproduceCooldownDays;
    }

    public void AssignRandomPersonalities()
    {
        int GenesCount = Random.Range(4, 8);

        List<Genes> allPersonalities = GeneManager.Instance.GetAllGenes();

        for (int i = 0; i < GenesCount; i++)
        {
            Genes randomGenes = allPersonalities[Random.Range(0, allPersonalities.Count)];
            if (!genes.Exists(p => p.name == randomGenes.name))
            {
                genes.Add(randomGenes);
            }
        }
    }

    public void ApplyGenesToStats()
    {
        foreach (Genes gene in genes)
        {
            foreach (StatModifier modifier in gene.statModifiers)
            {
                switch (modifier.statType)
                {
                    case StatType.MaxHealth: maxHealth += modifier.value; break;
                    case StatType.MaxHunger: maxHunger += modifier.value; break;
                    case StatType.MaxThirst: maxThirst += modifier.value; break;
                    case StatType.Regen: regenAmount += modifier.value; break;

                    case StatType.NeedsInterval: needsInterval += modifier.value; break;
                    case StatType.FoodEatPerSecond: foodEatPerSecond += modifier.value; break;
                    case StatType.DrinkPerSecond: drinkPerSecond += modifier.value; break;
                    case StatType.HungerDepletionRate: hungerDepletionRate += modifier.value; break;
                    case StatType.ThirstDepletionRate: thirstDepletionRate += modifier.value; break;

                    case StatType.LookWhileEatingInterval: lookWhileEatingInterval += modifier.value; break;
                    case StatType.LookAngleMin: lookAngleMin += modifier.value; break;
                    case StatType.LookAngleMax: lookAngleMax += modifier.value; break;
                    case StatType.LeaveBurrowWaitTime: waitBeforeLeavingBurrow += modifier.value; break;
                    case StatType.DetectionDistance: detectionDistance += modifier.value; break;
                    case StatType.DetectionAngle: detectionAngle += modifier.value; break;

                    case StatType.BaseSpeed: baseSpeed += modifier.value; break;
                    case StatType.RunSpeed: runSpeed += modifier.value; break;
                    case StatType.WanderDistanceMin: wanderDistanceMin += modifier.value; break;
                    case StatType.WanderDistanceMax: wanderDistanceMax += modifier.value; break;
                    case StatType.WanderInterval: wanderInterval += modifier.value; break;
                    
                    case StatType.SleepHours: additionalSleepHours += modifier.value; break;

                    case StatType.Fertile:
                        fertile = Mathf.RoundToInt(modifier.value);
                        break;
                    case StatType.BaseOffspringCount: baseOffSpringCount += Mathf.RoundToInt(modifier.value); break;
                    case StatType.MaxAdditionalOffspring: maxAdditionalOffSpring += Mathf.RoundToInt(modifier.value); break;
                    case StatType.MinPositiveGenesPrefered: minPositiveGenesPrefered += Mathf.RoundToInt(modifier.value); break;
                    case StatType.MaxNegativeGenesPrefered: maxNegativeGenesPrefered += Mathf.RoundToInt(modifier.value); break;
                    case StatType.ReproduceCooldownDays: reproduceCooldownDays += Mathf.RoundToInt(modifier.value); break;
                }
            }
        }

        wanderInterval = Mathf.Max(wanderInterval, 1f);
        wanderDistanceMax = Mathf.Max(wanderDistanceMax, 1);

        baseSpeed = Mathf.Max(baseSpeed, 0.75f);
        runSpeed = Mathf.Max(runSpeed, 1);

        maxHealth = Mathf.Max(maxHealth, 5);
        thirstDepletionRate = Mathf.Max(thirstDepletionRate, 0.025f);
        hungerDepletionRate = Mathf.Max(hungerDepletionRate, 0.025f);

        foodEatPerSecond = Mathf.Max(foodEatPerSecond, 0.5f);
        drinkPerSecond = Mathf.Max(drinkPerSecond, 0.5f);
        needsInterval = Mathf.Max(needsInterval, 0.25f);

        lookWhileEatingInterval = Mathf.Max(lookWhileEatingInterval, 0.5f);
        waitBeforeLeavingBurrow = Mathf.Max(waitBeforeLeavingBurrow, 0.5f);

        detectionDistance = Mathf.Max(detectionDistance, 3f);

        regenAmount = Mathf.Max(regenAmount, 0.2f);

        baseOffSpringCount = Mathf.Max(baseOffSpringCount, 1);
        minPositiveGenesPrefered = Mathf.Max(minPositiveGenesPrefered, 0);
        maxNegativeGenesPrefered = Mathf.Max(maxNegativeGenesPrefered, 0);
    }
}

public enum Gender
{
    Male,
    Female,
}
