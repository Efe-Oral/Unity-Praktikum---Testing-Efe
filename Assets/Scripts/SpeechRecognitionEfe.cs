using System;
using System.Threading.Tasks;
using UnityEngine;
using TMPro;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;

public class SpeechRecognitionEfe : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI inputField;
    [SerializeField] private AudioClip startSound;  // Sound for when recognition starts
    [SerializeField] private AudioClip endSound;    // Sound for when recognition ends
    [SerializeField] private AudioClip buzzSound;   // Buzz sound when input is blocked

    private AudioSource audioSource;

    private static string speechKey = "DuTF9airVdsZZpxpgaQBj0TgJbQtkxGW22Cwrb014SyboVhXoziOJQQJ99BCACPV0roXJ3w3AAAYACOGBSBH"; // Microsoft Azure
    private static string speechRegion = "germanywestcentral";

    private OllamaAPIClient ollamaClient;  // Reference to OllamaAPIClient

    void Start()
    {
        // Get or Add an AudioSource to the GameObject
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Find the OllamaAPIClient in the scene
        ollamaClient = FindObjectOfType<OllamaAPIClient>();

        if (ollamaClient == null)
        {
            Debug.LogError("OllamaAPIClient not found in the scene! Make sure it's attached to a GameObject.");
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // Only block input if a response is actively being processed
            if (ollamaClient != null && ollamaClient.isProcessing)
            {
                Debug.Log("CAN'T PRESS SPACE BUTTON! Speech recognition disabled: Waiting for the current response to finish.");
                PlaySound(buzzSound);
                return; // Stop processing
            }

            // If no prompt is being processed, allow speech recognition
            Debug.Log("Space key pressed! Starting speech recognition...");
            PlaySound(startSound);
            _ = RecognizeSpeechAsync();
        }
    }




    private async Task RecognizeSpeechAsync()
    {
        var speechConfig = SpeechConfig.FromSubscription(speechKey, speechRegion);
        speechConfig.SpeechRecognitionLanguage = "en-US";

        using var audioConfig = AudioConfig.FromDefaultMicrophoneInput();
        using var speechRecognizer = new SpeechRecognizer(speechConfig, audioConfig);

        Debug.Log("Listening for speech...");
        var result = await speechRecognizer.RecognizeOnceAsync();

        PlaySound(endSound);
        ProcessSpeechResult(result);
    }

    private void ProcessSpeechResult(SpeechRecognitionResult result)
    {
        if (result.Reason == ResultReason.RecognizedSpeech)
        {
            Debug.Log($"Recognized: {result.Text}");
            if (inputField != null)
            {
                inputField.text = result.Text;  // Update Unity with recognized text
            }

            // Trigger Start conversation in OllamaAPIClient script
            if (ollamaClient != null)
            {
                Debug.Log("Starting conversation with recognized speech...");
                ollamaClient.StartConversation();
            }
        }
        else
        {
            Debug.Log("Speech not recognized.");
        }
    }

    // Method to play sound effect
    private void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
}
