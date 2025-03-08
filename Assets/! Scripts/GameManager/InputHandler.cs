using Mono.Cecil.Cil;
using UnityEngine;

public class InputHandler : MonoBehaviour
{
    [Header("HUD")]
    [Tooltip("Reference to LineRenderer Component")] public LineRenderer lineRenderer;
    [Tooltip("Resolution of HUD when following target.")] public int segments = 20;

    [Header("Target")]
    [Tooltip("Reference to target")] public Transform target;
    [Tooltip("Current yOffset")] public float yOffset = 2f;
    [Tooltip("Camera distance from target.")] public float distanceFromTarget = 5f;
    [Tooltip("Camera rotation speed around target")] public float rotationSpeed = 100f;
    [Tooltip("Camera movement speed relative to target.")] public float movementSpeed = 5f;
    public float baseSpeed = 5f;
    public float fastSpeed = 10f;

    [Header("References")]
    //gene
    [Tooltip("is Gene panel open?")] public bool isGenePanelOpen = false;
    [Tooltip("Reference to Gene Menu in Canvas.")] public GameObject genePanel;

    //mode
    public bool isFollowingTarget = false;
    public bool isOverHeadDebugStats = false;

    //pausing
    public float savedTimeScale = 0f;
    public bool isPaused = false;

    //rotation
    private float yaw = 0f;
    private float pitch = 0f;

    public static InputHandler Instance;

    private void Start()
    {
        Instance = this;

        UIManager.Instance.ShowControls(isFollowingTarget);
        UIManager.Instance.SetDebugModeDisplayUI(isFollowingTarget);

        //LINE RENDERER FOR TARGET HUD
        lineRenderer.positionCount = segments + 3; // Start + End + Arc
        lineRenderer.loop = true; // Close the loop to form a cone
        lineRenderer.widthMultiplier = 0.1f;
        lineRenderer.useWorldSpace = true;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = new Color(0, 1, 1, 0.5f); // Semi-transparent yellow
        lineRenderer.endColor = new Color(0, 1, 1, 0.5f);
    }

    void Update()
    {
        //NO INPUT HANDLING WHEN DOING GENE
        if (isGenePanelOpen && Input.GetKeyDown(KeyCode.Escape)) ToggleGeneMenu();
        else if (isGenePanelOpen) return;

        //speed
        if (Input.GetKey(KeyCode.LeftShift))
            movementSpeed = fastSpeed;
        else
            movementSpeed = baseSpeed;

        //Handle Set Time to 1
        if (Input.GetKeyDown(KeyCode.P))
        {
            DayNightManager.Instance.PauseTime();
        }

        //handle Gene
        if (Input.GetKeyDown(KeyCode.G))
        {
            ToggleGeneMenu();
        }

        //handle pause
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }

        //handle arrow keys
        if (Input.GetKey(KeyCode.LeftArrow))
            DayNightManager.Instance.UpSpeed(-0.1f);
        else if (Input.GetKey(KeyCode.RightArrow))
            DayNightManager.Instance.UpSpeed(0.1f);

        if (Input.GetKeyDown(KeyCode.Backslash)) // Detects the \ key
        {
            Cursor.visible = !Cursor.visible;
            Cursor.lockState = Cursor.visible ? CursorLockMode.None : CursorLockMode.Locked;
        }

        if (Input.GetKeyDown(KeyCode.Slash)) // Detects the / key
        {
            isOverHeadDebugStats = !isOverHeadDebugStats;

            UIManager.Instance.SetDebugModeDisplayUI(!isOverHeadDebugStats);
            AnimalContainer.Instance.ToggleAnimalOverHeadUI(isOverHeadDebugStats);
            UIManager.Instance.UpdateTargetUI();
        }

        //Mode switch
        HandleModeSwitch();

        if (isFollowingTarget)
        {
            lineRenderer.enabled = true;
            FollowTargetMode();
        }
        else
        {
            lineRenderer.enabled = false;
            FreeRoamMode();
        }

