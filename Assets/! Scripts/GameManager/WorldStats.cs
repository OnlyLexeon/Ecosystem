using UnityEngine;
using System.Collections.Generic;
using System.IO;
using UnityEditor.Animations;
using TMPro;

public class WorldStats : MonoBehaviour
{
    [Header("Rabbit")]
    public int rabbitCount;
    public int rabbitTotalGenes;
    public int rabbitPositiveGenes;
    public int rabbitNegativeGenes;
    public int rabbitGeneration;

    public static WorldStats Instance;

    private void Awake()
    {
        Instance = this;
    }

    public void CheckRabbitGeneration(int value)
    {
        if (value > rabbitGeneration)
        {
            rabbitGeneration = value;


        }
    }

    public void PlusRabbitCount()
    {
        rabbitCount++;
    }

    public void MinusRabbitCount()
    {
        rabbitCount--;
    }

<<<<<<< Updated upstream:Assets/! Scripts/WorldStats.cs
    public void ToggleAnimalOverHeadUI(bool state)
    {
        isAnimalOverHeadUIEnabled = state;

        foreach (Transform child in transform)
        {
            Animal childAnimalScript = child.GetComponent<Animal>();
            if (childAnimalScript != null && !childAnimalScript.isDead)
            {
                childAnimalScript.ToggleUI(state);
            }
        }

    }
=======
    
>>>>>>> Stashed changes:Assets/! Scripts/GameManager/WorldStats.cs
}
