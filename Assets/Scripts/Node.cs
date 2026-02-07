using UnityEngine;

public class Node : MonoBehaviour
{
    public int level;
    public bool isLeftChild;
    public bool isFilled = false;
    public int nodeValue;

    private Material nodeMaterial;

    private Node[] allNodes;

    private void Start()
    {
        nodeMaterial = GetComponent<Renderer>().material;

        allNodes = GetComponentsInParent<Transform>(true)[0].GetComponentsInChildren<Node>();
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
        nodeMaterial.color = Color.yellow;
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


            if (!IsLevelUnlocked())
            {
                Debug.Log("Previous levels not filled yet!");
                return;
            }
            if (!isParentFilled())
            {
                Debug.Log("Parent node not filled yet!");
                return;
            }
            if (!isValliedBSTPlacment(ballNumber))
            {
                Debug.Log("Invalid Bst placement!");
                return;
            }

            fillNumber(ballNumber);
            Destroy(collision.gameObject);
            Debug.Log("Ball with number "+ballNumber+" hit the node at lvl "+level);
        }

    }
}
