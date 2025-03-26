using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestOllama : MonoBehaviour
{
    public OllamaAPIClient ollamaAPIClient;

    void Start()
    {
        // Initial start call
        ollamaAPIClient.StartConversation();
    }
}
