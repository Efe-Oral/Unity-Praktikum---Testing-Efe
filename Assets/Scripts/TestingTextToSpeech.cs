using System;
using System.Threading.Tasks;
using UnityEngine;

public class TestingTextToSpeech : MonoBehaviour
{
    private string input;
    private AudioSource audioSource;
    public ElevenLabsAudioSaver audioSaver;

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
        var api = new ElevenLabs.ElevenLabsClient("sk_69f1014b5da7d42cb4b36ebb64454ae8f764bc8c990fe145");
        //New temp API: sk_db3d8a9a858dd8c82a76501dd054efbe6e4e4212c1eda54f 
        //New temp API: sk_7e9d7a3627b7adafddf59ab33a75f03f0b5ae07bcdf84800 // Restricted key, allowed only Text to Speech, Voices (read), Voices (write)
        var text = input;
        var voices = await api.VoicesEndpoint.GetAllVoicesAsync();
        var neuerValue = 5;
        var Stimme = voices[neuerValue];
        Debug.Log($"Selected voice: {Stimme.Name}");

        var defaultVoiceSettings = await api.VoicesEndpoint.GetDefaultVoiceSettingsAsync();
        var voiceClip = await api.TextToSpeechEndpoint.TextToSpeechAsync(text, Stimme, defaultVoiceSettings);

        await audioSaver.SaveAudioToDesktop(input, "response");

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
