using Firebase.Firestore;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FriendsUIManager : MonoBehaviourPunCallbacks
{
    [Header("UI Referansları")]
    [SerializeField] private TMP_InputField addFriendInput;

    [SerializeField] private Transform friendListContent; 
    [SerializeField] private GameObject friendListItemPrefab;

    [SerializeField] private Transform requestListContent;
    [SerializeField] private GameObject friendRequestItemPrefab;

    [SerializeField] private TMP_Text notificationText;
    private Coroutine notificationCoroutine;
    private class FriendDisplayInfo
    {
        public string UserID;
        public string Nickname;
        public bool IsOnline;
        public bool IsInRoom;
        public string RoomName;
    }

    private List<FriendDisplayInfo> localFriendList = new List<FriendDisplayInfo>();

    private Dictionary<string, GameObject> friendUIObjects = new Dictionary<string, GameObject>();

    private ListenerRegistration requestListener;
    private ListenerRegistration friendListener;

    private FirebaseFirestore db;
    private bool hasInitialLoadCompleted = false; 
    private bool isFriendListLoading = false;

    private void Start()
    {
        db = FirebaseFirestore.DefaultInstance;

        StartCoroutine(WaitForDependenciesAndLoad());
    }

    private IEnumerator WaitForDependenciesAndLoad()
    {
        if(hasInitialLoadCompleted)
        {
            yield break;
        }

        Debug.Log("FriendUIManager, bağımlılıkların hazır olmasını bekliyor...");

        while (UserDataManager.instance == null || !UserDataManager.instance.isDataLoaded || !PhotonNetwork.IsConnectedAndReady)
        {
            yield return null; 
        }

        Debug.Log("Tüm bağımlılıklar hazır! Dinleyiciler ilk kez yükleniyor...");

        ListenForFriendListChanges();
        ListenForFriendRequests();

        hasInitialLoadCompleted = true;
        yield return null;
    }

    public async Task RefreshAllUILists()
    {
        await LoadAndDisplayFriends();
    }

    public override void OnJoinedLobby()
    {
        base.OnJoinedLobby();
        Debug.Log("FriendsUIManager: Lobiye başarıyla girildi. Arkadaş durumu güncellemeleri başlatılıyor.");

        StartFriendStatusUpdates();
    }
    public async Task LoadAndDisplayFriends(List<string> newFriendIDs = null)
    {
        if (isFriendListLoading) return;

        try
        {
            isFriendListLoading = true;

            foreach (var item in friendUIObjects.Values) if (item != null) Destroy(item.gameObject);
            friendUIObjects.Clear();
            localFriendList.Clear();

            if (UserDataManager.instance == null) return;

            List<string> friendIDs = newFriendIDs ?? UserDataManager.instance.FriendList;
            if (friendIDs == null || friendIDs.Count == 0)
            {
                foreach (Transform child in friendListContent) Destroy(child.gameObject);
                return;
            }

            var friendUserIDsForPhoton = new List<string>();

            foreach (string friendId in friendIDs)
            {
                DocumentSnapshot snapshot = await db.Collection("kullanicilar").Document(friendId).GetSnapshotAsync();
                if (snapshot.Exists)
                {
                    string nickname = snapshot.GetValue<string>("nickname");
                    localFriendList.Add(new FriendDisplayInfo { UserID = friendId, Nickname = nickname , IsOnline = false , IsInRoom = false});
                    friendUserIDsForPhoton.Add(friendId);
                    GameObject friendUI = Instantiate(friendListItemPrefab, friendListContent);
                    friendUI.transform.Find("FriendNicknameText").GetComponent<TMP_Text>().text = nickname;
                    TMP_Text statusText = friendUI.transform.Find("StatusText").GetComponent<TMP_Text>();
                    statusText.text = "Offline";
                    statusText.color = Color.grey;
                    friendUIObjects[friendId] = friendUI;
                }
            }

            if (friendUserIDsForPhoton.Count > 0)
            {
                PhotonNetwork.FindFriends(friendUserIDsForPhoton.ToArray());
            }
        }
        finally
        {
            isFriendListLoading = false;
        }
    }
    void ListenForFriendRequests()
    {
        requestListener?.Stop();

        if (UserDataManager.instance == null) return;
        string myId = UserDataManager.instance.UserID;
        DocumentReference docRef = db.Collection("kullanicilar").Document(myId);

        requestListener = docRef.Listen(snapshot =>
        {
            Debug.Log("Arkadaşlık istekleri verisi güncellendi!");
            if (snapshot.Exists)
            {
                UpdateRequestsUI(snapshot);
            }
        });
    }
    void ListenForFriendListChanges()
    {
        friendListener?.Stop();

        if (UserDataManager.instance == null) return;
        string myId = UserDataManager.instance.UserID;
        DocumentReference docRef = db.Collection("kullanicilar").Document(myId);

        friendListener = docRef.Listen(snapshot =>
        {
            Debug.Log("Arkadaş listesi verisi güncellendi! Arayüz yenileniyor...");
            if (snapshot.Exists)
            {
                var newFriendList = snapshot.ContainsField("friends") ? snapshot.GetValue<List<string>>("friends") : new List<string>();

                UserDataManager.instance.FriendList = newFriendList;

                _ = LoadAndDisplayFriends(newFriendList);
            }
        });
    }
    void UpdateRequestsUI(DocumentSnapshot snapshot)
    {
        foreach (Transform child in requestListContent)
        {
            Destroy(child.gameObject);
        }

        var requests = new List<Dictionary<string, object>>();
        if (snapshot.ContainsField("friendRequestsReceived"))
        {
            requests = snapshot.GetValue<List<Dictionary<string, object>>>("friendRequestsReceived");
        }

        foreach (var requestData in requests)
        {
            string senderNickname = requestData["senderNickname"].ToString();
            GameObject requestItem = Instantiate(friendRequestItemPrefab, requestListContent);
            requestItem.transform.Find("RequestSenderNameText").GetComponent<TMP_Text>().text = senderNickname;

            Button acceptButton = requestItem.transform.Find("AcceptButton").GetComponent<Button>();
            acceptButton.onClick.AddListener(() => OnClick_AcceptRequest(requestData));

            Button rejectButton = requestItem.transform.Find("RejectButton").GetComponent<Button>();
            rejectButton.onClick.AddListener(() => OnClick_RejectRequest(requestData));
        }
    }
    private void UpdateFriendStatuses()
    {
        if (localFriendList.Count == 0 ||!hasInitialLoadCompleted) return;

        string[] friendUserIDs = localFriendList.Select(f => f.UserID).ToArray();
        PhotonNetwork.FindFriends(friendUserIDs);
        Debug.Log("Arkadaşların online durumları güncelleniyor...");
    }
    private void StartFriendStatusUpdates()
    {
        if (!IsInvoking(nameof(UpdateFriendStatuses)))
        {
            Debug.Log("Arkadaş durumu güncelleme döngüsü başlatılıyor...");
            InvokeRepeating(nameof(UpdateFriendStatuses), 5f, 5f);
        }
    }

    private void StopFriendStatusUpdates()
    {
        Debug.Log("Arkadaş durumu güncelleme döngüsü durduruluyor...");
        CancelInvoke(nameof(UpdateFriendStatuses));
    }
    public override void OnFriendListUpdate(List<FriendInfo> photonFriendList)
    {
        Debug.Log($"Photon'dan arkadaş listesi güncellemesi geldi. {photonFriendList.Count} kişi hakkında bilgi içeriyor.");

        foreach (var friend in localFriendList)
        {
            friend.IsOnline = false;
            friend.IsInRoom = false;
            friend.RoomName = null;
        }
        
        foreach (var photonFriend in photonFriendList)
        {
            FriendDisplayInfo friendToUpdate = localFriendList.FirstOrDefault(f => f.UserID == photonFriend.UserId);

            if (friendToUpdate != null && photonFriend.IsOnline)
            {
                friendToUpdate.IsOnline = true;
                friendToUpdate.IsInRoom = photonFriend.IsInRoom;
                if (photonFriend.IsInRoom)
                {
                    friendToUpdate.RoomName = photonFriend.Room;
                }
            }
        }

        var sortedFriendList = localFriendList
        .OrderByDescending(f => f.IsOnline)  
        .ThenByDescending(f => f.IsInRoom)   
        .ThenBy(f => f.Nickname)            
        .ToList();

        foreach (var sortedFriend in sortedFriendList)
        {
            if (friendUIObjects.TryGetValue(sortedFriend.UserID, out GameObject friendUI))
            {
                TMP_Text statusText = friendUI.transform.Find("StatusText").GetComponent<TMP_Text>();
                if (sortedFriend.IsOnline)
                {
                    statusText.text = sortedFriend.IsInRoom ? "In Room" : "Online";
                    statusText.color = Color.green;
                }
                else
                {
                    statusText.text = "Offline";
                    statusText.color = Color.grey;
                }

                Button joinButton = friendUI.transform.Find("JoinButton").GetComponent<Button>();
                if (sortedFriend.IsInRoom && !string.IsNullOrEmpty(sortedFriend.RoomName))
                {
                    joinButton.gameObject.SetActive(true);
                    joinButton.onClick.RemoveAllListeners(); 
                    joinButton.onClick.AddListener(() => OnClick_JoinFriendRoom(sortedFriend.RoomName));
                }
                else
                {
                    joinButton.gameObject.SetActive(false);
                }

                friendUI.transform.SetAsLastSibling();
            }
        }
    }
    public void OnClick_JoinFriendRoom(string roomName)
    {
        Debug.Log($"Arkadaşın odasına katılınıyor: {roomName}");

        PhotonNetwork.JoinRoom(roomName);
        ShowNotification("Odaya Katılınıyor...", Color.white, 3f);
    }
    
    public void OnClick_SendFriendRequest()
    {
        string nicknameToAdd = addFriendInput.text.Trim();
        addFriendInput.text = "";

        Debug.Log($"Arkadaş ekleme denemesi: '{nicknameToAdd}'");

        if (string.IsNullOrEmpty(nicknameToAdd))
        {
            Debug.LogWarning("Nickname boş olamaz.");
            ShowNotification("Lütfen bir kullanıcı adı girin.", Color.yellow, 3f);
            return;
        }

        if (UserDataManager.instance != null && nicknameToAdd == UserDataManager.instance.UserNickname)
        {
            Debug.LogWarning("Kullanıcı kendini eklemeye çalıştı.");
            ShowNotification("Kendinizi arkadaş olarak ekleyemezsiniz.", Color.yellow, 3f);
            return;
        }

        _ = SendFriendRequestAsync(nicknameToAdd);
    }
    private async Task SendFriendRequestAsync(string nicknameToAdd)
    {
        Debug.Log($"SendFriendRequestAsync başlatıldı. Aranacak isim: '{nicknameToAdd}'");

        QuerySnapshot snapshot = await db.Collection("kullanicilar").WhereEqualTo("nickname", nicknameToAdd).GetSnapshotAsync();

        if (snapshot.Count == 0)
        {
            ShowNotification($"'{nicknameToAdd}' isminde bir kullanıcı bulunamadı.", Color.red, 4f);
            Debug.LogWarning($"Firestore'da '{nicknameToAdd}' isminde kullanıcı bulunamadı.");
            return;
        }

        string targetUserID = snapshot.Documents.First().Id;

        if (UserDataManager.instance.FriendList.Contains(targetUserID))
        {
            ShowNotification("Bu oyuncu zaten arkadaş listenizde.", Color.cyan, 4f);
            return;
        }

        DocumentReference targetUserDocRef = db.Collection("kullanicilar").Document(targetUserID);
        var targetUserSnapshot = await targetUserDocRef.GetSnapshotAsync();

        if (targetUserSnapshot.Exists)
        {
            var requests = new List<Dictionary<string, object>>();

            if (targetUserSnapshot.ContainsField("friendRequestsReceived"))
            {
                requests = targetUserSnapshot.GetValue<List<Dictionary<string, object>>>("friendRequestsReceived");
            }

            if (requests != null && requests.Any(req => req["senderId"].ToString() == UserDataManager.instance.UserID))
            {
                ShowNotification("Bu oyuncuya zaten bir istek göndermişsiniz.", Color.yellow, 4f);
                return;
            }
        }

        var newRequest = new Dictionary<string, object>
        {
            { "senderId", UserDataManager.instance.UserID },
            { "senderNickname", UserDataManager.instance.UserNickname }
        };

        Debug.Log($"'{targetUserID}' ID'li kullanıcıya istek gönderiliyor...");
        await targetUserDocRef.UpdateAsync("friendRequestsReceived", FieldValue.ArrayUnion(newRequest));

        ShowNotification("Arkadaşlık isteği gönderildi.", Color.green, 4f);
        Debug.Log($"Arkadaşlık isteği {nicknameToAdd} kullanıcısına başarıyla gönderildi.");
    }
    public async void OnClick_AcceptRequest(Dictionary<string, object> requestData)
    {
        string senderId = requestData["senderId"].ToString();
        string myId = UserDataManager.instance.UserID;

        DocumentReference myDocRef = db.Collection("kullanicilar").Document(myId);
        Task myTask = myDocRef.UpdateAsync("friends", FieldValue.ArrayUnion(senderId));

        DocumentReference senderDocRef = db.Collection("kullanicilar").Document(senderId);
        Task senderTask = senderDocRef.UpdateAsync("friends", FieldValue.ArrayUnion(myId));

        Task removeRequestTask = myDocRef.UpdateAsync("friendRequestsReceived", FieldValue.ArrayRemove(requestData));

        await Task.WhenAll(myTask, senderTask, removeRequestTask);

        Debug.Log($"Arkadaşlık isteği kabul edildi: {requestData["senderNickname"]}");
    }
    public async void OnClick_RejectRequest(Dictionary<string, object> requestData)
    {
        DocumentReference myDocRef = db.Collection("kullanicilar").Document(UserDataManager.instance.UserID);
        await myDocRef.UpdateAsync("friendRequestsReceived", FieldValue.ArrayRemove(requestData));

        Debug.Log($"Arkadaşlık isteği reddedildi: {requestData["senderNickname"]}");
    }
        
    private void ShowNotification(string message, Color color, float duration)
    {
        if (notificationText == null) return;

        if (notificationCoroutine != null)
        {
            StopCoroutine(notificationCoroutine);
        }

        notificationCoroutine = StartCoroutine(ShowNotificationRoutine(message, color, duration));
    }

    private IEnumerator ShowNotificationRoutine(string message, Color color, float duration)
    {
        notificationText.text = message;
        notificationText.color = color;
        notificationText.gameObject.SetActive(true);

        yield return new WaitForSeconds(duration);

        notificationText.gameObject.SetActive(false);
        notificationCoroutine = null; 
    }
    private void CleanupListeners()
    {
        Debug.Log("FriendsUIManager: Firestore dinleyicileri temizleniyor...");
        requestListener?.Stop();
        friendListener?.Stop();
        requestListener = null;
        friendListener = null;
    }

    private void HandleUserSignOut()
    {
        Debug.Log("Oturum kapatma sinyali alındı. FriendsUIManager temizleniyor.");
        StopFriendStatusUpdates();
        CleanupListeners();
    }

    public override void OnEnable()
    {
        base.OnEnable();
        UserDataManager.OnBeforeSignOut += HandleUserSignOut;
    } 
    public override void OnDisable()
    {
        UserDataManager.OnBeforeSignOut -= HandleUserSignOut;
    }
}
