using UnityEngine;
using System.Collections.Generic;

public class WorldStats : MonoBehaviour
{
    public static WorldStats Instance;

    private void Awake()
    {
        Instance = this;
    }

    // List to store animal stats in a way Unity can serialize
    public List<AnimalWorldStatsEntry> animalStatsList = new List<AnimalWorldStatsEntry>();

    // Get or create stats for a given animal type
    private AnimalWorldStatsEntry GetAnimalStatsEntry(AnimalType animalType)
    {
        var entry = animalStatsList.Find(e => e.animalType == animalType);
        if (entry == null)
        {
            entry = new AnimalWorldStatsEntry { animalType = animalType, stats = new AnimalWorldStats() };
            animalStatsList.Add(entry);
        }
        return entry;
    }

    private AnimalWorldStats GetAnimalStats(AnimalType animalType)
    {
        return GetAnimalStatsEntry(animalType).stats;
    }

    // Increment the count of an animal type
    public void PlusAnimalCount(AnimalType animalType)
    {
        GetAnimalStats(animalType).count++;
    }

    // Decrement the count of an animal type
    public void MinusAnimalCount(AnimalType animalType)
    {
        GetAnimalStats(animalType).count--;
    }

    // Update generation if a higher generation is found
    public void CheckAnimalGeneration(AnimalType animalType, int generation, Animal _animalHighestGeneration)
    {
        var entry = GetAnimalStatsEntry(animalType);
        if (generation > entry.stats.generation)
        {
            entry.stats.generation = generation;
            entry.generationHighestAnimal = _animalHighestGeneration;

            //Instantly Termination Condition Check
            TerminationManager.Instance.CheckTermination();
        }
    }

    // Get the count of a specific animal type
    public int GetAnimalCount(AnimalType animalType)
    {
        return GetAnimalStats(animalType).count;
    }

    public int GetAnimalGeneration(AnimalType animalType)
    {
        return GetAnimalStats(animalType).generation;
    }

    public Animal GetAnimalWithHighestGeneration(AnimalType animalType)
    {
        return GetAnimalStatsEntry(animalType).generationHighestAnimal;
    }

    public void UpdateGeneStats(AnimalType animalType, int positiveGenes, int negativeGenes, int neutralGenes)
    {
        int totalToAdd = positiveGenes + negativeGenes + neutralGenes;

        GetAnimalStats(animalType).totalGenes += totalToAdd;
        GetAnimalStats(animalType).positiveGenes += positiveGenes;
        GetAnimalStats(animalType).negativeGenes += negativeGenes;
    }
}

// Serializable wrapper for Unity to show in the Inspector
[System.Serializable]
public class AnimalWorldStatsEntry
{
    public AnimalType animalType;
    public Animal generationHighestAnimal;
    public AnimalWorldStats stats;
}

// Structure to store stats for each animal type
[System.Serializable]
public class AnimalWorldStats
{
    public int count;
    public int totalGenes;
    public int positiveGenes;
    public int negativeGenes;
    public int generation;
}
