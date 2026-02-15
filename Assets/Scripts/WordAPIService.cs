using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

public class WordAPIService : MonoBehaviour
{
    private string[] topics = new string[] {
        "animal", "food", "nature", "sport", "music",
        "travel", "house", "city", "space", "art",
        "color", "emotion", "weather", "summer", "winter"
    };

    public IEnumerator FetchWords(int count, int minLen, int maxLen, Action<List<string>> onSuccess, Action onFailure)
    {
        string randomTopic = topics[UnityEngine.Random.Range(0, topics.Length)];

        string apiUrl = $"https://api.datamuse.com/words?ml={randomTopic}&max=300";


        using (UnityWebRequest request = UnityWebRequest.Get(apiUrl))
        {
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                onFailure?.Invoke();
            }
            else
            {
                try
                {
                    string jsonText = request.downloadHandler.text;

                    MatchCollection matches = Regex.Matches(jsonText, "\"word\":\"(.*?)\"");

                    List<string> validWords = new List<string>();

                    foreach (Match m in matches)
                    {
                        string rawWord = m.Groups[1].Value;
                        string clean = rawWord.ToUpper().Trim();

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

                        onSuccess?.Invoke(finalSelection);
                    }
                    else
                    {
                        onFailure?.Invoke();
                    }
                }
                catch (Exception)
                {
                    onFailure?.Invoke();
                }
            }
        }
    }
}