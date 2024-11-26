using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestOllama : MonoBehaviour
{
    public OllamaAPIClient ollamaAPIClient;

    //public string userInput = "User input here";

    void Start()
    {
        ollamaAPIClient.StartConversation(ollamaAPIClient.userPrompt);
    }
}

