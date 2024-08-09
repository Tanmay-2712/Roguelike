using System;
using Godot;

public partial class bullet_mk1 : Node2D
{
    [Export]
    public float Speed = 1000f; // Adjustable speed
    private Vector2 direction;

    public override void _Ready()
    {
        // Calculate the initial direction when the bullet is created
        direction = (TargetPosition - GlobalPosition).Normalized();
        
        // Set the initial rotation of the bullet
        Rotation = direction.Angle() - Mathf.Pi / 2; // Adjust the rotation offset if needed
    }

    public override void _Process(double delta)
    {
        // Move the bullet in the initial direction
        Position += direction * Speed * (float)delta;
        
        // Check if the bullet has gone far off-screen and remove it if necessary
        if (IsOffScreen())
        {
            QueueFree();
        }
    }
private void Area_Entered(Area2D area)
{
    if (area.IsInGroup("Enemies"))
    QueueFree();
}


    private bool IsOffScreen()
    {
        var viewportRect = GetViewportRect();
        var globalPosition = GlobalPosition;
        float margin = 100f; // Extra margin to ensure it's well off-screen

        return globalPosition.X < viewportRect.Position.X - margin ||
               globalPosition.X > viewportRect.End.X + margin ||
               globalPosition.Y < viewportRect.Position.Y - margin ||
               globalPosition.Y > viewportRect.End.Y + margin;
    }

    // Property to set the target position
    public Vector2 TargetPosition { get; set; }
}