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
    [SerializeField] private TextMeshProUGUI inputField; // Prompt input for typing
    [SerializeField] private TextMeshProUGUI typeWriterEffect; // UI effect
    public bool isProcessing = false;
    private bool stopProcess = false;

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

        public OllamaRequest(ModelEnum selectedModel, string prompt, bool stream) // stream is true or false
        {
            this.model = GetModelName(selectedModel); // Convert to correct model name
            this.prompt = prompt;
            this.stream = stream;
        }
    }

    [System.Serializable]
    public class StreamedChunk // Response send back from the LLM, it recives the response until bool "done" = true which is the end of the answer sentence
    {
        public string model;
        public string response; // The actual text content
        public bool done;
    }

    private string apiUrl = "http://localhost:11434/api/generate"; // URL for the local Ollama API
    private List<string> conversationHistory = new List<string>();
    private const int maxHistory = 5; // Max logged chat history

    // Function to start the conversation with the current input from the InputField
    public void StartConversation()
    {
        if (isProcessing) // Prevent new prompts while processing the responce
        {
            Debug.Log("Previous response is still being processed. Please wait...");
            return;
        }

        stopProcess = false; // Allow for new prompt
        isProcessing = true; // Lock new requests until the current one is complete

        userPrompt = inputField.text + ". Give a very short answer."; // For testing purposes only!
        if (string.IsNullOrEmpty(userPrompt))
        {
            Debug.Log("Input field is empty. Please enter a prompt.");
            return;
        }

        // Add new user input to conversation history
        conversationHistory.Add("User: " + userPrompt);
        if (conversationHistory.Count > maxHistory)
        {
            conversationHistory.RemoveAt(0); // Remove the first sent messeage
        }

        string fullContext = string.Join("\n", conversationHistory); // Combining  all stored messages into one multi-line string
        // Debug.Log("Prompt sent: " + userPrompt); // Debug to confirm prompt is sent
        StartCoroutine(SendToOllama(fullContext, ProcessStreamedResponse)); // fullContext = prev messages, processedresponce = new message
    }

    // Send a POST request to Ollama's API with the user input
    private IEnumerator SendToOllama(string userInput, Action<string> callback)
    {
        // Get the selected model as a correctly formatted string
        string selectedModel = GetModelName(whichModel);

        // Create an instance of the request 
        OllamaRequest requestData = new OllamaRequest(whichModel, userInput, true); // Enable streaming "true"
        Debug.Log("Used model is: " + selectedModel);

        // JSON request
        string jsonData = JsonUtility.ToJson(requestData);
        // For example a JSON request might look like;
        //{"model":"llama3","prompt":"Hello","stream":true}

        // Log the JSON payload to verify it's correct
        Debug.Log("Sending JSON payload: " + jsonData);

        // Unity Web Request sends the JSON to Ollama API
        UnityWebRequest request = new UnityWebRequest(apiUrl, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        // Send the request
        request.SendWebRequest();

        string fullResponse = ""; // Holds the full progressive response chunks
        int lastProcessedIndex = 0; // 	Keeps track of how much of the responce has been read so far

        while (!request.isDone)
        {
            if (stopProcess)
            {
                Debug.Log("Process stopped by user. Aborting request!");
                request.Abort(); // Stop network request
                yield break; // Exit
            }

            string accumulatedData = request.downloadHandler.text;  // Gets all the text received so far from the LLM

            // Extract only the new unprocessed part
            if (accumulatedData.Length > lastProcessedIndex)
            {
                string newData = accumulatedData.Substring(lastProcessedIndex);
                lastProcessedIndex = accumulatedData.Length; // Update the index tracker

                // Process each JSON object in the new data
                string[] jsonObjects = newData.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries); // Each LLM response is a JSON object per line.
                foreach (string jsonObject in jsonObjects) // Lookin thorugh each recieved JSON chunks
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

    // Process response after streaming is done
    private void ProcessStreamedResponse(string fullResponse)
    {
        if (stopProcess) // If stopped, do nothing
        {
            Debug.Log("Process stopped. Ignoring final response.");
            isProcessing = false; // Allow new prompt 
            return;
        }

        Debug.Log("Final Full Response: " + fullResponse.Trim());
        LogToFile($"Prompt: {userPrompt}\nFinal Response: {fullResponse.Trim()}");

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
            Debug.Log("Text-to-Speech script is not assigned.");
        }

        isProcessing = false; // Allow new prompts after response is complete
    }

    public void StopProcess()
    {
        if (!isProcessing)
        {
            Debug.Log("No active process to stop.");
            return;
        }

        stopProcess = true; // Stop processing new data from API
        isProcessing = false; // Allow  prompt entry

        Debug.Log("Process stopped by user.");
        typeWriterEffect.text = "X Process stopped by user! X"; // Updateing UI

        if (conversationHistory.Count > 0)
        {
            conversationHistory.RemoveAt(conversationHistory.Count - 1); // Remove last user prompt if process is stopped
        }

        Debug.Log("Cleared last prompt to prevent response continuation.");

        // Stop TTS if it's currently playing
        if (textToSpeech != null)
        {
            textToSpeech.StopProcess();
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
    // Get the correct model name based on the selection
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
                return "llama3"; // Default model is llama3
        }
    }
}
