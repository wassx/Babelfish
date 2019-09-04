using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class DestroyLocalPlayerObjects : MonoBehaviourPun {
    [SerializeField] private GameObject[] _playerObjects;
    private void OnEnable() {
        if (photonView.IsMine) {
            foreach (GameObject playerObject in _playerObjects) {
                Destroy(playerObject);
            }
        }
    }
}
