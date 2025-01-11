using System;
using System.Collections;
using System.Collections.Generic;
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

        var api = new ElevenLabs.ElevenLabsClient();

        var text = input;
        var voice = await api.VoicesEndpoint.GetAllVoicesAsync();

        var neuerValue = 0;
        var Stimme = voice[neuerValue];
        Debug.Log(Stimme);







        var defaultVoiceSettings = await api.VoicesEndpoint.GetDefaultVoiceSettingsAsync();
        var (clipPath, audioClip) = await api.TextToSpeechEndpoint.TextToSpeechAsync(text, Stimme, defaultVoiceSettings);
        Debug.Log(clipPath);
        Debug.Log("Task starts TTS");
        AudioSource audio = GetComponent<AudioSource>();
        audio.clip = audioClip;
        audio.Play();
        Debug.Log("Playing audio with the input '" + input + "' from the path: " + clipPath);


    }


    public void ReadInputStringAndPlay(string s)
    {
        input = s;

        //Debug.Log("Have gotten this input: " + input);

        Debug.Log("Starting Task");
        Task task = StartAsync(input);
    }



}
