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
        StartCoroutine(ExitBurrowAfterTime(rabbit, time));
    }

    private IEnumerator ExitBurrowAfterTime(Rabbit rabbit, float time)
    {
        yield return new WaitForSeconds(time);

        rabbitsInside.Remove(rabbit);
        rabbit.gameObject.SetActive(true);
        rabbit.currentState = RabbitState.Wandering;
    }
}
