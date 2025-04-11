using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class ElevenLabsAudioSaver : MonoBehaviour
{
    [Header("Assign AudioClip to This AudioSource")]
    public AudioSource targetAudioSource;

    [Header("ElevenLabs Settings")]
    public string apiKey = "sk_69f1014b5da7d42cb4b36ebb64454ae8f764bc8c990fe145";
    //New sevi elevenlabs account API key: sk_0e626ff89cbc9cc9f829affe7dbc9ab1c466b6dcc832be89
    public string voiceId = "EXAVITQu4vr4xnSDxMaL"; // 5Q0t7uMcjvnagumLfvZi

    [ContextMenu("Test Save Audio")]
    public void TestSaveAudio()
    {
        _ = SaveAudioToDesktop("Hello from Unity! This is a test.");
    }

    [Serializable]
    private class VoiceSettings
    {
        public float stability = 0.5f;
        public float similarity_boost = 0.5f;
    }

    [Serializable]
    private class TTSRequest
    {
        public string text;
        public string model_id = "eleven_monolingual_v1";
        public VoiceSettings voice_settings = new VoiceSettings();
    }

    public async Task SaveAudioToDesktop(string text, string filenamePrefix = "tts")
    {
        if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(voiceId))
        {
            Debug.LogError("Missing API key or voice ID.");
            return;
        }

        string url = $"https://api.elevenlabs.io/v1/text-to-speech/{voiceId}";

        TTSRequest payload = new TTSRequest { text = text };
        string json = JsonUtility.ToJson(payload);

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
                Debug.Log($"Audio saved to: {filePath}");

                StartCoroutine(LoadAndAssignAudioClip(filePath));
            }
            else
            {
                Debug.LogError("Failed to download audio: " + request.error);
                Debug.LogError(request.downloadHandler.text);
            }
        }
    }

    private IEnumerator LoadAndAssignAudioClip(string filePath)
    {
        string uri = "file://" + filePath;
        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(uri, AudioType.MPEG))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
                if (targetAudioSource != null)
                {
                    targetAudioSource.clip = clip;
                    targetAudioSource.Play(); // Optional: auto-play to test
                    Debug.Log("AudioClip loaded and assigned!");
                }
            }
            else
            {
                Debug.LogError("Failed to load AudioClip: " + www.error);
            }
        }
    }
}
