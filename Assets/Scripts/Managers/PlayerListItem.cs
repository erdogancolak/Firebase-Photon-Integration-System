using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Realtime;

public class PlayerListItem : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text nicknameText;
    [SerializeField] private TMP_Text eloText;
    [SerializeField] private Button actionButton;

    [Header("Status")]
    [SerializeField] private Image statusIcon;
    [SerializeField] private Sprite onlineSprite;
    [SerializeField] private Sprite offlineSprite;

    //[Header("Button Sprites")]
    //[SerializeField] private Sprite addFriendSprite;    
    //[SerializeField] private Sprite alreadyFriendsSprite; 

    private Player targetPlayer;
    public void Setup(Player player)
    {
        this.targetPlayer = player;
        UpdatePlayerInfo(player);

        if(statusIcon != null)
        {
            if(player.IsInactive)
            {
                statusIcon.sprite = offlineSprite;
            }
            else
            {
                statusIcon.sprite = onlineSprite;
            }
        }

        //if(player.IsLocal)
        //{
        //    actionButton.gameObject.SetActive(false);
        //    return;
        //}

        //bool isAlreadyFriend = UserDataManager.instance.isFriend(player.UserId);

        //if (isAlreadyFriend)
        //{
        //    actionButton.image.sprite = alreadyFriendsSprite;
        //    actionButton.interactable = false;
        //}
        //else
        //{
        //    actionButton.image.sprite = addFriendSprite;
        //    actionButton.interactable = true;
        //    actionButton.onClick.RemoveAllListeners(); 
        //    actionButton.onClick.AddListener(OnAddFriendButtonClicked);
        //}
    }
    private void UpdatePlayerInfo(Player player)
    {
        if(player.IsMasterClient)
        {
            nicknameText.text = player.NickName + " (Kurucu)";
            nicknameText.color = Color.yellow;
        }
        else
        {
            nicknameText.text = player.NickName;
            nicknameText.color = Color.white;
        }
        if (player.CustomProperties.TryGetValue("elo", out object eloValue))
        {
            eloText.text = eloValue.ToString();
        }
        else
        {
            eloText.text = "?";
        }
    }
    //private void OnAddFriendButtonClicked()
    //{
    //    Debug.Log(targetPlayer.NickName + " (" + targetPlayer.UserId + ") için arkadaþlýk isteði gönderilecek.");

    //    actionButton.interactable = false;
    //}
}
