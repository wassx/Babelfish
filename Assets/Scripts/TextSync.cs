using System;
using Microsoft.CognitiveServices.Speech;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;
using XRTK.Services;

public class TextSync : MonoBehaviour, IPunObservable {
    [SerializeField] private Text _text;
    private SpeechToTextService _translationService;

    private void Start() {
        _translationService = MixedRealityToolkit.GetService<SpeechToTextService>();
    }

    public async void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        if (stream.IsWriting) {
            stream.SendNext(_text.text);
        } else {
            String text = (string) stream.ReceiveNext();
            if (_text.text != text) {
                _text.text = text;
                await _translationService.TextToSpeech(text);
            }
        }
    }

    public void SetText(string text) {
        _text.text = text;
    }
}