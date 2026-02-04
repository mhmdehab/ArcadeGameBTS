using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class TreeSpawner : MonoBehaviour 
{
    private GameObject currentTree=null;
    public GameObject Tree1;
    public GameObject Tree2;
    public GameObject Tree3;
    public Transform treeSpawnPoint;

    public GameObject numberedBallPrefab;
    public Transform basketPosition;

    private List<GameObject> currentBalls = new List<GameObject>();

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
    void GenerateBalls(int count)
    {
        List<int> numbers= new List<int>();
        while (numbers.Count<count)
        {
            int randomNum = Random.Range(1, 101);
            if (!numbers.Contains(randomNum))
            {
                numbers.Add(randomNum);
            }
        }
        for (int i = 0; i < numbers.Count; i++)
        {
            Vector3 spawnPos = basketPosition.position + new Vector3(Random.Range(-0.2f, 0.2f), Random.Range(0, 0.3f), Random.Range(-0.2f, 0.2f));
            GameObject ball = Instantiate(numberedBallPrefab, basketPosition.position, Quaternion.identity);
            ball.GetComponent<Ball>().setBallNumber(numbers[i]);
            currentBalls.Add(ball);
        }
    }

    private void Update()
    {
        if (playerInZone && currentTree==null)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                currentTree=Instantiate(Tree1 , treeSpawnPoint.position,treeSpawnPoint.rotation);
                GenerateBalls(3);
            }
            if (Input.GetKeyDown(KeyCode.R))
            {
                currentTree=Instantiate(Tree2,treeSpawnPoint.position,treeSpawnPoint.rotation);
                GenerateBalls(7);
            }
            if (Input.GetKeyDown(KeyCode.T))
            {
                currentTree=Instantiate(Tree3,treeSpawnPoint.position,treeSpawnPoint.rotation);
                GenerateBalls(15);
            }
        }
        if (currentTree!=null &&Input.GetKeyDown(KeyCode.X))
        {
            Destroy(currentTree);
            currentTree=null;

            foreach (GameObject ball in currentBalls) { 
            Destroy(ball);
            }
            currentBalls.Clear();
        }
    }
}
