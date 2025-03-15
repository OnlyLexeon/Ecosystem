using UnityEngine.AI;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEditor.U2D;
using static TreeEditor.TreeEditorHelper;
using UnityEngine.Rendering;

public enum AnimalState
{
    //Rabbit
    MakingBurrow,
    DiggingBurrow,

    Wandering,
    RunninngAway,
    Hunting,
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

public enum DeathCause
{
    Thirst,
    Starve,
    Eaten,
    OldAge,
}

public enum HomeType
{
    NearFood,
    NearTrees,
}

public class Animal : MonoBehaviour
{
    [Header("Animal Settings* (!!Set This!!)")]

    [Tooltip("Animals this animal will approach to snack on.")] public List<FoodType> foodTypeEdible;
    public AnimalType animalType;
    [Tooltip("Animals this animal will Run Away From)")] public List<LayerMask> predators;
    [Tooltip("Animals this animal will Hunt")]  public List<LayerMask> prey;
    [Tooltip("Where this animal will place their home")] public HomeType homeType;
    public float adultScaleSize = 1f;
    public float newbornScaleSize = 0.5f;

    [Header("References* (Ensure none empty)")]
    [Tooltip("Animal's Canvas (For OverHeadStats Toggle)")] public GameObject statsHUD;
    [Tooltip("Animal's Gene Display Prefab (For OverHeadStats Toggle)")] public GameObject genePrefab;

    [Header("Current Info (Debugging Use)")]
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
    [Tooltip("Reference to 'Model' GameObject in Rabbit GameObject's children in hierarchy.")] public Transform modelHolder;
    [Tooltip("Determine Animal Fur Color")] public FurType furType;

    [Header("Family References (Auto)")]
    public List<GameObject> children;
    public GameObject father;
    public GameObject mother;
    
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

    [Header("Debug Values")]
    public Transform targetFood;
    public Transform targetWater;
    public Vector3 targetHomeLocation;
    public Transform toMateBurrow;
    public Transform targetMate;

    public Animal detectedPredator;
    public Animal detectedPrey;
    public float attackTimer = 0f;

    public float needsTimer = 0f;
    
    public NavMeshAgent agent;

    public float nextLookTime;
    public float wanderTimer = 0f;
    public Vector3 randomDirection; // Cached to avoid new allocation each frame
    public NavMeshHit navHit;
    public RaycastHit groundHit;

    public float timeSlept = 0f;

    public bool takenDamage = false;

