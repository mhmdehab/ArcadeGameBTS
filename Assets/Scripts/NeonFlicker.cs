using System.Collections;
using UnityEngine;
using TMPro;

public class NeonFlicker : MonoBehaviour
{
    [Header("References")]
    public TMP_Text neonText;
    public Light neonLight;
    public Renderer[] neonBorders;

    [Header("Timing Settings")]
    [Tooltip("How long the light stays stable (in seconds) before glitching again.")]
    public float twitchFrequency = 10.0f;

    [Tooltip("How slow the individual blinks are. Higher = slower, lazy blinks.")]
    public float flickerSpeed = 0.2f;

    [Header("Chaos Settings")]
    [Tooltip("Maximum number of times it blinks during one glitch event.")]
    public int maxFlickers = 2;

    // Internal color memory
    private Color textOnColor;
    private Color textOffColor = new Color(0.1f, 0.1f, 0.1f, 1f);
    private Color borderOnColor;
    private Color borderOffColor = Color.black;

    void Start()
    {
        if (neonText != null) textOnColor = neonText.color;

        if (neonBorders.Length > 0 && neonBorders[0] != null)
        {
            borderOnColor = neonBorders[0].material.GetColor("_EmissionColor");
        }

        StartCoroutine(FlickerLoop());
    }

    IEnumerator FlickerLoop()
    {
        while (true)
        {
            SetLight(true);
            // Randomly wait between 1 second and chosen frequency
            yield return new WaitForSeconds(Random.Range(1.0f, twitchFrequency));

            //Uses 'maxFlickers' setting to control chaos
            int flickerCount = Random.Range(1, maxFlickers + 1);

            for (int i = 0; i < flickerCount; i++)
            {
                SetLight(false);
                yield return new WaitForSeconds(Random.Range(0.05f, flickerSpeed));

                SetLight(true);
                yield return new WaitForSeconds(Random.Range(0.05f, flickerSpeed));
            }
        }
    }

    void SetLight(bool isOn)
    {
        if (neonText != null) neonText.color = isOn ? textOnColor : textOffColor;
        if (neonLight != null) neonLight.enabled = isOn;

        Color currentBorderColor = isOn ? borderOnColor : borderOffColor;
        foreach (Renderer border in neonBorders)
        {
            if (border != null) border.material.SetColor("_EmissionColor", currentBorderColor);
        }
    }
}