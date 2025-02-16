using UnityEngine;

public class FoodSource : MonoBehaviour
{
    public bool canEat = false;
    public float foodAvailable = 0f;
    public float minFoodToEat = 10f;
    public float maxFood = 25f;
    public float foodReplenishedPerSecond = 0.1f;

    public float beingEatenTime = 2f;  // Delay before replenishing starts
    private float beingEatenTimer = 0f;
    private bool isBeingEaten = false; // Track if currently being eaten

    private void Start()
    {
        foodAvailable = maxFood / 2f;
    }

    void Update()
    {
        if (!isBeingEaten)
        {
            // Increment timer only if not being eaten
            beingEatenTimer += Time.deltaTime;

            // Start replenishing food only after beingEatenTime
            if (beingEatenTimer >= beingEatenTime && foodAvailable < maxFood)
            {
                foodAvailable += foodReplenishedPerSecond * Time.deltaTime;
                foodAvailable = Mathf.Min(foodAvailable, maxFood);

                if (foodAvailable >= minFoodToEat)
                    canEat = true;  // Enable eating when food is enough
            }
        }
    }

    public float ConsumeFood(float amount)
    {
        if (foodAvailable <= 0) return 0f;

        beingEatenTimer = 0f; // Reset timer when eaten
        isBeingEaten = true;  // Mark as being eaten

        float foodTaken = Mathf.Min(amount, foodAvailable);
        foodAvailable -= foodTaken;

        // Disable eating if food drops below minFoodToEat
        if (foodAvailable < minFoodToEat)
            canEat = false;

        // If food is empty, stop eating and reset state
        if (foodAvailable <= 0)
        {
            foodAvailable = 0;
            isBeingEaten = false;
        }

        return foodTaken;
    }

    public void StopEating()
    {
        isBeingEaten = false; // Called when rabbit stops eating
    }
}
