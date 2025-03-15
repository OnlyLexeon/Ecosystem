using UnityEngine;
using TMPro;
using UnityEngine.UI;
using static Stats;
using System.Collections.Generic;
using System.Resources;
using System.Collections;

public class UIManager : MonoBehaviour
{
    public GameObject followControls;
    public GameObject freeRoamControls;

    [Header("Family")]
    public GameObject familyPanel;
    public Button motherButton;
    public Button fatherButton;
    public Transform childrenButtonsPanel;
    public GameObject childrenButtonsPrefab;

    [Header("History Panel")]
    public TMP_Dropdown historyDropdown;
    public Transform historyContainer;
    public GameObject historyEventPrefab;
    public int maxHistoryCount = 99;

    [Header("About Target")]
    public TextMeshProUGUI targetText;
    public TextMeshProUGUI targetSpecies;
    public TextMeshProUGUI targetAction;
    public TextMeshProUGUI targetAge;
    public TextMeshProUGUI targetGeneration;
    public TextMeshProUGUI targetGender;

    [Header("Sliders")]
    public GameObject slidersPanel;
    public Slider healthSlider;
    public Slider hungerSlider;
    public Slider thirstSlider;
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI hungerText;
    public TextMeshProUGUI thirstText;

    [Header("Genes Display")]
    public TextMeshProUGUI geneCountText;
    public GameObject genePrefab;
    public Transform genePanel;

    [Header("Stats")]
    public GameObject statsPanel;
    public TextMeshProUGUI detectionRange;
    public TextMeshProUGUI detectionRadius;
    public TextMeshProUGUI baseSpeed;
    public TextMeshProUGUI runSpeed;
    public TextMeshProUGUI wanderInterval;
    public TextMeshProUGUI wanderMin;
    public TextMeshProUGUI wanderMax;
    public TextMeshProUGUI hungerDepletion;
    public TextMeshProUGUI thirstDepletion;
    public TextMeshProUGUI eatPerSec;
    public TextMeshProUGUI thirstPerSec;
    public TextMeshProUGUI needsInterval;

    [Header("Rabbit Exclusive")]
    public TextMeshProUGUI lookInverval;
    public TextMeshProUGUI lookMinAngle;
    public TextMeshProUGUI lookMaxAngle;
    public TextMeshProUGUI waitInBurrowTime;

    [Header("Seggs")]
    public TextMeshProUGUI baseOffSpring;
    public TextMeshProUGUI maxAdditionalOffSpring;
    public TextMeshProUGUI minPositiveGenes;
    public TextMeshProUGUI maxNegativeGenes;
    public TextMeshProUGUI reproduceCooldownDays;
    public TextMeshProUGUI reproduceDaysLeft;

    [Header("Chances")]
    public TextMeshProUGUI offSpringDominanceText;
    public TextMeshProUGUI furDominanceText;
    public TextMeshProUGUI mutationChanceText;

    [Header("Sleep")]
    public TextMeshProUGUI additionalSleep;

    [Header("Time")]
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI dayNightText;

    [Header("Aging")]
    public TextMeshProUGUI deathDaysText;
    public TextMeshProUGUI maxDeathTimeText;
    public TextMeshProUGUI minDeathTimeText;
    public TextMeshProUGUI deathTimeText;

    [Header("References")]
    public InputHandler cameraScript;
    public DayNightManager timeScript;

    public bool showDebugUI = true;

    public static UIManager Instance;

