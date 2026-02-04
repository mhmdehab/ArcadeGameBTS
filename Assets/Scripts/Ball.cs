using UnityEngine;

public class Ball : MonoBehaviour
{
    public int ballNumber;
    private TextMesh numberText;



 
  

    public void setBallNumber(int number)
    {
        ballNumber = number;
        if (numberText == null)
        {
            numberText = GetComponentInChildren<TextMesh>();
        }
        numberText.text = number.ToString();
    }
}
