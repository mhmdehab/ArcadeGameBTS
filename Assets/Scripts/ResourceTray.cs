using UnityEngine;
using System.Collections.Generic;
using System.Linq;

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
            while (trayBlocks.Count < arrayCapacity)
            {
                trayBlocks.Add(null);
            }
        }
        else
        {
            trayBlocks.RemoveAll(b => b == null);
            RearrangeBlocks();
        }
    }

    public Vector3 GetNextPosition(int index)
    {
        if (spawnStartPoint == null) return transform.position;
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
        if (currentMode == ResourceType.Array)
        {
            int emptySlot = trayBlocks.IndexOf(null);
            if (emptySlot != -1) trayBlocks[emptySlot] = block;
            else return;
        }
        else
        {
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
                trayBlocks[index] = null;
            }
            else
            {
                trayBlocks.Remove(block);
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

        if (currentMode == ResourceType.Array) ConfigureMode(ResourceType.Array);
    }

    public bool CanPickUp(DraggableBlock block)
    {
        int index = trayBlocks.IndexOf(block);

        if (index == -1) return true;

        switch (currentMode)
        {
            case ResourceType.Array:
                return true;

            case ResourceType.Queue:
                if (index == 0) return true;

                if (GameManager.Instance != null)
                {
                    GameManager.Instance.ShowMessage("In a Queue, you can only move (DEQUEUE) the first (leftmost) block!", 3f, Color.red);
                }
                return false;

            case ResourceType.Stack:
                if (index == trayBlocks.Count - 1) return true;

                if (GameManager.Instance != null)
                {
                    GameManager.Instance.ShowMessage("In a Stack, you can only move (POP) the top (rightmost) block!", 3f, Color.red);
                }
                return false;

            default:
                return true;
        }
    }

    public int GetBlockIndex(DraggableBlock block)
    {
        return trayBlocks.IndexOf(block);
    }
}