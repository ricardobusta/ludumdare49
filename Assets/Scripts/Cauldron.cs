using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Cauldron : InteractiveObject
{
    public int level = 1;
    [SerializeField] private float unstableSpeed = 0.1f;
    [SerializeField] private float stability = 1;

    [SerializeField] private Image stabilityBar;
    [SerializeField] private TMP_Text levelLabel;
    
    private void Update()
    {
        stability -= level * unstableSpeed * Time.deltaTime;
        stabilityBar.fillAmount = stability;
    }

    private void SetLevel(int l)
    {
        level = l;
        levelLabel.text = $"Cauldron Stability - Level {level}";
    }

    public override void Interact(HandController controller)
    {
        throw new System.NotImplementedException();
    }
}
