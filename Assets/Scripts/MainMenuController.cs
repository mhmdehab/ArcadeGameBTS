using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum ResourceType { None, Array, Stack, Queue }

public class MainMenuController : MonoBehaviour
{
    [Header("Word Count Section")]
    public Button[] wordCountButtons;
    public TMP_Text wordCountDescription;

    [Header("Resource Type Section")]
    public Button btnArray;
    public Button btnStack;
    public Button btnQueue;
    public TMP_Text resourceDescription;

    [Header("Start Control")]
    public Button startButton;

    private int selectedWordCount = 0;
    private ResourceType selectedType = ResourceType.None;

    private Color colorSelected = Color.green;
    private Color colorDefault = Color.white;

    void Start()
    {

        for (int i = 0; i < wordCountButtons.Length; i++)
        {
            int count = i + 1;
            wordCountButtons[i].onClick.AddListener(() => SelectWordCount(count));
        }

        btnArray.onClick.AddListener(() => SelectResourceType(ResourceType.Array));
        btnStack.onClick.AddListener(() => SelectResourceType(ResourceType.Stack));
        btnQueue.onClick.AddListener(() => SelectResourceType(ResourceType.Queue));

        startButton.onClick.AddListener(OnStartClicked);

        ResetUI();
    }

    void ResetUI()
    {
        selectedWordCount = 0;
        selectedType = ResourceType.None;

        wordCountDescription.text = "Select difficulty level...";
        resourceDescription.text = "Select a data structure...";

        startButton.interactable = false;
        UpdateButtonVisuals();
    }

    void SelectWordCount(int count)
    {
        selectedWordCount = count;

        if (count == 1) wordCountDescription.text = "Solve one word using the resource tray and one stack.";
        else if (count == 2) wordCountDescription.text = "Solve two words using the resource tray and two stacks.";
        else if (count == 3) wordCountDescription.text = "Solve three words using the resource tray and three stacks.";

        UpdateButtonVisuals();
        CheckReadyToStart();
    }

    void SelectResourceType(ResourceType type)
    {
        selectedType = type;

        switch (type)
        {
            case ResourceType.Array:
                resourceDescription.text = "Array: Indexed access (20 indices). Place or pick any block at any position.";
                break;
            case ResourceType.Stack:
                resourceDescription.text = "Stack (LIFO): Only the last placed block can be picked.";
                break;
            case ResourceType.Queue:
                resourceDescription.text = "Queue (FIFO): Blocks are picked in the order they were placed.";
                break;
        }

        UpdateButtonVisuals();
        CheckReadyToStart();
    }

    void UpdateButtonVisuals()
    {
        foreach (var btn in wordCountButtons) btn.GetComponent<Image>().color = colorDefault;
        btnArray.GetComponent<Image>().color = colorDefault;
        btnStack.GetComponent<Image>().color = colorDefault;
        btnQueue.GetComponent<Image>().color = colorDefault;

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
        if (selectedWordCount > 0 && selectedType != ResourceType.None)
        {
            startButton.interactable = true;
        }
    }

    void OnStartClicked()
    {
        GameManager.Instance.SetResourceMode(selectedType);
        GameManager.Instance.SelectDifficulty(selectedWordCount);
    }
}