    void Start()
    {
        if (!agent) agent = GetComponent<NavMeshAgent>();
        if (!stats) stats = GetComponentInChildren<Stats>();
        if (!animalCollider) animalCollider = GetComponentInChildren<Collider>();
        if (!modelHolder) modelHolder = FindFirstChildWithTag(transform, "Model");

        //NAME APPLY
        if (string.IsNullOrEmpty(animalName)) animalName = AnimalNameGet.GetRandomCuteName();

        //might be spawned as adult
        ScaleChild();
        if (stats.agedDays >= stats.adultDays) isAdult = true;

        //Assumes the rabbit was spawned, not natural
        //no genes set, age not 0, 
        if (stats.genes.Count <= 0)
        {
            Debug.Log("Assigned Genes!");

            stats.AssignRandomPersonalities();

            //Apply Model Skin
            if (animalType.furTypes.Count > 0)
            {
                furType = animalType.furTypes[Random.Range(0, animalType.furTypes.Count)];
                SetAnimalSkinModel();
            }
        }

        //APPLY STATS + GENES
        stats.ApplyGenesToStats();
        stats.SetStats();

        //WORLD STATS
        stats.UpdateWorldStatsGenes(animalType);
        //SPAWNING DEBUG STATS
        DoSpawnAddNumberToStats();
        CheckAnimalGeneration();

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

        if (!agent.isOnNavMesh)
        {
            SnapToNavMesh();
        }

        if (!agent.isOnNavMesh)
        {
            return;
        }

        //NEEDS DEPLETION
        if (needsTimer <= stats.needsInterval) needsTimer += Time.deltaTime;
        if (needsTimer >= stats.needsInterval && currentState != AnimalState.Sleeping)
        {
            needsTimer = 0f;
            DepleteNeeds();
            if (isOld) CheckOldAgeDeath();
        }

        //Threats
        if (currentState == AnimalState.RunninngAway && detectedPredator != null)
        {
            RunAway();
            return; // Stop other actions if running
        }
        else
        {
            DetectPredator(); // Always check for threats first
        }

        //Speed
        agent.speed = (currentState == AnimalState.RunninngAway || currentState == AnimalState.Hunting) ? stats.runSpeed : stats.baseSpeed;

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

                //Find Mates
                if (detectedPredator == null && stats.fertile == 1 && wantsToReproduce && !isThirstCritical() && !isFoodCritical() && !isHealthCritical() && !targetMate && DayNightManager.Instance.isDay)
                {
                    DetectMate();
                }

                if (isThirstCritical()) DetectDrink();
                //Prioritize Food (Runs after thirst)
                if (isFoodCritical())
                {
                    if (!DetectFood()) DetectPrey();
                }

                //Make Burrow
                if (detectedPredator == null && (home == null || Vector3.Distance(home.transform.position, transform.position) >= stats.newBurrowDistance) 
                    && currentState != AnimalState.MakingBurrow)
                {
                    if (FindPlaceToMakeHome(homeType))
                    {
                        MakeBurrow();
                    }
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
            case AnimalState.RunninngAway:
                RunAway();
                break;
            case AnimalState.Hunting:
                Hunt();
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

            //Check Death
            if (stats.health <= 0) Die(DeathCause.Starve);

            if (!takenDamage)
            {
                takenDamage = true;

                //History
                string eventString = $"Day {DayNightManager.Instance.dayNumber}, {DayNightManager.Instance.GetTimeString()}\n" +
                    $"{animalName} - {animalType.animalName.ToString()} ({furType.furName.ToString()}) has taken damage by Hunger!";
                UIManager.Instance.AddNewHistory(eventString, () => InputHandler.Instance.SetTargetAndFollow(transform));
            }
        }
        if (stats.thirst <= 0)
        {
            stats.health -= 0.25f;

            //Check Death
            if (stats.health <= 0) Die(DeathCause.Thirst);

            if (!takenDamage)
            {
                takenDamage = true;

                //History
                string eventString = $"Day {DayNightManager.Instance.dayNumber}, {DayNightManager.Instance.GetTimeString()}\n" +
                    $"{animalName} - {animalType.animalName.ToString()} ({furType.furName.ToString()}) has taken damage by Thirst!";
                UIManager.Instance.AddNewHistory(eventString, () => InputHandler.Instance.SetTargetAndFollow(transform));
            }
        }

        if (!isDead && !isDying
            && stats.health < stats.maxHealth
            && !isFoodCritical() && !isThirstCritical())
        {
            takenDamage = false;
            Regenerate();
        }
    }
    public void Die(DeathCause cause)
    {
        isDead = true;

        //Agent
        agent.isStopped = true;
        agent.enabled = false;

        //DESPAWN/DIE
        DoDieMinusNumberFromStats();
        WorldStats.Instance.UpdateGeneStats(animalType, -stats.GetPositiveGenesCount(), -stats.GetNegativeGenesCount(), -stats.GetNeutralGenesCount());

        //History
        string causeOfDeath = "";
        switch(cause)
        {
            case DeathCause.Thirst: causeOfDeath = "Thirst"; break;
            case DeathCause.Starve: causeOfDeath = "Starvation"; break;
            case DeathCause.OldAge: causeOfDeath = "Old Age"; break;
            case DeathCause.Eaten: causeOfDeath = "Being Eaten"; break;
            default: causeOfDeath = "Unknown"; break;
        }
        string eventString = $"Day {DayNightManager.Instance.dayNumber}, {DayNightManager.Instance.GetTimeString()}\n" +
            $"{animalName} - {animalType.animalName.ToString()} ({furType.furName.ToString()}) has died from: " + causeOfDeath + "!";
        UIManager.Instance.AddNewHistory(eventString, () => InputHandler.Instance.SetTargetAndFollow(transform), HistoryType.Death);

        //Disable Collider
        animalCollider.enabled = false;
        //Hide model
        modelHolder.gameObject.SetActive(false);

        //Spawm a corpse
        //Rabbit corpse will be food source for Fox
        //Do not despawn this script object, required for child button teleporting
        GameObject corpseToSpawn = Instantiate(animalType.animalDeadPrefab, transform.position, Quaternion.identity);
        FoodSource corpseScript = corpseToSpawn.GetComponent<FoodSource>();
        //MODEL SIZE
        corpseToSpawn.transform.localScale = transform.localScale;
        if (corpseScript)
        {
            if (corpseScript.convertHungerToFoodAvailable)
            {
                corpseScript.maxFood += stats.hunger / corpseScript.divideHungerBy;
                corpseScript.foodAvailable = corpseScript.maxFood;
            }
        }
        else Debug.LogWarning("This Dead Animal is not a food source! Add Food Source Script!");

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

        // Check NavMesh
        if (NavMesh.SamplePosition(randomDirection, out navHit, stats.wanderDistanceMax, NavMesh.AllAreas))
        {
            agent.SetDestination(navHit.position);
        }
    }

