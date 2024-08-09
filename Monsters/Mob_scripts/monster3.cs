using Godot;
using System;

public partial class monster3 : CharacterBody2D
{
    [Export] public float MoveSpeed { get; set; } = 100.0f;
    [Export] public float ChaseSpeed { get; set; } = 150.0f;
    [Export] public int MaxHealth { get; set; } = 100;
    [Export] public int AttackDamage { get; set; } = 10;
    [Export] public float DetectionRange { get; set; } = 200.0f;
    [Export] public float AlertDuration { get; set; } = 1.0f;
    [Export] public float WanderDuration { get; set; } = 10.0f;
    [Export] public float SearchRadius { get; set; } = 50.0f;
    [Export] public float SearchDuration { get; set; } = 15.0f;
    [Export] public float MovementSmoothing { get; set; } = 0.1f;

    public int CurrentHealth { get; private set; }

    private StateMachine stateMachine;
    private Timer attackCooldownTimer;
    private Timer alertTimer;
    private Timer wanderTimer;
    private Timer searchTimer;
    private Vector2 wanderDirection;
    private Node2D target;
    private RayCast2D rayCast;
    private Label stateLabel;
    private AnimationPlayer animationPlayer;
    private Vector2 lastKnownPlayerPosition;
    private Vector2 searchTarget;
    private Vector2 velocity = Vector2.Zero;
    private FastNoiseLite noise;
    private float noiseOffset = 0f;

    public override void _Ready()
    {
        CurrentHealth = MaxHealth;

        stateMachine = GetNode<StateMachine>("StateMachine");
        stateMachine.OnStateChanged += OnStateChanged;

        attackCooldownTimer = new Timer();
        AddChild(attackCooldownTimer);
        attackCooldownTimer.Connect("timeout", new Callable(this, nameof(OnAttackCooldownTimeout)));

        alertTimer = new Timer();
        AddChild(alertTimer);
        alertTimer.Connect("timeout", new Callable(this, nameof(OnAlertTimeout)));

        wanderTimer = new Timer();
        AddChild(wanderTimer);
        wanderTimer.Connect("timeout", new Callable(this, nameof(OnWanderTimeout)));

        searchTimer = new Timer();
        AddChild(searchTimer);
        searchTimer.Connect("timeout", new Callable(this, nameof(OnSearchTimeout)));

        rayCast = GetNode<RayCast2D>("RayCast2D");
        stateLabel = GetNode<Label>("StateLabel");
        animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        stateLabel.Visible = true; 

        SetNewWanderDirection();

        target = GetTree().GetFirstNodeInGroup("Player") as Node2D;

        noise = new FastNoiseLite();
        noise.Seed = (int)GD.Randi();
        noise.FractalOctaves = 2;
        noise.Frequency = 0.5f;

        stateMachine.ChangeState(StateMachine.State.Idle);
    }

    public override void _PhysicsProcess(double delta)
    {
        UpdateStateLabel();
        UpdateRayCast();

        noiseOffset += (float)delta * 0.5f;

        switch (stateMachine.CurrentState)
        {
            case StateMachine.State.Idle:
                HandleIdleState();
                break;
            case StateMachine.State.Wander:
                Wander(delta);
                break;
            case StateMachine.State.Chase:
                ChasePlayer(delta);
                break;
            case StateMachine.State.Attack:
                Attack();
                break;
            case StateMachine.State.TakeDamage:
                // Handle damage animation or effects
                break;
            case StateMachine.State.Alert:
                // Do nothing, just wait for the alert timer
                break;
            case StateMachine.State.Search:
                Search(delta);
                break;
        }
    }

    private void UpdateRayCast()
    {
        if (target != null)
        {
            Vector2 directionToPlayer = (target.GlobalPosition - GlobalPosition).Normalized();
            rayCast.Rotation = directionToPlayer.Angle() - Mathf.Pi / 2;
            rayCast.ForceRaycastUpdate();
        }
    }

    private void HandleIdleState()
    {
        if (IsPlayerInRange(DetectionRange))
        {
            stateMachine.ChangeState(StateMachine.State.Alert);
        }
        else if (GD.Randf() < 0.01) // 1% chance per frame to start wandering
        {
            stateMachine.ChangeState(StateMachine.State.Wander);
        }
    }

    private void OnStateChanged(StateMachine.State newState)
    {
        GD.Print($"Monster state changed to: {newState}");
        UpdateStateLabel();

        switch (newState)
        {
            case StateMachine.State.Wander:
                wanderTimer.Start(WanderDuration);
                break;
            case StateMachine.State.Alert:
                alertTimer.Start(AlertDuration);
                if (target != null)
                {
                    lastKnownPlayerPosition = target.GlobalPosition;
                }
                break;
            case StateMachine.State.Search:
                SetNewSearchTarget();
                searchTimer.Start(SearchDuration);
                break;
        }
    }

