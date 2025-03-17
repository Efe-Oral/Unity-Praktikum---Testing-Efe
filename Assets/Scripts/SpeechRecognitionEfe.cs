using System;
using System.Threading.Tasks;
using UnityEngine;
using TMPro;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;

public class SpeechRecognition : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI inputField;  // 🔵 Connect this in Unity Inspector

    private static string speechKey = "DuTF9airVdsZZpxpgaQBj0TgJbQtkxGW22Cwrb014SyboVhXoziOJQQJ99BCACPV0roXJ3w3AAAYACOGBSBH";
    private static string speechRegion = "germanywestcentral";

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))  // Press "Space" to activate speech recognition
        {
            Debug.Log("🎤 Space key pressed! Starting speech recognition...");
            _ = RecognizeSpeechAsync();  // Start async speech recognition
        }
    }

    private async Task RecognizeSpeechAsync()
    {
        var speechConfig = SpeechConfig.FromSubscription(speechKey, speechRegion);
        speechConfig.SpeechRecognitionLanguage = "en-US";  // Change to "de-DE" for German

        using var audioConfig = AudioConfig.FromDefaultMicrophoneInput();
        using var speechRecognizer = new SpeechRecognizer(speechConfig, audioConfig);

        Debug.Log("🎤 Listening for speech...");
        var result = await speechRecognizer.RecognizeOnceAsync();
        ProcessSpeechResult(result);
    }

    private void ProcessSpeechResult(SpeechRecognitionResult result)
    {
        if (result.Reason == ResultReason.RecognizedSpeech)
        {
            Debug.Log($"✅ Recognized: {result.Text}");
            if (inputField != null)
            {
                inputField.text = result.Text;  // 🔵 Update Unity InputField with recognized text
            }
        }
        else
        {
            Debug.Log("⚠️ Speech not recognized.");
        }
    }
}