    //AGING
    public void UpdateAge()
    {
        if (isDead) return;

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
        float scaleFactor = Mathf.Lerp(newbornScaleSize, adultScaleSize, stats.agedDays / (float)stats.adultDays);
        transform.localScale = new Vector3(scaleFactor, scaleFactor, scaleFactor);

        if (stats.agedDays >= stats.adultDays) isAdult = true;
    }
    public void CalculateDeathTime()
    {
        stats.deathTime = Random.Range(stats.minDeathTime, stats.maxDeathTime);
    }
    public void CheckOldAgeDeath()
    {
        if (!isDying && stats.deathTime >= DayNightManager.Instance.time && DayNightManager.Instance.isDay)
        {
            isDying = true;

            //History
            string eventString = $"Day {DayNightManager.Instance.dayNumber}, {DayNightManager.Instance.GetTimeString()}\n" +
            $"{animalName} - {animalType.animalName.ToString()} ({furType.furName.ToString()}) has started decaying from Old Age!";
            UIManager.Instance.AddNewHistory(eventString, () => InputHandler.Instance.SetTargetAndFollow(transform));
        }

        if (isDying)
        {
            stats.health -= 0.5f;

            //Check Death
            if (stats.health <= 0) Die(DeathCause.OldAge);
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
                    toMateBurrow = home.transform;
                    agent.SetDestination(toMateBurrow.position);
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
        // Count positive and negative genes
        int positiveGeneCount = targetScript.stats.GetPositiveGenesCount();
        int negativeGeneCount = targetScript.stats.GetNegativeGenesCount();

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
                toMateBurrow = homeToMate.transform;
                currentState = AnimalState.GoingToMate;
                agent.SetDestination(toMateBurrow.position);
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
    public FurType DetermineFurInheritance(Animal mateScript)
    {
        FurType motherFur = GetAnimalSpecies();
        FurType fatherFur = mateScript.GetAnimalSpecies();

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
                $"{childScript.animalName} - {childScript.animalType.animalName.ToString()} ({childScript.furType.furName.ToString()}) has mutated genetically with chance {mutationChance}%! Replaced Gene: {tempGene.name} | Mutated Into: {mutatedGene.name}";
            UIManager.Instance.AddNewHistory(eventString, () => InputHandler.Instance.SetTargetAndFollow(childScript.transform), HistoryType.Mutation);
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
            $"{animalName} - {animalType.animalName.ToString()} ({furType.furName.ToString()}) has given birth to {totalOffspring} children! Father: {mateScript.animalName} | Generation: {stats.generation + 1}";
        UIManager.Instance.AddNewHistory(eventString, () => InputHandler.Instance.SetTargetAndFollow(transform), HistoryType.Birth);

        //Spawning Rabbits
        for (int i = 0; i < totalOffspring; i++)
        {
            //spawn child
            GameObject child = Instantiate(GetChildPrefabBirth(), transform.position, Quaternion.identity, AnimalContainer.Instance.transform);
            Animal childScript = child.GetComponent<Animal>();

            //INITIALIZE CHILD DEFAULTS
            childScript.animalName = AnimalNameGet.GetRandomCuteName();
            //Set Home
            childScript.home = parentBurrow;
            //Init
            childScript.stats.agedDays = 0;
            childScript.stats.generation = stats.generation + 1; // child is mom's generation plus 1
            childScript.isAdult = false;
            childScript.timeSlept = timeSlept;
            //Set Fur Color
            FurType furColor = DetermineFurInheritance(mateScript); //object - because can be RabbitType, WolfType use object as common var
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

            //WORLD STATS
            //Check if generation has increased!
            //CheckAnimalGeneration(); <---- already called in Start()
        }

        //reset target mate
        mateScript.targetMate = null;
        targetMate = null; 
    }

    //BURROW
    bool FindPlaceToMakeHome(HomeType homeType)
    {
        LayerMask layerMask;
        System.Func<Collider, bool> isValidTarget = _ => true; // Default to always true

        switch (homeType)
        {
            case HomeType.NearTrees:
                layerMask = LayerMask.GetMask("Tree");
                break;
            case HomeType.NearFood:
                layerMask = LayerMask.GetMask("Food");
                isValidTarget = food => {
                    var foodScript = food.GetComponent<FoodSource>();
                    return foodScript != null &&
                           foodTypeEdible.Contains(foodScript.foodType) &&
                           foodScript.foodAvailable >= foodScript.minFoodToEat;
                };
                break;
            default:
                return false;
        }

        Transform closestTarget = FindClosestTarget(layerMask, isValidTarget);

        if (closestTarget != null)
        {
            targetHomeLocation = closestTarget.position + (Random.insideUnitSphere * 2f);
            return true;
        }

        targetHomeLocation = Vector3.zero;

        if (targetHomeLocation == Vector3.zero) return false;
        else return true;
    }
    Transform FindClosestTarget(LayerMask layerMask, System.Func<Collider, bool> isValidTarget)
    {
        Collider[] targets = Physics.OverlapSphere(transform.position, stats.detectionDistance, layerMask);

        Transform closestTarget = null;
        float closestDistance = Mathf.Infinity;

        foreach (Collider target in targets)
        {
            if (isValidTarget(target))
            {
                float distance = Vector3.Distance(transform.position, target.transform.position);
                if (distance < closestDistance)
                {
                    closestTarget = target.transform;
                    closestDistance = distance;
                }
            }
        }

        return closestTarget;
    }
    void MakeBurrow()
    {
        currentState = AnimalState.MakingBurrow;

        // Ensure burrow is on a valid NavMesh position
        NavMeshHit hit;
        if (NavMesh.SamplePosition(targetHomeLocation, out hit, 2f, NavMesh.AllAreas))
        {
            StartCoroutine(DigBurrow(hit.position));
        }
        else
        {
            Debug.LogWarning("Could not find a valid Home position!");
            currentState = AnimalState.Wandering; // Avoid softlock
        }
    }
    void GoToSleep()
    {
        if (home != null)
        {
            agent.SetDestination(home.transform.position);
            currentState = AnimalState.GoingToSleep;
        }
    }
    IEnumerator DigBurrow(Vector3 position)
    {
        while (!agent.isOnNavMesh)
        {
            yield return null; // Wait until the agent is placed on the NavMesh
        }

        agent.SetDestination(position);

        // Wait until the Rabbit reaches the position
        while (Vector3.Distance(transform.position, position) > 1.5f)
        {
            if (!agent.isOnNavMesh || agent.pathStatus == NavMeshPathStatus.PathInvalid)
            {
                currentState = AnimalState.Wandering;
                targetHomeLocation = Vector3.zero;
                yield break; // Exit if the NavMesh becomes invalid
            }

            yield return null; // Wait for the next frame
        }

        // Create burrow instantly upon arrival
        GameObject newBurrow = Instantiate(animalType.homePrefab, position, Quaternion.identity, MapGenerator.Instance.burrowHolder);

        home = newBurrow.GetComponent<Home>();

        //Should enter burrow after creation
    }

    // FOOD + DRINK
    bool DetectFood()
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
                if (food != null && foodTypeEdible.Contains(food.foodType))
                {
                    float distance = Vector3.Distance(transform.position, hit.transform.position);

                    if (distance < closestDistance)
                    {
                        if (!food.isDeadAnimal && (food.instantConsumable || food.foodAvailable >= food.minFoodToEat))
                        {
                            closestFood = hit.transform;
                            closestDistance = distance;
                        }
                        else if (food.isDeadAnimal && food.foodAvailable > 0)
                        {
                            closestFood = hit.transform;
                            closestDistance = distance;
                        }
                    }
                }
            }
        }

