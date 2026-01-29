using UnityEngine;
using System.Text;
using System.Collections.Generic;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Game Data")]
    public List<string> levels = new List<string> { "HELLO", "WORLD", "UNITY", "STACK", "CODE", "GAMER" };

    // The "Tracking List" - we deplete this as we play
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

    private string GetRandomWord()
    {
        // 1. If we ran out of words, reset the list from the master 'levels' list
        if (availableWords.Count == 0)
        {
            availableWords = new List<string>(levels);
        }

        // 2. Pick a random INDEX from the remaining words
        int randomIndex = Random.Range(0, availableWords.Count);

        // 3. Get the word at that index
        string selectedWord = availableWords[randomIndex];

        // 4. CRITICAL: Remove it from the list so it doesn't repeat
        availableWords.RemoveAt(randomIndex);

        return selectedWord;
    }
    // ---------------------------------------

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
            // Pick a random source tower (Left or Right)
            Tower randomTower = sourceTowers[Random.Range(0, sourceTowers.Count)];
            Vector3 spawnPos = randomTower.GetNextSnapPosition();

            GameObject newObj = Instantiate(blockPrefab, spawnPos, randomTower.transform.rotation);
            newObj.transform.SetParent(randomTower.transform);
            DraggableBlock blockScript = newObj.GetComponent<DraggableBlock>();

            blockScript.InitializeBlock(chars[i]);
            blockScript.SetCamera(miniGameCamera);
            randomTower.AddBlock(blockScript);
            blockScript.SetCurrentTower(randomTower);
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