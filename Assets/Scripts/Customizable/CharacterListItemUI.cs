using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterListItemUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image characterIcon;      
    [SerializeField] private TMP_Text characterNameText;   
    [SerializeField] private Button mainButton;

    [Header("Lock Status Sprites")]
    [SerializeField] private Image lockStatusIcon;     
    [SerializeField] private Sprite lockedSprite;        
    [SerializeField] private Sprite unlockedSprite;

    private CharacterData _characterData;
    public void Setup(CharacterData characterData, Action<CharacterData> onClickCallback)
    {
        _characterData = characterData;

        characterIcon.sprite = characterData.shopIcon;
        characterNameText.text = characterData.characterName;

        bool isOwned = false;

        if (UserDataManager.instance != null)
        {
            isOwned = UserDataManager.instance.OwnedCharacterIDs.Contains(_characterData.characterID);
        }

        if (isOwned)
        {
            lockStatusIcon.sprite = unlockedSprite;
        }
        else
        {
            lockStatusIcon.sprite = lockedSprite;
        }

        mainButton.onClick.RemoveAllListeners();
        mainButton.onClick.AddListener(() => onClickCallback?.Invoke(_characterData));
    }
}
