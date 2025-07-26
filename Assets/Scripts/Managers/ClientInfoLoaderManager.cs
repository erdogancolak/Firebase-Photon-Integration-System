using TMPro;
using UnityEngine;

public class ClientInfoLoaderManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text nicknameText;

    void Start()
    {
        if(UserDataManager.instance != null)
        {
            nicknameText.text = UserDataManager.instance.UserNickname;
        }
        else
        {
            Debug.Log("User Data Manager Bulunamadý!");
        }
    }
}
