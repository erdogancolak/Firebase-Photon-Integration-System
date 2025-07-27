using UnityEngine;

public class UserDataManager : MonoBehaviour
{
    public static UserDataManager instance { get; private set; }

    public string UserNickname { get; private set; }
    public string UserEmail { get; private set; }
    public string UserID { get; private set; }

    public bool isDataLoaded { get; private set; }
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

    public void setUserData(string nickname,string email,string userId)
    {
        UserNickname = nickname;
        UserEmail = email;
        UserID = userId;
        isDataLoaded = true;
    }
}
