using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEditor.Overlays;

public class GeneManager : MonoBehaviour
{
    private static string saveFilePath;
    public List<Genes> defaultGenes = new List<Genes>(); // Default genes list
    public List<Genes> customGenes = new List<Genes>();  // Loaded custom genes

    public static GeneManager Instance;

    private void Awake()
    {
        Instance = this;

        saveFilePath = Path.Combine(Application.persistentDataPath, "genes.json");

        LoadDefaultGenes();
        LoadCustomGenes();
    }

    private void LoadDefaultGenes()
    {
        //Wandering
        defaultGenes.Add(new Genes("Lazy", "Moves at a lower max distance when wandering.", -1,
                new List<StatModifier> {
                    new StatModifier(StatType.WanderDistanceMin, -1.5f)
                }));
        defaultGenes.Add(new Genes("Adventurer", "Moves at a higher max distance when wandering.", 1,
                new List<StatModifier> {
                    new StatModifier(StatType.WanderDistanceMax, 1.5f)
                }));
        defaultGenes.Add(new Genes("Restful", "Increases wander interval.", 1,
                new List<StatModifier> {
                    new StatModifier(StatType.WanderInterval, 2f)
                }));
        defaultGenes.Add(new Genes("Restless", "Decreases wander interval.", 1,
                new List<StatModifier> {
                    new StatModifier(StatType.WanderInterval, -2f)
                }));

        //Depletion Rate
        defaultGenes.Add(new Genes("Energetic", "Moves faster but loses more hunger.", 0,
                new List<StatModifier> {
                    new StatModifier(StatType.HungerDepletionRate, 0.05f),
                    new StatModifier(StatType.BaseSpeed, 0.5f),
                    new StatModifier(StatType.RunSpeed, 0.5f)
                }));
        defaultGenes.Add(new Genes("Sluggish", "Moves slower but loses less hunger.", 0,
                new List<StatModifier> {
                    new StatModifier(StatType.HungerDepletionRate, -0.05f),
                    new StatModifier(StatType.BaseSpeed, -0.5f),
                    new StatModifier(StatType.RunSpeed, -0.5f)
                }));
        defaultGenes.Add(new Genes("Thirsty", "Loses more thirst.", -1,
                new List<StatModifier> {
                    new StatModifier(StatType.ThirstDepletionRate, 0.05f),
                }));

        defaultGenes.Add(new Genes("Retention", "Loses less thirst.", 1,
                new List<StatModifier> {
                    new StatModifier(StatType.ThirstDepletionRate, -0.05f),
                }));

        defaultGenes.Add(new Genes("IronStomach", "Loses less hunger.", 1,
                new List<StatModifier> {
                    new StatModifier(StatType.HungerDepletionRate, -0.05f),
                }));

        defaultGenes.Add(new Genes("Hunger", "Loses more hunger.", -1,
                new List<StatModifier> {
                    new StatModifier(StatType.HungerDepletionRate, 0.05f),
                }));

        //Hunger
        defaultGenes.Add(new Genes("Gluttonous", "Moves slower but eats faster and has more hunger capacity.", 0,
                new List<StatModifier> {
                    new StatModifier(StatType.FoodEatPerSecond, 2f),
                    new StatModifier(StatType.MaxHunger, 15f),
                    new StatModifier(StatType.BaseSpeed, -0.5f),
                    new StatModifier(StatType.RunSpeed, -0.5f)
                }));
        defaultGenes.Add(new Genes("Storager", "More hunger capacity.", 1,
                new List<StatModifier> {
                    new StatModifier(StatType.MaxHunger, 20f),
                }));
        defaultGenes.Add(new Genes("Starver", "Less hunger capacity.", -1,
                new List<StatModifier> {
                    new StatModifier(StatType.MaxHunger, -20f),
                }));

        //Movement
        defaultGenes.Add(new Genes("Unathletic", "Moves slower.", -1,
                new List<StatModifier> {
                    new StatModifier(StatType.BaseSpeed, -0.4f),
                    new StatModifier(StatType.RunSpeed, -0.4f),
                }));
        defaultGenes.Add(new Genes("Athletic", "Moves faster.", 1,
                new List<StatModifier> {
                    new StatModifier(StatType.BaseSpeed, 0.4f),
                    new StatModifier(StatType.RunSpeed, 0.4f),
                }));
        defaultGenes.Add(new Genes("Runner", "Runs faster.", 1,
                new List<StatModifier> {
                    new StatModifier(StatType.RunSpeed, 0.4f),
                }));
        defaultGenes.Add(new Genes("Wanderer", "Wanders faster.", 1,
                new List<StatModifier> {
                    new StatModifier(StatType.BaseSpeed, 0.4f),
                }));
        defaultGenes.Add(new Genes("Snail", "Significantly slower.", -2,
                new List<StatModifier> {
                    new StatModifier(StatType.BaseSpeed, -0.6f),
                    new StatModifier(StatType.RunSpeed, -0.6f)
                }));
        defaultGenes.Add(new Genes("Hyper", "Significantly faster.", 2,
                new List<StatModifier> {
                    new StatModifier(StatType.BaseSpeed, 0.6f),
                    new StatModifier(StatType.RunSpeed, 0.6f)
                }));

        //Eat/Drink
        defaultGenes.Add(new Genes("FastEater", "Eats faster.", 1,
                new List<StatModifier> {
                    new StatModifier(StatType.FoodEatPerSecond, 1.5f)
                }));
        defaultGenes.Add(new Genes("SlowEater", "Eats slower.", -1,
                new List<StatModifier> {
                    new StatModifier(StatType.FoodEatPerSecond, -1.5f)
                }));
        defaultGenes.Add(new Genes("FastDrinker", "Drinks faster.", 1,
                new List<StatModifier> {
                    new StatModifier(StatType.DrinkPerSecond, 2f)
                }));
        defaultGenes.Add(new Genes("SlowDrinker", "Drinks slower.", -1,
                new List<StatModifier> {
                    new StatModifier(StatType.DrinkPerSecond, -2f)
                }));

        //Seggs
        //new Genes("Fertility", "Base offspring count increased by 2.", 2), //not done
        //new Genes("Non-Fertile", "Can't give birth to offspring.", 0), //not done
        //new Genes("Reproductive", "Base offspring count increased by 4.", 2), //not done
        //new Genes("Less Fertile", "Base offspring count decreased by 2.", 0), //not done
        //dont forget min and max offspring additional

        //Health
        defaultGenes.Add(new Genes("Strong", "Has more health capacity.", 1,
               new List<StatModifier> {
                    new StatModifier(StatType.MaxHealth, 15f)
               }));
        defaultGenes.Add(new Genes("Weak", "Has less health capacity.", -1,
                new List<StatModifier> {
                    new StatModifier(StatType.MaxHealth, -15f)
                }));
        defaultGenes.Add(new Genes("Healthy", "Regens more health when injured.", 1,
                new List<StatModifier> {
                    new StatModifier(StatType.Regen, 0.5f)
                }));
        defaultGenes.Add(new Genes("Unhealthy", "Regens less health when injured.", -1,
                new List<StatModifier> {
                    new StatModifier(StatType.Regen, -0.5f)
                }));

        //Detection
        defaultGenes.Add(new Genes("Alert", "Detection radius and range increased.", 1,
                new List<StatModifier> {
                    new StatModifier(StatType.DetectionAngle, 20f),
                    new StatModifier(StatType.DetectionDistance, 0.75f)
                }));
        defaultGenes.Add(new Genes("FarVision", "Detection range greatly increased.", 2,
                new List<StatModifier> {
                    new StatModifier(StatType.DetectionAngle, 1.5f),
                }));
        defaultGenes.Add(new Genes("Blurred", "Detection range and radius decreased.", -1,
                new List<StatModifier> {
                    new StatModifier(StatType.DetectionAngle, -20f),
                    new StatModifier(StatType.DetectionDistance, -0.75f)
                }));
        defaultGenes.Add(new Genes("Shortsight", "Detection range significantly reduced.", -2,
                new List<StatModifier> {
                    new StatModifier(StatType.DetectionAngle, -1.5f),
                }));

        //Look while Eating
        defaultGenes.Add(new Genes("Paranoid", "Looks around more often when eating.", 1,
                new List<StatModifier> {
                    new StatModifier(StatType.LookWhileEatingInterval, -1.5f),
                    new StatModifier(StatType.LookAngleMax, 10f),
                    new StatModifier(StatType.LookAngleMin, 10f)
                }));
        defaultGenes.Add(new Genes("Careful", "Stays in burrows longer after hiding.", 1,
                new List<StatModifier> {
                    new StatModifier(StatType.WaitBeforeLeavingBurrow, 2.5f),
                }));
        defaultGenes.Add(new Genes("Unaware", "Looks around less often when eating.", -1,
                new List<StatModifier> {
                    new StatModifier(StatType.LookWhileEatingInterval, 1.5f),
                    new StatModifier(StatType.LookAngleMax, -10f),
                    new StatModifier(StatType.LookAngleMin, -10f)
                }));
        defaultGenes.Add(new Genes("Impatient", "Leaves burrow faster after hiding.", -1,
                new List<StatModifier> {
                    new StatModifier(StatType.WaitBeforeLeavingBurrow, -2.5f),
                }));

        //Getting Detected/Stealth
        //new Genes("Quiet", "Less detectable.", 2), //not done
        //new Genes("Loud", "More detectable.", 0), //not done


        //Sleep
        defaultGenes.Add(new Genes("Sleeper", "Sleeps 1 hour more than normal.", -1,
            new List<StatModifier> {
                    new StatModifier(StatType.SleepHours, 1f),
            }));
        defaultGenes.Add(new Genes("Active", "Sleeps 1 hour less than normal.", 1,
                new List<StatModifier> {
                    new StatModifier(StatType.SleepHours, -1f),
                }));
        defaultGenes.Add(new Genes("EarlyWorm", "Sleeps 2 hours less than normal.", 2,
                new List<StatModifier> {
                    new StatModifier(StatType.SleepHours, -2f),
                }));
        defaultGenes.Add(new Genes("Hibernater", "Sleeps 2 hours more than normal.", -2,
                new List<StatModifier> {
                    new StatModifier(StatType.SleepHours, 2f),
                }));

        ////Needs interval
        defaultGenes.Add(new Genes("HighMetabolism", "Needs decrease faster", -1,
                new List<StatModifier> {
                    new StatModifier(StatType.NeedsInterval, -0.25f),
                }));
        defaultGenes.Add(new Genes("LowMetabolism", "Needs decrease slower", 1,
            new List<StatModifier> {
                    new StatModifier(StatType.NeedsInterval, 0.25f),
                }));
    }

