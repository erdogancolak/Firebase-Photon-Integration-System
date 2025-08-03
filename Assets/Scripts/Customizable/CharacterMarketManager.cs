using UnityEngine;
using System.Collections.Generic;

public class CharacterMarketManager : MonoBehaviour
{
    [Header("Market Settings")]
    [SerializeField] private List<CharacterData> allCharacters;

    [Header("UI References")]
    [SerializeField] private CharacterListItemUI listItemPrefab;
    [SerializeField] private Transform listContainer;

    [SerializeField] private CharacterPreviewPanel previewPanel;
    void Start()
    {
        GenerateCharacterList();
    }
    private void GenerateCharacterList()
    {
        foreach(var character in allCharacters)
        {
            CharacterListItemUI itemInstance = Instantiate(listItemPrefab, listContainer);

            itemInstance.Setup(character, OnCharacterListItemClicked);
        }
    }
    private void OnCharacterListItemClicked(CharacterData clickedCharacter)
    {
        previewPanel.DisplayCharacter(clickedCharacter);
    }
}
