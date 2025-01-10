using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestOllama : MonoBehaviour
{
    public OllamaAPIClient ollamaAPIClient;

    void Start()
    {
        // Call the StartConversation method without passing any arguments
        ollamaAPIClient.StartConversation();
    }
}
