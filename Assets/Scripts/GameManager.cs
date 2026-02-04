using UnityEngine;
using System.Text;
using System.Collections.Generic;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Game Data")]
    public List<string> wordLibrary = new List<string> {
        "DATA", "CODE", "LOOP", "VOID", "NULL", "BYTE", "BOOL", "CHAR", "ENUM",
        "IF", "ELSE", "CASE", "TRUE", "FALSE", "WHILE", "FOR", "BREAK",
        "ARRAY", "LIST", "STACK", "QUEUE", "TREE", "GRAPH", "NODE", "MAP", "HEAP",
        "UNITY", "SCENE", "GAME", "OBJECT", "PREFAB", "ASSET", "SCRIPT", "DEBUG",
        "PIXEL", "VECTOR", "RAY", "MESH", "INPUT", "LAYER", "BUILD",
        "CLASS", "STATIC", "PUBLIC", "ERROR", "BUG", "FIX", "PATCH", "TOKEN",
        "SYNTAX", "LOGIC", "MEMORY", "CACHE", "SERVER", "CLIENT", "LOGIN"
    };

    private List<string> availableWords = new List<string>();
    private List<string> activeTargetWords = new List<string>();
    private List<int> activeTowerIndices = new List<int>();
    private int currentDifficulty = 1;

    [Header("Game Mode Settings")]
    public ResourceType currentResourceMode;

    [Header("References")]
    public GameObject blockPrefab;
    public Camera miniGameCamera;

    [Header("References")]
    public GameObject helperTower;

    public ResourceTray resourceTray;
    public List<Tower> goalTowerSlots;

    [Header("UI References")]
    public GameObject setupMenu;
    public GameObject winPanel;
    public List<TMP_Text> targetTextSlots;

    private List<GameObject> activeBlocks = new List<GameObject>();

    void Awake()
    {
        Instance = this;
    }

    void OnEnable()
    {
        ShowDifficultyMenu();
    }

    public void SetResourceMode(ResourceType type)
    {
        currentResourceMode = type;
        Debug.Log("Resource Mode Updated: " + type);
    }

    public void ShowDifficultyMenu()
    {
        winPanel.SetActive(false);
        CleanupLevel();

        foreach (var t in goalTowerSlots) t.gameObject.SetActive(false);
        foreach (var txt in targetTextSlots) txt.gameObject.SetActive(false);

        if (helperTower != null)
        {
            helperTower.SetActive(false);
        }

        setupMenu.SetActive(true);
    }

    public void SelectDifficulty(int numberOfWords)
    {
        currentDifficulty = numberOfWords;
        setupMenu.SetActive(false);
        StartLevel(currentDifficulty);
    }

    // Now accepts a minimum length filter
    public string GetRandomWord(int minLength = 0)
    {
        // Refill if empty
        if (availableWords.Count == 0) availableWords = new List<string>(wordLibrary);

        // 1. Find all words that meet the length requirement
        List<string> candidates = availableWords.FindAll(w => w.Length >= minLength);

        // Safety: If no words match (e.g. library ran out of long words), fallback to anything
        if (candidates.Count == 0) candidates = availableWords;

        // 2. Pick random from the valid list
        int randomIndex = Random.Range(0, candidates.Count);
        string selectedWord = candidates[randomIndex];

        // 3. Remove it from the main pool so it doesn't repeat
        availableWords.Remove(selectedWord);

        return selectedWord;
    }

    public void StartLevel(int difficulty)
    {
        activeTargetWords.Clear();
        activeTowerIndices.Clear();
        CleanupLevel();

        // 1. Configure the Tray
        resourceTray.ConfigureMode(currentResourceMode);

        // 2. Toggle Helper Tower Logic
        if (helperTower != null)
        {
            // Show only for STACK or QUEUE
            bool showHelper = (currentResourceMode == ResourceType.Stack || currentResourceMode == ResourceType.Queue);
            helperTower.SetActive(showHelper);

            // If active, apply the "Hard Mode" capacity limit of 5
            if (showHelper)
            {
                Tower helperScript = helperTower.GetComponent<Tower>();
                if (helperScript != null)
                {
                    helperScript.maxCapacity = 5;
                }
            }
        }

        // 3. Determine Goal Towers
        if (difficulty == 1) activeTowerIndices.Add(1);
        else if (difficulty == 2) { activeTowerIndices.Add(0); activeTowerIndices.Add(2); }
        else if (difficulty == 3) { activeTowerIndices.Add(0); activeTowerIndices.Add(1); activeTowerIndices.Add(2); }

        foreach (int index in activeTowerIndices)
        {
            goalTowerSlots[index].gameObject.SetActive(true);
            targetTextSlots[index].gameObject.SetActive(true);

            // --- NEW: Word Length Filter ---
            // Difficulty 1: Minimum 4 letters
            // Difficulty 2/3: Any length (0)
            int minLength = (difficulty == 1) ? 4 : 0;
            string newWord = GetRandomWord(minLength);
            // -------------------------------

            activeTargetWords.Add(newWord);
            targetTextSlots[index].text = "Target: " + newWord;

            // Goal Tower Capacity = Word Length + 1
            goalTowerSlots[index].maxCapacity = newWord.Length + 1;

            SpawnBlocks(newWord);
        }
    }

    void CleanupLevel()
    {
        foreach (GameObject b in activeBlocks) if (b != null) Destroy(b);
        activeBlocks.Clear();

        // Clear the new tray
        if (resourceTray != null) resourceTray.ClearTray();

        foreach (var t in goalTowerSlots) t.blocks.Clear();
    }

    void SpawnBlocks(string word)
    {
        char[] chars = word.ToCharArray();

        // Keep shuffling until the result is NOT the same as the answer.
        // This prevents "DATA" from accidentally spawning as "DATA".
        int attempts = 0;
        do
        {
            // Standard Fisher-Yates Shuffle
            for (int i = 0; i < chars.Length; i++)
            {
                char temp = chars[i];
                int randomIndex = Random.Range(i, chars.Length);
                chars[i] = chars[randomIndex];
                chars[randomIndex] = temp;
            }
            attempts++;
        }
        while (new string(chars) == word && attempts < 10); // Safety limit

        // Spawn Logic 
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

            StringBuilder visualText = new StringBuilder("Target: ");
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
        StartLevel(currentDifficulty);
        winPanel.SetActive(false);
    }

    public void BackToMenu()
    {
        ShowDifficultyMenu();
    }
}