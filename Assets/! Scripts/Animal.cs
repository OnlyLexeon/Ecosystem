using UnityEngine.AI;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using TMPro;

public enum AnimalState
{
    //Rabbit
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

    GoingToMate,
    Mating,
}

public enum AnimalType
{
    Rabbit,
    Wolf,
}

public class Animal : MonoBehaviour
{
    [Header("Animal Settings* (!Set This!)")]
    public List<FoodType> foodTypeEdible;
    public AnimalType animalType;
    public List<LayerMask> predators;

    [Header("References* (Ensure none empty)")]
    [Tooltip("Animal's Canvas (For OverHeadStats Toggle)")] public GameObject statsHUD;
    [Tooltip("Animal's Gene Display Prefab (For OverHeadStats Toggle)")] public GameObject genePrefab;

    [Header("Current")]
    public string animalName;
    public AnimalState currentState = AnimalState.Wandering;
    public bool isDead = false;
    public bool wantsToReproduce = false;
    public bool isAdult = false;
    public bool isOld = false;
    public bool isDying = false;

    [Header("References (Auto)")]
    [Tooltip("This animal's Collider component.")] public Collider animalCollider;
    [Tooltip("This animal's stat script.")] public Stats stats;
    public Home home;

    [Header("Family References")]
    public List<GameObject> children;
    public GameObject father;
    public GameObject mother;

    [Header("Rabbit Only")]
    [Tooltip("Determines the Rabbit's fur color.")] public RabbitTypes rabbitType;
    [Tooltip("Reference to 'Model' GameObject in Rabbit GameObject's children in hierarchy.")] public Transform modelHolder;

    [Header("Stats HUD")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI actionText;
    public TextMeshProUGUI ageText;
    public TextMeshProUGUI genderText;
    public Transform genesPanel;
    public Slider healthSlider;
    public Slider hungerSlider;
    public Slider thirstSlider;
    public TextMeshProUGUI healthValueText;
    public TextMeshProUGUI hungerValueText;
    public TextMeshProUGUI thirstValueText;

    private float needsTimer = 0f;
    private NavMeshAgent agent;
    private Transform targetFood;
    private Transform targetWater;
    private Transform targetBurrow;
    private Transform targetMate;
    private Transform detectedPredator;
    private float nextLookTime;
    private float wanderTimer = 0f;
    private Vector3 randomDirection; // Cached to avoid new allocation each frame
    private NavMeshHit navHit;
    private RaycastHit groundHit;
    private float timeSlept = 0f;
    private bool takenDamage = false;
    public bool wasSpawnedByUser = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        if (!stats) stats = GetComponentInChildren<Stats>();
        if (!animalCollider) animalCollider = GetComponentInChildren<Collider>();

        //NAME APPLY
        if (string.IsNullOrEmpty(animalName)) animalName = AnimalNameGet.GetRandomCuteName();

        //might be spawned as adult
        if (stats.agedDays >= stats.adultDays) isAdult = true;

        //SPAWNING UP DEBG STATS
        DoSpawnAddNumberToStats();

        //Assumes the rabbit was spawned, not natural
        //no genes set, age not 0, 
        if (stats.genes.Count <= 0 && stats.agedDays > 0 && !wasSpawnedByUser)
        {
            Debug.Log("Assigned Genes!");

            stats.AssignRandomPersonalities();

            //Apply Model Skin
            rabbitType = (RabbitTypes)Random.Range(0, System.Enum.GetValues(typeof(RabbitTypes)).Length);
            SetAnimalSkinModel();
        }

        //APPLY STATS + GENES
        stats.ApplyGenesToStats();
        stats.SetStats();

        //SET AGE TIME
        CalculateDeathTime();

        //Start Updating UI
        InvokeRepeating(nameof(UpdateOverHeadStats), 2f, 1f);
        //UI that doesnt require constant updating
        UpdateOverHeadUI();
    }

