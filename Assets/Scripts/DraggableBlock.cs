using UnityEngine;
using TMPro;

public class DraggableBlock : MonoBehaviour
{
    [Header("Block Data")]
    public char letter;

    [Header("References")]
    public Camera mainCamera;
    public TMP_Text textComponent;

    [Header("Settings")]
    public Vector3 dragRotation = new Vector3(0, 180, 0);

    private Vector3 startPos;
    private Vector3 originalScale;
    private bool isDragging = false;
    private Rigidbody rb;
    private Tower currentTower;

    private float fixedX;

    private int storedArrayIndex = -1;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();

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
                ResourceTray tray = GetComponentInParent<ResourceTray>();
                if (tray != null)
                {
                    if (!tray.CanPickUp(this)) return;

                    storedArrayIndex = tray.GetBlockIndex(this);

                    tray.RemoveBlock(this);
                }

                if (currentTower != null)
                {
                    if (!currentTower.IsTopBlock(this))
                    {
                        if (GameManager.Instance != null)
                        {
                            GameManager.Instance.ShowMessage("In a Stack, you can only move (POP) the top block!", 3f, Color.red);
                        }
                        return; 
                    }

                    currentTower.RemoveBlock(this);
                    currentTower = null;
                    if (GameManager.Instance != null) GameManager.Instance.CheckForWin();
                }

                isDragging = true;
                startPos = transform.position;

                transform.SetParent(null);
                transform.localScale = originalScale; 

                fixedX = transform.position.x;

                transform.rotation = Quaternion.Euler(dragRotation);

                rb.isKinematic = true;
            }
        }
    }

    private void DragBlock()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

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

        bool droppedOnTray = false;
        if (tray != null)
        {
            Collider trayCollider = tray.GetComponent<Collider>();
            if (trayCollider != null)
            {
                Vector3 closest = trayCollider.ClosestPoint(transform.position);
                if (Vector3.Distance(transform.position, closest) < 1.0f) droppedOnTray = true;
            }
            else
            {
                float d = Vector3.Distance(transform.position, tray.transform.position);
                if (d < 6.0f) droppedOnTray = true;
            }
        }

        float distanceToTower = 100f;
        if (nearestTower != null)
        {
            distanceToTower = Vector3.Distance(transform.position, nearestTower.transform.position);
        }

        if (nearestTower != null && distanceToTower < 1.25f && nearestTower.HasSpace())
        {
            Vector3 newPos = nearestTower.GetNextSnapPosition();
            transform.position = newPos;
            nearestTower.AddBlock(this);
            SetCurrentTower(nearestTower);
            if (GameManager.Instance != null) GameManager.Instance.CheckForWin();
        }

        else if (droppedOnTray)
        {
            if (storedArrayIndex != -1)
            {
                tray.TryPlaceBlockInArray(this, transform.position, storedArrayIndex);
            }
            else
            {
                tray.ReturnBlockToTray(this);
            }

            SetCurrentTower(null);
            storedArrayIndex = -1; 
        }

        else
        {
            if (tray != null)
            {
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