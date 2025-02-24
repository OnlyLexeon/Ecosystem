using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEditor.Overlays;

public class GeneManager : MonoBehaviour
{
    private static string saveFilePath;
    public List<Genes> defaultGenes = new List<Genes>(); // Default genes list
    private List<Genes> customGenes = new List<Genes>();  // Loaded custom genes

    public static GeneManager Instance;

    private void Awake()
    {
        Instance = this;

        saveFilePath = Path.Combine(Application.persistentDataPath, "genes.json");

        //LoadDefaultGenes(); 
        LoadCustomGenes();
    }

    //Loading
    private void LoadDefaultGenes()
    {
        ////Wandering
        //defaultGenes.Add(new Genes("Lazy", "Moves at a lower max distance when wandering. (-1.5)", Positivity.Negative,
        //        new List<StatModifier> {
        //            new StatModifier(StatType.WanderDistanceMin, -1.5f)
        //        }));
        //defaultGenes.Add(new Genes("Adventurer", "Moves at a higher max distance when wandering. (+1.5)", Positivity.Positive,
        //        new List<StatModifier> {
        //            new StatModifier(StatType.WanderDistanceMax, 1.5f)
        //        }));
        //defaultGenes.Add(new Genes("Restful", "Increases wander interval. (+2)", Positivity.Negative,
        //        new List<StatModifier> {
        //            new StatModifier(StatType.WanderInterval, 2f)
        //        }));
        //defaultGenes.Add(new Genes("Restless", "Decreases wander interval. (-2)", Positivity.Positive,
        //        new List<StatModifier> {
        //            new StatModifier(StatType.WanderInterval, -2f)
        //        }));

        ////Depletion Rate
        //defaultGenes.Add(new Genes("Energetic", "Moves faster (+0.5) but loses more hunger. (+0.05)", Positivity.Neutral,
        //        new List<StatModifier> {
        //            new StatModifier(StatType.HungerDepletionRate, 0.05f),
        //            new StatModifier(StatType.BaseSpeed, 0.5f),
        //            new StatModifier(StatType.RunSpeed, 0.5f)
        //        }));
        //defaultGenes.Add(new Genes("Sluggish", "Moves slower (-0.5) but loses less hunger. (-0.05)", Positivity.Neutral,
        //        new List<StatModifier> {
        //            new StatModifier(StatType.HungerDepletionRate, -0.05f),
        //            new StatModifier(StatType.BaseSpeed, -0.5f),
        //            new StatModifier(StatType.RunSpeed, -0.5f)
        //        }));
        //defaultGenes.Add(new Genes("Thirsty", "Loses more thirst. (0.05)", Positivity.Negative,
        //        new List<StatModifier> {
        //            new StatModifier(StatType.ThirstDepletionRate, 0.05f),
        //        }));

        //defaultGenes.Add(new Genes("Retention", "Loses less thirst. (-0.05)", Positivity.Positive,
        //        new List<StatModifier> {
        //            new StatModifier(StatType.ThirstDepletionRate, -0.05f),
        //        }));

        //defaultGenes.Add(new Genes("IronStomach", "Loses less hunger. (-0.05)", Positivity.Positive,
        //        new List<StatModifier> {
        //            new StatModifier(StatType.HungerDepletionRate, -0.05f),
        //        }));

        //defaultGenes.Add(new Genes("Hunger", "Loses more hunger. (0.05)", Positivity.Negative,
        //        new List<StatModifier> {
        //            new StatModifier(StatType.HungerDepletionRate, 0.05f),
        //        }));

        ////Hunger
        //defaultGenes.Add(new Genes("Gluttonous", "Moves slower (-0.5) but eats faster (+2) and has more hunger capacity. (+15)", Positivity.Neutral,
        //        new List<StatModifier> {
        //            new StatModifier(StatType.FoodEatPerSecond, 2f),
        //            new StatModifier(StatType.MaxHunger, 15f),
        //            new StatModifier(StatType.BaseSpeed, -0.5f),
        //            new StatModifier(StatType.RunSpeed, -0.5f)
        //        }));
        //defaultGenes.Add(new Genes("Storager", "More hunger capacity. (+20)", Positivity.Positive,
        //        new List<StatModifier> {
        //            new StatModifier(StatType.MaxHunger, 20f),
        //        }));
        //defaultGenes.Add(new Genes("Starver", "Less hunger capacity. (-20)", Positivity.Negative,
        //        new List<StatModifier> {
        //            new StatModifier(StatType.MaxHunger, -20f),
        //        }));

        ////Movement
        //defaultGenes.Add(new Genes("Unathletic", "Moves slower. (-0.3)", Positivity.Negative,
        //        new List<StatModifier> {
        //            new StatModifier(StatType.BaseSpeed, -0.3f),
        //            new StatModifier(StatType.RunSpeed, -0.3f),
        //        }));
        //defaultGenes.Add(new Genes("Athletic", "Moves faster. (+0.3)", Positivity.Positive,
        //        new List<StatModifier> {
        //            new StatModifier(StatType.BaseSpeed, 0.3f),
        //            new StatModifier(StatType.RunSpeed, 0.3f),
        //        }));
        //defaultGenes.Add(new Genes("Runner", "Runs faster. (+0.4)", Positivity.Positive,
        //        new List<StatModifier> {
        //            new StatModifier(StatType.RunSpeed, 0.4f),
        //        }));
        //defaultGenes.Add(new Genes("Wanderer", "Wanders faster. (+0.4)", Positivity.Positive,
        //        new List<StatModifier> {
        //            new StatModifier(StatType.BaseSpeed, 0.4f),
        //        }));
        //defaultGenes.Add(new Genes("Snail", "Significantly slower. (-0.6)", Positivity.ExtremelyNegative,
        //        new List<StatModifier> {
        //            new StatModifier(StatType.BaseSpeed, -0.6f),
        //            new StatModifier(StatType.RunSpeed, -0.6f)
        //        }));
        //defaultGenes.Add(new Genes("Hyper", "Significantly faster. (+0.6)", Positivity.ExtremelyPositive,
        //        new List<StatModifier> {
        //            new StatModifier(StatType.BaseSpeed, 0.6f),
        //            new StatModifier(StatType.RunSpeed, 0.6f)
        //        }));

        ////Eat/Drink
        //defaultGenes.Add(new Genes("FastEater", "Eats faster. (+1.5)", Positivity.Positive,
        //        new List<StatModifier> {
        //            new StatModifier(StatType.FoodEatPerSecond, 1.5f)
        //        }));
        //defaultGenes.Add(new Genes("SlowEater", "Eats slower. (-1.5)", Positivity.Negative,
        //        new List<StatModifier> {
        //            new StatModifier(StatType.FoodEatPerSecond, -1.5f)
        //        }));
        //defaultGenes.Add(new Genes("FastDrinker", "Drinks faster. (+2)", Positivity.Positive,
        //        new List<StatModifier> {
        //            new StatModifier(StatType.DrinkPerSecond, 2f)
        //        }));
        //defaultGenes.Add(new Genes("SlowDrinker", "Drinks slower. (-2)", Positivity.Negative,
        //        new List<StatModifier> {
        //            new StatModifier(StatType.DrinkPerSecond, -2f)
        //        }));

        ////Seggs
        //defaultGenes.Add(new Genes("Fertile", "Increase base offspring count. (+2)", Positivity.Positive,
        //        new List<StatModifier> {
        //            new StatModifier(StatType.BaseOffspringCount, 2f)
        //        }));
        //defaultGenes.Add(new Genes("LessFertile", "Decrease base offspring count. (-2)", Positivity.Negative,
        //        new List<StatModifier> {
        //            new StatModifier(StatType.BaseOffspringCount, -2f)
        //        }));
        //defaultGenes.Add(new Genes("Reproductive", "Increase random additional offspring count. (+2)", Positivity.Positive,
        //        new List<StatModifier> {
        //            new StatModifier(StatType.MaxAdditionalOffspring, 2f)
        //        }));
        //defaultGenes.Add(new Genes("Non-Reproductive", "Decreases random additional offspring count. (-2)", Positivity.Negative,
        //        new List<StatModifier> {
        //            new StatModifier(StatType.MaxAdditionalOffspring, -2f)
        //        }));
        //defaultGenes.Add(new Genes("Non-Fertile", "Can't Reproduce.", Positivity.ExtremelyNegative,
        //        new List<StatModifier> {
        //            new StatModifier(StatType.Fertile, 0)
        //        }));
        //defaultGenes.Add(new Genes("GeneBias", "Prefers mate with more positive genes. (+1)", Positivity.Positive,
        //        new List<StatModifier> {
        //            new StatModifier(StatType.MinPositiveGenesPrefered, 1f)
        //        }));
        //defaultGenes.Add(new Genes("MateInstinct", "Prefers mate with less negative genes. (-1)", Positivity.Positive,
        //        new List<StatModifier> {
        //            new StatModifier(StatType.MaxNegativeGenesPrefered, -1f)
        //        }));
        //defaultGenes.Add(new Genes("GeneUnbias", "Decrease mate's positive gene required. (-1)", Positivity.Positive,
        //        new List<StatModifier> {
        //            new StatModifier(StatType.MinPositiveGenesPrefered, -1f)
        //        }));
        //defaultGenes.Add(new Genes("NonStandard", "Increase mate's negative gene cap. (+1)", Positivity.Positive,
        //        new List<StatModifier> {
        //            new StatModifier(StatType.MaxNegativeGenesPrefered, 1f)
        //        }));
        //defaultGenes.Add(new Genes("Horny", "Reduce reproduction cooldown day. (-1)", Positivity.Positive,
        //        new List<StatModifier> {
        //            new StatModifier(StatType.ReproduceCooldownDays, -1f)
        //        }));
        //defaultGenes.Add(new Genes("Lustless", "Increase reproduction cooldown day. (+1)", Positivity.Negative,
        //        new List<StatModifier> {
        //            new StatModifier(StatType.ReproduceCooldownDays, 1f)
        //        }));

        ////Health
        //defaultGenes.Add(new Genes("Strong", "Has more health capacity. (+15)", Positivity.Positive,
        //       new List<StatModifier> {
        //            new StatModifier(StatType.MaxHealth, 15f)
        //       }));
        //defaultGenes.Add(new Genes("Weak", "Has less health capacity. (-15)", Positivity.Negative,
        //        new List<StatModifier> {
        //            new StatModifier(StatType.MaxHealth, -15f)
        //        }));
        //defaultGenes.Add(new Genes("Healthy", "Regens more health when injured. (+0.5)", Positivity.Positive,
        //        new List<StatModifier> {
        //            new StatModifier(StatType.Regen, 0.5f)
        //        }));
        //defaultGenes.Add(new Genes("Unhealthy", "Regens less health when injured. (-0.5)", Positivity.Negative,
        //        new List<StatModifier> {
        //            new StatModifier(StatType.Regen, -0.5f)
        //        }));

        ////Detection
        //defaultGenes.Add(new Genes("Alert", "Detection radius (+20) and range increased. (+0.75)", Positivity.Positive,
        //        new List<StatModifier> {
        //            new StatModifier(StatType.DetectionAngle, 20f),
        //            new StatModifier(StatType.DetectionDistance, 0.75f)
        //        }));
        //defaultGenes.Add(new Genes("FarVision", "Detection range greatly increased. (+1.5)", Positivity.ExtremelyPositive,
        //        new List<StatModifier> {
        //            new StatModifier(StatType.DetectionDistance, 1.5f),
        //        }));
        //defaultGenes.Add(new Genes("Blurred", "Detection range (-20) and radius decreased. (-0.75)", Positivity.Negative,
        //        new List<StatModifier> {
        //            new StatModifier(StatType.DetectionAngle, -20f),
        //            new StatModifier(StatType.DetectionDistance, -0.75f)
        //        }));
        //defaultGenes.Add(new Genes("Shortsight", "Detection range significantly reduced. (-1.5)", Positivity.ExtremelyNegative,
        //        new List<StatModifier> {
        //            new StatModifier(StatType.DetectionDistance, -1.5f),
        //        }));

        ////Look while Eating
        //defaultGenes.Add(new Genes("Paranoid", "Decrease Look around Interval. (-1.5)", Positivity.Positive,
        //        new List<StatModifier> {
        //            new StatModifier(StatType.LookWhileEatingInterval, -1.5f),
        //            new StatModifier(StatType.LookAngleMax, 10f),
        //            new StatModifier(StatType.LookAngleMin, 10f)
        //        }));
        //defaultGenes.Add(new Genes("Unaware", "Increases Look around Interval (+1.5)", Positivity.Negative,
        //        new List<StatModifier> {
        //            new StatModifier(StatType.LookWhileEatingInterval, 1.5f),
        //            new StatModifier(StatType.LookAngleMax, -10f),
        //            new StatModifier(StatType.LookAngleMin, -10f)
        //        }));

        ////Wait in Burrow
        //defaultGenes.Add(new Genes("Careful", "Stays in burrows longer after hiding. (+5)", Positivity.Positive,
        //        new List<StatModifier> {
        //            new StatModifier(StatType.LeaveBurrowWaitTime, 5f),
        //        }));
        //defaultGenes.Add(new Genes("Impatient", "Leaves burrow faster after hiding. (-5)", Positivity.Negative,
        //        new List<StatModifier> {
        //            new StatModifier(StatType.LeaveBurrowWaitTime, -5f),
        //        }));

        ////Getting Detected/Stealth
        ////new Genes("Quiet", "Less detectable.", 2), //not done
        ////new Genes("Loud", "More detectable.", 0), //not done

        ////Sleep
        //defaultGenes.Add(new Genes("Sleeper", "Sleeps more than normal. (+1)", Positivity.Negative,
        //    new List<StatModifier> {
        //            new StatModifier(StatType.SleepHours, 1f),
        //    }));
        //defaultGenes.Add(new Genes("Active", "Sleeps less than normal. (-1)", Positivity.Positive,
        //        new List<StatModifier> {
        //            new StatModifier(StatType.SleepHours, -1f),
        //        }));
        //defaultGenes.Add(new Genes("EarlyWorm", "Sleeps a lot less than normal. (-2)", Positivity.ExtremelyPositive,
        //        new List<StatModifier> {
        //            new StatModifier(StatType.SleepHours, -2f),
        //        }));
        //defaultGenes.Add(new Genes("Hibernater", "Sleeps a lot more than normal. (+2)", Positivity.ExtremelyNegative,
        //        new List<StatModifier> {
        //            new StatModifier(StatType.SleepHours, 2f),
        //        }));

        //////Needs interval
        //defaultGenes.Add(new Genes("HighMetabolism", "Decrease needs depletion interval (-0.25)", Positivity.Negative,
        //        new List<StatModifier> {
        //            new StatModifier(StatType.NeedsInterval, -0.25f),
        //        }));
        //defaultGenes.Add(new Genes("LowMetabolism", "Increase needs depletion interval (+0.25)", Positivity.Positive,
        //    new List<StatModifier> {
        //            new StatModifier(StatType.NeedsInterval, 0.25f),
        //        }));
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
    //Saving
    public void SaveCustomGenes()
    {
        GeneList wrapper = new GeneList { genes = customGenes };
        string json = JsonUtility.ToJson(wrapper, true);
        File.WriteAllText(saveFilePath, json);
    }
    //Clearing
    public void ClearAllCustomGenes()
    {
        customGenes.Clear();
        SaveCustomGenes();
    }

    //Adding
    public void AddNewGene(string name, string description, Positivity positivity, List<StatModifier> statModifiers)
    {
        Genes newGene = new Genes(name, description, positivity, statModifiers);
        customGenes.Add(newGene);

        SaveCustomGenes();
    }

    //Getting lists
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

public enum Positivity
{
    ExtremelyNegative,
    Negative,
    Neutral,
    Positive,
    ExtremelyPositive,
}

[System.Serializable]
public class Genes
{
    public string name;
    public string description;
    public Positivity positivity; //-2 - extremely bad, -1 - negative, 0- neutral 1- positive, 2 - extremely positive
    public List<StatModifier> statModifiers = new List<StatModifier>();

    public Genes(string name, string description, Positivity positivity, List<StatModifier> statModifiers)
    {
        this.name = name;
        this.description = description;
        this.positivity = positivity;
        this.statModifiers = statModifiers ?? new List<StatModifier>();
    }
}