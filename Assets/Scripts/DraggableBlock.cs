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
        startPos = transform.position;
        TMP_Text textComponent = GetComponentInChildren<TMP_Text>();

        if (textComponent != null && textComponent.text.Length > 0)
        {
            letter = textComponent.text[0];
            gameObject.name = "Block_" + letter;
        }
    }

    void Update()
    {
        if (Mouse.current == null) return;

        // Mouse Down
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            TryPickUp();
        }

        // Mouse Up
        if (Mouse.current.leftButton.wasReleasedThisFrame && isDragging)
        {
            DropBlock();
        }

        // Dragging
        if (isDragging)
        {
            MoveBlock();
        }
    }

    public void InitializeBlock(char c)
    {
        letter = c;
        gameObject.name = "Block_" + c;
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
                if (currentTower != null)
                {
                    if (!currentTower.IsTopBlock(this))
                    {
                        return; // Can only pick up top block
                    }

                    // Remove from the tower
                    currentTower.RemoveBlock(this);
                    currentTower = null;

                    // --- FIX: Removed the broken "UpdateUI" call here ---
                    // Instead, we just check win conditions to update colors
                    if (GameManager.Instance != null)
                    {
                        GameManager.Instance.CheckForWin();
                    }
                    // ---------------------------------------------------
                }

                isDragging = true;
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
        float flatDistance = 100f;

        if (nearestTower != null)
        {
            Vector3 flatTowerPos = new Vector3(nearestTower.transform.position.x, 0, nearestTower.transform.position.z);
            Vector3 flatBlockPos = new Vector3(transform.position.x, 0, transform.position.z);
            flatDistance = Vector3.Distance(flatTowerPos, flatBlockPos);
        }

        if (nearestTower != null && flatDistance < 2.0f)
        {
            // Snap to tower
            Vector3 newPos = nearestTower.GetNextSnapPosition();
            transform.position = newPos;

            nearestTower.AddBlock(this);
            currentTower = nearestTower;

            // Check for Win (This works because GameManager has this function!)
            if (GameManager.Instance != null)
            {
                GameManager.Instance.CheckForWin();
            }
        }
        else
        {
            // Return to start
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