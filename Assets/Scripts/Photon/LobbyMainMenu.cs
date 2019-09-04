﻿using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

namespace Photon {
    public class LobbyMainMenu : MonoBehaviourPunCallbacks {
        [SerializeField] private Button _loginButton;

        #region UNITY

        public void Awake() {
            PhotonNetwork.AutomaticallySyncScene = true;
            _loginButton.enabled = false;
            PhotonNetwork.ConnectUsingSettings();
        }

        #endregion

        #region PUN CALLBACKS

        public override void OnConnectedToMaster() {
            _loginButton.enabled = true;
        }

        public override void OnRoomListUpdate(List<RoomInfo> roomList) { }

        public override void OnDisconnected(DisconnectCause cause) {
            Debug.LogWarning("Disconnected due to: " + cause);
        }

        public override void OnLeftLobby() { }

        public override void OnCreateRoomFailed(short returnCode, string message) {
            Debug.LogWarning("message:" + message);
        }

        public override void OnJoinRoomFailed(short returnCode, string message) {
            Debug.LogWarning("message:" + message);
        }

        public override void OnJoinRandomFailed(short returnCode, string message) { }

        public override void OnJoinedRoom() {
            if (PhotonNetwork.IsMasterClient) {
                PhotonNetwork.LoadLevel("XRSharing");
            }
        }

        public override void OnLeftRoom() { }

        public override void OnPlayerEnteredRoom(Player newPlayer) {
            Debug.LogWarning("Player entered room.");
        }

        public override void OnPlayerLeftRoom(Player otherPlayer) { }

        public override void OnMasterClientSwitched(Player newMasterClient) { }

        public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps) { }

        #endregion

        #region UI CALLBACKS

        public void OnLeaveGameButtonClicked() {
            PhotonNetwork.LeaveRoom();
        }

        public void OnStartGameButtonClicked() {
            string roomName = "MrHackfestRoom";
            RoomOptions options = new RoomOptions {MaxPlayers = 0};
            PhotonNetwork.JoinOrCreateRoom(roomName, options, null);
        }

        #endregion
    }
}