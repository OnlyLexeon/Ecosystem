using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewAnimalType", menuName = "Animals/Animal Type")]
public class AnimalType : ScriptableObject
{
    public string animalName;
    public List<FurType> furTypes; // Each animal has its own fur options
    public GameObject animalSpawnPrefab; // The default prefab for this animal

    public GameObject animalDeadPrefab; //corpse prefab for eating

    public override string ToString()
    {
        return animalName;
    }

    private void OnValidate()
    {
        name = animalName; // Ensures the asset name matches `animalName`
    }
}