        if (closestFood != null)
        {
            targetFood = closestFood;
            agent.SetDestination(targetFood.position);
            currentState = AnimalState.GoingToEat;

            return true;
        }

        return false;
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
        if (currentState == AnimalState.GoingToEat && 
            targetFood != null && Vector3.Distance(transform.position, targetFood.position) <= 2)
        {
            currentState = AnimalState.Eating;
            StartCoroutine(EatRoutine());
        }
        else if (currentState == AnimalState.GoingToEat && targetFood == null) currentState = AnimalState.Wandering;

        if (currentState == AnimalState.GoingToDrink && 
            targetWater != null && Vector3.Distance(transform.position, targetWater.position) <= 2f)
        {
            currentState = AnimalState.Drinking;
            StartCoroutine(DrinkRoutine());
        }
        else if (currentState == AnimalState.GoingToDrink && targetWater == null) currentState = AnimalState.Wandering;
    }
    IEnumerator EatRoutine()
    {
        FoodSource foodSource = targetFood?.GetComponent<FoodSource>();

        if (foodSource.instantConsumable == false)
        {
            while (stats.hunger < stats.maxHunger && currentState == AnimalState.Eating && foodSource != null && foodSource.foodAvailable > 0)
            {
                if (detectedPredator != null)
                {
                    currentState = AnimalState.RunninngAway;
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
        }
        else
        {
            float foodConsumed = foodSource.ConsumeFood(stats.foodEatPerSecond);
            stats.hunger += foodConsumed;
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
                currentState = AnimalState.RunninngAway;
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
            currentState = AnimalState.RunninngAway;
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
        DetectPredator(); // Check for wolves after looking around
    }


    //THREATS
    void DetectPredator()
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
                Animal threatScript = threat.GetComponent<Animal>();
                if (threatScript) detectedPredator = threatScript;
                else Debug.LogError("No Animal Script found in Predator!! Check Layer or Scripts");
                closestDistance = distanceToThreat;
            }
        }

        if (detectedPredator != null)
        {
            currentState = AnimalState.RunninngAway;
        }
    }
    void RunAway()
    {
        if (detectedPredator == null)
        {
            currentState = AnimalState.Wandering;
            return;
        }

        float distanceToThreat = Vector3.Distance(transform.position, detectedPredator.transform.position);
        // Calculate direction to predator
        Vector3 toPredator = (detectedPredator.transform.position - transform.position).normalized;
        float dot = Vector3.Dot(transform.forward, toPredator);

        // If burrow exists and is safe, run towards it
        if (home != null && distanceToThreat > 12f && dot < Mathf.Cos(60 * Mathf.Deg2Rad))
        {
            agent.SetDestination(home.transform.position);
            return;
        }
        else if (distanceToThreat > 32f || !detectedPredator.gameObject.activeSelf)
        {
            detectedPredator = null;
            return;
        }

        Vector3 escapeDirection = (transform.position - detectedPredator.transform.position).normalized;
        Vector3 escapeTarget = transform.position + escapeDirection * stats.detectionDistance;

        // Check if escape path is blocked
        NavMeshHit hit;
        if (NavMesh.Raycast(transform.position, escapeTarget, out hit, NavMesh.AllAreas))
        {
            // If blocked, try steering left or right
            Vector3 alternateDirection = Vector3.Cross(Vector3.up, escapeDirection).normalized;
            Vector3 alternateTarget1 = transform.position + alternateDirection * stats.detectionDistance;
            Vector3 alternateTarget2 = transform.position - alternateDirection * stats.detectionDistance;

            if (NavMesh.SamplePosition(alternateTarget1, out hit, 2f, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
            }
            else if (NavMesh.SamplePosition(alternateTarget2, out hit, 2f, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
            }
            else
            {
                Debug.LogWarning("No valid escape path found!");
            }
        }
        else
        {
            // No obstacles, move normally
            if (NavMesh.SamplePosition(escapeTarget, out hit, 2f, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
            }
        }
    }

    //PREY/HUNTING
    void DetectPrey()
    {
        int preyLayerMask = 0;
        foreach (LayerMask mask in prey)
        {
            preyLayerMask |= mask.value; // Combine all layer masks
        }

        Collider[] preysToHunt = Physics.OverlapSphere(transform.position, stats.detectionDistance, preyLayerMask);
        detectedPrey = null;
        float closestDistance = Mathf.Infinity;

        foreach (Collider prey in preysToHunt)
        {
            Vector3 directionToPrey = (prey.transform.position - transform.position).normalized;
            float distanceToPrey = Vector3.Distance(transform.position, prey.transform.position);

            if (Vector3.Angle(transform.forward, directionToPrey) < stats.detectionAngle / 2 && distanceToPrey < closestDistance)
            {
                Animal preyScript = prey.GetComponent<Animal>();
                if (preyScript) detectedPrey = preyScript;
                else Debug.LogError("No Animal Script found in Predator!! Check Layer or Scripts");
                closestDistance = distanceToPrey;
            }
        }

        if (detectedPrey != null)
        {
            currentState = AnimalState.Hunting;
        }
    }
    void Hunt()
    {
        if (detectedPrey == null)
        {
            currentState = AnimalState.Wandering;
            return;
        }

        Animal preyScript = detectedPrey.GetComponent<Animal>();
        if (!preyScript)
        {
            Debug.LogWarning("Prey Detected has no Animal Script!");
            currentState = AnimalState.Wandering;
            detectedPrey = null;
            return;
        }
        else //has animal script
        {
            if (!preyScript.isDead && preyScript.currentState != AnimalState.Hiding)
            {
                agent.SetDestination(detectedPrey.transform.position);

                //Timer
                if (attackTimer < 2f) attackTimer += Time.deltaTime;

                float distanceToPrey = Vector3.Distance(transform.position, detectedPrey.transform.position);
                // If close enough, attempt to attack (you can replace this with an attack function)
                if (distanceToPrey <= stats.attackRange)
                {
                    if (attackTimer >= 2f)
                    {
                        AttackPrey(preyScript);
                    }
                }
            }
            else
            {
                detectedPrey = null;
                currentState = AnimalState.Wandering;
            }
        }
    }
    void AttackPrey(Animal preyToAttack)
    {
        attackTimer = 0;

        preyToAttack.GetAttacked(stats.attackDamage, this);
        if (preyToAttack.isDead || preyToAttack.currentState == AnimalState.Hiding)
        {
            detectedPrey = null;
            currentState = AnimalState.Wandering;
        }
    }
    void GetAttacked(float damage, Animal predatorAttacking)
    {
        //Set detected predator as attacker
        detectedPredator = predatorAttacking;
        currentState = AnimalState.RunninngAway;

        stats.health -= damage;
        
        //Check Death
        if (stats.health <= 0)
        {
            Die(DeathCause.Eaten);
        }
    }

    //COLLISION
    private void OnTriggerStay(Collider other)
    {
        if (other == null) return;

        Home homeScript = other.GetComponent<Home>();
        if (homeScript != null && homeScript.animalType == animalType)
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

                case AnimalState.RunninngAway:
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
                    if (homeScript.transform == toMateBurrow)
                    {
                        currentState = AnimalState.Mating;

                        wantsToReproduce = false;
                        stats.reproduceDaysLeft = stats.reproduceCooldownDays;
                        toMateBurrow = null;

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
        //FOOD FIX
        if (currentState == AnimalState.GoingToEat && other.gameObject.layer == LayerMask.NameToLayer("Food"))
        {
            //check if target is collided
            if (targetFood == other.transform)
            {
                currentState = AnimalState.Eating;
                StartCoroutine(EatRoutine());
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        //Water BUG FIX (water target is diagonal thus cant reach:)
        if (currentState == AnimalState.GoingToDrink && collision.gameObject.layer == LayerMask.NameToLayer("Drink"))
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
        if (isDead) statsHUD.SetActive(false);
        else statsHUD.SetActive(state);
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
        return animalType?.animalSpawnPrefab;
    }
    public GameObject GetCorpsePrefabBirth()
    {
        return animalType?.animalDeadPrefab;
    }
    public FurType GetAnimalSpecies()
    {
        return furType;
    }
    public void DoSpawnAddNumberToStats()
    {
        WorldStats.Instance.PlusAnimalCount(animalType);
    }

    public void DoDieMinusNumberFromStats()
    {
        WorldStats.Instance.MinusAnimalCount(animalType);
    }

    public void CheckAnimalGeneration()
    {
        WorldStats.Instance.CheckAnimalGeneration(animalType, stats.generation, this);
    }
    public void SetAnimalSkinModel()
    {
        if (furType?.model != null)
        {
            ClearModelHolderChild();
            Instantiate(furType.model, modelHolder);
        }
    }
    public void SetAnimalSkinModel(FurType newFurType)
    {
        ClearModelHolderChild();
        furType = newFurType;
        if (furType?.model != null)
            Instantiate(furType.model, modelHolder);
    }

    //OTHER FUNCTIONS
    void SnapToNavMesh()
    {
        Debug.Log("Snap!");
        //Fix TP bug
        targetWater = null;
        targetFood = null;
        targetHomeLocation = Vector3.zero;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(agent.transform.position, out hit, Mathf.Infinity, NavMesh.AllAreas))
        {
            agent.Warp(hit.position); // Move agent to the nearest valid position

            currentState = AnimalState.Wandering;
            StopAllCoroutines();

            Debug.Log("Agent snapped to NavMesh at: " + hit.position);
        }
        else
        {
            Debug.LogWarning("No valid NavMesh position found nearby!");
        }
    }

    private void ClearModelHolderChild()
    {
        foreach (Transform child in modelHolder)
            Destroy(child.gameObject);
    }
    Transform FindFirstChildWithTag(Transform parent, string tag)
    {
        return parent.GetComponentsInChildren<Transform>(true)
                     .FirstOrDefault(t => t.CompareTag(tag));
    }

}