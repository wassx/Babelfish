using Photon.Pun;
using UnityEngine;
using XRTK.Services;

public class Babelfish : MonoBehaviourPun {
    private TextSync _textSync;
    private SpeechToTextService _speechToTextService;

    private void Start() {
        _textSync = GetComponent<TextSync>();

        _textSync.SetText("This is a test");
        _speechToTextService = MixedRealityToolkit.GetService<SpeechToTextService>();
        // TODO: add handlers for successful recognition result
    }

    private void OnDestroy() {
        _speechToTextService.StopRecognizeSpeech();
        // Remove handler
    }

    public async void OnStartSpeech() {
        Debug.Log("Babelfish starts listening.");
        _textSync.SetText("Babelfish starts listening.");
        await _speechToTextService.StartRecognizeSpeech();
    }

    public void OnStopSpeech() {
        Debug.Log("Babelfish stops listening.");
        _textSync.SetText("Babelfish stops listening.");
        _speechToTextService.StopRecognizeSpeech();
    }
}