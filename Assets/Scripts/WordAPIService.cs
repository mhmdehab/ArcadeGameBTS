using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

public class WordAPIService : MonoBehaviour
{
    // A list of broad topics to keep the game fresh every time
    private string[] topics = new string[] {
        "animal", "food", "nature", "sport", "music",
        "travel", "house", "city", "space", "art",
        "color", "emotion", "weather", "summer", "winter"
    };

    public IEnumerator FetchWords(int count, int minLen, int maxLen, Action<List<string>> onSuccess, Action onFailure)
    {
        // 1. Pick a random topic (e.g., "food" or "sport")
        string randomTopic = topics[UnityEngine.Random.Range(0, topics.Length)];

        // 2. Build the URL dynamically
        string apiUrl = $"https://api.datamuse.com/words?ml={randomTopic}&max=300";

        // Debug.Log($"Fetching words related to: <b>{randomTopic.ToUpper()}</b>");

        using (UnityWebRequest request = UnityWebRequest.Get(apiUrl))
        {
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Web API Failed: " + request.error);
                onFailure?.Invoke();
            }
            else
            {
                try
                {
                    string jsonText = request.downloadHandler.text;

                    // Regex Parsing
                    MatchCollection matches = Regex.Matches(jsonText, "\"word\":\"(.*?)\"");

                    List<string> validWords = new List<string>();

                    foreach (Match m in matches)
                    {
                        string rawWord = m.Groups[1].Value;
                        string clean = rawWord.ToUpper().Trim();

                        // --- STRICT FILTER ---
                        // Must be Letters Only (No 'PUTER, no 123, no hyphens)
                        if (clean.Length >= minLen &&
                            clean.Length <= maxLen &&
                            clean.All(char.IsLetter))
                        {
                            if (!validWords.Contains(clean))
                            {
                                validWords.Add(clean);
                            }
                        }
                    }

                    // Shuffle
                    for (int i = 0; i < validWords.Count; i++)
                    {
                        string temp = validWords[i];
                        int rnd = UnityEngine.Random.Range(i, validWords.Count);
                        validWords[i] = validWords[rnd];
                        validWords[rnd] = temp;
                    }

                    if (validWords.Count > 0)
                    {
                        int finalCount = Mathf.Min(count, validWords.Count);
                        List<string> finalSelection = validWords.GetRange(0, finalCount);

                        Debug.Log($"<color=green>API Success! ({randomTopic.ToUpper()})</color> Words: " + string.Join(", ", finalSelection));

                        onSuccess?.Invoke(finalSelection);
                    }
                    else
                    {
                        Debug.LogWarning($"API returned data for '{randomTopic}', but NO words matched your criteria.");
                        onFailure?.Invoke();
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError("Regex Parsing Error: " + e.Message);
                    onFailure?.Invoke();
                }
            }
        }
    }
}