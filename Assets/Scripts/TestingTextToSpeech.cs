using System;
using System.Threading.Tasks;
using UnityEngine;

public class TestingTextToSpeech : MonoBehaviour
{
    private string input;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }

    async Task StartAsync(string input)
    {
        // Use your API key
        var api = new ElevenLabs.ElevenLabsClient("sk_0bb620868c6f0780f8e6b78ad2981ebc29abdb00036c7edf");

        var text = input;
        var voices = await api.VoicesEndpoint.GetAllVoicesAsync();

        var neuerValue = 0; // Select the first voice in the list
        var Stimme = voices[neuerValue];
        Debug.Log($"Selected voice: {Stimme.Name}");

        var defaultVoiceSettings = await api.VoicesEndpoint.GetDefaultVoiceSettingsAsync();

        // Fetch the text-to-speech response
        var voiceClip = await api.TextToSpeechEndpoint.TextToSpeechAsync(text, Stimme, defaultVoiceSettings);

        // Debug the voiceClip object to see available properties
        Debug.Log($"VoiceClip object: {JsonUtility.ToJson(voiceClip)}");

        // Load the audio clip explicitly
        AudioClip audioClip = await voiceClip.LoadCachedAudioClipAsync(); // Load the audio clip
        if (audioClip == null)
        {
            Debug.LogError("Failed to load AudioClip from VoiceClip.");
            return;
        }

        Debug.Log("Task starts TTS");

        // Play the audio clip
        AudioSource audio = GetComponent<AudioSource>();
        if (audio != null)
        {
            audio.clip = audioClip;
            audio.Play();
            Debug.Log($"Playing audio with the input '{input}'");
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
}
