using UnityEngine;
using TMPro;

public class BallHud : MonoBehaviour
{
    public static BallHud Instance;

    [SerializeField] private GameObject hudPanel;
    [SerializeField] private TextMeshProUGUI numberLabel;

    void Awake()
    {
           Instance = this;
           hudPanel.SetActive(false);
    }
    public void ShowNumber(int number)
    {
        numberLabel.text = number.ToString();
        hudPanel.SetActive(true);
    }

    public void HideNumber()
    {
        hudPanel.SetActive(false);
    }
}
