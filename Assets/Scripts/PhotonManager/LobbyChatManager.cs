using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using System.Collections;

public class LobbyChatManager : MonoBehaviourPunCallbacks
{
    [Header("UI References")]
    [SerializeField] private TMP_InputField messageInputField;
    [SerializeField] private TMP_Text chatHistoryText;
    [SerializeField] private ScrollRect chatScroolView;


    public void SendMessageButton()
    {
        string message = messageInputField.text;

        if(!string.IsNullOrEmpty(message))
        {
            photonView.RPC("ReceiveChatMessage", RpcTarget.All, PhotonNetwork.NickName, message);
        }

        messageInputField.text = "";

        messageInputField.ActivateInputField();
    }
    [PunRPC]
    public void ReceiveChatMessage(string senderName,string message)
    {
        string formattedMessage = $"\n{senderName}: {message}";

        chatHistoryText.text += formattedMessage;

        StartCoroutine(ScrollToBottom());
    }

    IEnumerator ScrollToBottom()
    {
        yield return null;
        chatScroolView.verticalNormalizedPosition = 0f;
    }
}
