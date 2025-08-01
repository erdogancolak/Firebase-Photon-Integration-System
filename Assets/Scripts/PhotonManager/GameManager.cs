using Photon.Pun;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private void Start()
    {
        GameStateManager.LastSceneName = "GameScene";
        GameStateManager.LastRoomName = PhotonNetwork.CurrentRoom.Name;
    }
}
