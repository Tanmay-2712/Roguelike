using Godot;
using System;

public partial class monster1 : CharacterBody2D
{
    [Export]
    public float Speed = 100f;

    [Export]
    public float SteeringForce = 50f;

    private Node2D _player;

    public override void _Ready()
    {
        // Get the first node in the "player" group
        _player = GetTree().GetNodesInGroup("Hero")[0] as Node2D;
        if (_player == null)
        {
            GD.PrintErr("No player found in the 'Hero' group.");
            return;
        }
     
    }

    public override void _PhysicsProcess(double delta)
    {

        Vector2 direction = (_player.GlobalPosition - GlobalPosition).Normalized();
        Velocity = direction * Speed;

        // Apply steering movement
        Vector2 steering = direction * SteeringForce * (float)delta;
        Velocity += steering * (float)delta;

        // Move the monster towards the player
        MoveAndSlide();

    }

    public void Hit(Area2D area)
    {
        if (area.IsInGroup("Bullet"))
        {
            QueueFree();
        }
    }
}
