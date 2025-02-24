using UnityEngine;
using System.Collections.Generic;
using System.IO;
using UnityEditor.Animations;

public class AnimalStats : MonoBehaviour
{
    public bool isAnimalOverHeadUIEnabled = false;

    [Header("Rabbit")]
    public int rabbitCount;
    public int rabbitTotalGenes;
    public int rabbitPositiveGenes;
    public int rabbitNegativeGenes;

    [Header("References")]
    public GameObject animalHolder;

    public static AnimalStats Instance;

    private void Awake()
    {
        Instance = this;
    }

    public void PlusRabbitCount()
    {
        rabbitCount++;
    }

    public void MinusRabbitCount()
    {
        rabbitCount--;
    }

    public void ToggleAnimalOverHeadUI()
    {
        isAnimalOverHeadUIEnabled = !isAnimalOverHeadUIEnabled;

        foreach (Transform child in transform)
        {
            Animal childAnimalScript = child.GetComponent<Animal>();
            if (childAnimalScript != null)
            {
                childAnimalScript.ToggleUI(isAnimalOverHeadUIEnabled);
            }
        }

    }
}
