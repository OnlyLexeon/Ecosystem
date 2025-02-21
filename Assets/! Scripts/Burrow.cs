using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Burrow : MonoBehaviour
{
    public List<Rabbit> rabbitsInside = new List<Rabbit>();

    public bool threatNearby = false;

    public void EnterBurrow(Rabbit rabbit, float time)
    {
        Debug.Log("Burrow Entered!");

        rabbitsInside.Add(rabbit);
        rabbit.gameObject.SetActive(false);
        StartCoroutine(ExitBurrowAfterTime(rabbit, time, false));
    }

    public void EnterBurrowForSleep(Rabbit rabbit, float time)
    {
        Debug.Log("Sleeping in Burrow!");

        rabbitsInside.Add(rabbit);
        rabbit.gameObject.SetActive(false);
        StartCoroutine(ExitBurrowAfterTime(rabbit, time, true));
    }

    public void EnterBurrowForMating(Rabbit rabbit, float time)
    {
        Debug.Log("Burrow For Seggs!");

        rabbitsInside.Add(rabbit);
        rabbit.gameObject.SetActive(false);
        StartCoroutine(ExitBurrowGiveBirth(rabbit, time));
    }

    private IEnumerator ExitBurrowAfterTime(Rabbit rabbit, float time, bool isSlept)
    {
        yield return new WaitForSeconds(time);

        rabbitsInside.Remove(rabbit);
        rabbit.gameObject.SetActive(true);
        rabbit.currentState = RabbitState.Wandering;

        if (isSlept == true)
        {
            rabbit.WakeUpCheckHorniness();
            rabbit.WakeUpAgeUpdate();
        }
    }

    private IEnumerator ExitBurrowGiveBirth(Rabbit rabbit, float time)
    {
        yield return new WaitForSeconds(time);

        rabbitsInside.Remove(rabbit);
        rabbit.gameObject.SetActive(true);
        rabbit.currentState = RabbitState.Wandering;

        //Birth
        //Check if female, give birth
        if (rabbit.stats.gender == Gender.Female) rabbit.GiveBirth(this);
    }
}
