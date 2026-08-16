using UnityEngine;

public class CongratManager : MonoBehaviour
{
    public GameObject congratsPanel;
    public float displayDuration = 3f;

    void Start()
    {
        congratsPanel.SetActive(false);
    }
    public void ShowCongrats()
    {
        congratsPanel.SetActive(true);
        Invoke("HideCongrats", displayDuration);
    }

    void HideCongrats()
    {
        congratsPanel.SetActive(false);
    }
}
