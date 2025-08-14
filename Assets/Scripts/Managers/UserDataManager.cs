using Firebase.Auth;
using Firebase.Extensions;
using Firebase.Firestore;
using Google;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Globalization;
using System.Linq;

public class UserDataManager : MonoBehaviour
{
    public static UserDataManager instance { get; private set; }

    public static event System.Action OnBeforeSignOut;
    public string UserNickname { get; private set; }
    public string UserEmail { get; private set; }
    public string UserID { get; private set; }
    public int EloPoints { get; private set; }
    public int Coins { get; private set; }

    public bool isDataLoaded { get; private set; }

    [Header("Characters")]
    public List<string> OwnedCharacterIDs { get; private set; }
    public string SelectedCharacterID { get; private set; }

    [Header("Social")]
    public List<string> FriendList;
    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            isDataLoaded = false;
            FriendList = new List<string>();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void setUserData(string nickname,string email,string userId,int eloPoints,int coins, List<string> ownedCharacters, string selectedCharacter,List<string> friends)
    {
        UserNickname = nickname;
        UserEmail = email;
        UserID = userId;
        EloPoints = eloPoints;
        Coins = coins;
        OwnedCharacterIDs = ownedCharacters;
        SelectedCharacterID = selectedCharacter;
        FriendList = friends;
        isDataLoaded = true;
    }

    public bool isFriend(string userID)
    {
        if (string.IsNullOrEmpty(userID) || FriendList == null) return false;
        return FriendList.Contains(userID);
    }

    public async Task AddFriend(string friendUserID)
    {
        if(isFriend(friendUserID) || friendUserID == this.UserID)
        {
            Debug.LogWarning("Bu kullanýcý zaten arkadaþ listenizde veya kendinizi ekleyemezsiniz.");
            return;
        }

        FriendList.Add(friendUserID);
        Debug.Log($"Yerel listeye eklendi: {friendUserID}");
        DocumentReference userDocRef = FirebaseFirestore.DefaultInstance.Collection("kullanicilar").Document(this.UserID);
        await userDocRef.UpdateAsync("friends", FieldValue.ArrayUnion(friendUserID));
        Debug.Log($"Firestore güncellendi: {friendUserID} arkadaþ olarak eklendi.");
    }

    public async Task RemoveFriend(string friendUserID)
    {
        if (!isFriend(friendUserID)) return;

        FriendList.Remove(friendUserID);

        DocumentReference userDocRef = FirebaseFirestore.DefaultInstance.Collection("kullanicilar").Document(this.UserID);
        await userDocRef.UpdateAsync("friends", FieldValue.ArrayRemove(friendUserID));
        Debug.Log($"Firestore güncellendi: {friendUserID} arkadaþlýktan çýkarýldý.");
    }

    public Task<FirebaseUser> InitializeAndFetchUserAsync()
    {
        var completionSource = new TaskCompletionSource<FirebaseUser>();

        
        if (FirebaseAuth.DefaultInstance.CurrentUser != null)
        {
            Debug.Log("Firebase kullanýcýsý zaten hazýr. Veriler çekiliyor...");
            FetchData(FirebaseAuth.DefaultInstance.CurrentUser, () => completionSource.SetResult(FirebaseAuth.DefaultInstance.CurrentUser));
            return completionSource.Task;
        }

        EventHandler authStateChangedHandler = null;
        authStateChangedHandler = (sender, e) =>
        {
            var currentUser = FirebaseAuth.DefaultInstance.CurrentUser;
            if (currentUser != null)
            {
                Debug.Log("Firebase Auth durumu deðiþti, kullanýcý bulundu. Veriler çekiliyor...");
                
                FirebaseAuth.DefaultInstance.StateChanged -= authStateChangedHandler;
                FetchData(currentUser, () => completionSource.SetResult(currentUser));
            }
            
        };

        FirebaseAuth.DefaultInstance.StateChanged += authStateChangedHandler;

        return completionSource.Task;
    }

