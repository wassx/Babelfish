using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class TextSync : MonoBehaviour, IPunObservable {
    [SerializeField] private Text _text;

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