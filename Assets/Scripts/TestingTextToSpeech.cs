using System;
using System.Threading.Tasks;
using UnityEngine;

public class TestingTextToSpeech : MonoBehaviour
{
    private string input;
    private AudioSource audioSource;

    void Start()
    {
        ReadInputStringAndPlay("Hello world. Testing from Würzburg wassup?"); // Testing at the beginning to see if audio clip is working
        // init  the component
        audioSource = GetComponent<AudioSource>();

        // If there's no AudioSource, add one 
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            Debug.Log("No AudioSource found! Added one dynamically.");
        }
    }

    async Task StartAsync(string input)
    {
        var api = new ElevenLabs.ElevenLabsClient("sk_0d477f5af3dbc339e3d12f10d7117618eb21c4bb034333e3");
        //Testing API Key:          sk_aa4c68a93ac207564f8c3372555a2a86645190cfbe4aa346
        // Unity Praktikum API Key: sk_0bb620868c6f0780f8e6b78ad2981ebc29abdb00036c7edf
        // Sevi's API:              sk_0d477f5af3dbc339e3d12f10d7117618eb21c4bb034333e3 ✅✅✅ working APIc7edf");
        var text = input;
        var voices = await api.VoicesEndpoint.GetAllVoicesAsync();
        var neuerValue = 5;
        var Stimme = voices[neuerValue];
        Debug.Log($"Selected voice: {Stimme.Name}");

        var defaultVoiceSettings = await api.VoicesEndpoint.GetDefaultVoiceSettingsAsync();
        var voiceClip = await api.TextToSpeechEndpoint.TextToSpeechAsync(text, Stimme, defaultVoiceSettings);
        AudioClip audioClip = await voiceClip.LoadCachedAudioClipAsync();

        if (audioClip == null)
        {
            Debug.LogError("Failed to load AudioClip from VoiceClip.");
            return;
        }

        Debug.Log("Task starts TTS");

        // Play the audio clip 
        if (audioSource != null)
        {
            audioSource.clip = audioClip;
            audioSource.Stop();
            audioSource.Play();
            Debug.Log("Playing TTS audio...");
        }
        else
        {
            Debug.LogError("AudioSource component is missing on this GameObject.");
        }
    }

    public void ReadInputStringAndPlay(string s)
    {
        input = s;
        Debug.Log("Starting Task");
        Task task = StartAsync(input);
    }

    public void StopProcess()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
            Debug.Log("TTS playback stopped.");
        }
        else
        {
            Debug.Log("No active TTS to stop.");
        }
    }
}
