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
        if (UserDataManager.instance != null && UserDataManager.instance.isDataLoaded)
        {
            DisplayNickname();
        }
        else
        {
            Debug.LogError("ClientInfoLoaderManager: UserDataManager boþ veya veri yüklenmemiþ!");
            nicknameText.text = "Hata!";
        }
    }

    void DisplayNickname()
    {
        nicknameText.text = UserDataManager.instance.UserNickname;
        Debug.Log("Nickname baþarýyla yazdýrýldý: " + UserDataManager.instance.UserNickname);
    }
}