        //Using cursor to select target
        HandleTargetSelection();
    }
    
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;

        UIManager.Instance.UpdateTargetUI();

    }

    public void SetTargetAndFollow(Transform newTarget)
    {
        SetTarget(newTarget);

        isFollowingTarget = true;
        UIManager.Instance.ShowControls(isFollowingTarget);
        UIManager.Instance.SetDebugModeDisplayUI(!isOverHeadDebugStats);
        UIManager.Instance.UpdateTargetUI();

    }

    void HandleModeSwitch()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            isFollowingTarget = !isFollowingTarget;
            UIManager.Instance.ShowControls(isFollowingTarget);
            UIManager.Instance.SetDebugModeDisplayUI(!isOverHeadDebugStats);

            if (isFollowingTarget && target == null)
            {
                FindClosestTarget();

                if (target != null)
                    yaw = target.eulerAngles.y;
            }
            else if (isFollowingTarget == false)
            {
                target = null;
            }

            UIManager.Instance.UpdateTargetUI();
        }
    }

    void FollowTargetMode()
    {
        if (target == null)
        {
            FindClosestTarget();
            UIManager.Instance.UpdateTargetUI();
        }
        if (target == null) return;
        
        //Input
        if (Input.GetKey(KeyCode.A))
            yaw -= rotationSpeed * Time.unscaledDeltaTime;
        if (Input.GetKey(KeyCode.D))
            yaw += rotationSpeed * Time.unscaledDeltaTime;

        if (Input.GetKey(KeyCode.W))
            yOffset += movementSpeed * Time.unscaledDeltaTime;
        if (Input.GetKey(KeyCode.S))
            yOffset -= movementSpeed * Time.unscaledDeltaTime;

        if (Input.GetKey(KeyCode.Q))
            distanceFromTarget += movementSpeed * Time.unscaledDeltaTime;
        if (Input.GetKey(KeyCode.E))
            distanceFromTarget -= movementSpeed * Time.unscaledDeltaTime;

        // Prevent negative distance
        distanceFromTarget = Mathf.Max(distanceFromTarget, 1f);
        distanceFromTarget = Mathf.Min(distanceFromTarget, 15f);

        // Calculate position
        Vector3 targetPosition = target.position + Vector3.up * yOffset;
        Quaternion rotation = Quaternion.Euler(0f, yaw, 0f);
        Vector3 offset = rotation * new Vector3(0f, 0f, -distanceFromTarget);

        transform.position = targetPosition + offset;
        transform.LookAt(target);

        DrawVisionCone();
    }

    void DrawVisionCone()
    {
        if (target == null) return; // No target, no vision cone

        Animal animalScript = target.GetComponent<Animal>();
        if (animalScript == null) return; // Only works for Rabbits

        Vector3 startPosition = target.position;
        Vector3 forward = target.forward;
        float halfAngle = animalScript.stats.detectionAngle * 0.5f;
        float detectionRadius = animalScript.stats.detectionDistance;

        // First point is at the rabbit's position
        lineRenderer.SetPosition(0, startPosition);

        for (int i = 0; i <= segments + 1; i++)
        {
            float angle = -halfAngle + (i / (float)segments) * animalScript.stats.detectionAngle;

            Vector3 direction = Quaternion.Euler(0, angle, 0) * forward; // Rotate forward vector
            Vector3 point = startPosition + direction * detectionRadius; // Extend to detection distance

            lineRenderer.SetPosition(i + 1, point);
        }

        // Close the arc to form a cone
        lineRenderer.SetPosition(segments + 2, startPosition);
    }

    void FreeRoamMode()
    {
        //Mouse Turning
        if (Input.GetKey(KeyCode.Mouse1)) // Right-click
        {
            float mouseX = Input.GetAxis("Mouse X") * rotationSpeed * 0.05f;
            float mouseY = Input.GetAxis("Mouse Y") * rotationSpeed * 0.05f;
            yaw += mouseX;
            pitch -= mouseY;
            pitch = Mathf.Clamp(pitch, -80f, 80f);
            transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
        }
        

        //Moving
        Vector3 moveDirection = Vector3.zero;
        if (Input.GetKey(KeyCode.W)) moveDirection += transform.forward;
        if (Input.GetKey(KeyCode.S)) moveDirection -= transform.forward;
        if (Input.GetKey(KeyCode.A)) moveDirection -= transform.right;
        if (Input.GetKey(KeyCode.D)) moveDirection += transform.right;

        transform.position += moveDirection * movementSpeed * Time.unscaledDeltaTime;
    }

    //Finding/Clicking Targets
    void HandleTargetSelection()
    {
        if (Input.GetMouseButtonDown(0)) // Left click to select target
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, LayerMask.GetMask("Rabbit", "Wolf")))
            {
                target = hit.transform;
                isFollowingTarget = true;

                UIManager.Instance.ShowControls(isFollowingTarget);
                UIManager.Instance.SetDebugModeDisplayUI(!isOverHeadDebugStats);
                UIManager.Instance.UpdateTargetUI();
            }
        }
    }
    void FindClosestTarget()
    {
        Collider[] targets = Physics.OverlapSphere(transform.position, 100f, LayerMask.GetMask("Rabbit", "Wolf")); // or wolf

        Transform closest = null;
        float closestDistance = Mathf.Infinity;

        foreach (Collider col in targets)
        {
            float distance = Vector3.Distance(transform.position, col.transform.position);
            if (distance < closestDistance)
            {
                closest = col.transform;
                closestDistance = distance;
            }
        }

        target = closest;
    }

    public void TogglePause()
    {
        isPaused = !isPaused;

        if (isPaused)
        {
            savedTimeScale = Time.timeScale;
            Time.timeScale = 0;
            DayNightManager.Instance.UpdateSpeedText();
        }
        else
        {
            Time.timeScale = savedTimeScale;
            DayNightManager.Instance.UpdateSpeedText();
        }
    }

    public void ToggleGeneMenu()
    {
        isGenePanelOpen = !isGenePanelOpen;

        genePanel.SetActive(isGenePanelOpen);
    }
}