    public void LoadCustomGenes()
    {
        if (File.Exists(saveFilePath))
        {
            string json = File.ReadAllText(saveFilePath);
            var wrapper = JsonUtility.FromJson<GeneList>(json);

            customGenes.Clear();
            foreach (Genes geneData in wrapper.genes)
            {
                List<StatModifier> statModifiers = geneData.statModifiers ?? new List<StatModifier>();

                Genes gene = new Genes(geneData.name, geneData.description, geneData.positivity, statModifiers);
                customGenes.Add(gene);
            }
        }
    }

    public void SaveCustomGenes()
    {
        GeneList wrapper = new GeneList { genes = customGenes };
        string json = JsonUtility.ToJson(wrapper, true);
        File.WriteAllText(saveFilePath, json);
    }


    public void AddNewGene(string name, string description, int positivity, List<StatModifier> statModifiers)
    {
        Genes newGene = new Genes(name, description, positivity, statModifiers);
        customGenes.Add(newGene);

        SaveCustomGenes();
    }

    public List<Genes> GetAllDefaultGenes()
    {
        return defaultGenes;
    }

    public List<Genes> GetAllCustomGenes()
    {
        return customGenes;
    }

    public List<Genes> GetAllGenes()
    {
        List<Genes> allGenes = new List<Genes>(defaultGenes);
        allGenes.AddRange(customGenes);
        return allGenes;
    }
}

[System.Serializable]
public class GeneList
{
    public List<Genes> genes;
}

[System.Serializable]
public class Genes
{
    public string name;
    public string description;
    public int positivity; //-2 - extremely bad, -1 - negative, 0- neutral 1- positive, 2 - extremely positive
    public List<StatModifier> statModifiers = new List<StatModifier>();

    public Genes(string name, string description, int positivity, List<StatModifier> statModifiers)
    {
        this.name = name;
        this.description = description;
        this.positivity = positivity;
        this.statModifiers = statModifiers ?? new List<StatModifier>();
    }
}