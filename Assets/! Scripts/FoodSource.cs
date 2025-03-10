using UnityEngine;

public enum FoodType
{
    Berries,
    Carrots,

    //Animals
    Primary,
    Seconary,
    Tertiary,
}

public class FoodSource : MonoBehaviour
{
    [Header("References")]
    public Transform modelHolder;

    [Header("Food Settings* (Edit these!)")]
    public FoodType foodType;

    [Header("(Non Instant)")]
    public float minFoodToEat = 10f;
    public float maxFood = 25f;
    public float foodReplenishedPerSecond = 0.1f;

    [Header("(Instant?)")]
    [Tooltip("Animals that eat this will instantly gain 'instantFood' amount of food.")] public bool instantConsumable = false;
    public float instantFood = 0f;

    [Header("Food Stats (Debug)")]
    public bool canEat = false;
    public float foodAvailable = 0f;

    private float beingEatenTime = 2f;  // Delay before replenishing starts
    private float beingEatenTimer = 0f;
    private bool isBeingEaten = false; // Track if currently being eaten

    private void Start()
    {
        foodAvailable = maxFood / 2f;

        if (instantConsumable) canEat = true;
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

                UpdateFoodSourceModel();
            }
        }
    }

    public float ConsumeFood(float amount)
    {
        if (instantConsumable)
        {
            return instantFood;
        }
        else
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

            UpdateFoodSourceModel();

            return foodTaken;
        }
    }

    public void StopEating()
    {
        isBeingEaten = false; // Called when rabbit stops eating
    }

    public void UpdateFoodSourceModel()
    {
        ClearModelHolderChild();

        switch (foodType)
        {
            case FoodType.Berries:

                Instantiate(Environment.Instance.GetBushModel(DetermineBushType()), modelHolder);
                break;
        }
    }

    public BushTypes DetermineBushType()
    {
        BushTypes bushType = new BushTypes();

        if (canEat && foodAvailable < maxFood) bushType = BushTypes.Ready;
        else if (foodAvailable >= maxFood) bushType = BushTypes.Full;
        else if (foodAvailable < minFoodToEat && foodAvailable >= 0) bushType = BushTypes.Empty;

        return bushType;
    }   

    public void ClearModelHolderChild()
    {
        foreach (Transform child in modelHolder)
        {
            Destroy(child.gameObject);
        }
    }
}
