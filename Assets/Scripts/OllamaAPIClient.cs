using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;

public class OllamaAPIClient : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI inputField; // TextMeshPro InputField for user input
    [SerializeField] private TextMeshProUGUI typeWriterEffect; // UI TextMeshPro for the typewriter effect
    [SerializeField] public string userPrompt;

    public TestingTextToSpeech textToSpeech; // Reference to the TTS script

    public enum ModelEnum
    {
        Llama3,
        Gemma,
        DeepSeek,
        Phi3,
        Qwen2_5,
        Granite3

    };

    public ModelEnum whichModel;

    [System.Serializable]
    public class OllamaRequest
    {
        public string model;
        public string prompt;
        public bool stream;

        public OllamaRequest(ModelEnum selectedModel, string prompt, bool stream)
        {
            this.model = GetModelName(selectedModel); // Convert enum to correct model name
            this.prompt = prompt;
            this.stream = stream;
        }
    }

    [System.Serializable]
    public class StreamedChunk
    {
        public string model;
        public string response; // The actual text content
        public bool done;
    }

    private string apiUrl = "http://localhost:11434/api/generate"; // URL for the local Ollama API
    private List<string> conversationHistory = new List<string>();
    private const int maxHistory = 5;

    // Function to start the conversation with the current input from the InputField
    public void StartConversation()
    {
        userPrompt = inputField.text + ". Give a very short answer."; // For testing purposes only!
        if (string.IsNullOrEmpty(userPrompt))
        {
            Debug.LogWarning("Input field is empty. Please enter a prompt.");
            return;
        }

        // Append new user input to conversation history
        conversationHistory.Add("User: " + userPrompt);
        if (conversationHistory.Count > maxHistory)
        {
            conversationHistory.RemoveAt(0);
        }

        string fullContext = string.Join("\n", conversationHistory);
        StartCoroutine(SendToOllama(fullContext, ProcessStreamedResponse));
    }

    // Coroutine to send a POST request to Ollama's API with the user's input
    private IEnumerator SendToOllama(string userInput, Action<string> callback)
    {
        // Get the selected model as a correctly formatted string
        string selectedModel = GetModelName(whichModel);

        // Create an instance of the request payload class
        OllamaRequest requestData = new OllamaRequest(whichModel, userInput, true); // Enable streaming
        Debug.Log("Used model is: " + selectedModel);

        // Serialize the request data to JSON
        string jsonData = JsonUtility.ToJson(requestData);

        // Log the JSON payload to verify it's correct
        Debug.Log("Sending JSON payload: " + jsonData);

        // Set up the UnityWebRequest
        UnityWebRequest request = new UnityWebRequest(apiUrl, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        // Send the request
        request.SendWebRequest();

        string fullResponse = ""; // Holds the full progressive response
        int lastProcessedIndex = 0; // Tracks the last processed character index

        while (!request.isDone)
        {
            // Read the streamed response incrementally
            string accumulatedData = request.downloadHandler.text;

            // Extract only the new unprocessed part
            if (accumulatedData.Length > lastProcessedIndex)
            {
                string newData = accumulatedData.Substring(lastProcessedIndex);
                lastProcessedIndex = accumulatedData.Length; // Update the index tracker

                // Process each JSON object in the new data
                string[] jsonObjects = newData.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string jsonObject in jsonObjects)
                {
                    try
                    {
                        // Parse the individual JSON object
                        StreamedChunk streamedChunk = JsonUtility.FromJson<StreamedChunk>(jsonObject);

                        // Append only the new response text
                        if (!string.IsNullOrEmpty(streamedChunk.response))
                        {
                            fullResponse += streamedChunk.response; // Append the new text
                            typeWriterEffect.text = $"Assistant's Response:\n {fullResponse.Trim()}"; // Update UI
                        }

                        // Stop streaming if the "done" filed is true
                        if (streamedChunk.done)
                        {
                            callback(fullResponse.Trim());
                            yield break;
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogError("Error parsing chunk: " + e.Message);
                    }
                }
            }
            yield return null; // Wait for the next frame
        }

        if (request.result == UnityWebRequest.Result.Success)
        {
            callback(fullResponse.Trim()); // Final full response
        }
        else
        {
            Debug.LogError("Error: " + request.error);
        }
    }

    // Callback to process the full response after streaming is done
    private void ProcessStreamedResponse(string fullResponse)
    {
        Debug.Log("Final Full Response: " + fullResponse.Trim());
        LogToFile($"Prompt: {userPrompt}\nFinal Response: {fullResponse.Trim()}");
        // Send the final response to the Text-to-Speech system

        conversationHistory.Add("Assistant: " + fullResponse.Trim());
        if (conversationHistory.Count > maxHistory)
        {
            conversationHistory.RemoveAt(0);
        }

        if (textToSpeech != null)
        {
            textToSpeech.ReadInputStringAndPlay(fullResponse.Trim());
        }
        else
        {
            Debug.LogWarning("Text-to-Speech script is not assigned.");
        }
    }

    // Method to log the final response to a txt file
    private void LogToFile(string logEntry)
    {
        string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        string filePath = Path.Combine(desktopPath, "OllamaLogs.txt");

        try
        {
            using (StreamWriter writer = new StreamWriter(filePath, true)) // Append to file
            {
                writer.WriteLine(logEntry);
                writer.WriteLine("Timestamp: " + DateTime.Now);
                writer.WriteLine("---------------------------");
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to log to file: " + e.Message);
        }
    }
    // Get the correct model name based on the selected enum
    private static string GetModelName(ModelEnum selectedModel)
    {
        switch (selectedModel)
        {
            case ModelEnum.Llama3:
                return "llama3";
            case ModelEnum.Gemma:
                return "gemma";
            case ModelEnum.DeepSeek:
                return "deepseek-r1:1.5b";
            case ModelEnum.Phi3:
                return "phi3";
            case ModelEnum.Qwen2_5:
                return "qwen2.5:0.5b";
            case ModelEnum.Granite3:
                return "granite3-moe";
            default:
                return "deepseek-r1:1.5b"; // Default model is deepseek
        }
    }
}
