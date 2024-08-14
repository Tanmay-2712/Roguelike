using Godot;
using System;
using System.Collections.Generic;

public partial class MobSpawner : Node
{
    [Export]
    public PackedScene[] MonsterScenes { get; set; }

    [Export]
    public float InitialSpawnInterval = 2.0f;

    [Export]
    public Label TimeLabel { get; set; }

    private List<Marker2D> _spawnPoints = new List<Marker2D>();
    private int _activeEnemyTypes = 1;
    private Timer _spawnTimer;
    private Timer _gameTimer;
    private RandomNumberGenerator _rng = new RandomNumberGenerator();
    private float _elapsedTime = 0f;

    public override void _Ready()
    {
        _spawnTimer = new Timer();
        AddChild(_spawnTimer);
        _spawnTimer.WaitTime = InitialSpawnInterval;
        _spawnTimer.Timeout += OnSpawnTimerTimeout;
        _spawnTimer.Start();

        _gameTimer = new Timer();
        AddChild(_gameTimer);
        _gameTimer.WaitTime = 1.0f; // Update every second
        _gameTimer.Timeout += OnGameTimerTimeout;
        _gameTimer.Start();

        // Get all Marker2D children
        foreach (Node child in GetChildren())
        {
            if (child is Marker2D marker)
            {
                _spawnPoints.Add(marker);
            }
        }
    }

    private void OnSpawnTimerTimeout()
    {
        SpawnEnemy();
    }

    private void OnGameTimerTimeout()
    {
        _elapsedTime += 1f;
        UpdateTimeLabel();
        UpdateDifficulty();
    }

    private void UpdateTimeLabel()
    {
        if (TimeLabel != null)
        {
            int minutes = (int)(_elapsedTime / 60);
            int seconds = (int)(_elapsedTime % 60);
            TimeLabel.Text = $"{minutes:00} : {seconds:00}";
        }
    }

    private void UpdateDifficulty()
    {
        // Increase difficulty every 30 seconds
        if (_elapsedTime % 30 == 0)
        {
            // Decrease spawn interval
            _spawnTimer.WaitTime = Math.Max(0.5f, _spawnTimer.WaitTime - 0.1f);

            // Increase active enemy types
            if (_activeEnemyTypes < MonsterScenes.Length)
            {
                _activeEnemyTypes++;
                GD.Print($"New enemy type unlocked! Active types: {_activeEnemyTypes}");
            }
        }
    }

    private void SpawnEnemy()
    {
        if (MonsterScenes == null || MonsterScenes.Length == 0 || _spawnPoints.Count == 0)
        {
            return;
        }

        // Choose a random monster scene from the active types
        int monsterIndex = _rng.RandiRange(0, _activeEnemyTypes - 1);
        PackedScene monsterScene = MonsterScenes[monsterIndex];

        // Choose a random spawn point
        int spawnPointIndex = _rng.RandiRange(0, _spawnPoints.Count - 1);
        Marker2D spawnPoint = _spawnPoints[spawnPointIndex];

        // Instance the monster
        Node2D monster = (Node2D)monsterScene.Instantiate();
        GetParent().GetParent().AddChild(monster);
        monster.GlobalPosition = spawnPoint.GlobalPosition;
        monster.AddToGroup("Enemies");

        // Restart the timer for the next spawn
        _spawnTimer.Start();
    }
}