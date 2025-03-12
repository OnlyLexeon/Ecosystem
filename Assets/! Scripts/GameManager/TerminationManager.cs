using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;


public enum TerminationConditionType
{
    None,
    SurvivedTillOldAge,
    GenerationReached_, //+ int
    NumberOfAnimals_, //+ int
    PerfectIndividual,
    Extinction,
}

public class TerminationManager : MonoBehaviour
{
    [Header("'_' Symbol means Set Value Below (Condition)")]
    public TerminationCondition terminationCondition;

    public static TerminationManager Instance;

    private void Awake()
    {
        Instance = this;
    }

    public bool CheckTermination()
    {
        switch (terminationCondition.conditionType)
        {
            case TerminationConditionType.SurvivedTillOldAge:
                if (CheckSurvivedTillOldAge(terminationCondition.targetAnimalType))
                    return true;
                break;

            case TerminationConditionType.GenerationReached_:
                if (CheckGenerationCount(terminationCondition.targetAnimalType, terminationCondition.generationReachedThreshold))
                    return true;
                break;

            case TerminationConditionType.NumberOfAnimals_:
                if (CheckNumberOfAnimals(terminationCondition.targetAnimalType, terminationCondition.numberOfAnimalsThreshold))
                    return true;
                break;

            case TerminationConditionType.PerfectIndividual:
                if (CheckAnyAnimalWithAllPositiveGenes(terminationCondition.targetAnimalType))
                    return true;
                break;
            case TerminationConditionType.Extinction:
                if (CheckExtinction(terminationCondition.targetAnimalType))
                    return true;
                break;
            default:
                break;
        }

        return false;
    }

    private bool CheckSurvivedTillOldAge(AnimalType animalType)
    {
        // Check if any animal of the given type has reached old age
        foreach (Transform child in AnimalContainer.Instance.transform)
        {
            Animal childScript = child.GetComponent<Animal>();
            if (childScript != null && childScript.animalType == animalType && childScript.stats.agedDays >= childScript.stats.deathDays)
            {
                //HISTORYT EVENT
                DayNightManager.Instance.PauseTime();
                //History
                string eventString = $"Day {DayNightManager.Instance.dayNumber}, {DayNightManager.Instance.GetTimeString()}\n" +
                    $"Termination Condition Reached!\n{childScript.animalName} - {childScript.animalType.ToString()} ({childScript.furType.ToString()}) has Survived till Old Age! Current Age: {childScript.stats.agedDays}/{childScript.stats.deathDays}";
                UIManager.Instance.AddNewHistory(eventString, () => InputHandler.Instance.SetTargetAndFollow(childScript.transform));

                return true;
            }
        }
        return false;
    }

    private bool CheckGenerationCount(AnimalType animalType, int generationThreshold)
    {
        int currentGeneration = WorldStats.Instance.GetAnimalGeneration(animalType);
        Animal animal = WorldStats.Instance.GetAnimalWithHighestGeneration(animalType);

        if (animal != null && currentGeneration >= generationThreshold)
        {
            //HISTORYT EVENT
            DayNightManager.Instance.PauseTime();
            //History
            string eventString = $"Day {DayNightManager.Instance.dayNumber}, {DayNightManager.Instance.GetTimeString()}\n" +
                $"Termination Condition Reached!\n{animal.animalName} - {animal.animalType} ({animal.furType}) is the first animal born in the {currentGeneration}th Generation!!";
            UIManager.Instance.AddNewHistory(eventString, () => InputHandler.Instance.SetTargetAndFollow(animal.transform));

            return true;
        }
        return false;
    }

    private bool CheckNumberOfAnimals(AnimalType animalType, int countThreshold)
    {
        int count = WorldStats.Instance.GetAnimalCount(animalType);

        if (count >= countThreshold)
        {
            //HISTORYT EVENT
            DayNightManager.Instance.PauseTime();
            //History
            string eventString = $"Day {DayNightManager.Instance.dayNumber}, {DayNightManager.Instance.GetTimeString()}\n" +
                $"Termination Condition Reached!\n {animalType.animalName} has reached a Population of: {count}!";
            UIManager.Instance.AddNewHistory(eventString);
            return true;
        }
        return false;
    }

    private bool CheckAnyAnimalWithAllPositiveGenes(AnimalType animalType)
    {
        foreach (Transform child in AnimalContainer.Instance.transform)
        {
            Animal childScript = child.GetComponent<Animal>();
            if (childScript != null && childScript.animalType == animalType
                && childScript.stats.GetPositiveGenesCount() == childScript.stats.genes.Count)
            {
                //HISTORYT EVENT
                DayNightManager.Instance.PauseTime();
                //History
                string eventString = $"Day {DayNightManager.Instance.dayNumber}, {DayNightManager.Instance.GetTimeString()}\n" +
                    $"Termination Condition Reached!\n{childScript.animalName} - {childScript.animalType} ({childScript.furType}) has achieved peak fitness! Number of Positive Genes: {childScript.stats.GetPositiveGenesCount()}/{childScript.stats.genes.Count} genes";
                UIManager.Instance.AddNewHistory(eventString, () => InputHandler.Instance.SetTargetAndFollow(childScript.transform));

                return true;
            }
        }
        return false;
    }

    private bool CheckExtinction(AnimalType animalType)
    {
        int count = WorldStats.Instance.GetAnimalCount(animalType);
        if (count == 0)
        {
            //HISTORYT EVENT
            DayNightManager.Instance.PauseTime();
            string eventString = $"Day {DayNightManager.Instance.dayNumber}, {DayNightManager.Instance.GetTimeString()}\n" +
                $"Termination Condition Reached!\n{animalType} has gone extinct!";
            UIManager.Instance.AddNewHistory(eventString);
            return true;
        }
        return false;
    }
}


[System.Serializable]
public class TerminationCondition
{
    public TerminationConditionType conditionType;
    public AnimalType targetAnimalType; // Which animal this applies to

    [Header("Certain Conditions Only (Value)")]
    public int generationReachedThreshold; // Only used for GenerationCount
    public int numberOfAnimalsThreshold; // Used for NumberOfAnimals
}