    void Update()
    {
        if (isDead) return;

        //NEEDS DEPLETION
        if (needsTimer <= stats.needsInterval) needsTimer += Time.deltaTime;
        if (needsTimer >= stats.needsInterval && currentState != AnimalState.Sleeping)
        {
            needsTimer = 0f;
            DepleteNeeds();
            if (isOld) CheckOldAgeDeath();
        }

        //Threats
        DetectThreats(); // Always check for threats first
        if (currentState == AnimalState.Running)
        {
            RunAway();
            return; // Stop other actions if running
        }

        //Speed
        agent.speed = (currentState == AnimalState.Running) ? stats.runSpeed : stats.baseSpeed;

        //Sleeping
        if (DayNightManager.Instance.isNight)
        {
            //BUG FIX: rabbits sleeping twice in a night
            //Add cooldown for sleeping: 13 hours
            if ((Time.time - timeSlept) >= (13f * 60f))
            {
                GoToSleep();
                return; //stop all activity below
            }
            
        }

        switch (currentState)
        {
            case AnimalState.Wandering:
                if (wanderTimer < stats.wanderInterval && !agent.pathPending) wanderTimer += Time.deltaTime;
                else if (wanderTimer >= stats.wanderInterval) Wander();

                if (isFoodCritical()) DetectFood();
                if (isThirstCritical()) DetectDrink();
                
                //Make Burrow
                if (detectedPredator == null && home == null && currentState != AnimalState.MakingBurrow)
                {
                    MakeBurrow();
                }
                //Find Mates
                if (detectedPredator == null && stats.fertile == 1 && wantsToReproduce && !isThirstCritical() && !isFoodCritical() && !isHealthCritical() && !targetMate)
                {
                    DetectMate();
                }
                break;
            case AnimalState.GoingToEat:
            case AnimalState.GoingToDrink:
                CheckArrival();
                break;
            case AnimalState.Eating:
            case AnimalState.Drinking:
                LookAroundWhileEatingOrDrinking();
                break;
            case AnimalState.Running:
                RunAway();
                break;
        }
    }

    //Survival
    void DepleteNeeds()
    {
        stats.hunger -= stats.hungerDepletionRate;
        stats.thirst -= stats.thirstDepletionRate;

        //cap them
        stats.hunger = Mathf.Max(stats.hunger, 0);
        stats.thirst = Mathf.Max(stats.thirst, 0);

        if (stats.hunger <= 0)
        {
            stats.health -= 0.2f;

            if (!takenDamage)
            {
                takenDamage = true;

                //History
                string eventString = $"Day {DayNightManager.Instance.dayNumber}, {DayNightManager.Instance.GetTimeString()}\n" +
                    $"{animalName} - {animalType} ({rabbitType}) has taken damage by Hunger!";
                UIManager.Instance.AddNewHistory(eventString, () => InputHandler.Instance.SetTargetAndFollow(transform));
            }
        }
        if (stats.thirst <= 0)
        {
            stats.health -= 0.25f;

            if (!takenDamage)
            {
                takenDamage = true;

                //History
                string eventString = $"Day {DayNightManager.Instance.dayNumber}, {DayNightManager.Instance.GetTimeString()}\n" +
                    $"{animalName} - {animalType} ({rabbitType}) has taken damage by Thirst!";
                UIManager.Instance.AddNewHistory(eventString, () => InputHandler.Instance.SetTargetAndFollow(transform));
            }
        }

        //Health Manager
        if (stats.health <= 0)
            Die();
        else if (!isDead && !isDying
            && stats.health < stats.maxHealth
            && !isFoodCritical() && !isThirstCritical())
        {
            takenDamage = false;
            Regenerate();
        }
    }
    public void Die()
    {
        isDead = true;

        //DESPAWN/DIE
        DoDieMinusNumberFromStats();

        //History
        string eventString = $"Day {DayNightManager.Instance.dayNumber}, {DayNightManager.Instance.GetTimeString()}\n" +
            $"{animalName} - {animalType} ({rabbitType}) has died!";
        UIManager.Instance.AddNewHistory(eventString, () => InputHandler.Instance.SetTargetAndFollow(transform));

        //Disable Collider
        animalCollider.enabled = false;
        //Hide model
        modelHolder.gameObject.SetActive(false);

        //Spawm a corpse
        //Rabbit corpse will be food source for Fox
        //Do not despawn this script object, required for child button teleporting

        Debug.Log("Dieded");
    }
    public void Regenerate()
    {
        stats.health = Mathf.Min(stats.health + stats.regenAmount, stats.maxHealth);
    }
    public bool isFoodCritical()
    {
        if (stats.hunger <= stats.maxHunger * (80f/100f))
        {
            return true;
        }
        else return false;
    }
    public bool isThirstCritical()
    {
        if (stats.thirst <= stats.maxThirst * (80f / 100f))
        {
            return true;
        }
        else return false;
    }
    public bool isHealthCritical()
    {
        if (stats.health <= stats.maxHealth / 2)
        {
            return true;
        }
        else return false;
    }
    

