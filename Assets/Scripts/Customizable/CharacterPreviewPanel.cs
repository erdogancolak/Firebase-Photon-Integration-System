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

    private void Start()
    {
        closeButton.onClick.AddListener(ClosePanel);
        panelRoot.SetActive(false);
    }
    public void DisplayCharacter(CharacterData data)
    {
        panelRoot.SetActive(true);

        characterNameText.text = data.characterName;
        characterDescriptionText.text = data.description;

        if (_currentPreviewObject != null)
        {
            Destroy(_currentPreviewObject);
        }

        _currentPreviewObject = Instantiate(data.gamePrefab, Vector3.zero, Quaternion.identity);

        bool isOwned = false;

        if (UserDataManager.instance != null)
        {
            isOwned = UserDataManager.instance.OwnedCharacterIDs.Contains(data.characterID);
        }

        if (isOwned)
        {
            actionButtonText.text = "Use";
        }
        else
        {
            actionButtonText.text = "Buy";
        }

        actionButton.onClick.RemoveAllListeners();
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
