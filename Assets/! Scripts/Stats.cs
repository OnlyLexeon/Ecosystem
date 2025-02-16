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

    public List<Personality> personalities = new List<Personality>();

    public void SetStats()
    {
        gender = (Gender)Random.Range(0, System.Enum.GetValues(typeof(Gender)).Length);
        health = maxHealth;
        hunger = maxHunger / 2;
        thirst = maxThirst / 2;
    }

    public void AssignRandomPersonalities()
    {
        int personalityCount = Random.Range(3, 7);
        List<Personality> allPersonalities = Personality.GetAllPersonalities();

        for (int i = 0; i < personalityCount; i++)
        {
            Personality randomPersonality = allPersonalities[Random.Range(0, allPersonalities.Count)];
            if (!personalities.Exists(p => p.name == randomPersonality.name))
            {
                personalities.Add(randomPersonality);
            }
        }
    }
}

public enum Gender
{
    Male,
    Female,
}

public class Personality
{
    public string name;
    public string description;
    public int positive; //0 - negative, 1- neutral 2- positive

    public Personality(string name, string description, int positive)
    {
        this.name = name;
        this.description = description;
        this.positive = positive;
    }

    public static List<Personality> GetAllPersonalities()
    {
        return new List<Personality>
        {
            new Personality("Lazy", "Moves at a lower max distance when wandering.", 0),
            new Personality("Restful", "Increases wander interval.", 0),
            new Personality("Restless", "Decreases wander interval.", 2),
            new Personality("Adventurer", "Moves at a higher max distance when wandering.", 2),

            new Personality("Energetic", "Moves faster but loses more hunger.", 1),
            new Personality("Sluggish", "Moves slower but loses less hunger.", 1),

            new Personality("Thirsty", "Loses more thirst.", 0),
            new Personality("Retention", "Loses less thirst.", 2),
            new Personality("IronStomach", "Loses less hunger.", 2),
            new Personality("Hunger", "Loses more hunger.", 0),

            new Personality("Gluttonous", "Moves slower, eats faster, and has more hunger capacity.", 1),
            new Personality("Unathletic", "Moves slower.", 0),
            new Personality("Athletic", "Moves faster.", 2),
            new Personality("Runner", "Runs faster.", 2),
            new Personality("Wanderer", "Wanders faster.", 2),
            new Personality("Snail", "Significantly slower.", 0),

            new Personality("FastEater", "Eats faster.", 2),
            new Personality("SlowEater", "Eats slower.", 0),
            new Personality("FastDrinker", "Drinks faster.", 2),
            new Personality("SlowDrinker", "Drinks slower.", 0),

            new Personality("Fertility", "Base offspring count increased by 2.", 2), //not done
            new Personality("Non-Fertile", "Can't give birth to offspring.", 0), //not done
            new Personality("Reproductive", "Base offspring count increased by 4.", 2), //not done
            new Personality("Less Fertile", "Base offspring count decreased by 2.", 0), //not done

            new Personality("Healthy", "Has more health capacity.", 2),
            new Personality("Weak", "Has less health capacity.", 0),

            new Personality("Alert", "Detection radius and range increased.", 2),
            new Personality("FarVision", "Detection range greatly increased.", 2),
            new Personality("Blurred", "Detection range and radius decreased.", 0),
            new Personality("Shortsight", "Detection range significantly reduced.", 0),

            new Personality("Paranoid", "Looks around more often when eating.", 2),
            new Personality("Careful", "Stays in burrows longer after hiding.", 2),
            new Personality("Quiet", "Less detectable.", 2), //not done
            new Personality("Loud", "Less detectable.", 0), //not done

            new Personality("Sleeper", "Sleeps 0.5-1.5 hours more than normal.", 0),
            new Personality("Active", "Sleeps 0.5-1.5 hours less than normal.", 2),

            new Personality("HighMetabolism", "Needs decrease faster", 0),
            new Personality("LowMetabolism", "Needs decrease slower", 2),
        };
    }

    
    public static bool Has(string name)
    {
        return GetAllPersonalities().Exists(p => p.name == name);
    }
}
