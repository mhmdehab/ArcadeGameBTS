using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using TMPro;
public class TreeSpawner : MonoBehaviour
{
    public TMP_InputField nodeCountInput;
    private GameObject currentTree = null;
    public GameObject Tree1;
    public GameObject Tree2;
    public GameObject Tree3;
    public GameObject nodePrefab;
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
        List<int> numbers = new List<int>();
        while (numbers.Count < count)
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
    void GenerateTree(int totalNodes)
    {
        GameObject treeRoot = new GameObject("GeneratedTree");
        treeRoot.transform.position = treeSpawnPoint.position;
        treeRoot.transform.rotation = Quaternion.Euler(90f, 0f, 0f);

        List<GameObject> nodes = new List<GameObject>();

        GameObject rootnode = Instantiate(nodePrefab, treeRoot.transform);
        rootnode.GetComponent<Node>().level = 0;
        rootnode.transform.position = new Vector3(-1.335f, 0.93f, 5.39f);
        nodes.Add(rootnode);
        for (int i = 1; i < totalNodes; i++)
        {

            GameObject node = Instantiate(nodePrefab, treeRoot.transform);
            int nodeLevel = (int)Mathf.Floor(Mathf.Log(i + 1, 2));
            node.GetComponent<Node>().level = nodeLevel;
            int parentindex = (i - 1) / 2;
            bool isLeft = (i == 2 * parentindex + 1);
            node.GetComponent<Node>().isLeftChild = isLeft;
            GameObject parentNode = nodes[parentindex];
            node.transform.SetParent(parentNode.transform);
            float spacing = 4f / Mathf.Pow(2, nodeLevel - 1);
            float xOffset = isLeft ? -spacing : spacing;
            float yOffset = -2f;
            node.transform.localPosition = new Vector3(xOffset, yOffset, 0);
            nodes.Add(node);
        }

        currentTree = treeRoot;
    }

    private void Update()
    {
        if (playerInZone && currentTree == null)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                nodeCountInput.gameObject.SetActive(true);
                nodeCountInput.Select();
            }
        }

        // This should be separate, not inside the E key check
        if (nodeCountInput.gameObject.activeSelf && Input.GetKeyDown(KeyCode.Return))
        {
            string inputText = nodeCountInput.text;
            if (!string.IsNullOrEmpty(inputText))
            {
                int nodeCount;
                if (int.TryParse(inputText, out nodeCount) && nodeCount > 0)
                {
                    nodeCountInput.gameObject.SetActive(false);
                    nodeCountInput.text = "";
                    GenerateTree(nodeCount);
                    GenerateBalls(nodeCount);
                }
                else
                {
                    Debug.Log("Please enter a valid positive number");
                }
            }
        }

        if (currentTree != null && Input.GetKeyDown(KeyCode.X))
        {
            Destroy(currentTree);
            currentTree = null;
            foreach (GameObject ball in currentBalls)
            {
                Destroy(ball);
            }
            currentBalls.Clear();
        }
    }
}