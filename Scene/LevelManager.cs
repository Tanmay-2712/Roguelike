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

    private float _currentExperience = 0f;
    private float _experienceToNextLevel = 100f;
    private int _currentLevel = 1;

    private List<ExpOrb> _activeOrbs = new List<ExpOrb>();

    public override void _Ready()
    {
        _player = GetNode<Hero>("../Hero");
        _experienceBar = GetNode<ProgressBar>("CanvasLayer/ExperienceBar");
        _levelLabel = GetNode<Label>("CanvasLayer/ExperienceBar/LevelLabel");

        if (_player == null) GD.Print("LevelManager: Player not found");
        if (_experienceBar == null) GD.Print("LevelManager: ExperienceBar not found");
        if (_levelLabel == null) GD.Print("LevelManager: LevelLabel not found");

        UpdateUI();
    }

    public override void _Process(double delta)
    {
        if (_player == null || _activeOrbs.Count == 0) return;

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
    }

    private void UpdateUI()
    {
        if (_experienceBar != null && _levelLabel != null)
        {
            _experienceBar.Value = (_currentExperience / _experienceToNextLevel) * 100;
            _levelLabel.Text = $"Level: {_currentLevel}";
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