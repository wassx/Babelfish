using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using XRTK.Utilities;

public class CubeClickhandler : MonoBehaviour {
    public void OnCubeClicked() {
        Babbelfish babbelfish = CameraCache.Main.GetComponentInChildren<Babbelfish>();
        babbelfish.OnStartSpeech();
    }
}