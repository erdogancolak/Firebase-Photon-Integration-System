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

    [Header("Status Sprites")]
    [SerializeField] private Image statusIcon;     
    [SerializeField] private Sprite lockedSprite;        
    [SerializeField] private Sprite unlockedSprite;
    [SerializeField] private Sprite selectedSprite;

    [Header("Price Display")]
    [SerializeField] private TMP_Text priceText;
    [SerializeField] private Image priceIcon;

    private CharacterData _characterData;
    public void Setup(CharacterData characterData, Action<CharacterData> onClickCallback)
    {
        _characterData = characterData;

        characterIcon.sprite = characterData.shopIcon;
        characterNameText.text = characterData.characterName;

        bool isOwned = false;
        bool isSelected = false;

        if (UserDataManager.instance != null)
        {
            isOwned = UserDataManager.instance.OwnedCharacterIDs.Contains(_characterData.characterID);
            isSelected = _characterData.characterID == UserDataManager.instance.SelectedCharacterID;
        }

        if (isSelected)
        {
            priceText.gameObject.SetActive(false);
            priceIcon.gameObject.SetActive(false);
            statusIcon.sprite = selectedSprite;
        }
        else if (isOwned)
        {
            priceText.gameObject.SetActive(false);
            priceIcon.gameObject.SetActive(false);
            statusIcon.sprite = unlockedSprite;
        }
        else
        {
            statusIcon.sprite = lockedSprite;
            priceText.gameObject.SetActive(true);
            priceIcon.gameObject.SetActive(true);
            priceText.text = characterData.priceInCoins.ToString();
        }

        mainButton.onClick.RemoveAllListeners();
        mainButton.onClick.AddListener(() => onClickCallback?.Invoke(_characterData));
    }
}
