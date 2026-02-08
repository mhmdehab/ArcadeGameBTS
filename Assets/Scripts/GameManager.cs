using UnityEngine;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Web API Settings")]
    public WordAPIService apiService;
    public bool useOnlineWords = true;
    public TMP_Text startGameButtonText;

    [Header("Game Data")]
    public List<string> wordLibrary = new List<string> {
        "DATA", "CODE", "LOOP", "VOID", "NULL", "BYTE", "BOOL", "CHAR",
        "IF", "ELSE", "CASE", "TRUE", "WHILE", "FOR", "BREAK",
        "ARRAY", "LIST", "STACK", "QUEUE", "TREE", "GRAPH", "NODE", "HEAP",
        "UNITY", "GAME", "ASSET", "DEBUG", "PIXEL", "MESH", "INPUT", "BUILD",
        "CLASS", "ERROR", "BUG", "FIX", "PATCH", "TOKEN", "LOGIC", "LOGIN"
    };

    // State Variables
    private List<string> availableWords = new List<string>();
    private List<string> activeTargetWords = new List<string>();
    private List<int> activeTowerIndices = new List<int>();
    private List<GameObject> activeBlocks = new List<GameObject>();

    // Default settings
    private int selectedDifficulty = 1;
    private ResourceType selectedResourceMode = ResourceType.Array;

    [Header("Scene References")]
    public GameObject blockPrefab;
    public Camera miniGameCamera;
    public GameObject helperTower;
    public ResourceTray resourceTray;
    public List<Tower> goalTowerSlots;

    [Header("UI References")]
    public GameObject setupMenu;
    public GameObject winPanel;
    public List<TMP_Text> targetTextSlots;

    void Awake()
    {
        Instance = this;
    }

    void OnEnable()
    {
        ShowDifficultyMenu();
    }

    // ---------------------------------------------------------
    // 1. MENU & SELECTION LOGIC (Fixed for MainMenuController)
    // ---------------------------------------------------------

    public void ShowDifficultyMenu()
    {
        winPanel.SetActive(false);
        CleanupLevel();

        foreach (var t in goalTowerSlots) t.gameObject.SetActive(false);
        foreach (var txt in targetTextSlots) txt.gameObject.SetActive(false);

        if (helperTower != null) helperTower.SetActive(false);
        if (startGameButtonText != null) startGameButtonText.text = "START GAME";

        setupMenu.SetActive(true);
    }

    // --- FIX #1: Accept ResourceType directly ---
    public void SetResourceMode(ResourceType type)
    {
        selectedResourceMode = type;
        Debug.Log("Mode Selected: " + selectedResourceMode);
    }

    // --- FIX #2: Start the game when Difficulty is selected via Start Button ---
    public void SelectDifficulty(int numberOfWords)
    {
        selectedDifficulty = numberOfWords;
        Debug.Log("Difficulty Selected: " + selectedDifficulty);

        // Since MainMenuController calls this when "Start" is clicked, 
        // we trigger the start routine now.
        StartCoroutine(StartGameRoutine());
    }

    // ---------------------------------------------------------
    // 2. LOADING & API LOGIC
    // ---------------------------------------------------------

    private IEnumerator StartGameRoutine()
    {
        // A. Show Loading UI
        if (startGameButtonText != null) startGameButtonText.text = "GENERATING...";

        // B. Determine API Requirements
        // Difficulty 1: Min 4 letters. Difficulty 2/3: Min 3 letters.
        int minLen = (selectedDifficulty == 1) ? 4 : 3;
        int neededCount = (selectedDifficulty == 1) ? 1 : (selectedDifficulty == 2 ? 2 : 3);

        // Ask for a few extra words just in case
        if (useOnlineWords && apiService != null)
        {
            // Wait for API response...
            yield return apiService.FetchWords(neededCount + 3, minLen, 6,
                (words) => {
                    // Success! Use these words.
                    availableWords.Clear();
                    availableWords.AddRange(words);
                },
                () => {
                    // Fail! Log it and continue to fallback.
                    Debug.Log("API Failed. Using local library.");
                }
            );
        }

        // C. Start the Level
        if (startGameButtonText != null) startGameButtonText.text = "START GAME";
        setupMenu.SetActive(false);

        StartLevelLogic(selectedDifficulty);
    }

    // ---------------------------------------------------------
    // 3. LEVEL SETUP LOGIC
    // ---------------------------------------------------------

    private void StartLevelLogic(int difficulty)
    {
        activeTargetWords.Clear();
        activeTowerIndices.Clear();
        CleanupLevel();

        // 1. Configure Tray
        if (resourceTray != null) resourceTray.ConfigureMode(selectedResourceMode);

        // 2. Configure Helper Tower
        if (helperTower != null)
        {
            bool showHelper = (selectedResourceMode == ResourceType.Stack || selectedResourceMode == ResourceType.Queue);
            helperTower.SetActive(showHelper);

            if (showHelper)
            {
                Tower helperScript = helperTower.GetComponent<Tower>();
                if (helperScript != null) helperScript.maxCapacity = 5;
            }
        }

        // 3. Determine Goal Towers
        if (difficulty == 1) activeTowerIndices.Add(1);
        else if (difficulty == 2) { activeTowerIndices.Add(0); activeTowerIndices.Add(2); }
        else if (difficulty == 3) { activeTowerIndices.Add(0); activeTowerIndices.Add(1); activeTowerIndices.Add(2); }

        // 4. Spawn Words
        foreach (int index in activeTowerIndices)
        {
            goalTowerSlots[index].gameObject.SetActive(true);
            targetTextSlots[index].gameObject.SetActive(true);

            int minLength = (difficulty == 1) ? 4 : 0;
            string newWord = GetRandomWord(minLength);

            activeTargetWords.Add(newWord);
            targetTextSlots[index].text = newWord;

            goalTowerSlots[index].maxCapacity = newWord.Length + 1;

            SpawnBlocks(newWord);
        }
    }

    public string GetRandomWord(int minLength = 0)
    {
        if (availableWords.Count == 0) availableWords = new List<string>(wordLibrary);

        List<string> candidates = availableWords.FindAll(w => w.Length >= minLength);
        if (candidates.Count == 0) candidates = availableWords;

        int randomIndex = Random.Range(0, candidates.Count);
        string selectedWord = candidates[randomIndex];
        availableWords.Remove(selectedWord);

        return selectedWord;
    }

    // ---------------------------------------------------------
    // 4. GAMEPLAY LOGIC
    // ---------------------------------------------------------

    void CleanupLevel()
    {
        foreach (GameObject b in activeBlocks) if (b != null) Destroy(b);
        activeBlocks.Clear();

        if (resourceTray != null) resourceTray.ClearTray();

        foreach (var t in goalTowerSlots) t.blocks.Clear();
    }

    void SpawnBlocks(string word)
    {
        char[] chars = word.ToCharArray();
        int attempts = 0;
        do
        {
            for (int i = 0; i < chars.Length; i++)
            {
                char temp = chars[i];
                int randomIndex = Random.Range(i, chars.Length);
                chars[i] = chars[randomIndex];
                chars[randomIndex] = temp;
            }
            attempts++;
        }
        while (new string(chars) == word && attempts < 10);

        for (int i = 0; i < chars.Length; i++)
        {
            Vector3 spawnPos = resourceTray.transform.position;
            GameObject newObj = Instantiate(blockPrefab, spawnPos, Quaternion.identity);

            DraggableBlock blockScript = newObj.GetComponent<DraggableBlock>();
            if (blockScript != null)
            {
                blockScript.InitializeBlock(chars[i]);
                blockScript.SetCamera(miniGameCamera);
                resourceTray.AddBlock(blockScript);
            }
            activeBlocks.Add(newObj);
        }
    }

    public void CheckForWin()
    {
        int solvedCount = 0;

        for (int i = 0; i < activeTargetWords.Count; i++)
        {
            int realTowerIndex = activeTowerIndices[i];
            Tower t = goalTowerSlots[realTowerIndex];
            string targetWord = activeTargetWords[i];

            StringBuilder visualText = new StringBuilder("");

            int correctCharCount = 0;
            bool chainBroken = false;

            for (int j = 0; j < targetWord.Length; j++)
            {
                char requiredChar = targetWord[j];
                bool isCorrectChar = false;

                if (j < t.blocks.Count)
                {
                    if (t.blocks[j].letter == requiredChar) isCorrectChar = true;
                }

                if (isCorrectChar && !chainBroken)
                {
                    visualText.Append("<color=green>" + requiredChar + "</color>");
                    correctCharCount++;
                }
                else
                {
                    visualText.Append(requiredChar);
                    if (!isCorrectChar) chainBroken = true;
                }
            }

            targetTextSlots[realTowerIndex].text = visualText.ToString();

            if (correctCharCount == targetWord.Length && t.blocks.Count == targetWord.Length)
            {
                solvedCount++;
            }
        }

        if (solvedCount == activeTargetWords.Count)
        {
            winPanel.SetActive(true);
        }
    }

    public void RestartGame()
    {
        winPanel.SetActive(false);
        // Reuse the start logic with current settings
        StartCoroutine(StartGameRoutine());
    }

    public void BackToMenu()
    {
        ShowDifficultyMenu();
    }
}