using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using System.Collections;
using Firebase.Auth;
using Firebase.Firestore;
using System.Collections.Generic;

public class LobbyChatManager : MonoBehaviourPunCallbacks
{
    [Header("UI References")]
    [SerializeField] private TMP_InputField messageInputField;
    [SerializeField] private TMP_Text chatHistoryText;
    [SerializeField] private ScrollRect chatScroolView;

    public override void OnEnable()
    {
        base.OnEnable();
    }
    public override void OnDisable()
    {
        base.OnDisable();
    }

    public void SendMessageButton()
    {
        string message = messageInputField.text;

        if(!string.IsNullOrEmpty(message))
        {
            photonView.RPC("ReceiveChatMessage", RpcTarget.All, PhotonNetwork.NickName, message);

            SaveMessageForFirestore(message);
        }

        messageInputField.text = "";
        messageInputField.DeactivateInputField();
    }
    public void SaveMessageForFirestore(string messageText)
    {
        FirebaseUser user = FirebaseAuth.DefaultInstance.CurrentUser;
        if(user == null)
        {
            Debug.LogError("Mesaj veritabanýna kaydedilemedi: Kullanýcý giriþi yapýlmamýþ!");
            return;
        }

        FirebaseFirestore db = FirebaseFirestore.DefaultInstance;

        DocumentReference userChatDocRef = db.Collection("user_chats").Document(user.UserId);

        Dictionary<string, object> messageData = new Dictionary<string, object>
        {
            { "message_text", messageText },
            { "chat_context", "Lobby" },
            { "timestamp", FieldValue.ServerTimestamp }
        };

        userChatDocRef.Collection("messages").AddAsync(messageData).ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("Mesaj kaydedilirken hata oluþtu: " + task.Exception);
            }
        });

        Dictionary<string, object> userDocUpdate = new Dictionary<string, object>
        {
            { "last_message_timestamp", FieldValue.ServerTimestamp },
            { "nickname", PhotonNetwork.NickName }
        };

        userChatDocRef.SetAsync(userDocUpdate, SetOptions.MergeAll).ContinueWith(task =>
        {
            if (task.IsCompleted)
            {
                Debug.Log("Kullanýcý mesajý ve bilgileri baþarýyla Firestore'a kaydedildi.");
            }
        });
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
        Canvas.ForceUpdateCanvases();

        LayoutRebuilder.ForceRebuildLayoutImmediate(chatScroolView.content);

        chatScroolView.verticalNormalizedPosition = 0f;

        yield return null;
        chatScroolView.verticalNormalizedPosition = 0f;
    }
}
