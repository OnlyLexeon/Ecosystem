using UnityEngine.AI;
using UnityEngine;
using System.Collections;
using Mono.Cecil.Cil;
using System.Diagnostics.CodeAnalysis;
using static Stats;
using UnityEditorInternal;
using System.Linq;
using Unity.VisualScripting.Antlr3.Runtime.Misc;

public enum RabbitState
{
    MakingBurrow,
    DiggingBurrow,

    Wandering,
    Running,
    Hiding,

    Eating,
    Drinking,
    GoingToEat,
    GoingToDrink,

    GoingToSleep,
    Sleeping,
}

public class Rabbit : MonoBehaviour
{
    public Burrow homeBurrow;
    public bool isDead = false;

    [Header("References")]
    public Stats stats;
    public RabbitState currentState = RabbitState.Wandering;
    public GameObject burrowPrefab;

    [Header("Eat Settings")]
    public float needsInterval = 1f;
    public float foodEatPerSecond = 2f;
    public float drinkPerSecond = 4f;
    public float lookWhileEatingInterval = 5f;
    public float lookAngleMin = 30f;
    public float lookAngleMax = 90f;

    [Header("Hiding Settings")]
    public float burrowThreatDetectProximity = 12f;
    public float waitBeforeLeavingBurrow = 5f;

    [Header("Detect Settings")]
    public float detectionDistance = 6f;
    public float detectionAngle = 90f;

    [Header("Move Settings")]
    public float baseSpeed = 1.5f;
    public float runSpeed = 3f;
    public float wanderDistanceMin = 2f;
    public float wanderDistanceMax = 5f;
    public float runDistance = 6f;
    public float wanderInterval = 4f;

    private float needsTimer = 0f;
    private NavMeshAgent agent;
    private Transform targetFood;
    private Transform targetWater;
    private Transform targetBurrow;
    private Transform detectedWolf;
    private float nextLookTime;
    private Coroutine eatingRoutine;
    private float wanderTimer = 0f;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        stats = GetComponent<Stats>();

