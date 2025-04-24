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

    //burrow
    NewHomeDistance,
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
    [Header("Change Everything For Your Animal!\nActs as Default Settings (Modified by Genes later)")]

    [Header("About")]
    [Tooltip("The gender of this animal.")] public Gender gender;
    [Range(0, 10)] [Tooltip("Spawn this animal at this age.\n(Make sure it is between 0 to deathDays value)")] public int agedDays = 0;
    [Tooltip("Current generation of this animal.\n(Recommended: 0)")] public int generation = 0;

    [Header("Aging Settings")]
    [Range(0, 10)] [Tooltip("Days taken to fully turn into an Adult." +
        "\n(Recommended: 3)")] 
    public int adultDays = 3;
    [Range(10, 20)] [Tooltip("Days taken till Animal Starts Dying of Old Age." +
        "\n(Recommended: More than adultDays)\n(Default: 15))")] 
    public int deathDays = 15;
    [Range(0, 1440)] [Tooltip("Min Time taken till Animal Dies of Old Age AFTER reaching deathDays." +
        "\n(60 seconds in-game is 1 hour)\n(Recommended: 8am (460)).")] 
    public int minDeathTime = 460; //8am
    [Range(0, 1440)] [Tooltip("Max Time taken till Animal Dies of Old Age AFTER reaching deathDays." +
        "\n(60 seconds in-game is 1 hour)\n(Recommended: 4pm (960)).")] 
    public int maxDeathTime = 960; //4pm

    [Header("Stats")]
    [Range(10, 100)]
    [Tooltip("Maximum health of this animal." +
        "\nEffects how much this animal can be attacked, or start losing health from old age decay, thirst, and hunger." +
        "\n(Recommended: 10-20)")] public float maxHealth = 20f;
    [Range(50, 200)]
    [Tooltip("Maximum hunger storage of this animal." +
        "\nEffects how much food this animal can store. They will stay full longer the larger this value is." +
        "\n(Recommended: 100)")] public float maxHunger = 100f;
    [Range(50, 200)]
    [Tooltip("Maximum thirst storage of this animal." +
        "\nEffects how much food this animal can store. They will stay full longer the larger this value is." +
        "\n(Recommended: 100)")] public float maxThirst = 100f;
    [Range(0, 10)]
    [Tooltip("Regeneration per second if this animal is hurt, and thirst + hunger bars are replenished." +
        "\nSet to 0 if you don't want any regeneration." +
        "\n(Recommended: 1-5)")] public float regenAmount = 1f;

    [Header("Eat/Drink Settings")]
    [Range(0.1f, 5)] [Tooltip("Every needsInterval of seconds needs such as Hunger and Thirst are deducted by hungerDepletionRate & thirstDepletionRate values." +
        "\n(Recommended: 1)")] public float needsInterval = 1f;
    [Range(1, 6)] [Tooltip("The amount of food this animal will consume per second at a food source." +
        "\n(Default: 2)")]
    public float foodEatPerSecond = 2f;
    [Range(1, 6)]
    [Tooltip("The amount of water this animal will drink per second at a water source." +
         "\n(Default: 4)")]
    public float drinkPerSecond = 4f;

    [Header("Depletion Settings")]
    [Range(0.1f, 5)]
    [Tooltip("The amount of hunger this animal will drain per second." +
         "\n(Default: 0.15)")]
    public float hungerDepletionRate = 0.15f;
    [Range(0.1f, 5)]
    [Tooltip("The amount of thirst this animal will lose per second." +
         "\n(Default: 0.2)")]
    public float thirstDepletionRate = 0.2f;

    [Header("Awareness while Eating")]
    [Range(1f, 10f)]
    [Tooltip("Time between this animal to change direction of looking when eating/drinking at a source." +
        "\nThis behavior mimics animals in real life as they are aware of their surroundings when replenishing hunger or thirst." +
         "\n(Default: 5)")]
    public float lookWhileEatingInterval = 5f;
    [Range(0, 180)]
    [Tooltip("Min Angle this animal will rotate their looking direction." +
        "\n(Recommended: 10 < lookAngleMin < 90)")]
    public float lookAngleMin = 30f;
    [Range(90, 360)]
    [Tooltip("Max Angle this animal will rotate their looking direction." +
        "\n(Recommended: 90 < lookAngleMin < 180)")]
    public float lookAngleMax = 90f;

    [Header("Attack Settings (Hunters)")]
    [Range(0.1f, 4f)]
    [Tooltip("'Bite' range of this animal when hunting/chasing prey." +
        "\n(Does not affect distance it can reach food/water sources)" +
        "\n(Recommended: 2)" +
        "\n(Close: 1, Far: 4)")]
    public float attackRange = 2f;
    [Range(5f, 25f)]
    [Tooltip("Damage this predator will deal to prey." +
        "\n(5 is very less, 20 is a lot)" +
        "\nIf your prey has around 10 health, Recommended: 10 damage")]
    public float attackDamage = 10f;

    [Header("Burrow Settings")]
    [Range(10f, 100f)]
    [Tooltip("If this animal exceeds this distance from its original home, it will make a new one." +
        "\n(10 is very low, animal will commonly make new houses)" +
        "\n(100 is almost never if they don't wander fast)" +
        "\n(Recommended higher for prey constantly chased away from home.)" +
        "\n(Default: 32)")]
    public float newBurrowDistance = 32f;
    [Range(5f, 30f)] [Tooltip("Time in seconds the animal will wait inside its home before reappearing if chased into it by a predator.")] 
    public float waitBeforeLeavingBurrow = 10f;

    [Header("Detect Settings")]
    [Range(4f, 20f)] [Tooltip("Visual range of this animal." +
        "\n(5 is shortsighted)" +
        "\n(20 is extremely far)" +
        "\n(Recommended: <10 for prey, >10 for predators)")]
    public float detectionDistance = 10f;
    [Range(90f, 370f)]
    [Tooltip("Visual Field of Vision of this animal." +
        "\n(90 is extremely narrow - Predator eyes)" +
        "\n(370 is nearly full awareness - Prey eyes, specifically a horse)" +
        "\n(Recommended: >100 for prey, between 90-100 for predators)" +
        "\n(Default: 100)")]
    public float detectionAngle = 100f;

    [Header("Move/Wander Settings")]
    [Range(0.5f, 5f)]
    [Tooltip("Speed of animal when wandering/walking." +
        "\n(0.5 - Turtle, 1.5-2 four-legged, >2 sprint walkers)" +
        "\n(Default: 1.5)")]
    public float baseSpeed = 1.5f;
    [Range(0.5f, 10f)]
    [Tooltip("Speed of animal when running away or hunting prey." +
        "\n(Suggested: x2 or x3 the amounts of baseSpeed)" +
        "\n(Default: 3.5)")]
    public float runSpeed = 3.5f;
    [Range(1f, 10f)]
    [Tooltip("The minimal distance this animal will choose to walk/wander to." +
        "\n(Rabbit hop should be 1-2, more thoughtful animals trek further such as >5)")]
    public float wanderDistanceMin = 2f;
    [Range(1f, 10f)]
    [Tooltip("The maximum distance this animal will choose to walk/wander to." +
        "\n(Suggested: x2 or x3 the amount of wanderDistanceMin)")]
    public float wanderDistanceMax = 5f;
    [Range(0.5f, 5f)]
    [Tooltip("The time in seconds this animal will rest before wandering to another position." +
        "\n(Recommended: 1-2)")] 
    public float wanderInterval = 2f;
    
    [Header("Sleep Settings")]
    [Range(0, 8f)]
    [Tooltip("Extra hours this animal will rest for." +
        "\n(Recommended: 1-2 if a predator, or 0 for prey)")]
    public float additionalSleepHours = 0f;

    [Header("Seggs Settings")]
    [Range(0, 1f)]
    [Tooltip("1 - True, can breed, 0 - False, can't breed" +
        "\n(Default: 1)")]
    public int fertile = 1; // 0 - false 1 - true
    [Range(1, 3f)]
    [Tooltip("Amount of offspring confirmed to be born after breeding." +
        "\n(Default: 2)")]
    public int baseOffSpringCount = 2;
    [Range(0, 10f)]
    [Tooltip("Amount of additional offspring extra randomly to be born after breeding." +
        "\n(Default: 2)")]
    public int maxAdditionalOffSpring = 6;
    [Range(0, 4f)]
    [Tooltip("The least positive genes this animal wants its mate to have." +
        "\n(Default: 2)")]
    public int minPositiveGenesPrefered = 2;
    [Range(0, 4f)]
    [Tooltip("The most negative genes this animal wants its mate to have." +
        "\n(Default: 3)")]
    public int maxNegativeGenesPrefered = 3;
    [Range(0, 7f)]
    [Tooltip("Amount of days this animal will wait before looking for a mate again." +
        "\n(0 = No cooldown, can happen the next day)" +
        "\n(Default: 2)")]
    public int reproduceCooldownDays = 2;

    [Header("Birth Settings")]
    [Range(0, 100f)]
    [Tooltip("The control this animal has against determining its offspring's fur color to be of its own." +
        "\nDoesn't really matter as the male and female variants of this animal will have this same value." +
        "\n(Default: 100)")]
    public float furDominance = 100f;
    [Range(0, 100f)]
    [Tooltip("The chance of this animal determining offspring count." +
        "\nAs both parents may have different baseOffspringCount or maxAdditionalOffSpring," +
        "\nThis value is used to determine which parent is more likely to be chosen to be used to determine offspring count." +
        "\n(Default: 100)")]
    public float seedDominance = 100f;
    [Range(0, 100f)]
    [Tooltip("Chance of offspring's genetics to mutate per gene." +
        "\n(Default: 5%)")]
    public float geneMutationChance = 5f;

    [Header("Genes")]
    [Tooltip("Current genes of this animal." +
       "\nAdd genes here if you want this animal to ALWAYS have this gene.")]
    public List<Genes> genes = new List<Genes>();

    [Header("Debug Zone (Changing these do nothing!)")]
    public float health = 20f;
    public float hunger = 100f;
    public float thirst = 100f;
    [Tooltip("Exact Time taken till Animal Dies of Old Age AFTER reaching deathDays.")] public int deathTime = 0;
    [Tooltip("Days left before this animal tries to reproduce again.")] public int reproduceDaysLeft;

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

                    case StatType.NewHomeDistance: newBurrowDistance += modifier.value; break;
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

        newBurrowDistance = Mathf.Max(newBurrowDistance, 24f);

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
        int positiveGenes = GetPositiveGenesCount();
        int negativeGenes = GetNegativeGenesCount();
        int neutralGenes = GetNeutralGenesCount();

        WorldStats.Instance.UpdateGeneStats(animalType, positiveGenes, negativeGenes, neutralGenes);
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
