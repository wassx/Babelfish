using System.Collections.Generic;
using Photon.Pun;
using XRTK.Services;

public class Babelfish : MonoBehaviourPun {
    private SpeechToTextService _translationService;
    private TextSync _textSyncScript;

    private void Start() {
        _translationService = MixedRealityToolkit.GetService<SpeechToTextService>();
        _translationService.OnRecognitionSuccessful += OnTranslationSuccessful;
        _textSyncScript = GetComponent<TextSync>();
    }

    private void OnDestroy() {
        _translationService.OnRecognitionSuccessful -= OnTranslationSuccessful;
        _translationService.StopRecognition();
    }

    private void OnTranslationSuccessful(Dictionary<string, string> results) {
        if (!photonView.IsMine) {
            return;
        }

        _textSyncScript.SetText(results);
    }

    public async void OnStartSpeech() {
        await _translationService.StartRecognizeSpeech();
    }

    public void OnStopSpeech() {
        _translationService.StopRecognition();
    }
}