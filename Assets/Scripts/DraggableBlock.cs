using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class DraggableBlock : MonoBehaviour
{
    public char letter = 'A';
    private Vector3 offset;
    private Plane dragPlane;
    private bool isDragging = false;
    private Vector3 startPos; 
    private Camera puzzleCamera;

    private Tower currentTower;

    public void SetCamera(Camera cam)
    {
        puzzleCamera = cam;
    }

    void Start()
    {
        // 1. Keep your existing start position logic
        startPos = transform.position;

        // 2. Find the TextMeshPro component (it's likely on a child object)
        TMP_Text textComponent = GetComponentInChildren<TMP_Text>();

        // 3. If we found it, update our internal 'letter' variable to match the visual text
        if (textComponent != null && textComponent.text.Length > 0)
        {
            // We take the first character of the string (e.g., "H" -> 'H')
            letter = textComponent.text[0];

            // Rename block
            gameObject.name = "Block_" + letter;
        }
        else
        {
            Debug.LogWarning("Block " + name + " has no TextMeshPro component or empty text!");
        }
    }

    void Update()
    {
        if (Mouse.current == null) return;

        // 1. Mouse Down
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            TryPickUp();
        }

        // 2. Mouse Up (The Snapping Logic triggers here!)
        if (Mouse.current.leftButton.wasReleasedThisFrame && isDragging)
        {
            DropBlock();
        }

        // 3. Dragging
        if (isDragging)
        {
            MoveBlock();
        }
    }

    // Call this immediately after spawning the block to set it up
    public void InitializeBlock(char c)
    {
        letter = c;

        // Update the name in the Hierarchy
        gameObject.name = "Block_" + c;

        // Update the visual text
        TMP_Text textComponent = GetComponentInChildren<TMP_Text>();
        if (textComponent != null)
        {
            textComponent.text = c.ToString();
        }
    }

    private void TryPickUp()
    {
        if (puzzleCamera == null) return;

        Vector2 mousePos = Mouse.current.position.ReadValue();

        Ray ray = puzzleCamera.ScreenPointToRay(mousePos);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            if (hit.transform == transform)
            {
                // --- NEW CHECK: STACK RULES ---
                if (currentTower != null)
                {
                    // Ask the tower: "Am I at the top?"
                    if (!currentTower.IsTopBlock(this))
                    {
                        return; // STOP! Do not pick up.
                    }

                    // If we passed the check, remove us from the tower logic
                    currentTower.RemoveBlock(this);
                    currentTower = null;

                    if (GameManager.Instance != null)
                    {
                        GameManager.Instance.UpdateUI();
                    }
                }
                // -----------------------------

                isDragging = true;

                // Setup drag plane (Rest of your existing code...)
                dragPlane = new Plane(-puzzleCamera.transform.forward, transform.position);
                float enter;
                if (dragPlane.Raycast(ray, out enter))
                {
                    Vector3 hitPoint = ray.GetPoint(enter);
                    offset = transform.position - hitPoint;
                }
            }
        }
    }

    private void MoveBlock()
    {
        if (puzzleCamera == null) return;

        Vector2 mousePos = Mouse.current.position.ReadValue();
        Ray ray = puzzleCamera.ScreenPointToRay(mousePos);

        float enter;

        if (dragPlane.Raycast(ray, out enter))
        {
            Vector3 hitPoint = ray.GetPoint(enter);
            transform.position = hitPoint + offset;
        }
    }

    private void DropBlock()
    {
        isDragging = false;

        Tower nearestTower = FindClosestTower();
        float flatDistance = 100f; // Default high value

        // 1. Calculate Horizontal Distance (Ignoring Height)
        if (nearestTower != null)
        {
            // We create "Shadows" on the floor to measure distance
            Vector3 flatTowerPos = new Vector3(nearestTower.transform.position.x, 0, nearestTower.transform.position.z);
            Vector3 flatBlockPos = new Vector3(transform.position.x, 0, transform.position.z);

            flatDistance = Vector3.Distance(flatTowerPos, flatBlockPos);
        }

        // 2. Check if we are close enough (Horizontally)
        // We use 2.0f to be generous so you don't have to be perfect
        if (nearestTower != null && flatDistance < 2.0f)
        {
            // --- SNAP LOGIC STARTS HERE ---

            // Get the forced position from the Tower script
            Vector3 newPos = nearestTower.GetNextSnapPosition();

            // APPLY: This forces the block to teleport to the correct stack position
            transform.position = newPos;

            // Add to the tower's list
            nearestTower.AddBlock(this);
            currentTower = nearestTower;

            // Check for Win Condition
            if (GameManager.Instance != null)
            {
                GameManager.Instance.CheckForWin();
            }

            // --- SNAP LOGIC ENDS HERE ---
        }
        else
        {
            // Invalid Drop -> Return to start
            transform.position = startPos;
        }
    }
    private Tower FindClosestTower()
    {
        GameObject[] bases = GameObject.FindGameObjectsWithTag("TowerBase");
        Tower closest = null;
        float minDist = Mathf.Infinity;
        Vector3 currentPos = transform.position;

        foreach (GameObject t in bases)
        {
            // IGNORE HEIGHT: We only care about X and Z distance
            // Create temporary vectors that are flat on the ground
            Vector3 flatTowerPos = new Vector3(t.transform.position.x, 0, t.transform.position.z);
            Vector3 flatBlockPos = new Vector3(currentPos.x, 0, currentPos.z);

            float dist = Vector3.Distance(flatTowerPos, flatBlockPos);

            if (dist < minDist)
            {
                closest = t.GetComponent<Tower>();
                minDist = dist;
            }
        }
        return closest;
    }

    public void SetCurrentTower(Tower t)
    {
        currentTower = t;
    }
}