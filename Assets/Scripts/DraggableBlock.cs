using UnityEngine;
using TMPro; // Required for TextMeshPro

public class DraggableBlock : MonoBehaviour
{
    [Header("Block Data")]
    public char letter;

    [Header("References")]
    public Camera mainCamera;
    public TMP_Text textComponent;

    [Header("Settings")]
    public Vector3 dragRotation = new Vector3(0, 180, 0); // Faces camera while dragging

    // Internal State
    private Vector3 startPos;
    private Vector3 originalScale;
    private bool isDragging = false;
    private Rigidbody rb;
    private Tower currentTower;

    // Lock X-axis (Depth)
    private float fixedX;

    // MEMORY: Remembers which slot it came from (for Array Mode)
    private int storedArrayIndex = -1;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();

        // Auto-find TextMeshPro component
        if (textComponent == null) textComponent = GetComponentInChildren<TMP_Text>();

        originalScale = transform.localScale;
    }

    public void InitializeBlock(char c)
    {
        letter = c;
        if (textComponent != null) textComponent.text = c.ToString();
        gameObject.name = "Block_" + c;
    }

    public void SetCamera(Camera cam)
    {
        mainCamera = cam;
    }

    public void SetCurrentTower(Tower t)
    {
        currentTower = t;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) TryPickUp();
        if (isDragging) DragBlock();
        if (Input.GetMouseButtonUp(0) && isDragging) DropBlock();
    }

    private void TryPickUp()
    {
        if (mainCamera == null) return;

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            if (hit.transform == transform)
            {
                // --- RESOURCE TRAY CHECKS ---
                ResourceTray tray = GetComponentInParent<ResourceTray>();
                if (tray != null)
                {
                    // 1. Check if allowed (e.g. Stack/Queue rules)
                    if (!tray.CanPickUp(this)) return;

                    // 2. Remember our index (Crucial for Array Mode)
                    storedArrayIndex = tray.GetBlockIndex(this);

                    // 3. Remove from tray logic
                    tray.RemoveBlock(this);
                }

                // --- GOAL TOWER CHECKS ---
                if (currentTower != null)
                {
                    if (!currentTower.IsTopBlock(this)) return;
                    currentTower.RemoveBlock(this);
                    currentTower = null;
                    if (GameManager.Instance != null) GameManager.Instance.CheckForWin();
                }

                // --- SETUP DRAG ---
                isDragging = true;
                startPos = transform.position;

                transform.SetParent(null);
                transform.localScale = originalScale; // Restore size

                // LOCK X-AXIS: Memorize depth
                fixedX = transform.position.x;

                // APPLY ROTATION: Face the camera
                transform.rotation = Quaternion.Euler(dragRotation);

                rb.isKinematic = true;
            }
        }
    }

    private void DragBlock()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        // INVISIBLE WALL: Faces Right/Left at the block's X-depth
        // If dragging feels inverted, swap Vector3.right for Vector3.left
        Plane dragWall = new Plane(Vector3.right, new Vector3(fixedX, 0, 0));

        float enterDist;
        if (dragWall.Raycast(ray, out enterDist))
        {
            Vector3 hitPoint = ray.GetPoint(enterDist);
            transform.position = hitPoint;
        }
    }

    private void DropBlock()
    {
        isDragging = false;

        Tower nearestTower = FindClosestTower();
        ResourceTray tray = FindFirstObjectByType<ResourceTray>();

        // 1. Check if we dropped it inside the Tray's "Drop Zone" Collider
        bool droppedOnTray = false;
        if (tray != null)
        {
            Collider trayCollider = tray.GetComponent<Collider>();
            if (trayCollider != null)
            {
                Vector3 closest = trayCollider.ClosestPoint(transform.position);
                // If we are within 1 unit of the collider box, count it
                if (Vector3.Distance(transform.position, closest) < 1.0f) droppedOnTray = true;
            }
            else
            {
                // Fallback (Distance check) if you forgot the Collider
                float d = Vector3.Distance(transform.position, tray.transform.position);
                if (d < 6.0f) droppedOnTray = true;
            }
        }

        // Calculate distance to nearest Goal Tower
        float distanceToTower = 100f;
        if (nearestTower != null)
        {
            distanceToTower = Vector3.Distance(transform.position, nearestTower.transform.position);
        }

        // --- OPTION A: Drop on Goal Tower ---
        // Conditions: 
        // 1. Tower exists
        // 2. We are close enough (Radius 1.25f)
        // 3. Tower has space (Word Length + 1 rule)
        if (nearestTower != null && distanceToTower < 1.25f && nearestTower.HasSpace())
        {
            Vector3 newPos = nearestTower.GetNextSnapPosition();
            transform.position = newPos;
            nearestTower.AddBlock(this);
            SetCurrentTower(nearestTower);
            if (GameManager.Instance != null) GameManager.Instance.CheckForWin();
        }

        // --- OPTION B: Return to Tray ---
        else if (droppedOnTray)
        {
            // IF we have a stored index, try the specific "Place In Array" logic.
            // (The Tray script safely ignores this if we are in Stack/Queue mode)
            if (storedArrayIndex != -1)
            {
                tray.TryPlaceBlockInArray(this, transform.position, storedArrayIndex);
            }
            else
            {
                tray.ReturnBlockToTray(this);
            }

            SetCurrentTower(null);
            storedArrayIndex = -1; // Reset memory
        }

        // --- OPTION C: Reset (Missed everything) ---
        else
        {
            if (tray != null)
            {
                // If we drifted off into space, put it back in the old slot
                if (storedArrayIndex != -1)
                    tray.TryPlaceBlockInArray(this, startPos, storedArrayIndex);
                else
                    tray.ReturnBlockToTray(this);

                SetCurrentTower(null);
            }
            else
            {
                transform.position = startPos;
            }
            storedArrayIndex = -1;
        }
    }

    private Tower FindClosestTower()
    {
        Tower closest = null;
        float minDst = Mathf.Infinity;
        Tower[] allTowers = FindObjectsByType<Tower>(FindObjectsSortMode.None);

        foreach (Tower t in allTowers)
        {
            if (!t.gameObject.activeInHierarchy) continue;

            float dst = Vector3.Distance(transform.position, t.transform.position);
            if (dst < minDst)
            {
                minDst = dst;
                closest = t;
            }
        }
        return closest;
    }
}