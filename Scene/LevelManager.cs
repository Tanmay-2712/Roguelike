using Godot;
using System;
using System.Collections.Generic;

public partial class LevelManager : Node2D
{
    [Export] public PackedScene ExpOrbScene { get; set; }
    [Export] public float CollectionRadius { get; set; } = 100f;
    [Export] public float OrbSpeed { get; set; } = 200f;
    [Export] public float ExperiencePerOrb { get; set; } = 10f;

    private Hero _player;
    private ProgressBar _experienceBar;
    private Label _levelLabel;
    private Panel _upgradePanel;
    private bool _isSelectingUpgrade = false;
    private List<TouchScreenButton> _cardButtons = new List<TouchScreenButton>();
    private Area2D joystick;

    private float _currentExperience = 0f;
    private float _experienceToNextLevel = 100f;
    private int _currentLevel = 1;

    private List<ExpOrb> _activeOrbs = new List<ExpOrb>();

    public override void _Ready()
    {
        joystick = GetNode<Area2D>("../Hero/UI/JoyStick");
        _player = GetNode<Hero>("../Hero");
        _experienceBar = GetNode<ProgressBar>("CanvasLayer/MarginContainer/VBoxContainer/ExperienceBar");
        _levelLabel = GetNode<Label>("CanvasLayer/MarginContainer/VBoxContainer/ExperienceBar/LevelLabel");
        _upgradePanel = GetNode<Panel>("CanvasLayer/MarginContainer/VBoxContainer/Upgrades_panel");

        if (_player == null) GD.Print("LevelManager: Player not found");
        if (_experienceBar == null) GD.Print("LevelManager: ExperienceBar not found");
        if (_levelLabel == null) GD.Print("LevelManager: LevelLabel not found");
        if (_upgradePanel == null) GD.Print("LevelManager: UpgradePanel not found");

        for (int i = 1; i <= 5; i++)
        {
            var cardButton = GetNode<TouchScreenButton>($"CanvasLayer/MarginContainer/VBoxContainer/Upgrades_panel/HBoxContainer/Card_panel{i}/Card/TouchScreenButton");
            _cardButtons.Add(cardButton);
        }

        _upgradePanel.Visible = false;

        UpdateUI();
    }

    public override void _Process(double delta)
    {
        if (_player == null || _activeOrbs.Count == 0 || _isSelectingUpgrade) return;

        Vector2 playerPosition = _player.GlobalPosition;

        for (int i = _activeOrbs.Count - 1; i >= 0; i--)
        {
            ExpOrb orb = _activeOrbs[i];
            float distanceToPlayer = orb.GlobalPosition.DistanceTo(playerPosition);

            if (distanceToPlayer <= CollectionRadius)
            {
                Vector2 direction = (playerPosition - orb.GlobalPosition).Normalized();
                orb.GlobalPosition += direction * OrbSpeed * (float)delta;

                if (distanceToPlayer <= 10f)
                {
                    CollectOrb(orb);
                    i--;
                }
            }
        }
    }

    private void CollectOrb(ExpOrb orb)
    {
        _activeOrbs.Remove(orb);
        orb.QueueFree();

        _currentExperience += ExperiencePerOrb;

        if (_currentExperience >= _experienceToNextLevel)
        {
            LevelUp();
        }

        UpdateUI();
    }

    private void LevelUp()
    {
        _currentLevel++;
        _currentExperience -= _experienceToNextLevel;
        _experienceToNextLevel *= 1.2f;

        ShowUpgradePanel();
    }

    private void ShowUpgradePanel()
    {
        _isSelectingUpgrade = true;
        _upgradePanel.Visible = true;
        GetTree().Paused = true;
        joystick.Visible = false;

        PopulateUpgradeCards();
    }

    private void PopulateUpgradeCards()
    {
        // This is where you would set the content of each card
        // For now, we'll just set some placeholder text
        for (int i = 0; i < _cardButtons.Count; i++)
        {
            var card = _cardButtons[i].GetParent();
            card.GetNode<Label>("CardName/NameLbl").Text = $"Upgrade Option {i + 1}";
            card.GetNode<Label>("CardDescription").Text = $"This is upgrade description {i + 1}";
        }
    }

    // This method will be connected to the TouchScreenButton's pressed signal in the inspector
    public void OnCardPressed(int cardIndex)
    {
        if (!_isSelectingUpgrade) return;

        // Here you would apply the upgrade effect based on the selected card
        GD.Print($"Selected upgrade card {cardIndex}");

        _isSelectingUpgrade = false;
        _upgradePanel.Visible = false;
        GetTree().Paused = false;

        // Continue with the game
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (_experienceBar != null && _levelLabel != null)
        {
            _experienceBar.Value = (_currentExperience / _experienceToNextLevel) * 100;
            _levelLabel.Text = $"{_currentLevel}";
        }
        else
        {
            GD.Print("LevelManager: UI elements are null, can't update");
        }
    }

    public void SpawnExpOrb(Vector2 position)
    {
        if (ExpOrbScene == null)
        {
            GD.Print("LevelManager: ExpOrbScene is null, can't spawn orb");
            return;
        }

        CallDeferred(nameof(DeferredSpawnExpOrb), position);
    }

    private void DeferredSpawnExpOrb(Vector2 position)
    {
        ExpOrb newOrb = ExpOrbScene.Instantiate<ExpOrb>();
        newOrb.GlobalPosition = position;
        AddChild(newOrb);
        _activeOrbs.Add(newOrb);
    }
}