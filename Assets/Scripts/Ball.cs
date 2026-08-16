using UnityEngine;
using TMPro;
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
    public void HideNumber()
    {
        if (numberText != null)
        {
            numberText.text = "";
        }
    }
}
