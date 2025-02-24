using UnityEngine;
using TMPro;
using UnityEngine.UI;
using static Stats;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    public TextMeshProUGUI targetText;
    public TextMeshProUGUI targetAction;
    public TextMeshProUGUI targetAge;
    public TextMeshProUGUI targetGender;

    public GameObject followControls;
    public GameObject freeRoamControls;

    [Header("Family")]
    public GameObject familyPanel;
    public Button motherButton;
    public Button fatherButton;
    public Transform childrenButtonsPanel;
    public GameObject childrenButtonsPrefab;

    [Header("Sliders")]
    public GameObject slidersPanel;
    public Slider healthSlider;
    public Slider hungerSlider;
    public Slider thirstSlider;
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI hungerText;
    public TextMeshProUGUI thirstText;
    public GameObject personalityPrefab;
    public Transform personalityPanel;

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

    [Header("Sleep")]
    public TextMeshProUGUI additionalSleep;

    [Header("Time")]
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI dayNightText;

    [Header("References")]
    public InputHandler cameraScript;
    public DayNightManager timeScript;

    public static UIManager Instance;

    private void Awake()
    {
        Instance = this;
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

    //Stats = changing variables 
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

                healthText.text = statScript.health.ToString("F2") + "/" + statScript.maxHealth.ToString("F2");
                hungerText.text = statScript.hunger.ToString("F2") + "/" + statScript.maxHunger.ToString("F2");
                thirstText.text = statScript.thirst.ToString("F2") + "/" + statScript.maxThirst.ToString("F2");

                targetAge.text = "Age (Days): " + statScript.agedDays;
                targetGender.text = "Gender: " + statScript.gender.ToString();
            }
            else Debug.LogWarning("No Stats script found!");

            // UPDATING ACTION TEXT
            Animal animalScript = cameraScript.target.GetComponent<Animal>();
            if (animalScript != null) targetAction.text = "Action: " + animalScript.currentState.ToString();
        }
    }

    //target = personality or target type
    public void UpdateTargetUI()
    {
        foreach (Transform child in personalityPanel.transform)
        {
            Destroy(child.gameObject);
        }

        if (cameraScript.target != null)
        {
            slidersPanel.SetActive(true);
            statsPanel.SetActive(true);

            string targetName = cameraScript.target.gameObject.layer == LayerMask.NameToLayer("Wolf") ? "Wolf" :
                                cameraScript.target.gameObject.layer == LayerMask.NameToLayer("Rabbit") ? "Rabbit" :
                                "Unknown";

            targetText.text = "Target: " + targetName;

            Stats targetStats = cameraScript.target.GetComponent<Stats>();
            if (targetStats)
            {
                //Personality
                List<Genes> genes = targetStats.genes;
                foreach (Genes gene in genes)
                {
                    GameObject newPersonality = Instantiate(personalityPrefab, personalityPanel);
                    PersonalityButton buttonScript = newPersonality.GetComponent<PersonalityButton>();

                    if (buttonScript)
                    {
                        buttonScript.nameText.text = gene.name;
                        buttonScript.descriptionText.text = gene.description;
                        buttonScript.positivity = gene.positivity;
                    }
                }

                //STATS
                hungerDepletion.text = "Hunger Depletion Rate: " + targetStats.hungerDepletionRate.ToString();
                thirstDepletion.text = "Thirst Depletion Rate: " + targetStats.thirstDepletionRate.ToString();

                Animal animalScript = cameraScript.target.GetComponent<Animal>();
                if (animalScript != null)
                {
                    UpdateRabbitStats(animalScript);
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
            targetAction.text = "Action: -";
            targetAge.text = "Age: -";
            targetGender.text = "Gender: -";
        }
    }

    public void UpdateTime()
    {
        float time = timeScript.time;
        int minutes = Mathf.FloorToInt(time % 60);
        int hours = Mathf.FloorToInt(time / 60);

        timeText.text = hours.ToString("D2") + ":" + minutes.ToString("D2");

        if (DayNightManager.Instance.isDay)
            dayNightText.text = "DayTime " + "| "  + "Day " + DayNightManager.Instance.dayNumber;
        else dayNightText.text = "NightTime "+ "| " + "Day " + +DayNightManager.Instance.dayNumber;
    }

    public void UpdateRabbitStats(Animal animalScript)
    {
        detectionRange.text = "Detection Range: " + animalScript.stats.detectionDistance.ToString();
        detectionRadius.text = "Detection Angle: " + animalScript.stats.detectionAngle.ToString();

        baseSpeed.text = "Base Speed: " + animalScript.stats.baseSpeed.ToString();
        runSpeed.text = "Run Speed: " + animalScript.stats.runSpeed.ToString();
        wanderInterval.text = "Wander Interval: " + animalScript.stats.wanderInterval.ToString();
        wanderMin.text = "Wander Min Distance: " + animalScript.stats.wanderDistanceMin.ToString();
        wanderMax.text = "Wander Max Distance: " + animalScript.stats.wanderDistanceMax.ToString();

        eatPerSec.text = "Eat/Second: " + animalScript.stats.foodEatPerSecond.ToString();
        thirstPerSec.text = "Drink/Second: " + animalScript.stats.drinkPerSecond.ToString();
        needsInterval.text = "Needs Deplete Interval: " + animalScript.stats.needsInterval.ToString();

        baseOffSpring.text = "Base Offspring Count: " + animalScript.stats.baseOffSpringCount.ToString();
        maxAdditionalOffSpring.text = "Max Random Extra OffSpring: " + animalScript.stats.maxAdditionalOffSpring.ToString();
        minPositiveGenes.text = "Min Positive Genes: " + animalScript.stats.minPositiveGenesPrefered.ToString();
        maxNegativeGenes.text = "Max Negative Genes: " + animalScript.stats.maxNegativeGenesPrefered.ToString();
        reproduceCooldownDays.text = "Reproduce Cooldown (Days): " + animalScript.stats.reproduceCooldownDays.ToString();
        reproduceDaysLeft.text = "Reproduce Ready (Days Left): " + animalScript.stats.reproduceDaysLeft.ToString();

        lookInverval.text = "Look While Eat/Drink Interval: " + animalScript.stats.lookWhileEatingInterval.ToString();
        lookMinAngle.text = "Look Min Angle: " + animalScript.stats.lookAngleMin.ToString();
        lookMaxAngle.text = "Look Max Angle: " + animalScript.stats.lookAngleMax.ToString();

        additionalSleep.text = "Additional Sleep Hours: " + animalScript.stats.additionalSleepHours.ToString();

        waitInBurrowTime.text = "Wait in Burrow after Chase Time: " + animalScript.stats.waitBeforeLeavingBurrow.ToString();
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
                    AssignButton(childButton, animalScript.children[i].transform);

                    if (animalScript.isDead) buttonText.text = $"Dead {i + 1}";
                    else buttonText.text = $"Child {i + 1}";
                }
            }
            else
            {
                childrenButtonsPanel.gameObject.SetActive(false);
            }
            
        }

        ResizeUI();
    }

    void AssignButton(Button button, Transform child)
    {
        button.onClick.AddListener(() => InputHandler.Instance.SetTarget(child.transform));
    }
}
