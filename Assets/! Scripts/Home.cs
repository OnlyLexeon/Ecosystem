using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Home : MonoBehaviour
{
    [Header("Settings")]
    public int destroyIfNoEnterInDays = 3;
    public int daysWithoutEntering = 0;

    public AnimalType animalType;
    public List<Animal> animalInside = new List<Animal>();


    //ENTERING
    public void EnterBurrow(Animal animal, float time) //hiding/making home
    {
        //Debug.Log("Burrow Entered!");

        animalInside.Add(animal);
        animal.gameObject.SetActive(false);
        StartCoroutine(ExitBurrowAfterTime(animal, time, false));
    }
    public void EnterBurrowForSleep(Animal animal, float time) //update age+horny
    {
        //Debug.Log("Sleeping in Burrow!");

        animalInside.Add(animal);
        animal.gameObject.SetActive(false);
        StartCoroutine(ExitBurrowAfterTime(animal, time, true));
    }
    public void EnterBurrowForMating(Animal animal, float time) //mate, give birth
    {
        //Debug.Log("Burrow For Seggs!");

        string eventString = $"Day {DayNightManager.Instance.dayNumber}, {DayNightManager.Instance.GetTimeString()}\n" +
                $"{animal.animalName} - {animal.animalType} ({animal.furType}) entered a burrow to mate <3";
        UIManager.Instance.AddNewHistory(eventString, () => InputHandler.Instance.SetTargetAndFollow(animal.transform), HistoryType.Mating);

        animalInside.Add(animal);
        animal.gameObject.SetActive(false);
        StartCoroutine(ExitBurrowGiveBirth(animal, time));
    }

    //EXITING
    private IEnumerator ExitBurrowAfterTime(Animal animal, float time, bool isSlept)
    {
        yield return new WaitForSeconds(time);

        animalInside.Remove(animal);
        animal.gameObject.SetActive(true);
        animal.currentState = AnimalState.Wandering;

        if (isSlept == true)
        {
            animal.WakeUpCheckHorniness();
        }
    }

    private IEnumerator ExitBurrowGiveBirth(Animal animal, float time)
    {
        yield return new WaitForSeconds(time);

        animalInside.Remove(animal);
        animal.gameObject.SetActive(true);
        animal.currentState = AnimalState.Wandering;

        //Birth
        //Check if female, give birth
        if (animal.stats.gender == Gender.Female) animal.GiveBirth(this);
    }


    public void UpdateDays()
    {
        if (animalInside.Count <= 0)
        {
            daysWithoutEntering += 1;
        }
        else
        {
            daysWithoutEntering = 0;
        }

        if (daysWithoutEntering >= destroyIfNoEnterInDays) CallDestroy();
    }

    public void CallDestroy()
    {
        for (int i = animalInside.Count - 1; i >= 0; i--)
        {
            var animal = animalInside[i];
            animal.gameObject.SetActive(true);
            animal.currentState = AnimalState.Wandering;
            animalInside.RemoveAt(i);
        }

        Destroy(gameObject);
    }
}
