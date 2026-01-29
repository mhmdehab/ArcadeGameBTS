using UnityEngine;
using System.Text;
using System.Collections.Generic;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Game Data")]
    // --- UPDATED WORD LIST ---
    public List<string> wordLibrary = new List<string> {
        // Basics
        "DATA", "CODE", "LOOP", "VOID", "NULL", "BYTE", "BOOL", "CHAR", "ENUM", 
        // Logic
        "IF", "ELSE", "CASE", "TRUE", "FALSE", "WHILE", "FOR", "BREAK", 
        // Data Structures
        "ARRAY", "LIST", "STACK", "QUEUE", "TREE", "GRAPH", "NODE", "MAP", "HEAP",
        // Unity / Game Dev
        "UNITY", "SCENE", "GAME", "OBJECT", "PREFAB", "ASSET", "SCRIPT", "DEBUG",
        "PIXEL", "VECTOR", "RAY", "MESH", "INPUT", "LAYER", "BUILD",
        // Advanced / Concepts
        "CLASS", "STATIC", "PUBLIC", "ERROR", "BUG", "FIX", "PATCH", "TOKEN",
        "SYNTAX", "LOGIC", "MEMORY", "CACHE", "SERVER", "CLIENT", "LOGIN"
    };
    // -------------------------

    // The "Tracking List" - we deplete this as we play so we don't repeat words
    private List<string> availableWords = new List<string>();
    private string currentWord;

    [Header("References")]
    public GameObject blockPrefab;
    public Tower goalTower;
    public List<Tower> sourceTowers;
    public Camera miniGameCamera;

    [Header("UI")]
    public TMP_Text targetText;
    public TMP_Text currentWordText;
    public GameObject winPanel;

    private List<GameObject> activeBlocks = new List<GameObject>();

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // Start the first level immediately
        LoadNextLevel();
    }

    // This function is ready for "Multiple Towers" later!
    // Every time you call it, it gives you a unique word from the remaining pile.
    public string GetRandomWord()
    {
        // 1. If we ran out of words (or are low), refill the list
        if (availableWords.Count == 0)
        {
            availableWords = new List<string>(wordLibrary);
        }

        // 2. Pick a random INDEX
        int randomIndex = Random.Range(0, availableWords.Count);

        // 3. Get the word
        string selectedWord = availableWords[randomIndex];

        // 4. Remove it so we don't pick "UNITY" twice in the same game session
        availableWords.RemoveAt(randomIndex);

        return selectedWord;
    }

    public void StartLevel(string word)
    {
        currentWord = word;
        winPanel.SetActive(false);
        targetText.text = "Target: " + currentWord;

        // Cleanup old blocks
        foreach (GameObject b in activeBlocks)
        {
            if (b != null) Destroy(b);
        }
        activeBlocks.Clear();

        // Cleanup Towers
        goalTower.blocks.Clear();
        foreach (var t in sourceTowers) t.blocks.Clear();

        // Spawn
        SpawnBlocks(currentWord);

        // UI Reset
        UpdateUI();
    }

    public void LoadNextLevel()
    {
        string nextWord = GetRandomWord();
        StartLevel(nextWord);
    }

    void SpawnBlocks(string word)
    {
        char[] chars = word.ToCharArray();

        // Shuffle Letters
        for (int i = 0; i < chars.Length; i++)
        {
            char temp = chars[i];
            int randomIndex = Random.Range(i, chars.Length);
            chars[i] = chars[randomIndex];
            chars[randomIndex] = temp;
        }

        // Spawn Loop
        for (int i = 0; i < chars.Length; i++)
        {
            Tower randomTower = sourceTowers[Random.Range(0, sourceTowers.Count)];

            // Check if randomTower is valid
            if (randomTower == null) continue;

            Vector3 spawnPos = randomTower.GetNextSnapPosition();

            GameObject newObj = Instantiate(blockPrefab, spawnPos, randomTower.transform.rotation);
            newObj.transform.SetParent(randomTower.transform);

            DraggableBlock blockScript = newObj.GetComponent<DraggableBlock>();

            // Safety check
            if (blockScript != null)
            {
                blockScript.InitializeBlock(chars[i]);
                blockScript.SetCamera(miniGameCamera);
                randomTower.AddBlock(blockScript);
                blockScript.SetCurrentTower(randomTower);
            }

            activeBlocks.Add(newObj);
        }
    }

    public void UpdateUI()
    {
        StringBuilder builtWord = new StringBuilder();
        foreach (var block in goalTower.blocks)
        {
            builtWord.Append(block.letter);
        }
        currentWordText.text = "Built: " + builtWord.ToString();
    }

    public void CheckForWin()
    {
        UpdateUI();

        if (goalTower.blocks.Count != currentWord.Length) return;

        StringBuilder builtWord = new StringBuilder();
        foreach (var block in goalTower.blocks) builtWord.Append(block.letter);

        if (builtWord.ToString() == currentWord)
        {
            winPanel.SetActive(true);
        }
    }
}