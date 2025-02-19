using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

public enum StatType
{
    MaxHealth,
    MaxHunger,
    MaxThirst,
    Regen,

    NeedsInterval,
    FoodEatPerSecond,
    DrinkPerSecond,

    LookWhileEatingInterval,
    LookAngleMin,
    LookAngleMax,
    WaitBeforeLeavingBurrow,

    DetectionDistance,
    DetectionAngle,

    BaseSpeed,
    RunSpeed,

    WanderDistanceMin,
    WanderDistanceMax,
    WanderInterval,

    HungerDepletionRate,
    ThirstDepletionRate,

    BaseOffspringCount,
    MinAdditionalOffspring,
    MaxAdditionalOffspring,

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
    [Header("Gender")]
    public Gender gender;

    [Header("Current Stats")]
    public float health = 20f;
    public float hunger = 100f;
    public float thirst = 100f;

    [Header("Stats")]
    public float maxHealth = 20f;
    public float maxHunger = 100f;
    public float maxThirst = 100f;
    public float regenAmount = 1f;

    [Header("Eat Settings")]
    public float needsInterval = 1f;
    public float foodEatPerSecond = 2f;
    public float drinkPerSecond = 4f;
    public float lookWhileEatingInterval = 5f;
    public float lookAngleMin = 30f;
    public float lookAngleMax = 90f;

    [Header("Hiding Settings")]
    public float waitBeforeLeavingBurrow = 5f;

    [Header("Detect Settings")]
    public float detectionDistance = 6f;
    public float detectionAngle = 90f;

    [Header("Move Settings")]
    public float baseSpeed = 1.5f;
    public float runSpeed = 3f;
    public float wanderDistanceMin = 2f;
    public float wanderDistanceMax = 5f;
    public float wanderInterval = 2f;

    [Header("Deplete Settings")]
    public float hungerDepletionRate = 0.1f;
    public float thirstDepletionRate = 0.15f;

    [Header("Sleep Settings")]
    public float additionalSleepHours = 0f;

    [Header("Seggs Settings")]
    public int baseOffSpringCount = 2;
    public int minAdditionalOffSpring = 0;
    public int maxAdditionalOffSpring = 4;

    public List<Genes> genes = new List<Genes>();

    public void SetStats()
    {
        gender = (Gender)Random.Range(0, System.Enum.GetValues(typeof(Gender)).Length);
        health = maxHealth;
        hunger = maxHunger / 2;
        thirst = maxThirst / 2;
    }

    public void AssignRandomPersonalities()
    {
        int GenesCount = Random.Range(3, 7);
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
                    case StatType.NeedsInterval: needsInterval += modifier.value; break;
                    case StatType.FoodEatPerSecond: foodEatPerSecond += modifier.value; break;
                    case StatType.DrinkPerSecond: drinkPerSecond += modifier.value; break;
                    case StatType.LookWhileEatingInterval: lookWhileEatingInterval += modifier.value; break;
                    case StatType.LookAngleMin: lookAngleMin += modifier.value; break;
                    case StatType.LookAngleMax: lookAngleMax += modifier.value; break;
                    case StatType.WaitBeforeLeavingBurrow: waitBeforeLeavingBurrow += modifier.value; break;
                    case StatType.DetectionDistance: detectionDistance += modifier.value; break;
                    case StatType.DetectionAngle: detectionAngle += modifier.value; break;
                    case StatType.BaseSpeed: baseSpeed += modifier.value; break;
                    case StatType.RunSpeed: runSpeed += modifier.value; break;
                    case StatType.WanderDistanceMin: wanderDistanceMin += modifier.value; break;
                    case StatType.WanderDistanceMax: wanderDistanceMax += modifier.value; break;
                    case StatType.WanderInterval: wanderInterval += modifier.value; break;
                    case StatType.HungerDepletionRate: hungerDepletionRate += modifier.value; break;
                    case StatType.ThirstDepletionRate: thirstDepletionRate += modifier.value; break;
                    case StatType.Regen: regenAmount += modifier.value; break;
                    case StatType.SleepHours: additionalSleepHours += modifier.value; break;
                    case StatType.BaseOffspringCount: baseOffSpringCount += Mathf.RoundToInt(modifier.value); break;
                    case StatType.MinAdditionalOffspring: minAdditionalOffSpring += Mathf.RoundToInt(modifier.value); break;
                    case StatType.MaxAdditionalOffspring: maxAdditionalOffSpring += Mathf.RoundToInt(modifier.value); break;
                }
            }
        }

        wanderInterval = Mathf.Max(wanderInterval, 2f);
        wanderDistanceMax = Mathf.Max(wanderDistanceMax, 1);

        baseSpeed = Mathf.Max(baseSpeed, 1);
        runSpeed = Mathf.Max(runSpeed, 1);

        maxHealth = Mathf.Max(maxHealth, 5);
        thirstDepletionRate = Mathf.Max(thirstDepletionRate, 0.05f);
        hungerDepletionRate = Mathf.Max(hungerDepletionRate, 0.05f);

        foodEatPerSecond = Mathf.Max(foodEatPerSecond, 1);
        drinkPerSecond = Mathf.Max(drinkPerSecond, 1);
        needsInterval = Mathf.Max(needsInterval, 0.5f);

        lookWhileEatingInterval = Mathf.Max(lookWhileEatingInterval, 1);
        waitBeforeLeavingBurrow = Mathf.Max(waitBeforeLeavingBurrow, 1);

        detectionDistance = Mathf.Max(detectionDistance, 4f);

        regenAmount = Mathf.Max(regenAmount, 0.25f);
    }
}

public enum Gender
{
    Male,
    Female,
}