    //BEHAVIOR DEFAULT
    void Wander()
    {
        if (currentState != AnimalState.Wandering || agent.pathPending || agent.remainingDistance > 0.5f)
            return;

        wanderTimer = 0f;

        // Get random position
        randomDirection = transform.position + (Random.insideUnitSphere * Random.Range(stats.wanderDistanceMin, stats.wanderDistanceMax));

        // Adjust to land
        randomDirection = AdjustPositionToLand(randomDirection);

        // Check NavMesh
        if (NavMesh.SamplePosition(randomDirection, out navHit, stats.wanderDistanceMax, NavMesh.AllAreas))
        {
            agent.SetDestination(navHit.position);
        }
    }

    //AGING
    public void WakeUpAgeUpdate()
    {
        stats.agedDays++;

        if (!isOld && stats.agedDays >= stats.deathDays)
        {
            isOld = true;
        }

        if (!isAdult)
        {
            ScaleChild();
        }
    }
    public void ScaleChild()
    {
        //scale
        float scaleFactor = Mathf.Lerp(0.15f, 0.4f, stats.agedDays / (float)stats.adultDays);
        transform.localScale = new Vector3(scaleFactor, scaleFactor, scaleFactor);

        if (stats.agedDays >= stats.adultDays) isAdult = true;
    }
    public void CalculateDeathTime()
    {
        stats.deathTime = Random.Range(stats.minDeathTime, stats.maxDeathTime);
    }
    public void CheckOldAgeDeath()
    {
        if (!isDying && stats.deathTime >= DayNightManager.Instance.time)
        {
            isDying = true;
        }

        if (isDying)
        {
            stats.health -= 0.5f;
            Debug.Log("Ouch!");
        }
    }

