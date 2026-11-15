using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class CardOption
{
    public string cardName;
    [TextArea] public string description;
    public Sprite icon;
    public UnityEvent onCardChosen; // Drag your CardsScript methods here in the Inspector!
}