    private bool isClamping = false;
    private List<HistoryEvent> historyEvents = new List<HistoryEvent>();

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        historyDropdown.onValueChanged.AddListener(FilterHistory);
        SetupDropdownOptions();
    }

    private void Update()
    {
        UpdateTargetStats();

        UpdateTime();
    }

    public void ResizeUI()
    {
        familyPanel.SetActive(false);
        familyPanel.SetActive(true);
    }

    //Showing/NotShowing
    public void ShowControls(bool isFollowing)
    {
        if (followControls && freeRoamControls)
        {
            if (isFollowing)
            {
                followControls.SetActive(true);
                freeRoamControls.SetActive(false);
            }
            else
            {
                followControls.SetActive(false);
                freeRoamControls.SetActive(true);
            }

        }
    }
    public void SetDebugModeDisplayUI(bool state)
    {
        showDebugUI = state;

        slidersPanel.SetActive(state);
        statsPanel.SetActive(state);

        if (cameraScript.target == null)
        {
            slidersPanel.SetActive(false);
            statsPanel.SetActive(false);
        }
    }

    //UpdateTargetStats sets things constantly
    public void UpdateTargetStats()
    {
        if (cameraScript.target != null && statsPanel.activeSelf)
        {
            Stats statScript = cameraScript.target.GetComponent<Stats>();

            if (statScript != null)
            {
                healthSlider.maxValue = statScript.maxHealth;
                hungerSlider.maxValue = statScript.maxHunger;
                thirstSlider.maxValue = statScript.maxThirst;

                healthSlider.value = statScript.health;
                hungerSlider.value = statScript.hunger;
                thirstSlider.value = statScript.thirst;

                healthText.text = statScript.health.ToString("F0") + "/" + statScript.maxHealth.ToString("F0");
                hungerText.text = statScript.hunger.ToString("F0") + "/" + statScript.maxHunger.ToString("F0");
                thirstText.text = statScript.thirst.ToString("F0") + "/" + statScript.maxThirst.ToString("F0");

                targetAge.text = "Age (Days): " + statScript.agedDays + " / " + statScript.deathDays;
                targetGender.text = "Gender: " + statScript.gender.ToString();
            }
            else Debug.LogWarning("No Stats script found!");

            // UPDATING ACTION TEXT
            Animal animalScript = cameraScript.target.GetComponent<Animal>();
            if (animalScript != null) targetAction.text = "Action: " + animalScript.currentState.ToString();
        }
    }

    //UpdateTargetUI sets ui once every new target
    public void UpdateTargetUI()
    {
        foreach (Transform child in genePanel.transform)
        {
            Destroy(child.gameObject);
        }

        if (cameraScript.target != null && showDebugUI)
        {
            slidersPanel.SetActive(true);
            statsPanel.SetActive(true);

            Transform target = cameraScript.target;
            Animal targetAnimalScript = target.GetComponent<Animal>();
            Stats targetStats = cameraScript.target.GetComponent<Stats>();

            //Setting Target Information
            string targetType = targetAnimalScript.animalType.animalName.ToString();
            string targetName = targetAnimalScript.animalName;
            targetText.text = "Target: " + targetName + " - " + targetType;

            if (targetAnimalScript.furType) targetSpecies.text = "Fur Type: " + targetAnimalScript.furType.furName.ToString();

            targetGeneration.text = "Generation: " + targetStats.generation;

            //GENES
            if (targetStats)
            {
                List<Genes> genes = targetStats.genes;

                //Text
                geneCountText.text = "Genes (" + genes.Count + ") :";

                //Gene Buttons
                foreach (Genes gene in genes)
                {
                    GameObject newPersonality = Instantiate(genePrefab, genePanel);
                    PersonalityButton buttonScript = newPersonality.GetComponent<PersonalityButton>();

                    if (buttonScript)
                    {
                        buttonScript.nameText.text = gene.name;
                        buttonScript.descriptionText.text = gene.description + "\nWeightage: " + gene.weightage;
                        buttonScript.positivity = gene.positivity;
                    }
                }

                Animal animalScript = cameraScript.target.GetComponent<Animal>();
                if (animalScript != null)
                {
                    UpdateAnimalStats(animalScript);
                }

                //Family Buttons
                PopulateFamilyUI(cameraScript.target);
            }
        }
        else
        {
            slidersPanel.SetActive(false);
            statsPanel.SetActive(false);

            targetText.text = "Target: None";
            targetSpecies.text = "Species: -";
            targetAction.text = "Action: -";
            targetAge.text = "Age: -";
            targetGeneration.text = "Generation: -";
            targetGender.text = "Gender: -";
        }
    }
    public void UpdateTime()
    {
        timeText.text = DayNightManager.Instance.GetTimeString();

        if (DayNightManager.Instance.isDay)
            dayNightText.text = "DayTime " + "| "  + "Day " + DayNightManager.Instance.dayNumber;
        else dayNightText.text = "NightTime "+ "| " + "Day " + +DayNightManager.Instance.dayNumber;
    }

    //UpdateAnimalStats runs once called by UpdateTargetUI
    public void UpdateAnimalStats(Animal animalScript)
    {
        Stats statScript = animalScript.stats;

        detectionRange.text = "Detection Range: " + statScript.detectionDistance.ToString();
        detectionRadius.text = "Detection Angle: " + statScript.detectionAngle.ToString();

        baseSpeed.text = "Base Speed: " + statScript.baseSpeed.ToString();
        runSpeed.text = "Run Speed: " + statScript.runSpeed.ToString();
        wanderInterval.text = "Wander Interval: " + statScript.wanderInterval.ToString();
        wanderMin.text = "Wander Min Distance: " + statScript.wanderDistanceMin.ToString();
        wanderMax.text = "Wander Max Distance: " + statScript.wanderDistanceMax.ToString();

        eatPerSec.text = "Eat/Second: " + statScript.foodEatPerSecond.ToString();
        thirstPerSec.text = "Drink/Second: " + statScript.drinkPerSecond.ToString();
        needsInterval.text = "Needs Deplete Interval: " + statScript.needsInterval.ToString();

        hungerDepletion.text = "Hunger Depletion Rate: " + statScript.hungerDepletionRate.ToString();
        thirstDepletion.text = "Thirst Depletion Rate: " + statScript.thirstDepletionRate.ToString();

        baseOffSpring.text = "Base Offspring Count: " + statScript.baseOffSpringCount.ToString();
        maxAdditionalOffSpring.text = "Max Random Extra OffSpring: " + statScript.maxAdditionalOffSpring.ToString();
        minPositiveGenes.text = "Min Positive Genes: " + statScript.minPositiveGenesPrefered.ToString();
        maxNegativeGenes.text = "Max Negative Genes: " + statScript.maxNegativeGenesPrefered.ToString();
        reproduceCooldownDays.text = "Reproduce Cooldown (Days): " + statScript.reproduceCooldownDays.ToString();
        reproduceDaysLeft.text = "Reproduce Ready (Days Left): " + statScript.reproduceDaysLeft.ToString();

        offSpringDominanceText.text = "Dominance Over Child Count: " + statScript.seedDominance.ToString() + "%";
        furDominanceText.text = "Dominance Over Fur Gene: " + statScript.furDominance.ToString() + "%";
        mutationChanceText.text = "Children Mutation Chance: " + statScript.geneMutationChance.ToString() + "%";

        lookInverval.text = "Look While Eat/Drink Interval: " + statScript.lookWhileEatingInterval.ToString();
        lookMinAngle.text = "Look Min Angle: " + statScript.lookAngleMin.ToString();
        lookMaxAngle.text = "Look Max Angle: " + statScript.lookAngleMax.ToString();

        additionalSleep.text = "Additional Sleep Hours: " + statScript.additionalSleepHours.ToString();

        waitInBurrowTime.text = "Wait in Burrow after Chase Time: " + statScript.waitBeforeLeavingBurrow.ToString();

        deathDaysText.text = "Start Dying at Age (Days): " + statScript.deathDays.ToString();
        deathTimeText.text = "Time Starts Dying (Decaying): " + statScript.deathTime.ToString();
        minDeathTimeText.text = "Min Time in Day Decay: " + statScript.minDeathTime.ToString();
        maxDeathTimeText.text = "Max Time in Day Decay: " + statScript.maxDeathTime.ToString();
    }
    public void PopulateFamilyUI(Transform target)
    {
        Animal animalScript = target.GetComponent<Animal>();

        if (animalScript != null)
        {

            // Set Mother Button
            if (animalScript.mother != null)
            {
                motherButton.gameObject.SetActive(true);
                motherButton.onClick.RemoveAllListeners();
                motherButton.onClick.AddListener(() => InputHandler.Instance.SetTarget(animalScript.mother.transform));
            }
            else
            {
                motherButton.gameObject.SetActive(false);
            }

            // Set Father Button
            if (animalScript.father != null)
            {
                fatherButton.gameObject.SetActive(true);
                fatherButton.onClick.RemoveAllListeners();
                fatherButton.onClick.AddListener(() => InputHandler.Instance.SetTarget(animalScript.father.transform));
            }
            else
            {
                fatherButton.gameObject.SetActive(false);
            }

            // Clear previous children buttons
            foreach (Transform child in childrenButtonsPanel)
            {
                Destroy(child.gameObject);
            }

            if (animalScript.children.Count > 0)
            {
                childrenButtonsPanel.gameObject.SetActive(true);

                // Populate Children Buttons
                for (int i = 0; i < animalScript.children.Count; i++)
                {
                    GameObject childButtonObj = Instantiate(childrenButtonsPrefab, childrenButtonsPanel);
                    Button childButton = childButtonObj.GetComponent<Button>();
                    TMP_Text buttonText = childButtonObj.GetComponentInChildren<TMP_Text>();

                    childButton.onClick.RemoveAllListeners();
                    AssignChildButtonListener(childButton, animalScript.children[i].transform);

                    Animal childScript = animalScript.children[i].GetComponent<Animal>();
                    if (childScript.isDead) buttonText.text = $"Dead";
                    else buttonText.text = $"{childScript.animalName}";
                }
            }
            else
            {
                childrenButtonsPanel.gameObject.SetActive(false);
            }
            
        }

        ResizeUI();
    }
    void AssignChildButtonListener(Button button, Transform child)
    {
        button.onClick.AddListener(() => InputHandler.Instance.SetTarget(child.transform));
    }

    //History UI
    public void AddNewHistory(string text, System.Action onButtonClick = null, HistoryType type = HistoryType.Default)
    {
        GameObject historyEvent = Instantiate(historyEventPrefab, historyContainer);
        HistoryEvent historyScript = historyEvent.GetComponent<HistoryEvent>();
        historyScript.SetHistory(text, onButtonClick, type);

        historyEvents.Add(historyScript);

        if (!isClamping) StartCoroutine(ClampHistoryCount());

        FilterHistory(historyDropdown.value);
    }
    public IEnumerator ClampHistoryCount()
    {
        isClamping = true;

        int excess = historyContainer.childCount - maxHistoryCount;

        // Limit the number of deletions per frame (prevents freezing)
        for (int i = 0; i < excess; i++)
        {
            if (historyContainer.childCount == 0) break;

            HistoryEvent oldestEvent = historyEvents[0];
            historyEvents.RemoveAt(0);
            Destroy(oldestEvent.gameObject);

            yield return null;
        }

        isClamping = false;
    }

    public void FilterHistory(int dropdownIndex)
    {
        if (dropdownIndex == 0) // "All" option
        {
            foreach (var historyEvent in historyEvents)
            {
                historyEvent.gameObject.SetActive(true);
            }
            return;
        }

        HistoryType selectedType = (HistoryType)(dropdownIndex - 1); // Adjust index since "All" is index 0

        foreach (var historyEvent in historyEvents)
        {
            bool shouldBeActive = historyEvent.historyType == selectedType;
            historyEvent.gameObject.SetActive(shouldBeActive);
        }
    }
    private void SetupDropdownOptions()
    {
        historyDropdown.ClearOptions();
        List<string> options = new List<string>() { "All" }; // Add "All" option at index 0

        foreach (HistoryType type in System.Enum.GetValues(typeof(HistoryType)))
        {
            options.Add(type.ToString());
        }

        historyDropdown.AddOptions(options);
    }
}
