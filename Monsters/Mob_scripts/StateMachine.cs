using Godot;
using System;

public partial class StateMachine : Node
{
    public enum State
    {
        Idle,
        Walk,
        Wander,
        Chase,
        Search,
        Attack,
        TakeDamage,
        Alert
    }

    public State CurrentState { get; private set; } = State.Idle;

    public delegate void StateChangedHandler(State newState);
    public event StateChangedHandler OnStateChanged;

    private Timer stateTimer;

    public override void _Ready()
    {
        stateTimer = new Timer();
        AddChild(stateTimer);
        stateTimer.Connect("timeout", new Callable(this, nameof(OnStateTimerTimeout)));
    }

    public void ChangeState(State newState)
    {
        CurrentState = newState;
        stateTimer.Stop();

        switch (newState)
        {
            case State.Idle:
                stateTimer.Start(GD.RandRange(2.0f, 5.0f));
                break;
            case State.TakeDamage:
                stateTimer.Start(0.5f);
                break;
            case State.Alert:
                stateTimer.Start(1.5f);
                break;
        }

        OnStateChanged?.Invoke(newState);
    }

    private void OnStateTimerTimeout()
    {
        switch (CurrentState)
        {
            case State.Idle:
                ChangeState(State.Wander);
                break;
            case State.TakeDamage:
                ChangeState(State.Chase);
                break;
            case State.Alert:
                ChangeState(State.Chase);
                break;
        }
    }
}