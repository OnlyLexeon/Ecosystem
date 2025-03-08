using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewAnimalType", menuName = "Animals/Animal Type")]
public class AnimalType : ScriptableObject
{
    public string animalName;
    public List<FurType> furTypes; // Each animal has its own fur options
    public GameObject animalPrefab; // The default prefab for this animal
}
