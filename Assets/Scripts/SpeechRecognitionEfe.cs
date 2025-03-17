using System;
using System.Threading.Tasks;
using UnityEngine;
using TMPro;  // ✅ Required for TextMeshPro InputField
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;

public class SpeechRecognitionEfe : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI inputField; // ✅ Connect this to your Unity InputField (TextMeshPro)

    // 🎤 Azure Speech-to-Text API Credentials
    private static string speechKey = "DuTF9airVdsZZpxpgaQBj0TgJbQtkxGW22Cwrb014SyboVhXoziOJQQJ99BCACPV0roXJ3w3AAAYACOGBSBH"; // Efe's key
    // static string speechKey = "7f4c8bd36c224713a919a41e8d854b44";//David's key
    private static string speechRegion = "germanywestcentral"; // Region

    void Start()
    {
        // Ensure InputField is assigned in the Unity Inspector
        if (inputField == null)
        {
            Debug.LogError("❌ No InputField assigned! Please set it in the Unity Inspector.");
        }
    }

    // 🎤 Start Speech Recognition
    public async void StartRecognition()
    {
        Debug.Log("🎤 Listening for speech input...");

        var speechConfig = SpeechConfig.FromSubscription(speechKey, speechRegion);
        speechConfig.SpeechRecognitionLanguage = "en-US"; // Change to "de-DE" for German

        using var audioConfig = AudioConfig.FromDefaultMicrophoneInput();
        using var speechRecognizer = new SpeechRecognizer(speechConfig, audioConfig);

        var result = await speechRecognizer.RecognizeOnceAsync();
        ProcessSpeechResult(result);
    }

    // 📝 Process the Recognized Speech
    private void ProcessSpeechResult(SpeechRecognitionResult result)
    {
        switch (result.Reason)
        {
            case ResultReason.RecognizedSpeech:
                Debug.Log($"✅ Recognized Speech: {result.Text}");

                // 📝 Set the recognized text in the InputField
                if (inputField != null)
                {
                    inputField.text = result.Text;
                }
                break;

            case ResultReason.NoMatch:
                Debug.Log("⚠️ No speech recognized.");
                break;

            case ResultReason.Canceled:
                var cancellation = CancellationDetails.FromResult(result);
                Debug.LogError($"❌ Speech recognition canceled: {cancellation.Reason}");
                if (cancellation.Reason == CancellationReason.Error)
                {
                    Debug.LogError($"❌ Error Details: {cancellation.ErrorDetails}");
                }
                break;
        }
    }
}
