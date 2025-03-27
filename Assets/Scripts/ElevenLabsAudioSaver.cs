using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class ElevenLabsAudioSaver : MonoBehaviour
{
    [Header("ElevenLabs Settings")]
    [Tooltip("Your ElevenLabs API key")]
    public string apiKey = "sk_0e626ff89cbc9cc9f829affe7dbc9ab1c466b6dcc832be89";
    //New sevi elevenlabs account API key: sk_0e626ff89cbc9cc9f829affe7dbc9ab1c466b6dcc832be89

    [Tooltip("Voice ID to use")]
    public string voiceId = "5Q0t7uMcjvnagumLfvZi";

    [ContextMenu("Test Save Audio")]
    public void TestSaveAudio()
    {
        _ = SaveAudioToDesktop("Hello from Unity! This is a test.");
    }

    public async Task SaveAudioToDesktop(string text, string filenamePrefix = "tts")
    {
        if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(voiceId))
        {
            Debug.LogError("Missing API key or voice ID.");
            return;
        }

        string url = $"https://api.elevenlabs.io/v1/text-to-speech/{voiceId}";

        string json = "{\"text\":\"" + text + "\",\"model_id\":\"eleven_monolingual_v1\",\"voice_settings\":{\"stability\":0.5,\"similarity_boost\":0.5}}";

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("xi-api-key", apiKey);

            Debug.Log("Sending request to ElevenLabs...");

            var operation = request.SendWebRequest();
            while (!operation.isDone)
                await Task.Yield();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                string folderPath = Path.Combine(desktopPath, "audio files");
                Directory.CreateDirectory(folderPath);

                string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
                string fileName = $"{filenamePrefix}_{timestamp}.mp3";
                string filePath = Path.Combine(folderPath, fileName);

                File.WriteAllBytes(filePath, request.downloadHandler.data);
                Debug.Log($"MP3 saved to: {filePath}");
            }
            else
            {
                Debug.LogError("Failed to download MP3: " + request.error);
                Debug.LogError(request.downloadHandler.text);
            }
        }
    }
}
