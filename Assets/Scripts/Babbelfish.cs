using UnityEngine;
using XRTK.Services;

public class Babbelfish : MonoBehaviour {
    private SpeechToTextService _translationService;
    private TextSync _textSyncScript;

    private void Start() {
        _translationService = MixedRealityToolkit.GetService<SpeechToTextService>();
        _textSyncScript = GetComponent<TextSync>();
    }

    public async void OnStartSpeech() {
        string result = await _translationService.RecognizeSpeech();
        Debug.LogWarning("Text result: " + result);
        _textSyncScript.SetText(result);
    }

}