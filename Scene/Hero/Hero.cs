using Godot;
using System;

public partial class Hero : CharacterBody2D
{
    [Export]
    public float Speed = 300.0f;
    
    [Export]
    public PackedScene bulletScene;

    [Export] 
    public float fireRate = 1.0f; // Fire rate in seconds


    private AnimationPlayer animationPlayer;
    private JoyStick joyStick;
    private Sprite2D pointer;
    private Marker2D marker;
    private RayCast2D rayCast2D;
    private Sprite2D sprite;
    private Timer fireTimer;

    public override void _Ready()
    {
        //animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        joyStick = GetNode<JoyStick>("UI/JoyStick");
        pointer = GetNode<Sprite2D>("Pointer");
        marker = GetNode<Marker2D>("Marker2D");
        rayCast2D = GetNode<RayCast2D>("RayCast2D");
        //sprite = GetNode<Sprite2D>("Sprite");

            // Setup fire timer
        fireTimer = new Timer();
        fireTimer.OneShot = false;
        fireTimer.WaitTime = fireRate; // Set fire rate
        fireTimer.Connect("timeout", new Callable(this, nameof(OnFireTimeout)));
        AddChild(fireTimer);

        fireTimer.Start(); // Start firing
    }

    public override void _PhysicsProcess(double delta)
    {
        Vector2 input = joyStick.GetValue();

        Vector2 velocity;
        if (input != Vector2.Zero)
        {
            // Normalize the input to get a unit vector for direction
            velocity = input.Normalized() * Speed;
            //animationPlayer.Play("walk");
        }
        else
        {
            velocity = Vector2.Zero;
            //animationPlayer.Play("idle");
        }

        Velocity = velocity;
        MoveAndSlide();
        if (input != Vector2.Zero)
        {
            pointer.Visible = true;
            pointer.Rotation = input.Angle() + Mathf.Pi / 2;
        }
        else
        {
            pointer.Visible = false;

        }
        
        Vector2 closestEnemyPosition = FindClosestEnemy();
        if (closestEnemyPosition != Vector2.Zero)
        {
            // Rotate turret towards the closest enemy
            Vector2 direction = (closestEnemyPosition - GlobalPosition).Normalized();
            float angle = direction.Angle();
            rayCast2D.Rotation = angle - Mathf.Pi / 2; // Adjust the rotation offset as needed
            //sprite.Rotation = angle - Mathf.Pi / 2; // Adjust the rotation offset as needed
        }
    }

    private Vector2 FindClosestEnemy()
    {
        Vector2 closestPosition = Vector2.Zero;
        float shortestDistance = float.MaxValue;

        foreach (var enemy in GetTree().GetNodesInGroup("Enemies"))
        {
            if (enemy is Node2D enemyNode)
            {
                float distance = GlobalPosition.DistanceTo(enemyNode.GlobalPosition);
                if (distance < shortestDistance)
                {
                    shortestDistance = distance;
                    closestPosition = enemyNode.GlobalPosition;
                }
            }
        }

        return closestPosition;
    }

    private void OnFireTimeout()
    {
        if (rayCast2D.IsColliding())
        {
            var collider = rayCast2D.GetCollider();
            if (collider is Node enemy && enemy.IsInGroup("Enemies"))
            {
                // Fire bullet
                var closestEnemyPosition = FindClosestEnemy();
                if (closestEnemyPosition != Vector2.Zero)
                {
                    InstantiateBulletAtMarker(closestEnemyPosition);
                }
            }
        }
    }

    private void InstantiateBulletAtMarker(Vector2 targetPosition)
    {
        bullet_mk1 bulletInstance = (bullet_mk1)bulletScene.Instantiate();

        bulletInstance.Position = marker.GlobalPosition;
        bulletInstance.TargetPosition = targetPosition;


        GetParent().AddChild(bulletInstance);
    }
}