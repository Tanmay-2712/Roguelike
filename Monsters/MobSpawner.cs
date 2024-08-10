using Godot;
using System;
using System.Collections.Generic;

public partial class MobSpawner : Node
{
    [Export]
    public PackedScene[] MonsterScenes { get; set; }

    [Export]
    public float SpawnInterval = 2.0f;

    private List<Marker2D> _spawnPoints = new List<Marker2D>();
    private int _totalSpawned = 0;
    private int _activeEnemyTypes = 1;
    private Timer _spawnTimer;
    private RandomNumberGenerator _rng = new RandomNumberGenerator();

    public override void _Ready()
    {
        _spawnTimer = new Timer();
        AddChild(_spawnTimer);
        _spawnTimer.WaitTime = SpawnInterval;
        _spawnTimer.Timeout += OnSpawnTimerTimeout;
        _spawnTimer.Start();

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

        _totalSpawned++;

        // Check if we need to increase active enemy types
        if (_totalSpawned % 5 == 0 && _activeEnemyTypes < MonsterScenes.Length)
        {
            _activeEnemyTypes++;
            GD.Print($"New enemy type unlocked! Active types: {_activeEnemyTypes}");
        }

        // Restart the timer for the next spawn
        _spawnTimer.Start();
    }
}