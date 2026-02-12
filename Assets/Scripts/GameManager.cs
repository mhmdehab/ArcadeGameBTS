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
    public GameObject helperPromptPanel;
    public TMP_Text messageText;

    [Header("Timer Settings")]
    public TMP_Text timerText;
    private bool isTimerRunning = false;
    private float elapsedTime = 0f;

    private Coroutine helperHintCoroutine;
    private Coroutine messageCoroutine; 

    void Awake()
    {
        Instance = this;
    }

    void OnEnable()
    {
        ShowDifficultyMenu();
    }

    void Update()
    {
        if (isTimerRunning)
        {
            elapsedTime += Time.deltaTime;
            UpdateTimerUI();
        }
    }

    void UpdateTimerUI()
    {
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(elapsedTime / 60F);
            int seconds = Mathf.FloorToInt(elapsedTime % 60F);
            timerText.text = string.Format("Time: {0:00}:{1:00}", minutes, seconds);
        }
    }

    public void ShowDifficultyMenu()
    {
        winPanel.SetActive(false);
        CleanupLevel();

        foreach (var t in goalTowerSlots) t.gameObject.SetActive(false);
        foreach (var txt in targetTextSlots) txt.gameObject.SetActive(false);

        if (helperTower != null) helperTower.SetActive(false);
        if (startGameButtonText != null) startGameButtonText.text = "START GAME";

        isTimerRunning = false; 
        if (timerText != null) timerText.text = "";

        setupMenu.SetActive(true);
    }

    public void SetResourceMode(ResourceType type)
    {
        selectedResourceMode = type;
        Debug.Log("Mode Selected: " + selectedResourceMode);
    }

    public void SelectDifficulty(int numberOfWords)
    {
        selectedDifficulty = numberOfWords;
        Debug.Log("Difficulty Selected: " + selectedDifficulty);

        StartCoroutine(StartGameRoutine());
    }


    private IEnumerator StartGameRoutine()
    {
        if (startGameButtonText != null) startGameButtonText.text = "GENERATING...";

        int minLen = (selectedDifficulty == 1) ? 4 : 3;
        int neededCount = (selectedDifficulty == 1) ? 1 : (selectedDifficulty == 2 ? 2 : 3);

        if (useOnlineWords && apiService != null)
        {
            yield return apiService.FetchWords(neededCount + 3, minLen, 6,
                (words) => {
                    availableWords.Clear();
                    availableWords.AddRange(words);
                },
                () => {
                    Debug.Log("API Failed. Using local library.");
                }
            );
        }

        if (startGameButtonText != null) startGameButtonText.text = "START GAME";
        setupMenu.SetActive(false);

        StartLevelLogic(selectedDifficulty);
    }

    private void StartLevelLogic(int difficulty)
    {
        activeTargetWords.Clear();
        activeTowerIndices.Clear();
        CleanupLevel();

        if (resourceTray != null) resourceTray.ConfigureMode(selectedResourceMode);

        if (helperTower != null)
        {
            helperTower.SetActive(false);

            if (difficulty == 1 && selectedResourceMode == ResourceType.Stack)
            {
                helperHintCoroutine = StartCoroutine(ShowHelperWithDelay());
            }
            else
            {
                helperTower.SetActive(false);
            }
        }

        if (difficulty == 1) activeTowerIndices.Add(1);
        else if (difficulty == 2) { activeTowerIndices.Add(0); activeTowerIndices.Add(2); }
        else if (difficulty == 3) { activeTowerIndices.Add(0); activeTowerIndices.Add(1); activeTowerIndices.Add(2); }

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

        elapsedTime = 0f;
        isTimerRunning = true;
    }

    private IEnumerator ShowHelperWithDelay()
    {
        yield return new WaitForSeconds(20f);

        if (helperPromptPanel != null)
        {
            helperPromptPanel.SetActive(true);
            isTimerRunning = false;
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

    void CleanupLevel()
    {
        if (messageCoroutine != null) StopCoroutine(messageCoroutine);
        if (messageText != null) messageText.text = "";

        if (helperPromptPanel != null) helperPromptPanel.SetActive(false);
        if (helperHintCoroutine != null) StopCoroutine(helperHintCoroutine);

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
            isTimerRunning = false;
        }
    }

    public void RestartGame()
    {
        winPanel.SetActive(false);
        StartCoroutine(StartGameRoutine());
    }

    public void BackToMenu()
    {
        ShowDifficultyMenu();
    }

    public void UnlockHelper()
    {
        if (helperPromptPanel != null) helperPromptPanel.SetActive(false);

        isTimerRunning = true;

        if (helperTower != null)
        {
            helperTower.SetActive(true);

            Tower t = helperTower.GetComponent<Tower>();
            if (t != null) t.maxCapacity = 5;
        }

        ShowMessage("Helper Stack Added! Try moving blocks there.", 3f, Color.green);
    }

    public void ShowMessage(string msg, float duration = 3f, Color? textColor = null)
    {
        if (messageText == null) return;

        messageText.text = msg;
        messageText.color = textColor ?? Color.white;

        if (messageCoroutine != null) StopCoroutine(messageCoroutine);

        messageCoroutine = StartCoroutine(FadeMessageRoutine(duration));
    }

    private IEnumerator FadeMessageRoutine(float totalDuration)
    {
        float fadeSpeed = 0.5f; 
        float waitTime = totalDuration - (fadeSpeed * 2);

        // 1. Setup: Start Invisible
        Color c = messageText.color;
        c.a = 0f;
        messageText.color = c;

        // 2. Fade In
        float timer = 0f;
        while (timer < fadeSpeed)
        {
            timer += Time.deltaTime;
            c.a = Mathf.Lerp(0f, 1f, timer / fadeSpeed);
            messageText.color = c;
            yield return null;
        }
        c.a = 1f; // Ensure fully visible
        messageText.color = c;

        // 3. Wait
        yield return new WaitForSeconds(waitTime > 0 ? waitTime : 0.1f);

        // 4. Fade Out
        timer = 0f;
        while (timer < fadeSpeed)
        {
            timer += Time.deltaTime;
            c.a = Mathf.Lerp(1f, 0f, timer / fadeSpeed);
            messageText.color = c;
            yield return null;
        }

        // 5. Clean up
        messageText.text = "";
        c.a = 1f; // Reset alpha for next time
        messageText.color = c;
    }
}