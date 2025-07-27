using TMPro;
using UnityEngine;
using Firebase.Auth;
using Firebase.Firestore;
using Firebase.Extensions;

public class ClientInfoLoaderManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text nicknameText;

    void Start()
    {
        nicknameText.text = "";

        if (UserDataManager.instance == null)
        {
            Debug.LogError("User Data Manager Bulunamadý! Sistemde kritik bir hata var.");
            return;
        }

        if (UserDataManager.instance.isDataLoaded)
        {
            DisplayNickname();
        }
        else
        {
            FetchDataFromFirestore();
        }
    }

    void FetchDataFromFirestore()
    {
        FirebaseUser user = FirebaseAuth.DefaultInstance.CurrentUser;
        if (user == null)
        {
            Debug.LogError("Kullanýcý giriþi bulunamadý! Login sahnesine yönlendiriliyor.");
            return;
        }

        Debug.Log("Veri UserDataManager'da bulunamadý. Firestore'dan çekiliyor...");
        DocumentReference docRef = FirebaseFirestore.DefaultInstance.Collection("kullanicilar").Document(user.UserId);
        docRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && !task.IsFaulted && task.Result.Exists)
            {
                DocumentSnapshot snapshot = task.Result;
                var userData = snapshot.ToDictionary();

                UserDataManager.instance.setUserData(
                    userData["nickname"].ToString(),
                    userData["email"].ToString(),
                    user.UserId
                );

                DisplayNickname();
            }
            else
            {
                Debug.LogError("Kritik Hata: Kullanýcý giriþi yapýlmýþ ama Firestore'da veri bulunamadý.");
            }
        });
    }

    void DisplayNickname()
    {
        nicknameText.text = UserDataManager.instance.UserNickname;
        Debug.Log("Nickname baþarýyla yazdýrýldý: " + UserDataManager.instance.UserNickname);
    }
}