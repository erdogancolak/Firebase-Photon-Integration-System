using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterPreviewPanel : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject panelRoot; 
    [SerializeField] private TMP_Text characterNameText;
    [SerializeField] private TMP_Text characterDescriptionText;
    [SerializeField] private Button actionButton; 
    [SerializeField] private TMP_Text actionButtonText;
    [SerializeField] private Button closeButton;

    private GameObject _currentPreviewObject;
    private CharacterData currentCharacter;
    private CharacterMarketManager currentMarketManager;

    private void Awake()
    {
        closeButton.onClick.AddListener(ClosePanel);
        panelRoot.SetActive(false);
    }
    public void DisplayCharacter(CharacterData data,CharacterMarketManager marketManager)
    {
        currentCharacter = data;
        currentMarketManager = marketManager;
        panelRoot.SetActive(true);

        characterNameText.text = data.characterName;
        characterDescriptionText.text = data.description;

        if (_currentPreviewObject != null)
        {
            Destroy(_currentPreviewObject);
        }

        _currentPreviewObject = Instantiate(data.gamePrefab, Vector3.zero, Quaternion.identity);

        bool isOwned = UserDataManager.instance.OwnedCharacterIDs.Contains(data.characterID);
        bool isSelected = data.characterID == UserDataManager.instance.SelectedCharacterID;

        if (isSelected) 
        {
            actionButtonText.text = "✔";
            actionButton.interactable = false;
        }
        else if (isOwned)
        {
            actionButtonText.text = "Use";
            actionButton.interactable = true;
        }
        else
        {
            actionButtonText.text = "Buy";
            actionButton.interactable = (UserDataManager.instance != null && UserDataManager.instance.Coins >= data.priceInCoins);
        }

        actionButton.onClick.RemoveAllListeners();
        actionButton.onClick.AddListener(OnActionButtonClicked);
    }
    private async void OnActionButtonClicked()
    {
        actionButton.interactable = false;

        bool isOwned = UserDataManager.instance.OwnedCharacterIDs.Contains(currentCharacter.characterID);

        if(isOwned)
        {
            Debug.Log($"{currentCharacter.characterName} seçiliyor...");
            await UserDataManager.instance.SelectCharacter(currentCharacter.characterID);
        }
        else
        {
            Debug.Log($"{currentCharacter.characterName} satın alınıyor...");
            bool success = await UserDataManager.instance.UnlockCharacter(currentCharacter);
            if (success)
            {
                await UserDataManager.instance.SelectCharacter(currentCharacter.characterID);
            }
        }
        currentMarketManager.GenerateCharacterList();
        DisplayCharacter(currentCharacter, currentMarketManager);
    }
    public void ClosePanel()
    {
        panelRoot.SetActive(false);

        if(_currentPreviewObject != null)
        {
            Destroy( _currentPreviewObject);
        }
    }
}
