using UnityEngine;
using System.Collections.Generic;

public class Tower : MonoBehaviour
{
    [Header("Configuration")]
    public Transform snapStartPoint;
    public float verticalSpacing = 0.2f;
    public int maxCapacity = 100;

    public Vector3 customBlockRotation = new Vector3(0, 180, 0);
    public List<DraggableBlock> blocks = new List<DraggableBlock>();

    public bool HasSpace()
    {
        return blocks.Count < maxCapacity;
    }

    public Vector3 GetNextSnapPosition()
    {
        if (snapStartPoint == null) return transform.position;

        Vector3 direction = Vector3.up;

        float startLift = 0.1f;

        float heightOffset = (blocks.Count * verticalSpacing) + startLift;

        return snapStartPoint.position + (direction * heightOffset);
    }

    public void AddBlock(DraggableBlock block)
    {
        if (!HasSpace()) return;

        blocks.Add(block);
        block.transform.SetParent(transform);
        block.transform.rotation = Quaternion.Euler(customBlockRotation);
    }

    public void RemoveBlock(DraggableBlock block)
    {
        if (blocks.Contains(block)) blocks.Remove(block);
    }

    public bool IsTopBlock(DraggableBlock block)
    {
        if (blocks.Count == 0) return false;
        return blocks[blocks.Count - 1] == block;
    }
}