    //SEGGS
    public void WakeUpCheckHorniness()
    {
        if (isAdult)
        {
            //called by burrow
            stats.reproduceDaysLeft -= 1;

            if (stats.reproduceDaysLeft <= 0)
            {
                wantsToReproduce = true;
            }
            else
            {
                wantsToReproduce = false;
            }
        }
    }
    void DetectMate()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, stats.detectionDistance, 1 << gameObject.layer); //same species

        Transform closestMate = null;
        float closestDistance = Mathf.Infinity;

        foreach (Collider hit in hitColliders)
        {
            Vector3 directionToMate = (hit.transform.position - transform.position).normalized;
            //check in angle
            if (Vector3.Angle(transform.forward, directionToMate) < stats.detectionAngle / 2)
            {
                //checking part
                Animal targetScript = hit.GetComponent<Animal>();
                if (targetScript != null)
                {
                    //is horny & no target yet
                    if (targetScript.wantsToReproduce && !targetScript.targetMate) 
                    {
                        //check gender
                        if (CheckGender(targetScript))
                        {
                            //lastly, check if suitable genetics (more complex so checks it last)
                            if (CheckPreferedGenes(targetScript))
                            {
                                //mark as closest
                                float distance = Vector3.Distance(transform.position, hit.transform.position);
                                if (distance < closestDistance)
                                {
                                    closestMate = hit.transform;
                                    closestDistance = distance;
                                }
                            }
                        }
                    }
                }
            }
        }

        if (closestMate != null)
        {
            targetMate = closestMate;
            Animal mateScript = closestMate.GetComponent<Animal>();
            
            if (mateScript != null)
            {
                bool mateFound = mateScript.SignalMating(transform, home, this);

                if (mateFound)
                {
                    currentState = AnimalState.GoingToMate;
                    agent.SetDestination(home.transform.position);
                }
                else
                {
                    targetMate = null;
                }
            }
        }
    }
    public bool CheckGender(Animal targetScript)
    {
        //Check Gender
        bool oppositeGender = false;
        switch (stats.gender)
        {
            case Gender.Male:
                if (targetScript.stats.gender == Gender.Female) oppositeGender = true;
                break;
            case Gender.Female:
                if (targetScript.stats.gender == Gender.Male) oppositeGender = true;
                break;
        }

        return oppositeGender;
    }
    public bool CheckPreferedGenes(Animal targetScript)
    {
        List<Genes> targetGenes = targetScript.stats.genes;
        // Count positive and negative genes
        int positiveGeneCount = targetGenes.Count(g => g.positivity == Positivity.Positive || g.positivity == Positivity.ExtremelyPositive);
        int negativeGeneCount = targetGenes.Count(g => g.positivity == Positivity.Negative || g.positivity == Positivity.ExtremelyNegative);

        // Check conditions
        bool meetsPositiveRequirement = positiveGeneCount >= stats.minPositiveGenesPrefered;
        bool meetsNegativeRequirement = negativeGeneCount <= stats.maxNegativeGenesPrefered;

        if (meetsPositiveRequirement && meetsNegativeRequirement)
            return true;

        return false;
    }
    public bool SignalMating(Transform mate, Home homeToMate, Animal mateScript)
    {
        if (wantsToReproduce && !targetMate)
        {
            //check if signaller rabbit is prefered
            bool prefered = CheckPreferedGenes(mateScript);
            if (prefered)
            {
                targetMate = mate;
                targetBurrow = homeToMate.transform;
                currentState = AnimalState.GoingToMate;
                agent.SetDestination(homeToMate.transform.position);
                return true;
            }
            else return false;
        }
        else
        {
            return false;
        }
    }

    //Only called by the female
    //GENETIC ALGORITHM!!
    public object DetermineFurInheritance(Animal mateScript)
    {
        object motherFur = GetAnimalSpecies();
        object fatherFur = mateScript.GetAnimalSpecies();

        //Determine WINNER
        float motherDominance = stats.furDominance;
        float fatherDominance = mateScript.stats.furDominance;

        // Calculate total dominance for probability scaling
        float totalDominance = motherDominance + fatherDominance;

        // Generate a random float between 0 and totalDominance
        float randomValue = UnityEngine.Random.Range(0f, totalDominance);

        // Pick based on weighted probability
        return randomValue < motherDominance ? motherFur : fatherFur;
    }
    public int DetermineOffSpringCount(Animal mateScript)
    {
        // Determine WINNER (Higher seedDominance decides)
        bool isMom = stats.seedDominance >= mateScript.stats.seedDominance;

        // Calculating Total Offspring
        int baseOffspringCount = isMom ? stats.baseOffSpringCount : mateScript.stats.baseOffSpringCount;
        int additionalOffspring = isMom ? Random.Range(0, stats.maxAdditionalOffSpring) : Random.Range(0, mateScript.stats.maxAdditionalOffSpring);

        return baseOffspringCount + additionalOffspring;
    }
    public List<Genes> DetermineMutation(Animal mateScript, List<Genes> childGenes, Animal childScript)
    {
        //Should mutate?
        float mutationChance = Mathf.Clamp(stats.geneMutationChance + mateScript.stats.geneMutationChance, 0, 100);
        bool mutationHappens = Random.Range(0f, 100f) < mutationChance;

        if (mutationHappens)
        {
            //Random gene
            Genes mutatedGene = stats.GetARandomGene();

            // Apply mutation
            int randomIndex = Random.Range(0, childGenes.Count);
            Genes tempGene = childGenes[randomIndex];
            childGenes[randomIndex] = mutatedGene;

            //History
            string eventString = $"Day {DayNightManager.Instance.dayNumber}, {DayNightManager.Instance.GetTimeString()}\n" +
                $"{childScript.animalName} - {childScript.animalType} ({childScript.rabbitType}) has mutated genetically with chance {mutationChance}%! Replaced Gene: {tempGene.name} | Mutated Into: {mutatedGene.name}";
            UIManager.Instance.AddNewHistory(eventString, () => InputHandler.Instance.SetTargetAndFollow(childScript.transform));
        }

        return childGenes;
    }
    public List<Genes> GetParentsGenetics()
    {
        List<Genes> parentsGenes = new List<Genes>();

        parentsGenes.AddRange(stats.genes); //female

        Animal mateScript = targetMate.GetComponent<Animal>();
        parentsGenes.AddRange(mateScript.stats.genes); //male

        return parentsGenes;
    }
    public List<Genes> GetChildsGenetics()
    {
        //Genetic Algorithm
        List<Genes> parentsGenes = GetParentsGenetics();
        List<Genes> childGenes = new List<Genes>();
        int numberOfGenesToInherit = Mathf.RoundToInt(parentsGenes.Count / 2f);

        // Weighted List, adds gene multiplied by weight
        List<Genes> weightedGenes = new List<Genes>();
        foreach (Genes gene in parentsGenes)
        {
            int weight = gene.weightage;
            for (int i = 0; i < weight; i++)
            {
                weightedGenes.Add(gene);
            }
        }

        // Random shuffle after weightage
        weightedGenes = weightedGenes.OrderBy(g => Random.value).ToList();

        // Pick genes from the shuffled list
        foreach (Genes gene in weightedGenes)
        {
            if (childGenes.Count >= numberOfGenesToInherit)
                break; // Stop once we reach the required amount

            if (!childGenes.Contains(gene))
            {
                childGenes.Add(gene);
            }
        }

        return childGenes;
    }
    public void GiveBirth(Home parentBurrow)
    {
        Animal mateScript = targetMate.GetComponent<Animal>();

        //OFF SPRING COUNT
        int totalOffspring = DetermineOffSpringCount(mateScript);

        //History
        string eventString = $"Day {DayNightManager.Instance.dayNumber}, {DayNightManager.Instance.GetTimeString()}\n" +
            $"{animalName} - {animalType} ({rabbitType}) has given birth to {totalOffspring} children! Father: {mateScript.animalName}";
        UIManager.Instance.AddNewHistory(eventString, () => InputHandler.Instance.SetTargetAndFollow(transform));

        //Spawning Rabbits
        for (int i = 0; i < totalOffspring; i++)
        {
            //spawn child
            GameObject child = Instantiate(GetChildPrefabBirth(), transform.position, Quaternion.identity, WorldStats.Instance.transform);
            Animal childScript = child.GetComponent<Animal>();

            //INITIALIZE CHILD DEFAULTS
            childScript.animalName = AnimalNameGet.GetRandomCuteName();
            //Set Home
            childScript.home = parentBurrow;
            //Set Size
            childScript.stats.agedDays = 0;
            childScript.isAdult = false;
            childScript.timeSlept = timeSlept;
            childScript.ScaleChild(); //set size
            //Set Fur Color
            object furColor = DetermineFurInheritance(mateScript); //object - because can be RabbitType, WolfType use object as common var
            childScript.SetAnimalSkinModel(furColor);

            //GENETIC ALGORITHM!!
            //Determine genes
            List<Genes> childGenes = GetChildsGenetics();
            //Mutation
            childGenes = DetermineMutation(mateScript, childGenes, childScript);
            //Apply genes
            childScript.stats.genes.Clear();
            childScript.stats.genes = childGenes;

            //Add child to children list
            children.Add(child);
            mateScript.children.Add(child);

            //Add parent reference to child for debugging
            childScript.father = targetMate.gameObject; //dad was the mate
            childScript.mother = gameObject; //mom is this script
        }

        //reset target mate
        mateScript.targetMate = null;
        targetMate = null; 
    }

    //BURROW
    void MakeBurrow()
    {
        if (home != null) return; // Already has a burrow

        currentState = AnimalState.MakingBurrow;
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
            currentState = AnimalState.Wandering; // Avoid softlock
        }
    }
    void GoToSleep()
    {
        if (home == null)
        {
            DetectBurrow();
            if (targetBurrow != null)
            {
                home = targetBurrow.GetComponent<Home>();
            }
            else
            {
                Debug.LogWarning("Failed to find a Burrow to call Home!");
            }
        }
        else if (home != null)
        {
            agent.SetDestination(home.transform.position);
            currentState = AnimalState.GoingToSleep;
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
        while (Vector3.Distance(transform.position, position) > 1.5f)
        {
            yield return null; // Wait for the next frame
        }

        // Create burrow instantly upon arrival
        GameObject newBurrow = Instantiate(Environment.Instance.burrowPrefab, position, Quaternion.identity);
        home = newBurrow.GetComponent<Home>();

        //Should enter burrow after creation
    }
    Vector3 AdjustPositionToLand(Vector3 position)
    {
        // Reuse RaycastHit instead of creating new
        if (Physics.Raycast(new Vector3(position.x, 10f, position.z), Vector3.down, out groundHit, Mathf.Infinity, LayerMask.GetMask("Land")))
        {
            position.y = groundHit.point.y; // Adjust to land surface
        }

        return position;
    }

    // FOOD + DRINK
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
                if (food != null && food.foodAvailable >= food.minFoodToEat && foodTypeEdible.Contains(food.foodType))
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
            currentState = AnimalState.GoingToEat;
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
            currentState = AnimalState.GoingToDrink;
        }
    }
    void CheckArrival()
    {
        if (currentState == AnimalState.GoingToEat && targetFood != null && Vector3.Distance(transform.position, targetFood.position) <= 1.5f)
        {
            currentState = AnimalState.Eating;
            StartCoroutine(EatRoutine());
        }
        else if (currentState == AnimalState.GoingToDrink && 
            (targetWater != null && Vector3.Distance(transform.position, targetWater.position) <= 1.5f))
        {
            currentState = AnimalState.Drinking;
            StartCoroutine(DrinkRoutine());
        }
    }
    IEnumerator EatRoutine()
    {
        FoodSource foodSource = targetFood?.GetComponent<FoodSource>();

        while (stats.hunger < stats.maxHunger && currentState == AnimalState.Eating && foodSource != null && foodSource.foodAvailable > 0)
        {
            if (detectedPredator != null)
            {
                currentState = AnimalState.Running;
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

        currentState = AnimalState.Wandering;
        foodSource?.StopEating(); // Ensure we reset state
    }
    IEnumerator DrinkRoutine()
    {
        while (stats.thirst < stats.maxThirst && currentState == AnimalState.Drinking)
        {
            if (detectedPredator != null) // Interrupt drinking if a wolf is detected
            {
                currentState = AnimalState.Running;
                yield break;
            }

            stats.thirst += stats.drinkPerSecond * Time.deltaTime;
            yield return null;
        }

        currentState = AnimalState.Wandering;
    }
    void LookAroundWhileEatingOrDrinking()
    {
        if (detectedPredator != null)
        {
            currentState = AnimalState.Running;
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
        int predatorLayerMask = 0;
        foreach (LayerMask mask in predators)
        {
            predatorLayerMask |= mask.value; // Combine all layer masks
        }

        Collider[] threats = Physics.OverlapSphere(transform.position, stats.detectionDistance, predatorLayerMask);
        detectedPredator = null;
        float closestDistance = Mathf.Infinity;

        foreach (Collider threat in threats)
        {
            Vector3 directionToThreat = (threat.transform.position - transform.position).normalized;
            float distanceToThreat = Vector3.Distance(transform.position, threat.transform.position);

            if (Vector3.Angle(transform.forward, directionToThreat) < stats.detectionAngle / 2 && distanceToThreat < closestDistance)
            {
                detectedPredator = threat.transform;
                closestDistance = distanceToThreat;
            }
        }

        if (detectedPredator != null)
        {
            currentState = AnimalState.Running;
        }
    }

    void RunAway()
    {
        if (detectedPredator == null)
        {
            currentState = AnimalState.Wandering;
            return;
        }

        Vector3 directionToThreat = (detectedPredator.position - transform.position).normalized;
        float distanceToThreat = Vector3.Distance(transform.position, detectedPredator.position);

        // If burrow exists and is safe, run towards it
        if (home != null && distanceToThreat > 5f)
        {
            agent.SetDestination(home.transform.position);
            return;
        }

        // If no burrow or Wolf is too close, run in the opposite direction
        Vector3 escapeDirection = (transform.position - detectedPredator.position).normalized;
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

    //COLLISION
    private void OnTriggerStay(Collider other)
    {
        if (other == null) return;

        Home homeScript = other.GetComponent<Home>();
        if (homeScript != null)
        {
            float time = 0f;

            switch (currentState)
            {
                case AnimalState.GoingToSleep:
                    currentState = AnimalState.Sleeping;

                    timeSlept = Time.time;

                    time = (Random.Range(8.5f, 9.5f) * 60f) + (stats.additionalSleepHours * 60f); //6-8 hours + extra sleep hours
                    homeScript.EnterBurrowForSleep(this, time);
                    break;
                case AnimalState.Running:
                    currentState = AnimalState.Hiding;
                    time = stats.waitBeforeLeavingBurrow;

                    homeScript.EnterBurrow(this, time);
                    break;
                case AnimalState.MakingBurrow:
                    currentState = AnimalState.DiggingBurrow;
                    time = 5f;

                    homeScript.EnterBurrow(this, time);

                    //bug fix, some other rabbit made the burrow but this one enters,
                    home = homeScript;
                    break;
                case AnimalState.GoingToMate:
                    if (homeScript.GetComponent<Home>() == targetBurrow)
                    {
                        currentState = AnimalState.Mating;

                        wantsToReproduce = false;
                        stats.reproduceDaysLeft = stats.reproduceCooldownDays;

                        time = 30f;

                        //giving birth is called by burrow after 20 seconds
                        homeScript.EnterBurrowForMating(this, time);
                    }
                    break;
            }
        }

        //Water BUG FIX (water target is diagonal thus cant reach:)
        if (currentState == AnimalState.GoingToDrink && other.gameObject.layer == LayerMask.NameToLayer("Drink"))
        {
            currentState = AnimalState.Drinking;
            StartCoroutine(DrinkRoutine());
        }
    }

    //UI/HUD
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
    public void ToggleUI(bool state)
    {
        statsHUD.SetActive(state);

        //if (state)
        //{
        //    UpdateOverHeadUI();
        //    UpdateOverHeadStats();
        //}
    }
    void UpdateOverHeadUI() //UI updated once only
    {
        statsHUD.SetActive(true);

        //name 
        nameText.text = animalName;

        //Personality
        List<Genes> genes = stats.genes;
        foreach (Genes gene in genes)
        {
            GameObject newPersonality = Instantiate(genePrefab, genesPanel);
            PersonalityButton buttonScript = newPersonality.GetComponent<PersonalityButton>();

            if (buttonScript)
            {
                buttonScript.nameText.text = gene.name;
                buttonScript.descriptionText.text = gene.description;
                buttonScript.positivity = gene.positivity;
            }
        }

        //Gender
        genderText.text = "Gender: " + stats.gender.ToString();

        statsHUD.SetActive(false);
    }
    public void UpdateOverHeadStats() //updated every second
    {
        //OVER HEAD UI/HUD STATS
        if (statsHUD.activeSelf == true && !isDead)
        {
            //Update Stats
            ageText.text = "Age: " + stats.agedDays.ToString();
            actionText.text = "Action: " + currentState.ToString();

            healthSlider.maxValue = stats.maxHealth;
            healthSlider.value = stats.health;
            hungerSlider.maxValue = stats.maxHunger;
            hungerSlider.value = stats.hunger;
            thirstSlider.maxValue = stats.maxThirst;
            thirstSlider.value = stats.thirst;

            healthValueText.text = stats.health.ToString("F2");
            hungerValueText.text = stats.hunger.ToString("F2");
            thirstValueText.text = stats.thirst.ToString("F2");
        }
    }

    //DIFFERENT ANIMALS, DIFFERENT CALCS, BEHAVIOR, FUNCTIONS
    public GameObject GetChildPrefabBirth()
    {
        switch(animalType)
        {
            case AnimalType.Rabbit:
                return Rabbit.Instance.rabbitPrefab;
            default:
                return Rabbit.Instance.rabbitPrefab;
        }
    }
    public object GetAnimalSpecies()
    {
        switch(animalType)
        {
            case AnimalType.Rabbit:
                return rabbitType;
            case AnimalType.Wolf:
                return rabbitType;
            default:
                return null;
        }
    }
    public void DoSpawnAddNumberToStats()
    {
        switch (animalType)
        {
            case AnimalType.Rabbit:
                WorldStats.Instance.PlusRabbitCount();
                break;
            default:
                break;
        }
    }
    public void DoDieMinusNumberFromStats()
    {
        switch (animalType)
        {
            case AnimalType.Rabbit:
                WorldStats.Instance.MinusRabbitCount();
                break;
            default:
                break;
        }
    }
    public void SetAnimalSkinModel() //spawning
    {
        ClearModelHolderChild();

        switch (animalType)
        {
            case AnimalType.Rabbit:
                Instantiate(Rabbit.Instance.GetRabbitModel(rabbitType), modelHolder);
                break;
            default:
                break;
        }
    }
    public void SetAnimalSkinModel(object furType) //give birth
    {
        ClearModelHolderChild();

        switch (animalType)
        {
            case AnimalType.Rabbit:
                if (furType is RabbitTypes rabbitFur)
                {
                    Instantiate(Rabbit.Instance.GetRabbitModel(rabbitFur), modelHolder);
                    rabbitType = rabbitFur;
                }
                else
                    Debug.LogError("Invalid fur type passed to SetAnimalSkinModel!");
                break;
            default:
                break;
        }
    }
    public void ClearModelHolderChild()
    {
        foreach (Transform child in modelHolder)
        {
            Destroy(child.gameObject);
        }
    }
}