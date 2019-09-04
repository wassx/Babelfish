using System;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;
using XRTK.Services;

public class TextSync : MonoBehaviour, IPunObservable {
    [SerializeField] private Text _text;
    private SpeechToTextService _translationService;

    private void Awake() {
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

    public void SetText(Dictionary<string, string> results) {
        string message = "";
        foreach (string result in results.Values) {
            message += result + "\n";
        }

        Debug.Log("results: " + message);
        //results.TryGetValue("de", out message);
        _text.text = message;
    }
}