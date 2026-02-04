using UnityEngine;
using UnityEngine.UI;
using TMPro;

// This Enum lets us pick between the 3 modes
public enum ResourceType { None, Array, Stack, Queue }

public class MainMenuController : MonoBehaviour
{
    [Header("Word Count Section")]
    public Button[] wordCountButtons; // We will drag Btn_1, Btn_2, Btn_3 here
    public TMP_Text wordCountDescription;

    [Header("Resource Type Section")]
    public Button btnArray;
    public Button btnStack;
    public Button btnQueue;
    public TMP_Text resourceDescription;

    [Header("Start Control")]
    public Button startButton;

    // Internal Variables
    private int selectedWordCount = 0;
    private ResourceType selectedType = ResourceType.None;

    // Colors
    private Color colorSelected = Color.green;
    private Color colorDefault = Color.white;

    void Start()
    {
        // 1. Setup Word Count Buttons (1, 2, 3)
        // Note: The index 'i' is 0, 1, 2... so we add +1 to get the actual count
        for (int i = 0; i < wordCountButtons.Length; i++)
        {
            int count = i + 1;
            wordCountButtons[i].onClick.AddListener(() => SelectWordCount(count));
        }

        // 2. Setup Resource Buttons
        btnArray.onClick.AddListener(() => SelectResourceType(ResourceType.Array));
        btnStack.onClick.AddListener(() => SelectResourceType(ResourceType.Stack));
        btnQueue.onClick.AddListener(() => SelectResourceType(ResourceType.Queue));

        // 3. Setup Start Button
        startButton.onClick.AddListener(OnStartClicked);

        ResetUI();
    }

    void ResetUI()
    {
        selectedWordCount = 0;
        selectedType = ResourceType.None;

        wordCountDescription.text = "Select difficulty level...";
        resourceDescription.text = "Select a data structure...";

        startButton.interactable = false; // Disable start button
        UpdateButtonVisuals();
    }

    void SelectWordCount(int count)
    {
        selectedWordCount = count;

        if (count == 1) wordCountDescription.text = "Beginner: Solve 1 word (Middle Tower).";
        else if (count == 2) wordCountDescription.text = "Intermediate: Solve 2 words (Left & Right).";
        else if (count == 3) wordCountDescription.text = "Expert: Solve 3 words (All Towers).";

        UpdateButtonVisuals();
        CheckReadyToStart();
    }

    void SelectResourceType(ResourceType type)
    {
        selectedType = type;

        switch (type)
        {
            case ResourceType.Array:
                resourceDescription.text = "<b>Array (Easy)</b>\nRandom Access: Pick ANY block you want.";
                break;
            case ResourceType.Stack:
                resourceDescription.text = "<b>Stack (Medium)</b>\nLIFO: You can only pick the RIGHTMOST block.";
                break;
            case ResourceType.Queue:
                resourceDescription.text = "<b>Queue (Hard)</b>\nFIFO: You can only pick the LEFTMOST block.";
                break;
        }

        UpdateButtonVisuals();
        CheckReadyToStart();
    }

    void UpdateButtonVisuals()
    {
        // Reset all buttons to White first
        foreach (var btn in wordCountButtons) btn.GetComponent<Image>().color = colorDefault;
        btnArray.GetComponent<Image>().color = colorDefault;
        btnStack.GetComponent<Image>().color = colorDefault;
        btnQueue.GetComponent<Image>().color = colorDefault;

        // Color the selected ones Green
        if (selectedWordCount > 0)
        {
            wordCountButtons[selectedWordCount - 1].GetComponent<Image>().color = colorSelected;
        }

        if (selectedType == ResourceType.Array) btnArray.GetComponent<Image>().color = colorSelected;
        if (selectedType == ResourceType.Stack) btnStack.GetComponent<Image>().color = colorSelected;
        if (selectedType == ResourceType.Queue) btnQueue.GetComponent<Image>().color = colorSelected;
    }

    void CheckReadyToStart()
    {
        // Only unlock if BOTH choices are made
        if (selectedWordCount > 0 && selectedType != ResourceType.None)
        {
            startButton.interactable = true;
        }
    }

    void OnStartClicked()
    {
        // This connects to the GameManager to actually start the game
        GameManager.Instance.SetResourceMode(selectedType);
        GameManager.Instance.SelectDifficulty(selectedWordCount);
    }
}