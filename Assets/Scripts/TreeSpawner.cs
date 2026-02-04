using UnityEditor;
using UnityEngine;

public class TreeSpawner : MonoBehaviour 
{
    private GameObject currentTree=null;
    public GameObject Tree1;
    public GameObject Tree2;
    public GameObject Tree3;
    public Transform treeSpawnPoint;

    private bool playerInZone = false;
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInZone = true;
            Debug.Log("player entered");
        }
        
    }
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInZone = false;
            Debug.Log("player left");
        }
        
    }

    private void Update()
    {
        if (playerInZone && currentTree==null)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                currentTree=Instantiate(Tree1 , treeSpawnPoint.position,treeSpawnPoint.rotation);
            }
            if (Input.GetKeyDown(KeyCode.R))
            {
                currentTree=Instantiate(Tree2,treeSpawnPoint.position,treeSpawnPoint.rotation);
            }
            if (Input.GetKeyDown(KeyCode.T))
            {
                currentTree=Instantiate(Tree3,treeSpawnPoint.position,treeSpawnPoint.rotation);
            }
        }
        if (currentTree!=null &&Input.GetKeyDown(KeyCode.X))
        {
            Destroy(currentTree);
            currentTree=null;
        }
    }
}
