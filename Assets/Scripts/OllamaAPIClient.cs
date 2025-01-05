using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.IO;
using System;

public class OllamaAPIClient : MonoBehaviour
{
    [SerializeField] public string userPrompt;

    public enum ModelEnum
    {
        Llama3,
        Gemma
    };

    public ModelEnum whichModel;

    [System.Serializable]
    public class OllamaRequest
    {
        public string model;
        public string prompt;
        public bool stream;

        public OllamaRequest(string model, string prompt, bool stream)
        {
            this.model = model;
            this.prompt = prompt;
            this.stream = stream;
        }
    }

    // URL for the local Ollama API
    private string apiUrl = "http://localhost:11434/api/generate";

    // Function to start the conversation, sending user input to the Ollama API
    public void StartConversation(string userInput)
    {
        userInput = userPrompt;
        StartCoroutine(SendToOllama(userInput, ProcessResponse));
    }

    // Coroutine to send a POST request to Ollama's API with the user's input
    private IEnumerator SendToOllama(string userInput, System.Action<string> callback)
    {
        // Convert the selected modelEnum to string
        string selectedModel = whichModel.ToString();

        // Create an instance of the request payload class
        OllamaRequest requestData = new OllamaRequest(selectedModel, userInput, false); //final paramater determines "streaming" the response
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

        // Wait for the request to complete
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            callback(request.downloadHandler.text);
        }
        else
        {
            Debug.LogError("Error: " + request.error);
        }
    }

    // Callback function to process the response from Ollama API
    private void ProcessResponse(string responseText)
    {
        try
        {
            // Parse the JSON response to extract only the "response" field
            SimpleResponse simpleResponse = JsonUtility.FromJson<SimpleResponse>(responseText);

            if (!string.IsNullOrEmpty(simpleResponse.response))
            {
                // Log only the "response" field to the console
                Debug.Log("Assistant's Response: " + simpleResponse.response);

                // Log to file
                LogToFile($"Prompt: {userPrompt}\nResponse: {simpleResponse.response}");
            }
            else
            {
                Debug.LogWarning("No valid response received.");
                LogToFile($"Prompt: {userPrompt}\nResponse: No valid response received.");
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error processing response: " + e.Message);
        }
    }

    // Class structure to map the JSON response (only includes the "response" field)
    [System.Serializable]
    public class SimpleResponse
    {
        public string response;


    }


    // Method to log data to a file on the desktop
    private void LogToFile(string logEntry)
    {
        string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        string filePath = Path.Combine(desktopPath, "OllamaLogs.txt");

        try
        {
            using (StreamWriter writer = new StreamWriter(filePath, true))
            {
                writer.WriteLine(logEntry);
                writer.WriteLine("Timestamp: " + DateTime.Now);
                writer.WriteLine("---------------------------");
            }
            // .txt file where the conversation logged
            //Debug.Log("Logged to file: " + filePath);
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to log to file: " + e.Message);
        }
    }
}

// Class structure to map the JSON response from Ollama API
[System.Serializable]
public class OllamaResponse
{
    public string id;
    public string model;
    public Choice[] choices;

    [System.Serializable]
    public class Choice
    {
        public string text;
    }
}