    private void UpdateStateLabel()
    {
        if (stateLabel != null)
        {
            stateLabel.Text = stateMachine.CurrentState.ToString();
        }
    }

    private void SetNewWanderDirection()
    {
        wanderDirection = new Vector2(GD.Randf() * 2 - 1, GD.Randf() * 2 - 1).Normalized();
    }

    private void SetNewSearchTarget()
    {
        float angle = (float)GD.RandRange(0, Mathf.Tau);
        float radius = (float)GD.RandRange(0, SearchRadius);
        Vector2 offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
        searchTarget = lastKnownPlayerPosition + offset;
    }

    private void Wander(double delta)
    {
        Vector2 targetVelocity = wanderDirection * MoveSpeed;
        ApplyOrganicMovement(targetVelocity, delta);

        if (GD.Randf() < 0.01) // 1% chance per frame to change direction
        {
            SetNewWanderDirection();
        }
        if (GD.Randf() < 0.01) // 1% chance per frame to change direction
        {
            stateMachine.ChangeState(StateMachine.State.Idle);

        }

        if (IsPlayerInRange(DetectionRange))
        {
            stateMachine.ChangeState(StateMachine.State.Alert);
        }
    }

    private void ChasePlayer(double delta)
    {
        if (target != null)
        {
            Vector2 direction = (target.GlobalPosition - GlobalPosition).Normalized();
            Vector2 targetVelocity = direction * ChaseSpeed;
            ApplyOrganicMovement(targetVelocity, delta);
            animationPlayer.Play("walk");

            if (CanAttackPlayer())
            {
                stateMachine.ChangeState(StateMachine.State.Attack);
            }
            else if (!IsPlayerInRange(DetectionRange))
            {
                stateMachine.ChangeState(StateMachine.State.Alert);
            }
        }
        else
        {
            stateMachine.ChangeState(StateMachine.State.Wander);
        }
    }

    private void Search(double delta)
    {
        if (IsPlayerInRange(DetectionRange))
        {
            stateMachine.ChangeState(StateMachine.State.Alert);
        }
        else
        {
            Vector2 direction = (searchTarget - GlobalPosition).Normalized();
            Vector2 targetVelocity = direction * MoveSpeed;
            ApplyOrganicMovement(targetVelocity, delta);
            animationPlayer.Play("walk");

            if (GlobalPosition.DistanceTo(searchTarget) < 10f)
            {
                SetNewSearchTarget();
            }
        }
    }

    private void ApplyOrganicMovement(Vector2 targetVelocity, double delta)
    {
        // Apply noise to the target velocity
        float noiseValue = noise.GetNoise2D(GlobalPosition.X * 0.01f, GlobalPosition.Y * 0.01f + noiseOffset);
        Vector2 noiseVector = new Vector2(Mathf.Cos(noiseValue * Mathf.Tau), Mathf.Sin(noiseValue * Mathf.Tau));
        targetVelocity += noiseVector * MoveSpeed * 0.2f;

        // Smoothly interpolate between current velocity and target velocity
        velocity = velocity.Lerp(targetVelocity, (float)delta / MovementSmoothing);

        Velocity = velocity;
        MoveAndSlide();
    }

    private void Attack()
    {
        if (CanAttackPlayer() && attackCooldownTimer.IsStopped())
        {
            GD.Print($"Monster attacks player for {AttackDamage} damage!");
            // Here you would call a method on the player to deal damage
            // For example: (target as Player)?.TakeDamage(AttackDamage);
            attackCooldownTimer.Start(2.0f); // 2 second cooldown
        }
        else if (!CanAttackPlayer())
        {
            stateMachine.ChangeState(StateMachine.State.Chase);
        }
    }

    private bool CanAttackPlayer()
    {
        return rayCast.IsColliding() && rayCast.GetCollider() is Node2D collider && collider == target;
    }

    private void OnAttackCooldownTimeout()
    {
        attackCooldownTimer.Stop();
    }

    private void OnAlertTimeout()
    {
        if (IsPlayerInRange(DetectionRange))
        {
            stateMachine.ChangeState(StateMachine.State.Chase);
        }
        else
        {
            stateMachine.ChangeState(StateMachine.State.Search);
        }
    }

    private void OnWanderTimeout()
    {
        stateMachine.ChangeState(StateMachine.State.Idle);
    }

    private void OnSearchTimeout()
    {
        stateMachine.ChangeState(StateMachine.State.Idle);
    }

    public void TakeDamage(int damage)
    {
        CurrentHealth -= damage;
        if (CurrentHealth <= 0)
        {
            Die();
        }
        else
        {
            stateMachine.ChangeState(StateMachine.State.TakeDamage);
        }
    }

    private void Die()
    {
        // Handle monster death (e.g., play death animation, spawn loot, etc.)
        QueueFree();
    }

    private bool IsPlayerInRange(float range)
    {
        if (target != null)
        {
            return GlobalPosition.DistanceTo(target.GlobalPosition) <= range;
        }
        return false;
    }
}