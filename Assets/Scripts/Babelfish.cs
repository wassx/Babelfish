using Photon.Pun;
using UnityEngine;

public class Babelfish : MonoBehaviourPun {
    private TextSync _textSync;

    private void Start() {
        _textSync = GetComponent<TextSync>();

        _textSync.SetText("This is a test");
        // TODO: Get reference to speech to text service
        // TODO: add handlers for successful recognition result
    }

    private void OnDestroy() {
        // Stop recognition
        // Remove handler
    }

    public async void OnStartSpeech() {
        Debug.Log("Babelfish starts listening.");
        _textSync.SetText("Babelfish starts listening.");
        // TODO: Start speech recognition by calling service
    }

    public void OnStopSpeech() {
        Debug.Log("Babelfish stops listening.");
        _textSync.SetText("Babelfish stops listening.");
        // TODO: Stop speech recognition by calling service
    }
}