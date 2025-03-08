using UnityEngine;
using System.Collections.Generic;
using System.IO;
using UnityEditor.Animations;
using TMPro;

public class WorldStats : MonoBehaviour
{
    public static WorldStats Instance;

    public Animal animalHighestGeneration;

    private void Awake()
    {
        Instance = this;
    }

    // Dictionary to store animal stats dynamically
    public Dictionary<string, AnimalWorldStats> animalStats = new Dictionary<string, AnimalWorldStats>();


    // Get or create stats for a given animal type
    private AnimalWorldStats GetAnimalStats(string animalType)
    {
        if (!animalStats.ContainsKey(animalType))
        {
            animalStats[animalType] = new AnimalWorldStats();
        }
        return animalStats[animalType];
    }

    // Increment the count of an animal type
    public void PlusAnimalCount(string animalType)
    {
        GetAnimalStats(animalType).count++;
    }

    // Decrement the count of an animal type
    public void MinusAnimalCount(string animalType)
    {
        GetAnimalStats(animalType).count--;
    }

    // Update generation if a higher generation is found
    public void CheckAnimalGeneration(string animalType, int generation, Animal _animalHighestGeneration)
    {
        var stats = GetAnimalStats(animalType);
        if (generation > stats.generation)
        {
            stats.generation = generation;
            animalHighestGeneration = _animalHighestGeneration;
        }
    }

    // Get the count of a specific animal type
    public int GetAnimalCount(string animalType)
    {
        return GetAnimalStats(animalType).count;
    }

    public int GetAnimalGeneration(string animalType)
    {
        return GetAnimalStats(animalType).generation;
    }

    public Animal GetAnimalWithHighestGeneration()
    {
        return animalHighestGeneration;
    }
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

