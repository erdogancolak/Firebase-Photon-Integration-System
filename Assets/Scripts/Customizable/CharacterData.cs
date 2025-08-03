using UnityEngine;

[CreateAssetMenu(fileName = "New Character" , menuName = "Game/Character")]
public class CharacterData : ScriptableObject
{
    [Header("Character Info")]
    public string characterID;
    public string characterName;
    [TextArea] public string description;

    [Header("Shop & Game")]
    public int priceInCoins;
    public Sprite shopIcon; 
    public GameObject gamePrefab; 

    [Header("Status")]
    public bool isDefaultCharacter = false;
   
}
