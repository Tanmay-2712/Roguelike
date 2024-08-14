using Godot;
using System;

public partial class monster2 : CharacterBody2D
{
    [Export]
    public float Speed = 100f;

    [Export]
    public float SteeringForce = 50f;

    [Export]
    public int MaxHealth = 100;
    [Export]
    public PackedScene ExpOrbScene;
       [Export]
    public float Damage = 10.0f;
    private int _currentHealth;
    private Node2D _player;
    private Marker2D _damagePosition;
    private Sprite2D sprite;

    public override void _Ready()
    {
        _currentHealth = MaxHealth;
        sprite = GetNode<Sprite2D>("Monster");
        _player = GetTree().GetNodesInGroup("Hero")[0] as Node2D;
        if (_player == null)
        {
            GD.PrintErr("No player found in the 'Hero' group.");
            return;
        }

        _damagePosition = GetNode<Marker2D>("DamageNumber/Marker2D");
        if (_damagePosition == null)
        {
            GD.PrintErr("Damage position Marker2D not found.");
        }
        
    }

    public override void _PhysicsProcess(double delta)
    {     
        Vector2 direction = (_player.GlobalPosition - GlobalPosition).Normalized();
        Velocity = direction * Speed;

        Vector2 steering = direction * SteeringForce * (float)delta;
        Velocity += steering * (float)delta;

        MoveAndSlide();

        // Flip the sprite to face the player
        if (direction.X < 0)
        {
            sprite.FlipH = true;
        }
        else
        {
            sprite.FlipH = false;
        }
        
    }

    public void Hit(Area2D area)
    {
        if (area.IsInGroup("Bullet"))
        {
            int damage = GetBulletDamage(area);
            TakeDamage(damage);
        }
    }

    private int GetBulletDamage(Area2D bullet)
    {
        if (bullet.HasMethod("GetDamage"))
        {
            return (int)bullet.Call("GetDamage");
        }
        return 10; // Default damage
    }

    private void TakeDamage(int damage)
    {
        _currentHealth -= damage;
        
        // Call the autoloaded DamageNumber scene to display the damage
        DisplayDamageNumber(damage);

        if (_currentHealth <= 0)
        {
            Die();
        }
    }

    private void DisplayDamageNumber(int damage)
    {
        if (_damagePosition != null)
        {
            Vector2 position = _damagePosition.GlobalPosition;
            bool isCritical = damage > 20; // Adjust this threshold as needed

            // Assuming DamageNumber is the name of your autoloaded script
            GetNode<Node>("/root/DamageNumbers").Call("display_number", damage, position, isCritical);
        }
        else
        {
            GD.PrintErr("Damage position not set. Unable to display damage number.");
        }
    }

    private void Die()
    {
        // Implement death logic here (e.g., play animation, spawn particles, etc.)
        CallDeferred(nameof(DeferredSpawnExpOrb));
    }

    private void DeferredSpawnExpOrb()
    {
        LevelManager levelManager = GetNode<LevelManager>("../Level_manager"); // Adjust the path as necessary
        if (levelManager != null)
        {
            levelManager.SpawnExpOrb(GlobalPosition);
            QueueFree();
        }
        else
        {
            GD.Print("LevelManager not found");
        }
    }
}