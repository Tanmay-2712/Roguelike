using Godot;
using System;
using System.Data.Common;

public partial class Hero : CharacterBody2D
{
    [Export]
    public float Speed = 300.0f;
    
    [Export]
    public PackedScene bulletScene;

    [Export] 
    public float fireRate = 1.0f; // Fire rate in seconds

    [Export]
    public float MaxHealth = 100.0f;

    private float currentHealth;
    private AnimationPlayer animationPlayer;
    private JoyStick joyStick;
    private Sprite2D pointer;
    private Marker2D marker;
    private RayCast2D rayCast2D;
    private Sprite2D sprite;
    private Timer fireTimer;
    private Sprite2D heroSprite;
    private TextureProgressBar healthBar;

    public override void _Ready()
    {
        animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        joyStick = GetNode<JoyStick>("UI/JoyStick");
        pointer = GetNode<Sprite2D>("Pointer");
        marker = GetNode<Marker2D>("RayCast2D/Gun/Marker2D");
        rayCast2D = GetNode<RayCast2D>("RayCast2D");
        sprite = GetNode<Sprite2D>("RayCast2D/Gun");
        heroSprite = GetNode<Sprite2D>("Walk");
        healthBar = GetNode<TextureProgressBar>("HealthBar");

        // Setup fire timer
        fireTimer = new Timer();
        fireTimer.OneShot = false;
        fireTimer.WaitTime = fireRate; // Set fire rate
        fireTimer.Connect("timeout", new Callable(this, nameof(OnFireTimeout)));
        AddChild(fireTimer);

        fireTimer.Start(); // Start firing

        // Initialize health
        currentHealth = MaxHealth;
        UpdateHealthBar();

    }

    public override void _PhysicsProcess(double delta)
    {
        Vector2 input = joyStick.GetValue();

        Vector2 velocity;
        if (input != Vector2.Zero)
        {
            velocity = input.Normalized() * Speed;
            animationPlayer.SpeedScale = 1.0f;
        }
        else
        {
            velocity = Vector2.Zero;
            animationPlayer.SpeedScale = 0.5f;
        }

        Velocity = velocity;
        MoveAndSlide();

        if (input.X < 0)
        {
            heroSprite.FlipH = true;
        }
        else if (input.X > 0)
        {
            heroSprite.FlipH = false;
        }

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
            Vector2 direction = (closestEnemyPosition - GlobalPosition).Normalized();
            float angle = direction.Angle();
            rayCast2D.Rotation = angle - Mathf.Pi / 2;
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

    private void UpdateHealthBar()
    {
        healthBar.Value = (currentHealth / MaxHealth) * 100;
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        if (currentHealth < 0)
        {
            currentHealth = 0;
            // Handle player death here
            QueueFree(); // Or implement a proper death sequence
        }
        UpdateHealthBar();
    }

    private void OnAreaEntered(Area2D area)
    {
        if (area.IsInGroup("Enemies"))
        {
            // Assume the monster has a "Damage" property
            float monsterDamage = area.GetParent().Get("Damage").As<float>();
            TakeDamage(monsterDamage);
        }
    }
}