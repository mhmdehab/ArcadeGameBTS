using System.Net.NetworkInformation;
using TMPro;
using UnityEngine;
using static UnityEngine.Tilemaps.TilemapRenderer;

public class Node : MonoBehaviour
{
    public int level;
    public bool isLeftChild;
    public bool isFilled = false;
    public int nodeValue;
    private Color nodeColor;

    private Material nodeMaterial;

    private Node[] allNodes;

    private TextMeshPro nodeText;
    private CongratManager congratManager;
    private TreeSpawner treeSpawner;
    private ScoreTimer scoreTimer;
    public int preorderIndex;
    public int postorderIndex;
    public TreeSpawner.GameStyle currentGameStyle;
    private void Start()
    {
        congratManager = FindObjectOfType<CongratManager>();
        nodeMaterial = GetComponent<Renderer>().material;
        nodeColor = nodeMaterial.color;
        treeSpawner = FindObjectOfType<TreeSpawner>();
        scoreTimer = FindObjectOfType<ScoreTimer>();

        Transform root = transform;
        while (root.parent != null)
        {
            root = root.parent;
        }
        allNodes = root.GetComponentsInChildren<Node>();
        nodeText = GetComponentInChildren<TextMeshPro>();
        foreach (Transform child in transform) { 
            TextMeshPro temp= child.GetComponent<TextMeshPro>();
            if (temp !=null )
            {
                nodeText = temp;
                nodeText.text = "";
                break;
            }
        }
    }
    private void FlashRed()
    {
        nodeMaterial.color = Color.red;
        Invoke("ResetColor", 0.3f);
        scoreTimer.subScore();

    }
    private void ResetColor()
    {
        nodeMaterial.color = nodeColor;
        

    }
    bool IsLevelUnlocked()
    {
        if (level==0)
        {
            return true;
        }
        for (int i = 0; i < level; i++)
        {
            foreach (Node node in allNodes)
            {
                if (node.level == i && !node.isFilled)
                {
                    return false;
                }
            }
        }
        return true;
    }
    bool IsNextInSequence()
    {
        int filledCount = 0;
        foreach (Node node in allNodes)
        {
            if (node.isFilled)
            {
                filledCount++;
            }
        }
        Debug.Log("Checking node with postorderIndex=" + postorderIndex + ", preorderIndex=" + preorderIndex + ", filledCount=" + filledCount + ", style=" + currentGameStyle);

        if (currentGameStyle == TreeSpawner.GameStyle.PostOrder)
        {
            return filledCount == postorderIndex;
        }
        if (currentGameStyle == TreeSpawner.GameStyle.PreOrder)
        {
            return filledCount == preorderIndex;
        }
        return false;
    }

    bool isParentFilled()
    {
        if (level == 0) return true;
        
            Node parentNode = transform.parent.GetComponent<Node>();

        return parentNode.isFilled;
        
    }
    bool isValliedBSTPlacment(int number) {

        if (level==0) return true;
        Node parentNode = transform.parent.GetComponent<Node>();

        if (isLeftChild)
        {
            if (number >= parentNode.nodeValue)
            { return false; }    
        }
        else
        {
            if (number<= parentNode.nodeValue)
            {
                return false;
            }
        }
        return true;



    }
    public void fillNumber(int number)
    {
        isFilled = true;
        nodeValue = number;
        nodeMaterial.color = Color.black;
        scoreTimer.addScore();

        Debug.Log("Filling node with number: " + number + ", nodeText is null? " + (nodeText == null));

        if (nodeText != null)
        {
            nodeText.text = number.ToString();
            Debug.Log("Text updated to: " + nodeText.text);
        }
       
    }


    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ball"))
        {
            if (isFilled)
            {
                Debug.Log("Node already filled");
                return;
            }

            Ball ball = collision.gameObject.GetComponent<Ball>();
            int ballNumber = ball.ballNumber;

            if (currentGameStyle == TreeSpawner.GameStyle.Normal)
            {

                if (!IsLevelUnlocked())
                {
                    FlashRed();
                    Debug.Log("Previous levels not filled yet!");
                    return;
                }
                if (!isParentFilled())
                {
                    FlashRed();
                    Debug.Log("Parent node not filled yet!");
                    return;
                }
                if (!isValliedBSTPlacment(ballNumber))
                {
                    FlashRed();
                    Debug.Log("Invalid Bst placement!");
                    return;
                }

                fillNumber(ballNumber);
            }
            else
            {
                if (!IsNextInSequence())
                {
                    FlashRed();
                    Debug.Log("Not the correct next node in sequence!");
                    return;
                }
                fillNumber(ballNumber);
            }
                Destroy(collision.gameObject);
            if (BallHud.Instance != null)
                BallHud.Instance.HideNumber();
            if (treeSpawner.IsTreeComplete())
            {
                Debug.Log("Tree is complete!");
                congratManager.ShowCongrats();
                scoreTimer.StopTimer();
            }
            Debug.Log("Ball with number "+ballNumber+" hit the node at lvl "+level);
        }

    }
}
