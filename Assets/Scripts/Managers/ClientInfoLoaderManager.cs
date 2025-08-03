using TMPro;
using UnityEngine;

public class ClientInfoLoaderManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text nicknameText;
    [SerializeField] private TMP_Text eloPointsText;
    [SerializeField] private TMP_Text coinPointsText;

    void Start()
    {
        if (UserDataManager.instance != null && UserDataManager.instance.isDataLoaded)
        {
            DisplayUserInfo();
        }
        else
        {
            Debug.LogError("ClientInfoLoaderManager: UserDataManager boþ veya veri yüklenmemiþ!");
            nicknameText.text = "Hata!";
            eloPointsText.text = "?";
            if(coinPointsText != null) coinPointsText.text = "?";
        }
    }

    void DisplayUserInfo()
    {
        nicknameText.text = UserDataManager.instance.UserNickname;
        eloPointsText.text = $"{UserDataManager.instance.EloPoints}";

        if(coinPointsText != null)
        {
            coinPointsText.text = $"{UserDataManager.instance.Coins}";
        }
        Debug.Log("Nickname baþarýyla yazdýrýldý: " + UserDataManager.instance.UserNickname);
    }
}