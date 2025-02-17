using System.Collections.Generic;
using UnityEngine;

public class Stats : MonoBehaviour
{
    [Header("Gender")]
    public Gender gender;

    [Header("Current Stats")]
    public float health = 20f;
    public float hunger = 100f;
    public float thirst = 100f;

    [Header("Max Stats")]
    public float maxHealth = 20f;
    public float maxHunger = 100f;
    public float maxThirst = 100f;

    [Header("Deplete Settings")]
    public float hungerDepletionRate = 0.1f;
    public float thirstDepletionRate = 0.15f;

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
        List<Genes> allPersonalities = Genes.GetAllPersonalities();

        for (int i = 0; i < GenesCount; i++)
        {
            Genes randomGenes = allPersonalities[Random.Range(0, allPersonalities.Count)];
            if (!genes.Exists(p => p.name == randomGenes.name))
            {
                genes.Add(randomGenes);
            }
        }
    }
}

public enum Gender
{
    Male,
    Female,
}

public class Genes
{
    public string name;
    public string description;
    public int positive; //0 - negative, 1- neutral 2- positive

    public Genes(string name, string description, int positive)
    {
        this.name = name;
        this.description = description;
        this.positive = positive;
    }

    public static List<Genes> GetAllPersonalities()
    {
        return new List<Genes>
        {
            new Genes("Lazy", "Moves at a lower max distance when wandering.", 0),
            new Genes("Restful", "Increases wander interval.", 0),
            new Genes("Restless", "Decreases wander interval.", 2),
            new Genes("Adventurer", "Moves at a higher max distance when wandering.", 2),

            new Genes("Energetic", "Moves faster but loses more hunger.", 1),
            new Genes("Sluggish", "Moves slower but loses less hunger.", 1),

            new Genes("Thirsty", "Loses more thirst.", 0),
            new Genes("Retention", "Loses less thirst.", 2),
            new Genes("IronStomach", "Loses less hunger.", 2),
            new Genes("Hunger", "Loses more hunger.", 0),

            new Genes("Gluttonous", "Moves slower, eats faster, and has more hunger capacity.", 1),
            new Genes("Unathletic", "Moves slower.", 0),
            new Genes("Athletic", "Moves faster.", 2),
            new Genes("Runner", "Runs faster.", 2),
            new Genes("Wanderer", "Wanders faster.", 2),
            new Genes("Snail", "Significantly slower.", 0),

            new Genes("FastEater", "Eats faster.", 2),
            new Genes("SlowEater", "Eats slower.", 0),
            new Genes("FastDrinker", "Drinks faster.", 2),
            new Genes("SlowDrinker", "Drinks slower.", 0),

            new Genes("Fertility", "Base offspring count increased by 2.", 2), //not done
            new Genes("Non-Fertile", "Can't give birth to offspring.", 0), //not done
            new Genes("Reproductive", "Base offspring count increased by 4.", 2), //not done
            new Genes("Less Fertile", "Base offspring count decreased by 2.", 0), //not done

            new Genes("Healthy", "Has more health capacity.", 2),
            new Genes("Weak", "Has less health capacity.", 0),

            new Genes("Alert", "Detection radius and range increased.", 2),
            new Genes("FarVision", "Detection range greatly increased.", 2),
            new Genes("Blurred", "Detection range and radius decreased.", 0),
            new Genes("Shortsight", "Detection range significantly reduced.", 0),

            new Genes("Paranoid", "Looks around more often when eating.", 2),
            new Genes("Careful", "Stays in burrows longer after hiding.", 2),
            new Genes("Quiet", "Less detectable.", 2), //not done
            new Genes("Loud", "Less detectable.", 0), //not done

            new Genes("Sleeper", "Sleeps 0.5-1.5 hours more than normal.", 0),
            new Genes("Active", "Sleeps 0.5-1.5 hours less than normal.", 2),

            new Genes("HighMetabolism", "Needs decrease faster", 0),
            new Genes("LowMetabolism", "Needs decrease slower", 2),
        };
    }

    
    public static bool Has(string name)
    {
        return GetAllPersonalities().Exists(p => p.name == name);
    }
}
