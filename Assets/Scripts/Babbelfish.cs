using UnityEngine;
using XRTK.Services;

public class Babbelfish : MonoBehaviour {
    private SpeechToTextService _translationService;
    private TextSync _textSyncScript;

    private void Start() {
        _translationService = MixedRealityToolkit.GetService<SpeechToTextService>();
        _translationService.OnRecognitionSuccessful += OnTranslationSuccessful;
        _textSyncScript = GetComponent<TextSync>();
    }

    private void OnDestroy() {
        _translationService.OnRecognitionSuccessful -= OnTranslationSuccessful;
    }

    private void OnTranslationSuccessful(string result) {
        _textSyncScript.SetText(result);
        Debug.LogWarning("Result: " + result);
    }

    public async void OnStartSpeech() {
        await _translationService.StartRecognizeSpeech();
    }

    public void OnStopSpeech() {
        _translationService.StopRecognition();
    }
}