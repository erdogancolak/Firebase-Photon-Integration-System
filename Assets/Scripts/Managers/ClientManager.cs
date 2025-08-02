using UnityEngine;

public class ClientManager : MonoBehaviour
{
    private void Start()
    {
        GameStateManager.LastSceneName = "ClientScene";
        GameStateManager.LastRoomName = null;
    }

    public void SignOutButton()
    {
        GameStateManager.IsSigningOut = true;

        if (UserDataManager.instance != null)
        {
            UserDataManager.instance.SignOut();
        }
        else
        {
            Debug.LogError("UserDataManager bulunamadý! Çýkýþ yapýlamýyor.");
        }
    }
}
