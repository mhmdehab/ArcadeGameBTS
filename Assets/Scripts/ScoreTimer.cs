using TMPro;
using UnityEngine;

public class ScoreTimer : MonoBehaviour
{
    int score;
    float timer;
    bool isrunning = false;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI scoreText;
    private TreeSpawner treeSpawner;
    public GameObject gameOverPanel;
    void Start()
    {
        treeSpawner = FindObjectOfType<TreeSpawner>();
        timerText.gameObject.SetActive(false);
        scoreText.gameObject.SetActive(false);
    }
    public void StopTimer()
    {
        isrunning = false;
    }
    void HideGameOver()
    {
        gameOverPanel.SetActive(false);
    }
    void Update()
    {
        
        if (isrunning)
        {
            timerText.text = "Time : " + Mathf.CeilToInt(timer);
            scoreText.text = "Score : " + score;
            timer = timer - Time.deltaTime;
            if (timer <= 0)
            {
                timer = 0;
                isrunning = false;
                gameOverPanel.SetActive(true);
                treeSpawner.OnResetButtonPressed();
                Invoke("HideGameOver", 3f);
            }
        }
        

    }
    public void startTimer(int nodeCount)
    {
        timerText.gameObject.SetActive(true);
        scoreText.gameObject.SetActive(true);
        isrunning = true;
        timer = nodeCount * 10;

    }
    public void addScore() { score = score + 10; }
    public void subScore() { score = score - 5; }
    public void resetTimer()
    {
        timerText.gameObject.SetActive(false);
        scoreText.gameObject.SetActive(false);
        isrunning = false;
        timer = 0;
        score = 0;
        timerText.text = "Time : 0";
        scoreText.text = "Score : 0";
    }
}