    private void FetchData(FirebaseUser user, System.Action onComplete)
    {
        DocumentReference docRef = FirebaseFirestore.DefaultInstance.Collection("kullanicilar").Document(user.UserId);
        docRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && !task.IsFaulted && task.Result.Exists)
            {
                var userData = task.Result.ToDictionary();
                int elo = System.Convert.ToInt32(userData["eloPoints"]);
                int coins = userData.ContainsKey("coins") ? System.Convert.ToInt32(userData["coins"]) : 0;

                List<string> ownedCharacters = task.Result.GetValue<List<string>>("ownedCharacterIDs") ?? new List<string>();
                string selectedCharacter = task.Result.GetValue<string>("selectedCharacterID");
                List<string> friends = task.Result.GetValue<List<string>>("friends") ?? new List<string>();

                if (ownedCharacters.Count == 0 || string.IsNullOrEmpty(selectedCharacter))
                {
                    string defaultCharacterID = "civciv_01";
                    if (!ownedCharacters.Contains(defaultCharacterID)) { ownedCharacters.Add(defaultCharacterID); }
                    selectedCharacter = defaultCharacterID;
                }

                setUserData(
                    userData["nickname"].ToString(),
                    userData["email"].ToString(),
                    user.UserId,
                    elo,
                    coins,
                    ownedCharacters,
                    selectedCharacter,
                    friends);
            }
            else
            {
                Debug.LogError("Firestore'dan kullanýcý verisi çekilemedi veya kullanýcý mevcut deðil.");
            }
            onComplete?.Invoke();
        });
    }
    public async Task UpdateElo(int eloChange)
    {
        if (!isDataLoaded) return;

        EloPoints += eloChange; 
        Debug.Log($"Yerel Elo puaný güncellendi: {EloPoints}");

        DocumentReference userDocRef = FirebaseFirestore.DefaultInstance.Collection("kullanicilar").Document(UserID);
        var updates = new System.Collections.Generic.Dictionary<string, object>
        {
            { "eloPoints", EloPoints }
        };

        await userDocRef.UpdateAsync(updates);
        Debug.Log("Firestore'daki Elo puaný baþarýyla güncellendi.");
    }

    public async Task UpdateCoins(int amount)
    {
        if(!isDataLoaded) return;

        Coins += amount;
        Debug.Log($"Yerel coin miktarý güncellendi: {Coins}");

        DocumentReference userDocRef = FirebaseFirestore.DefaultInstance.Collection("kullanicilar").Document(UserID);
        var updates = new System.Collections.Generic.Dictionary<string, object>
        {
            { "coins", Coins }
        };

        await userDocRef.UpdateAsync(updates);
        Debug.Log("Firestore'daki coin miktarý baþarýyla güncellendi.");
    }

    public async Task SelectCharacter(string characterID)
    {
        if(!isDataLoaded || !OwnedCharacterIDs.Contains(characterID))
        {
            Debug.LogError("Bu karakter seçilemez veya sahip deðilsiniz!");
            return;
        }

        SelectedCharacterID = characterID;

        DocumentReference userDocRef = FirebaseFirestore.DefaultInstance.Collection("kullanicilar").Document(UserID);
        var updates = new Dictionary<string, object> { { "selectedCharacterID", characterID } };
        await userDocRef.UpdateAsync(updates);  

        Debug.Log($"Karakter seçildi ve kaydedildi: {characterID}");
    }
    public async Task<bool> UnlockCharacter(CharacterData characterData)
    {
        if (OwnedCharacterIDs.Contains(characterData.characterID))
        {
            Debug.LogError("Bu karaktere zaten sahipsin!");
            return false;
        }
        if (Coins < characterData.priceInCoins)
        {
            Debug.LogError("Yetersiz Bakiye! Gerekli: " + characterData.priceInCoins + ", Mevcut: " + Coins);
            return false;
        }
        if (!isDataLoaded)
        {
            Debug.LogError("Kullanýcý verisi yüklenemedi, satýn alma iþlemi iptal edildi.");
            return false;
        }
        
        await UpdateCoins(-characterData.priceInCoins);

        OwnedCharacterIDs.Add(characterData.characterID);

        DocumentReference userDocRef = FirebaseFirestore.DefaultInstance.Collection("kullanicilar").Document(UserID);
        var updates = new Dictionary<string, object> { { "ownedCharacterIDs", OwnedCharacterIDs } };
        await userDocRef.UpdateAsync(updates);

        Debug.Log($"Karakter satýn alýndý ve kaydedildi: {characterData.characterID}");
        return true;
    }
    public void SignOut()
    {
        StartCoroutine(SignOutRoutine());
    }
    private IEnumerator SignOutRoutine()
    {
        Debug.Log("Çýkýþ yapma iþlemi baþlatýldý...");

        OnBeforeSignOut?.Invoke();
        Debug.Log("OnBeforeSignOut event'i tetiklendi.");

        if (GoogleSignIn.DefaultInstance != null)
        {
            GoogleSignIn.DefaultInstance.SignOut();
            Debug.Log("Google oturumu kapatýldý.");
        }

        FirebaseAuth.DefaultInstance.SignOut();
        Debug.Log("Firebase oturumu kapatýldý.");

        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.Disconnect();
            Debug.Log("Photon baðlantýsý kesildi.");
        }

        PlayerPrefs.DeleteKey("IsLoggedIn");
        Debug.Log("PlayerPrefs 'IsLoggedIn' anahtarý silindi.");

        this.UserNickname = null;
        this.UserEmail = null;
        this.UserID = null;
        this.EloPoints = 0;
        this.Coins = 0;
        this.OwnedCharacterIDs = new List<string>();
        this.SelectedCharacterID = null;
        this.FriendList = new List<string>();
        this.isDataLoaded = false;
        Debug.Log("UserDataManager verileri sýfýrlandý.");

        GameStateManager.LastSceneName = "ClientScene";
        GameStateManager.LastRoomName = null;
        Debug.Log("GameStateManager durumu sýfýrlandý.");
 
        yield return new WaitForSeconds(0.5f);
        SceneManager.LoadScene("LoginScene");
    }
    
}
