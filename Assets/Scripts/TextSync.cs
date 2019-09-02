using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using TMPro;
using UnityEngine;

public class TextSync : MonoBehaviour, IPunObservable {
    [SerializeField] private TextMeshPro _text;

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        if (stream.IsWriting) {
            stream.SendNext(_text.text);
        } else {
            _text.text = (string) stream.ReceiveNext();
        }
    }

    public void SetText(string text) {
        _text.text = text;
    }
}