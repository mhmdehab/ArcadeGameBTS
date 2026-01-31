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

    [Header("References")]
    public GameObject blockPrefab;
    public Camera miniGameCamera;

    [Header("Towers")]
    public List<Tower> sourceTowers;
    public List<Tower> goalTowerSlots;

    [Header("UI References")]
    public GameObject difficultyPanel;
    public GameObject winPanel;
    public List<TMP_Text> targetTextSlots;

    // --- REMOVED: currentWordText (We don't need it anymore!) ---

    private List<GameObject> activeBlocks = new List<GameObject>();

    void Awake()
    {
        Instance = this;
    }

    void OnEnable()
    {
        ShowDifficultyMenu();
    }

    public void ShowDifficultyMenu()
    {
        winPanel.SetActive(false);
        CleanupLevel();

        foreach (var t in goalTowerSlots) t.gameObject.SetActive(false);
        foreach (var txt in targetTextSlots) txt.gameObject.SetActive(false);

        difficultyPanel.SetActive(true);
    }

    public void SelectDifficulty(int numberOfWords)
    {
        currentDifficulty = numberOfWords;
        difficultyPanel.SetActive(false);
        StartLevel(currentDifficulty);
    }

    public string GetRandomWord()
    {
        if (availableWords.Count == 0) availableWords = new List<string>(wordLibrary);
        int randomIndex = Random.Range(0, availableWords.Count);
        string selectedWord = availableWords[randomIndex];
        availableWords.RemoveAt(randomIndex);
        return selectedWord;
    }

    public void StartLevel(int difficulty)
    {
        activeTargetWords.Clear();
        activeTowerIndices.Clear();
        CleanupLevel();

        if (difficulty == 1) activeTowerIndices.Add(1);
        else if (difficulty == 2) { activeTowerIndices.Add(0); activeTowerIndices.Add(2); }
        else if (difficulty == 3) { activeTowerIndices.Add(0); activeTowerIndices.Add(1); activeTowerIndices.Add(2); }

        foreach (int index in activeTowerIndices)
        {
            goalTowerSlots[index].gameObject.SetActive(true);
            targetTextSlots[index].gameObject.SetActive(true);

            string newWord = GetRandomWord();
            activeTargetWords.Add(newWord);

            targetTextSlots[index].text = "Target: " + newWord;

            SpawnBlocks(newWord);
        }
    }

    void CleanupLevel()
    {
        foreach (GameObject b in activeBlocks) if (b != null) Destroy(b);
        activeBlocks.Clear();
        foreach (var t in sourceTowers) t.blocks.Clear();
        foreach (var t in goalTowerSlots) t.blocks.Clear();
    }

    void SpawnBlocks(string word)
    {
        char[] chars = word.ToCharArray();

        for (int i = 0; i < chars.Length; i++)
        {
            char temp = chars[i];
            int randomIndex = Random.Range(i, chars.Length);
            chars[i] = chars[randomIndex];
            chars[randomIndex] = temp;
        }

        int startIndex = Random.Range(0, sourceTowers.Count);

        for (int i = 0; i < chars.Length; i++)
        {
            int towerIndex = (startIndex + i) % sourceTowers.Count;
            Tower selectedTower = sourceTowers[towerIndex];

            if (selectedTower == null) continue;

            Vector3 spawnPos = selectedTower.GetNextSnapPosition();

            GameObject newObj = Instantiate(blockPrefab, spawnPos, selectedTower.transform.rotation);
            newObj.transform.SetParent(selectedTower.transform);

            DraggableBlock blockScript = newObj.GetComponent<DraggableBlock>();
            if (blockScript != null)
            {
                blockScript.InitializeBlock(chars[i]);
                blockScript.SetCamera(miniGameCamera);
                selectedTower.AddBlock(blockScript);
                blockScript.SetCurrentTower(selectedTower);
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

            // --- NEW: The Chain Flag ---
            // If this becomes true, NO future letters can be green.
            bool chainBroken = false;
            // ---------------------------

            for (int j = 0; j < targetWord.Length; j++)
            {
                char requiredChar = targetWord[j];
                bool isCorrectChar = false;

                // 1. Is the block at this position actually the right letter?
                if (j < t.blocks.Count)
                {
                    if (t.blocks[j].letter == requiredChar)
                    {
                        isCorrectChar = true;
                    }
                }

                // 2. COLOR LOGIC: Green ONLY if correct AND the chain is unbroken
                if (isCorrectChar && !chainBroken)
                {
                    visualText.Append("<color=green>" + requiredChar + "</color>");
                    correctCharCount++;
                }
                else
                {
                    // It's white because it's either wrong OR the chain broke earlier
                    visualText.Append(requiredChar);

                    // If THIS specific block was the wrong one, break the chain now!
                    if (!isCorrectChar)
                    {
                        chainBroken = true;
                    }
                }
            }

            targetTextSlots[realTowerIndex].text = visualText.ToString();

            // Win if every letter was green (part of the unbroken chain) 
            // AND there are no extra junk blocks on top
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