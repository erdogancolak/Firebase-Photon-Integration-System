using Firebase.Auth;
using Firebase.Firestore;
using Google;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TextCore.Text;

public class UserDataManager : MonoBehaviour
{
    public static UserDataManager instance { get; private set; }

    public string UserNickname { get; private set; }
    public string UserEmail { get; private set; }
    public string UserID { get; private set; }
    public int EloPoints { get; private set; }
    public int Coins { get; private set; }

    public bool isDataLoaded { get; private set; }

    [Header("Characters")]
    public List<string> OwnedCharacterIDs { get; private set; }
    public string SelectedCharacterID { get; private set; }
    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            isDataLoaded = false;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void setUserData(string nickname,string email,string userId,int eloPoints,int coins, List<string> ownedCharacters, string selectedCharacter)
    {
        UserNickname = nickname;
        UserEmail = email;
        UserID = userId;
        EloPoints = eloPoints;
        Coins = coins;
        OwnedCharacterIDs = ownedCharacters;
        SelectedCharacterID = selectedCharacter;
        isDataLoaded = true;
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
        this.isDataLoaded = false;
        Debug.Log("UserDataManager verileri sýfýrlandý.");

        GameStateManager.LastSceneName = "ClientScene";
        GameStateManager.LastRoomName = null;
        Debug.Log("GameStateManager durumu sýfýrlandý.");
 
        yield return new WaitForSeconds(0.5f);
        SceneManager.LoadScene("LoginScene");
    }
}
