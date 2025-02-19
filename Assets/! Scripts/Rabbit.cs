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

    private float needsTimer = 0f;
    private NavMeshAgent agent;
    private Transform targetFood;
    private Transform targetWater;
    private Transform targetBurrow;
    private Transform detectedWolf;
    private float nextLookTime;
    private float wanderTimer = 0f;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        stats = GetComponent<Stats>();

        stats.AssignRandomPersonalities();
        stats.ApplyGenesToStats();
        stats.SetStats();
    }

    void Update()
    {
        if (isDead) return;

        //NEEDS DEPLETION
        if (needsTimer <= stats.needsInterval) needsTimer += Time.deltaTime;
        if (needsTimer >= stats.needsInterval && currentState != RabbitState.Sleeping)
        {
            needsTimer = 0f;
            DepleteNeeds();
        }

        //Threats
        DetectThreats(); // Always check for threats first
        if (currentState == RabbitState.Running)
        {
            RunAway();
            return; // Stop other actions if running
        }

        //Speed
        agent.speed = (currentState == RabbitState.Running) ? stats.runSpeed : stats.baseSpeed;

        //Sleeping
        if (DayNightManager.Instance.isNight)
        {
            GoToSleep();
            return; //stop all activity below
        }

        switch (currentState)
        {
            case RabbitState.Wandering:
                if (wanderTimer < stats.wanderInterval && !agent.pathPending) wanderTimer += Time.deltaTime;
                else if (wanderTimer >= stats.wanderInterval) Wander();

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
        
        //Health Manager
        if (stats.health <= 0)
            Die();
        else if (!isDead && stats.health < stats.maxHealth
            && !isFoodCritical() && !isThirstCritical())
        {
            Regenerate();
        }
    }

    public void Die()
    {
        isDead = true;
        Debug.Log("Dieded");
    }
    public void Regenerate()
    {
        stats.health = Mathf.Min(stats.health + stats.regenAmount, stats.maxHealth);
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

        wanderTimer = 0f;

        Vector3 randomDirection = Random.insideUnitSphere * Random.Range(stats.wanderDistanceMin, stats.wanderDistanceMax);
        randomDirection += transform.position;
        randomDirection = AdjustPositionToLand(randomDirection);

        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, Random.Range(stats.wanderDistanceMin, stats.wanderDistanceMax), NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);

        }
    }

    //BURROW
    void MakeBurrow()
    {
        if (homeBurrow != null) return; // Already has a burrow

        currentState = RabbitState.MakingBurrow;
        Vector3 burrowLocation = transform.position; // Default to current position

        Collider[] foodSources = Physics.OverlapSphere(transform.position, stats.detectionDistance, LayerMask.GetMask("Food"));

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
        Collider[] burrows = Physics.OverlapSphere(transform.position, stats.detectionDistance, LayerMask.GetMask("Burrow"));
        foreach (Collider burrow in burrows)
        {
            Vector3 directionToBurrow = (burrow.transform.position - transform.position).normalized;
            if (Vector3.Angle(transform.forward, directionToBurrow) < stats.detectionAngle / 2)
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
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, stats.detectionDistance, LayerMask.GetMask("Food"));

        Transform closestFood = null;
        float closestDistance = Mathf.Infinity;

        foreach (Collider hit in hitColliders)
        {
            Vector3 directionToResource = (hit.transform.position - transform.position).normalized;
            if (Vector3.Angle(transform.forward, directionToResource) < stats.detectionAngle / 2)
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
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, stats.detectionDistance, LayerMask.GetMask("Drink"));

        Transform closestWater = null;
        float closestDistance = Mathf.Infinity;

        foreach (Collider hit in hitColliders)
        {
            Vector3 directionToResource = (hit.transform.position - transform.position).normalized;
            if (Vector3.Angle(transform.forward, directionToResource) < stats.detectionAngle / 2)
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

            float foodConsumed = foodSource.ConsumeFood(stats.foodEatPerSecond * Time.deltaTime);
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

            stats.thirst += stats.drinkPerSecond * Time.deltaTime;
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
            float randomAngle = Random.Range(stats.lookAngleMin, stats.lookAngleMax);
            Quaternion targetRotation = Quaternion.Euler(0, transform.eulerAngles.y + randomAngle, 0);
            StartCoroutine(SmoothLook(targetRotation));
            nextLookTime = Time.time + stats.lookWhileEatingInterval;
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
        Collider[] threats = Physics.OverlapSphere(transform.position, stats.detectionDistance, LayerMask.GetMask("Wolf"));
        detectedWolf = null;

        foreach (Collider threat in threats)
        {
            Vector3 directionToThreat = (threat.transform.position - transform.position).normalized;
            if (Vector3.Angle(transform.forward, directionToThreat) < stats.detectionAngle / 2)
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
        Vector3 escapeTarget = transform.position + escapeDirection * stats.detectionDistance;

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
                    time = Random.Range(6.0f, 8.0f) * 60f + stats.additionalSleepHours; //6-8 hours + extra sleep hours

                    burrowScript.EnterBurrow(this, time);
                    break;
                case RabbitState.Running:
                    currentState = RabbitState.Hiding;
                    time = stats.waitBeforeLeavingBurrow;

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
        DrawDetectionCone(transform.position, transform.forward, stats.detectionDistance, stats.detectionAngle); 
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