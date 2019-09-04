using System;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class TextSync : MonoBehaviour, IPunObservable {
    [SerializeField] private Text _text;

    private void Awake() {
        // Optional get handle to TextToSpeechService
    }

    public async void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        if (stream.IsWriting) {
            stream.SendNext(_text.text);
        } else {
            String text = (string) stream.ReceiveNext();
            if (_text.text != text) {
                _text.text = text;

                // Optional call speech synthesis in TextToSpeechService
            }
        }
    }

    public void SetText(string message) {
        _text.text = message;
    }
}