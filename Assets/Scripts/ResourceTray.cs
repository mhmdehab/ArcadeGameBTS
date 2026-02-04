using UnityEngine;
using System.Collections.Generic;
using System.Linq; // NEED THIS for RemoveAll

public class ResourceTray : MonoBehaviour
{
    [Header("Configuration")]
    public float blockSpacing = 1.2f;
    public Transform spawnStartPoint;
    public int arrayCapacity = 20;

    public Vector3 customBlockRotation = new Vector3(0, 0, 0);

    public List<DraggableBlock> trayBlocks = new List<DraggableBlock>();
    private ResourceType currentMode;

    void Awake()
    {
        if (trayBlocks == null) trayBlocks = new List<DraggableBlock>();
    }

    public void ConfigureMode(ResourceType mode)
    {
        currentMode = mode;

        if (currentMode == ResourceType.Array)
        {
            // Fill with nulls if needed
            while (trayBlocks.Count < arrayCapacity)
            {
                trayBlocks.Add(null);
            }
        }
        else
        {
            // --- FIX 1: CLEANUP ---
            // If switching to Stack/Queue, DELETE all empty ghost slots immediately
            trayBlocks.RemoveAll(b => b == null);
            RearrangeBlocks();
        }
    }

    public Vector3 GetNextPosition(int index)
    {
        if (spawnStartPoint == null) return transform.position;
        // Tip: Rotate your 'StartPoint' object in Unity to change this direction!
        Vector3 direction = spawnStartPoint.right;
        Vector3 offset = direction * (index * blockSpacing);
        return spawnStartPoint.position + offset;
    }

    public int GetIndexFromWorldPos(Vector3 worldPos)
    {
        if (spawnStartPoint == null) return 0;
        Vector3 diff = worldPos - spawnStartPoint.position;
        float distAlongShelf = Vector3.Dot(diff, spawnStartPoint.right);
        int index = Mathf.RoundToInt(distAlongShelf / blockSpacing);
        return Mathf.Clamp(index, 0, arrayCapacity - 1);
    }

    public void AddBlock(DraggableBlock block)
    {
        // 1. Array Logic
        if (currentMode == ResourceType.Array)
        {
            int emptySlot = trayBlocks.IndexOf(null);
            if (emptySlot != -1) trayBlocks[emptySlot] = block;
            else return;
        }
        // 2. Stack/Queue Logic
        else
        {
            // --- FIX 2: NO GHOSTS ---
            // Ensure we don't have nulls before adding
            trayBlocks.RemoveAll(b => b == null);
            trayBlocks.Add(block);
        }

        block.transform.SetParent(transform);
        ResetBlockScale(block);
        RearrangeBlocks();
    }

    public void RemoveBlock(DraggableBlock block)
    {
        int index = trayBlocks.IndexOf(block);
        if (index != -1)
        {
            if (currentMode == ResourceType.Array)
            {
                trayBlocks[index] = null; // Leave hole
            }
            else
            {
                trayBlocks.Remove(block); // Collapse hole
                RearrangeBlocks();
            }
        }
    }

    public bool TryPlaceBlockInArray(DraggableBlock block, Vector3 dropPos, int oldIndex)
    {
        if (currentMode != ResourceType.Array)
        {
            ReturnBlockToTray(block);
            return true;
        }

        int targetIndex = GetIndexFromWorldPos(dropPos);

        if (trayBlocks[targetIndex] == null)
        {
            trayBlocks[targetIndex] = block;
            block.transform.SetParent(transform);
            ResetBlockScale(block);
            RearrangeBlocks();
            return true;
        }
        else
        {
            trayBlocks[oldIndex] = block;
            block.transform.SetParent(transform);
            ResetBlockScale(block);
            RearrangeBlocks();
            return false;
        }
    }

    public void ReturnBlockToTray(DraggableBlock block)
    {
        if (currentMode == ResourceType.Array)
        {
            AddBlock(block);
        }
        else
        {
            // --- FIX 3: SIMPLE ADD ---
            if (!trayBlocks.Contains(block)) trayBlocks.Add(block);
        }

        block.transform.SetParent(transform);
        ResetBlockScale(block);
        RearrangeBlocks();
    }

    private void ResetBlockScale(DraggableBlock block)
    {
        block.transform.localScale = new Vector3(
            1f / transform.localScale.x,
            1f / transform.localScale.y,
            1f / transform.localScale.z
        );
    }

    public void RearrangeBlocks()
    {
        // --- SAFETY CHECK ---
        // If we are in Stack/Queue, remove any nulls that snuck in
        if (currentMode != ResourceType.Array)
        {
            trayBlocks.RemoveAll(b => b == null);
        }

        for (int i = 0; i < trayBlocks.Count; i++)
        {
            if (trayBlocks[i] != null)
            {
                trayBlocks[i].transform.position = GetNextPosition(i);
                trayBlocks[i].transform.localRotation = Quaternion.Euler(customBlockRotation);
            }
        }
    }

    public void ClearTray()
    {
        foreach (var b in trayBlocks) if (b != null) Destroy(b.gameObject);
        trayBlocks.Clear();

        // Only refill nulls if we are staying in Array mode
        if (currentMode == ResourceType.Array) ConfigureMode(ResourceType.Array);
    }

    public bool CanPickUp(DraggableBlock block)
    {
        int index = trayBlocks.IndexOf(block);
        if (index == -1) return true;

        switch (currentMode)
        {
            case ResourceType.Array: return true;

            // Queue: Pick the HEAD (First item, Left side)
            case ResourceType.Queue: return index == 0;

            // Stack: Pick the TOP (Last item, Right side)
            case ResourceType.Stack: return index == trayBlocks.Count - 1;

            default: return true;
        }
    }

    public int GetBlockIndex(DraggableBlock block)
    {
        return trayBlocks.IndexOf(block);
    }
}