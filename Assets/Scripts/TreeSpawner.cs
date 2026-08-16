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
    private ScoreTimer scoreTimer;
    public GameObject cylinderPrefab;
    public GameObject numberedBallPrefab;
    public Transform basketPosition;
    public GameObject startButton;
    public GameObject resetButton;
    private List<Node> preOrderSequence = new List<Node>();
    private List<Node> postOrderSequence = new List<Node>();
    public enum GameStyle
    {
        Normal, PreOrder, PostOrder
    }
    private GameStyle gameStyle;

    private List<GameObject> currentBalls = new List<GameObject>();

    private bool playerInZone = false;
    void Start()
    {
            scoreTimer = FindObjectOfType<ScoreTimer>();
            startButton.SetActive(false);
            resetButton.SetActive(false);
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInZone = true;
            Debug.Log("player entered");
        }

    }

    void BuildPreOrder(List<GameObject> nodes, int index, List<Node> result)
    {
        if (index < nodes.Count)
        {
            result.Add(nodes[index].GetComponent<Node>());
            if (2 * index + 1 < nodes.Count)
            {
                BuildPreOrder(nodes, 2 * index + 1, result);
            }
            if (2 * index + 2 < nodes.Count)
            {
                BuildPreOrder(nodes, 2 * index + 2, result);
            }


        }
    }
    void BuildPostOrder(List<GameObject> nodes, int index, List<Node> result)
    {
        if (index < nodes.Count)
        {
            
            if (2 * index + 2 < nodes.Count)
            {
                BuildPostOrder(nodes, 2 * index + 2, result);
            }
            if (2 * index + 1 < nodes.Count)
            {
                BuildPostOrder(nodes, 2 * index + 1, result);
                
            }
            result.Add(nodes[index].GetComponent<Node>());
        }
     }

    void CreateCylinder(Vector3 start, Vector3 end)
    {
        Vector3 midpoint = (start + end) / 2;
        float distance = Vector3.Distance(start, end);

        GameObject cylinder = Instantiate(cylinderPrefab, midpoint, Quaternion.identity);
        cylinder.transform.up = (end - start).normalized;
        cylinder.transform.localScale = new Vector3(0.1f, distance / 2, 0.1f);
        cylinder.transform.SetParent(currentTree.transform);
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
            Vector3 spawnPos = basketPosition.position + new Vector3(Random.Range(-1f, 1f), Random.Range(0, 1f), Random.Range(-1f, 1f));
            GameObject ball = Instantiate(numberedBallPrefab, spawnPos, Quaternion.identity);
            ball.GetComponent<Ball>().setBallNumber(numbers[i]);
            currentBalls.Add(ball);
        }
    }
    void GenerateTree(int totalNodes)
    {
        GameObject treeRoot = new GameObject("GeneratedTree");
        treeRoot.transform.position = treeSpawnPoint.position;
        treeRoot.transform.rotation = Quaternion.identity;
        currentTree = treeRoot;
        List<GameObject> nodes = new List<GameObject>();

        GameObject rootnode = Instantiate(nodePrefab, treeRoot.transform);
        rootnode.GetComponent<Node>().level = 0;
        rootnode.transform.position = new Vector3(-1.335f, 5f, 5.39f);
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
            CreateCylinder(parentNode.transform.position, node.transform.position);
            nodes.Add(node);
        }

        preOrderSequence.Clear();
        postOrderSequence.Clear();
        BuildPreOrder(nodes, 0, preOrderSequence);
        for (int i = 0; i < preOrderSequence.Count; i++)
        {
            preOrderSequence[i].preorderIndex = i;
        }
        BuildPostOrder(nodes, 0, postOrderSequence);
        for (int i = 0; i < postOrderSequence.Count; i++)
        {
            postOrderSequence[i].postorderIndex = i;
        }
        foreach (GameObject go in nodes)
        {
            go.GetComponent<Node>().currentGameStyle = gameStyle;
        }

    }

    private void Update()
    {
        startButton.SetActive(playerInZone && currentTree == null && !nodeCountInput.gameObject.activeSelf);
        resetButton.SetActive(playerInZone && currentTree != null);

        if (playerInZone && currentTree == null)
        {
            if (Input.GetKeyDown(KeyCode.J))
            {
                gameStyle = GameStyle.Normal;
                startButton.SetActive(false);
                nodeCountInput.gameObject.SetActive(true);
                nodeCountInput.text = "";
                nodeCountInput.Select();
            }
            if (Input.GetKeyDown(KeyCode.K))
            {
                gameStyle = GameStyle.PostOrder;
                startButton.SetActive(false);
                nodeCountInput.gameObject.SetActive(true);
                nodeCountInput.text = "";
                nodeCountInput.Select();
            }
            if (Input.GetKeyDown(KeyCode.L))
            {
                gameStyle = GameStyle.PreOrder;
                startButton.SetActive(false);
                nodeCountInput.gameObject.SetActive(true);
                nodeCountInput.text = "";
                nodeCountInput.Select();
            }
        }

        if (nodeCountInput.gameObject.activeSelf && Input.GetKeyDown(KeyCode.Return))
        {
            string inputText = nodeCountInput.text;
            if (!string.IsNullOrEmpty(inputText))
            {
                int nodeCount;
                if (int.TryParse(inputText, out nodeCount) && nodeCount > 0 && nodeCount <= 32)
                    {
                    nodeCountInput.gameObject.SetActive(false);
                    nodeCountInput.text = "";
                    GenerateTree(nodeCount);
                    GenerateBalls(nodeCount);
                    scoreTimer.startTimer(nodeCount);
                }
                else
                {
                    Debug.Log("Please enter a valid positive number");
                }
            }
          
        }

        if (currentTree != null && Input.GetKeyDown(KeyCode.X))
        {
            scoreTimer.resetTimer();
            Destroy(currentTree);
            currentTree = null;
            foreach (GameObject ball in currentBalls)
            {
                Destroy(ball);
            }
            currentBalls.Clear();
        }
    }


    public void OnStartButtonPressed()
    {
        startButton.SetActive(false);
        nodeCountInput.gameObject.SetActive(true);
        nodeCountInput.text = "";
        nodeCountInput.Select();
    }
    public void OnResetButtonPressed()
    {
        scoreTimer.resetTimer();
        Destroy(currentTree);
        currentTree = null;
        foreach (GameObject ball in currentBalls)
        {
            Destroy(ball);
        }
        currentBalls.Clear();
    }
    public bool IsTreeComplete()
    {
        Node[] allNodes = currentTree.GetComponentsInChildren<Node>();
        foreach (Node node in allNodes)
        {
            if (!node.isFilled)
                return false;
        }
        return true;
    }

}