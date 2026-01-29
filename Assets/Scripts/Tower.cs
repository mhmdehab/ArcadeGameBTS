using UnityEngine;
using System.Collections.Generic;

public class Tower : MonoBehaviour
{
    // A list to track what blocks are currently on this tower
    public List<DraggableBlock> blocks = new List<DraggableBlock>();

    [Header("Stacking Settings")]
    // CHANGE: Adjust this to match your block's Y scale (e.g., 0.2 or 0.25)
    public float blockHeight = 0.25f;

    // CHANGE: Adjust this to lower the first block (e.g., 0.1) so it sits flat
    public float startOffset = 0.1f;

    // Helper: Where should the next block go?
    public Vector3 GetNextSnapPosition()
    {
        // 1. Start with the Tower Base position
        Vector3 snapPos = transform.position;

        // 2. Calculate the new Y based on your custom settings
        // Formula: Base Y + StartOffset + (Number of existing blocks * Block Height)
        snapPos.y = transform.position.y + startOffset + (blocks.Count * blockHeight);

        // 3. Keep the X and Z aligned with the tower base
        snapPos.x = transform.position.x;
        snapPos.z = transform.position.z;

        return snapPos;
    }

    public void AddBlock(DraggableBlock block)
    {
        blocks.Add(block);
    }

    public void RemoveBlock(DraggableBlock block)
    {
        blocks.Remove(block);
    }

    // Check if the specific block is the last one in the list (the Top)
    public bool IsTopBlock(DraggableBlock block)
    {
        if (blocks.Count == 0) return false;

        // Compare the requested block with the last block in the list
        return blocks[blocks.Count - 1] == block;
    }
}