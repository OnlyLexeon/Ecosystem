using UnityEngine;

public class BurrowManager : MonoBehaviour
{
    public Transform burrowHolder;

    public static BurrowManager Instance;
    private void Awake()
    {
        Instance = this;
    }

    public void UpdateBurrows()
    {
        foreach (Transform child in burrowHolder)
        {
            Home home = child.GetComponent<Home>();
            if (home != null)
            {
                home.UpdateDays();
            }
            else Debug.Log("Unexpected GameObject in Burrow Holder (Doesn't have Home Script)");
        }
    }
}