        stats.AssignRandomPersonalities();
        ApplyPersonality();
        stats.SetStats();
    }

    void Update()
    {
        if (isDead) return;

        //NEEDS DEPLETION
        if (needsTimer <= needsInterval) needsTimer += Time.deltaTime;
        if (needsTimer >= needsInterval && currentState != RabbitState.Sleeping)
        {
            needsTimer = 0f;
            DepleteNeeds();
        }

        DetectThreats(); // Always check for threats first
        if (currentState == RabbitState.Running)
        {
            RunAway();
            return; // Stop other actions if running
        }

        //Speed
        agent.speed = (currentState == RabbitState.Running) ? runSpeed : baseSpeed;

        //Sleeping
        if (DayNightManager.Instance.isNight)
        {
            GoToSleep();
            return; //stop all activity below
        }

        switch (currentState)
        {
            case RabbitState.Wandering:
                if (wanderTimer < wanderInterval || !agent.pathPending || agent.remainingDistance <= 0.5f) wanderTimer += Time.deltaTime;
                else if (wanderTimer >= wanderInterval) Wander();

                if (isFoodCritical()) DetectFood();
                if (isThirstCritical()) DetectDrink();

                if (detectedWolf == null && homeBurrow == null && currentState != RabbitState.MakingBurrow)
                {
                    MakeBurrow();
                }
                break;
            case RabbitState.GoingToEat:
            case RabbitState.GoingToDrink:
                CheckArrival();
                break;
            case RabbitState.Eating:
            case RabbitState.Drinking:
                LookAroundWhileEatingOrDrinking();
                break;
            case RabbitState.Running:
                RunAway();
                break;
        }
    }

    void DepleteNeeds()
    {
        stats.hunger -= stats.hungerDepletionRate;
        stats.thirst -= stats.thirstDepletionRate;

        if (stats.hunger <= 0 || stats.thirst <= 0)
        {
            stats.health -= 0.5f;
            Debug.Log("Ouch!");
        }
            
        if (stats.health <= 0)
            Die();
    }

    public void Die()
    {
        isDead = true;
        Debug.Log("Dieded");
    }
    bool isFoodCritical()
    {
        if (stats.hunger <= stats.maxHunger * (80f/100f))
        {
            return true;
        }
        else return false;
    }
    bool isThirstCritical()
    {
        if (stats.thirst <= stats.maxThirst * (80f / 100f))
        {
            return true;
        }
        else return false;
    }
    bool isHealthCritical()
    {
        if (stats.health <= stats.maxHealth / 2)
        {
            return true;
        }
        else return false;
    }

    void Wander()
    {
        if (currentState != RabbitState.Wandering || agent.pathPending || agent.remainingDistance > 0.5f)
            return;

        Vector3 randomDirection = Random.insideUnitSphere * Random.Range(wanderDistanceMin, wanderDistanceMax);
        randomDirection += transform.position;
        randomDirection = AdjustPositionToLand(randomDirection);

        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, Random.Range(wanderDistanceMin, wanderDistanceMax), NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);

        }
    }
    void ApplyPersonality()
    {
        foreach (Personality personality in stats.personalities)
        {
            switch (personality.name)
            {
                case "Lazy":
                    wanderDistanceMax -= 1.5f;
                    break;
                case "Restful":
                    wanderInterval += 2.5f;
                    break;
                case "Restless":
                    waitBeforeLeavingBurrow -= 1.5f;
                    break;
                case "Adventurer":
                    wanderDistanceMax += 2.5f;
                    break;

                case "Energetic":
                    stats.hungerDepletionRate += 0.05f;
                    baseSpeed += 0.5f;
                    runSpeed += 0.5f;
                    break;
                case "Sluggish":
                    stats.hungerDepletionRate -= 0.05f;
                    baseSpeed -= 0.5f;
                    runSpeed -= 0.5f;
                    break;

                case "Thirsty":
                    stats.thirstDepletionRate += 0.05f;
                    break;
                case "Retention":
                    stats.thirstDepletionRate -= 0.05f;
                    break;
                case "IronStomach":
                    stats.hungerDepletionRate -= 0.05f;
                    break;
                case "Hunger":
                    stats.hungerDepletionRate += 0.05f;
                    break;

                case "Gluttonous":
                    stats.maxHunger += 15f;
                    foodEatPerSecond += 2f;
                    baseSpeed -= 0.5f;
                    runSpeed -= 0.5f;
                    break;

                case "Unathletic":
                    baseSpeed -= 0.4f;
                    runSpeed -= 0.4f;
                    break;
                case "Athletic":
                    baseSpeed += 0.4f;
                    runSpeed += 0.4f;
                    break;
                case "Runner":
                    runSpeed += 0.4f;
                    break;
                case "Wanderer":
                    baseSpeed += 0.4f;
                    break;
                case "Snail":
                    baseSpeed -= 0.75f;
                    runSpeed -= 0.75f;
                    break;

                case "Healthy":
                    stats.maxHealth += 10f;
                    break;
                case "Weak":
                    stats.maxHealth -= 10f;
                    break;

                case "Alert":
                    detectionDistance += 0.75f;
                    detectionAngle += 20f;
                    break;
                case "Blurred":
                    detectionDistance -= 0.75f;
                    detectionAngle -= 20f;
                    break;
                case "FarVision":
                    detectionDistance += 1.5f;
                    break;
                case "Shortsight":
                    detectionDistance -= 1.5f;
                    break;

                case "Paranoid":
                    lookWhileEatingInterval -= 1f;
                    lookAngleMax += 10f;
                    lookAngleMin += 10f;
                    break;
                case "Careful":
                    waitBeforeLeavingBurrow += 7.5f;
                    break;

                

                case "FastEater":
                    foodEatPerSecond += 2f;
                    break;
                case "SlowEater":
                    foodEatPerSecond -= 1f;
                    break;
                case "FastDrinker":
                    drinkPerSecond += 2f;
                    break;
                case "SlowDrinker":
                    drinkPerSecond -= 1f;
                    break;

                case "HighMetabolism":
                    needsInterval -= 0.25f;
                    break;
                case "LowMetabolism":
                    needsInterval += 0.25f;
                    break;
            }
        }

        wanderInterval = Mathf.Max(wanderInterval, 2f);
        wanderDistanceMax = Mathf.Max(wanderDistanceMax, 1);

        baseSpeed = Mathf.Max(baseSpeed, 1);
        runSpeed = Mathf.Max(runSpeed, 1);

        stats.maxHealth = Mathf.Max(stats.maxHealth, 5);
        stats.thirstDepletionRate = Mathf.Max(stats.thirstDepletionRate, 0.05f);
        stats.hungerDepletionRate = Mathf.Max(stats.hungerDepletionRate, 0.05f);
        
        foodEatPerSecond = Mathf.Max(foodEatPerSecond, 1);
        drinkPerSecond = Mathf.Max(drinkPerSecond, 1);
        needsInterval = Mathf.Max(needsInterval, 0.5f);

        lookWhileEatingInterval = Mathf.Max(lookWhileEatingInterval, 1);
        waitBeforeLeavingBurrow = Mathf.Max(waitBeforeLeavingBurrow, 1);

        detectionDistance = Mathf.Max(detectionDistance, 4f);
    }

    //BURROW
    void MakeBurrow()
    {
        if (homeBurrow != null) return; // Already has a burrow

        currentState = RabbitState.MakingBurrow;
        Vector3 burrowLocation = transform.position; // Default to current position

        Collider[] foodSources = Physics.OverlapSphere(transform.position, detectionDistance, LayerMask.GetMask("Food"));

        if (foodSources.Length > 0)
        {
            // Find the closest food source
            Transform closestFood = foodSources[0].transform;
            float closestDistance = Vector3.Distance(transform.position, closestFood.position);

            foreach (Collider food in foodSources)
            {
                float distance = Vector3.Distance(transform.position, food.transform.position);
                if (distance < closestDistance)
                {
                    closestFood = food.transform;
                    closestDistance = distance;
                }
            }

            // Set burrow location near the food source
            burrowLocation = closestFood.position + (Random.insideUnitSphere * 2f);
        }
        else
        {
            // Pick a random spot if no food is found
            burrowLocation += Random.insideUnitSphere * 5f;
        }

        burrowLocation = AdjustPositionToLand(burrowLocation);

        // Ensure burrow is on a valid NavMesh position
        NavMeshHit hit;
        if (NavMesh.SamplePosition(burrowLocation, out hit, 2f, NavMesh.AllAreas))
        {
            StartCoroutine(DigBurrow(hit.position));
        }
        else
        {
            Debug.LogWarning("Could not find a valid burrow position!");
            currentState = RabbitState.Wandering; // Avoid softlock
        }
    }
    void GoToSleep()
    {
        if (homeBurrow == null)
        {
            DetectBurrow();
            if (targetBurrow != null)
            {
                homeBurrow = targetBurrow.GetComponent<Burrow>();
            }
            else
            {
                Debug.LogWarning("Failed to find a Burrow to call Home!");
            }
        }
        else if (homeBurrow != null)
        {
            agent.SetDestination(homeBurrow.transform.position);
            currentState = RabbitState.GoingToSleep;
        }
    }
    void DetectBurrow()
    {
        Collider[] burrows = Physics.OverlapSphere(transform.position, detectionDistance, LayerMask.GetMask("Burrow"));
        foreach (Collider burrow in burrows)
        {
            Vector3 directionToBurrow = (burrow.transform.position - transform.position).normalized;
            if (Vector3.Angle(transform.forward, directionToBurrow) < detectionAngle / 2)
            {
                targetBurrow = burrow.transform;
            }
        }
    }
    IEnumerator DigBurrow(Vector3 position)
    {
        agent.SetDestination(position);

        // Wait until the Rabbit reaches the position
        while (Vector3.Distance(transform.position, position) > 0.5f)
        {
            yield return null; // Wait for the next frame
        }

        // Create burrow instantly upon arrival
        GameObject newBurrow = Instantiate(burrowPrefab, position, Quaternion.identity);
        homeBurrow = newBurrow.GetComponent<Burrow>();

        //Should enter burrow after creation
    }
    Vector3 AdjustPositionToLand(Vector3 position)
    {
        RaycastHit hit;
        if (Physics.Raycast(new Vector3(position.x, 10f, position.z), Vector3.down, out hit, Mathf.Infinity, LayerMask.GetMask("Land")))
        {
            position.y = hit.point.y; // Adjust to land surface
        }
        else
        {
            Debug.LogWarning("No land found for burrow placement! Using default Y.");
        }
        return position;
    }

    
    //FOOD + DRINK
    void DetectFood()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, detectionDistance, LayerMask.GetMask("Food"));

        Transform closestFood = null;
        float closestDistance = Mathf.Infinity;

        foreach (Collider hit in hitColliders)
        {
            Vector3 directionToResource = (hit.transform.position - transform.position).normalized;
            if (Vector3.Angle(transform.forward, directionToResource) < detectionAngle / 2)
            {
                FoodSource food = hit.GetComponent<FoodSource>();
                if (food != null && food.foodAvailable >= food.minFoodToEat)
                {
                    float distance = Vector3.Distance(transform.position, hit.transform.position);
                    if (distance < closestDistance)
                    {
                        closestFood = hit.transform;
                        closestDistance = distance;
                    }
                }
            }
        }

        if (closestFood != null)
        {
            targetFood = closestFood;
            agent.SetDestination(targetFood.position);
            currentState = RabbitState.GoingToEat;
        }
    }
    void DetectDrink()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, detectionDistance, LayerMask.GetMask("Drink"));

        Transform closestWater = null;
        float closestDistance = Mathf.Infinity;

        foreach (Collider hit in hitColliders)
        {
            Vector3 directionToResource = (hit.transform.position - transform.position).normalized;
            if (Vector3.Angle(transform.forward, directionToResource) < detectionAngle / 2)
            {
                float distance = Vector3.Distance(transform.position, hit.transform.position);
                if (distance < closestDistance)
                {
                    closestWater = hit.transform;
                    closestDistance = distance;
                }
            }
        }

        if (closestWater != null)
        {
            targetWater = closestWater;
            agent.SetDestination(targetWater.position);
            currentState = RabbitState.GoingToDrink;
        }
    }
    void CheckArrival()
    {
        if (currentState == RabbitState.GoingToEat && targetFood != null && Vector3.Distance(transform.position, targetFood.position) < 1f)
        {
            currentState = RabbitState.Eating;
            StartCoroutine(EatRoutine());
        }
        else if (currentState == RabbitState.GoingToDrink && targetWater != null && Vector3.Distance(transform.position, targetWater.position) < 1f)
        {
            currentState = RabbitState.Drinking;
            StartCoroutine(DrinkRoutine());
        }
    }
    IEnumerator EatRoutine()
    {
        FoodSource foodSource = targetFood?.GetComponent<FoodSource>();

        while (stats.hunger < 100 && currentState == RabbitState.Eating && foodSource != null && foodSource.foodAvailable > 0)
        {
            if (detectedWolf != null)
            {
                currentState = RabbitState.Running;
                foodSource.StopEating(); // Stop eating when interrupted
                yield break;
            }

            float foodConsumed = foodSource.ConsumeFood(foodEatPerSecond * Time.deltaTime);
            stats.hunger += foodConsumed;

            if (foodSource.foodAvailable <= 0)
            {
                foodSource.StopEating();
                break; // Stop eating if food is gone
            }

            yield return null;
        }

        currentState = RabbitState.Wandering;
        foodSource?.StopEating(); // Ensure we reset state
    }
    IEnumerator DrinkRoutine()
    {
        while (stats.thirst < 100 && currentState == RabbitState.Drinking)
        {
            if (detectedWolf != null) // Interrupt drinking if a wolf is detected
            {
                currentState = RabbitState.Running;
                yield break;
            }

            stats.thirst += drinkPerSecond * Time.deltaTime;
            yield return null;
        }

        currentState = RabbitState.Wandering;
    }
    void LookAroundWhileEatingOrDrinking()
    {
        if (detectedWolf != null)
        {
            currentState = RabbitState.Running;
            return;
        }

        if (Time.time >= nextLookTime)
        {
            float randomAngle = Random.Range(lookAngleMin, lookAngleMax);
            Quaternion targetRotation = Quaternion.Euler(0, transform.eulerAngles.y + randomAngle, 0);
            StartCoroutine(SmoothLook(targetRotation));
            nextLookTime = Time.time + lookWhileEatingInterval;
        }
    }
    IEnumerator SmoothLook(Quaternion targetRotation)
    {
        float duration = 0.5f; // Adjust for smoother/faster rotation
        float elapsedTime = 0f;
        Quaternion startRotation = transform.rotation;

        while (elapsedTime < duration)
        {
            transform.rotation = Quaternion.Slerp(startRotation, targetRotation, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.rotation = targetRotation; // Ensure final rotation is correct
        DetectThreats(); // Check for wolves after looking around
    }


    //THREATS
    void DetectThreats()
    {
        Collider[] threats = Physics.OverlapSphere(transform.position, detectionDistance, LayerMask.GetMask("Wolf"));
        detectedWolf = null;

        foreach (Collider threat in threats)
        {
            Vector3 directionToThreat = (threat.transform.position - transform.position).normalized;
            if (Vector3.Angle(transform.forward, directionToThreat) < detectionAngle / 2)
            {
                detectedWolf = threat.transform;
                currentState = RabbitState.Running;
                return;
            }
        }
        detectedWolf = null;
    }
    void RunAway()
    {
        if (detectedWolf == null)
        {
            currentState = RabbitState.Wandering;
            return;
        }

        Vector3 directionToWolf = (detectedWolf.position - transform.position).normalized;
        float distanceToWolf = Vector3.Distance(transform.position, detectedWolf.position);

        // If burrow exists and is safe, run towards it
        if (homeBurrow != null && distanceToWolf > 5f)
        {
            agent.SetDestination(homeBurrow.transform.position);
            return;
        }

        // If no burrow or Wolf is too close, run in the opposite direction
        Vector3 escapeDirection = (transform.position - detectedWolf.position).normalized;
        Vector3 escapeTarget = transform.position + escapeDirection * detectionDistance;

        // Ensure the escape target is valid
        NavMeshHit hit;
        if (NavMesh.SamplePosition(escapeTarget, out hit, 2f, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
        else
        {
            Debug.LogWarning("No valid escape path found!");
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other == null) return;

        Burrow burrowScript = other.GetComponent<Burrow>();
        if (burrowScript != null)
        {
            float time = 0f;

            switch (currentState)
            {
                case RabbitState.GoingToSleep:
                    currentState = RabbitState.Sleeping;
                    time = Random.Range(6.0f, 8.0f); //6-8 hours

                    if (stats.personalities.Exists(p => p.name == "Sleeper"))
                        time += Random.Range(0.5f, 1.5f);
                    if (stats.personalities.Exists(p => p.name == "Active"))
                        time -= Random.Range(0.5f, 1.5f);

                    burrowScript.EnterBurrow(this, time);
                    break;
                case RabbitState.Running:
                    currentState = RabbitState.Hiding;
                    time = waitBeforeLeavingBurrow;

                    burrowScript.EnterBurrow(this, time);
                    break;
                case RabbitState.MakingBurrow:
                    currentState = RabbitState.DiggingBurrow;
                    time = 5f;

                    burrowScript.EnterBurrow(this, time);

                    //bug fix, some other rabbit made the burrow but this one enters,
                    homeBurrow = burrowScript;
                    break;
            }
        }
    }



    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        DrawDetectionCone(transform.position, transform.forward, detectionDistance, detectionAngle);
    }

    void DrawDetectionCone(Vector3 position, Vector3 forward, float distance, float angle)
    {
        int segments = 20;
        Vector3 leftBoundary = Quaternion.Euler(0, -angle / 2, 0) * forward * distance;
        Vector3 rightBoundary = Quaternion.Euler(0, angle / 2, 0) * forward * distance;

        Gizmos.DrawLine(position, position + leftBoundary);
        Gizmos.DrawLine(position, position + rightBoundary);

        for (int i = 0; i < segments; i++)
        {
            float segmentAngle = -angle / 2 + (angle / segments) * i;
            float nextSegmentAngle = -angle / 2 + (angle / segments) * (i + 1);

            Vector3 segmentStart = Quaternion.Euler(0, segmentAngle, 0) * forward * distance;
            Vector3 segmentEnd = Quaternion.Euler(0, nextSegmentAngle, 0) * forward * distance;

            Gizmos.DrawLine(position + segmentStart, position + segmentEnd);
        }
    }
}