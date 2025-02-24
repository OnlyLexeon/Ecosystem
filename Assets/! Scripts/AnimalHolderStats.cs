using UnityEngine;
using System.Collections.Generic;
using System.IO;
using UnityEditor.Animations;

public class AnimalHolderStats : MonoBehaviour
{
    public bool isAnimalOverHeadUIEnabled = false;

    [Header("Rabbit")]
    public int rabbitCount;
    public int rabbitTotalGenes;
    public int rabbitPositiveGenes;
    public int rabbitNegativeGenes;

    public static AnimalHolderStats Instance;

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

    public void ToggleAnimalOverHeadUI(bool state)
    {
        isAnimalOverHeadUIEnabled = state;

        foreach (Transform child in transform)
        {
            Animal childAnimalScript = child.GetComponent<Animal>();
            if (childAnimalScript != null)
            {
                childAnimalScript.ToggleUI(state);
            }
        }

    }
}
