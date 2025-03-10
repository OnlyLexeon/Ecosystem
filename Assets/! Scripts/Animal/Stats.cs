using System.Collections.Generic;
using System.Linq;
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
    DominanceOverChildCount,
    BaseOffspringCount,
    MaxAdditionalOffspring,
    MinPositiveGenesPrefered,
    MaxNegativeGenesPrefered,
    ReproduceCooldownDays,

    //Eep
    SleepHours,

    //Color
    FurTypeDominance,

    //Mutation
    GeneMutationChance,

    //Aging
    DaysTillDeath,
    MinTimeTillDeath,
    MaxTimeTillDeath,
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
    public int generation = 0;

    [Header("Current Stats")]
    public float health = 20f;
    public float hunger = 100f;
    public float thirst = 100f;

    [Header("Stats")]
    public float maxHealth = 20f;
    public float maxHunger = 100f;
    public float maxThirst = 100f;
    public float regenAmount = 1f;

    [Header("Aging Settings")]
    [Tooltip("Days taken to fully turn into an Adult.")] public int adultDays = 3;
    [Tooltip("Days taken till Animal Starts Dying of Old Age.")] public int deathDays = 15;
    [Tooltip("Min Time taken till Animal Dies of Old Age AFTER reaching deathDays.")] public int minDeathTime = 470; //8am
    [Tooltip("Max Time taken till Animal Dies of Old Age AFTER reaching deathDays.")] public int maxDeathTime = 960; //4pm
    [Tooltip("Exact Time taken till Animal Dies of Old Age AFTER reaching deathDays.")] public int deathTime = 0;


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
    public float detectionDistance = 7f;
    public float detectionAngle = 100f;

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
    public int reproduceDaysLeft = 2; // a dynamic value
    public int fertile = 1; // 0 - false 1 - true
    public int baseOffSpringCount = 2;
    public int maxAdditionalOffSpring = 6;
    public int minPositiveGenesPrefered = 2;
    public int maxNegativeGenesPrefered = 3;
    public int reproduceCooldownDays = 2; // fixed

    [Header("Birth Settings")]
    public float furDominance = 100f;
    public float seedDominance = 100f;
    public float geneMutationChance = 5f;

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

    public Genes GetARandomGene()
    {
        List<Genes> allPersonalities = GeneManager.Instance.GetAllGenes();

        return allPersonalities[Random.Range(0, allPersonalities.Count)];
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

                    case StatType.Fertile: fertile = Mathf.RoundToInt(modifier.value); break; // 1 or 0
                    case StatType.BaseOffspringCount: baseOffSpringCount += Mathf.RoundToInt(modifier.value); break;
                    case StatType.MaxAdditionalOffspring: maxAdditionalOffSpring += Mathf.RoundToInt(modifier.value); break;
                    case StatType.MinPositiveGenesPrefered: minPositiveGenesPrefered += Mathf.RoundToInt(modifier.value); break;
                    case StatType.MaxNegativeGenesPrefered: maxNegativeGenesPrefered += Mathf.RoundToInt(modifier.value); break;
                    case StatType.ReproduceCooldownDays: reproduceCooldownDays += Mathf.RoundToInt(modifier.value); break;

                    case StatType.FurTypeDominance: furDominance += modifier.value; break;
                    case StatType.DominanceOverChildCount: seedDominance += modifier.value; break;

                    case StatType.GeneMutationChance: geneMutationChance += modifier.value; break;

                    case StatType.DaysTillDeath: deathDays += Mathf.RoundToInt(modifier.value); break;
                    case StatType.MaxTimeTillDeath: maxDeathTime += Mathf.RoundToInt(modifier.value); break;
                    case StatType.MinTimeTillDeath: minDeathTime += Mathf.RoundToInt(modifier.value); break;
                }
            }
        }

        maxHealth = Mathf.Max(maxHealth, 1);
        maxHunger = Mathf.Max(maxHunger, 1);
        maxThirst = Mathf.Max(maxThirst, 1);
        regenAmount = Mathf.Max(regenAmount, 0.1f);

        needsInterval = Mathf.Max(needsInterval, 0.1f);
        foodEatPerSecond = Mathf.Max(foodEatPerSecond, 0.1f);
        drinkPerSecond = Mathf.Max(drinkPerSecond, 0.1f);
        thirstDepletionRate = Mathf.Max(thirstDepletionRate, 0.01f);
        hungerDepletionRate = Mathf.Max(hungerDepletionRate, 0.01f);

        lookWhileEatingInterval = Mathf.Max(lookWhileEatingInterval, 0.5f);
        waitBeforeLeavingBurrow = Mathf.Max(waitBeforeLeavingBurrow, 0.5f);
        detectionDistance = Mathf.Max(detectionDistance, 3f);

        baseSpeed = Mathf.Max(baseSpeed, 0.5f);
        runSpeed = Mathf.Max(runSpeed, 1f);
        wanderInterval = Mathf.Max(wanderInterval, 0.1f);
        wanderDistanceMin = Mathf.Max(wanderDistanceMin, 0.1f);
        wanderDistanceMax = Mathf.Max(wanderDistanceMax, 0.2f);

        baseOffSpringCount = Mathf.Max(baseOffSpringCount, 1);
        minPositiveGenesPrefered = Mathf.Max(minPositiveGenesPrefered, 0);
        maxNegativeGenesPrefered = Mathf.Max(maxNegativeGenesPrefered, 0);
        reproduceCooldownDays = Mathf.Max(reproduceCooldownDays, 0);

        deathDays = Mathf.Max(deathDays, adultDays + 1);
        maxDeathTime = Mathf.Min(maxDeathTime, 1200); // 8pm
        minDeathTime = Mathf.Max(minDeathTime, 0); //0 - Start of day
    }

    public void UpdateWorldStatsGenes(AnimalType animalType)
    {
        foreach (Genes gene in genes)
        {
            int positiveGenes = GetPositiveGenesCount();
            int negativeGenes = GetNegativeGenesCount();
            int neutralGenes = GetNeutralGenesCount();

            WorldStats.Instance.UpdateGeneStats(animalType, positiveGenes, negativeGenes, neutralGenes);
        }
    }

    public int GetPositiveGenesCount()
    {
        return genes.Count(g => g.positivity == Positivity.Positive || g.positivity == Positivity.ExtremelyPositive);
    }

    public int GetNegativeGenesCount()
    {
        return genes.Count(g => g.positivity == Positivity.Negative || g.positivity == Positivity.ExtremelyNegative);
    }

    public int GetNeutralGenesCount()
    {
        return genes.Count(g => g.positivity == Positivity.Neutral);
    }

}

public enum Gender
{
    Male,
    Female,
}
