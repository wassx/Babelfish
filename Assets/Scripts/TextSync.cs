using System;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;
using XRTK.Services;

public class TextSync : MonoBehaviour, IPunObservable {
    [SerializeField] private Text _text;
    private TextToSpeechService _textToSpeechService;

    private void Awake() {
        _textToSpeechService = MixedRealityToolkit.GetService<TextToSpeechService>();
    }

    public async void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        if (stream.IsWriting) {
            stream.SendNext(_text.text);
        } else {
            String text = (string) stream.ReceiveNext();
            if (_text.text != text) {
                _text.text = text;
                await _textToSpeechService.TextToSpeech(text);
            }
        }
    }

    public void SetText(Dictionary<string, string> results) {
        string message = "";
        foreach (string result in results.Values) {
            message += result + "\n";
        }
        _text.text = message;
    }
}