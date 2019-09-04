using System.Collections.Generic;
using Photon.Pun;
using XRTK.Services;

public class Babelfish : MonoBehaviourPun {
    private TextSync _textSync;
    private SpeechToTextService _speechToTextService;

    private void Start() {
        _textSync = GetComponent<TextSync>();

        _speechToTextService = MixedRealityToolkit.GetService<SpeechToTextService>();
        _speechToTextService.OnTranslationSuccessful += OnTranslationSuccessful;
    }

    private void OnTranslationSuccessful(Dictionary<string, string> results) {
        if (!photonView.IsMine) {
            return;
        }
        _textSync.SetText(results);
    }

    private void OnDestroy() {
        _speechToTextService.StopRecognizeSpeech();
        _speechToTextService.OnTranslationSuccessful -= OnTranslationSuccessful;
    }

    public async void OnStartSpeech() {
        await _speechToTextService.StartRecognizeSpeech();
    }

    public void OnStopSpeech() {
        _speechToTextService.StopRecognizeSpeech();
    }
}