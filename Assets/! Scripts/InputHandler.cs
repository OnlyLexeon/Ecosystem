using UnityEngine;

public class InputHandler : MonoBehaviour
{
    public Transform target;
    public float yOffset = 2f;
    public float distanceFromTarget = 5f;
    public float rotationSpeed = 100f;
    public float movementSpeed = 5f;

    //mode
    public bool isFollowingTarget = true;

    private float yaw = 0f;
    private float pitch = 0f;

    private void Start()
    {
        UIManager.Instance.ShowControls(isFollowingTarget);
    }

    void Update()
    {
        HandleModeSwitch();
        HandleToggleCursor();

        if (isFollowingTarget)
            FollowTargetMode();
        else
            FreeRoamMode();

        HandleTargetSelection();
    }

    public void HandleToggleCursor()
    {
        if (Input.GetKeyDown(KeyCode.Backslash)) // Detects the \ key
        {
            Cursor.visible = !Cursor.visible;
            Cursor.lockState = Cursor.visible ? CursorLockMode.None : CursorLockMode.Locked;
        }
    }

    void HandleModeSwitch()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            isFollowingTarget = !isFollowingTarget;
            UIManager.Instance.ShowControls(isFollowingTarget);

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
    }

    void FreeRoamMode()
    {
        float mouseX = Input.GetAxis("Mouse X") * rotationSpeed * 0.05f;
        float mouseY = Input.GetAxis("Mouse Y") * rotationSpeed * 0.05f;

        yaw += mouseX;
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, -80f, 80f);

        transform.rotation = Quaternion.Euler(pitch, yaw, 0f);

        Vector3 moveDirection = Vector3.zero;
        if (Input.GetKey(KeyCode.W)) moveDirection += transform.forward;
        if (Input.GetKey(KeyCode.S)) moveDirection -= transform.forward;
        if (Input.GetKey(KeyCode.A)) moveDirection -= transform.right;
        if (Input.GetKey(KeyCode.D)) moveDirection += transform.right;

        transform.position += moveDirection * movementSpeed * Time.unscaledDeltaTime;
    }

    void HandleTargetSelection()
    {
        if (Input.GetMouseButtonDown(0)) // Left click to select target
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, LayerMask.GetMask("Rabbit", "Wolf")))
            {
                target = hit.transform;
                isFollowingTarget = true;

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
}
