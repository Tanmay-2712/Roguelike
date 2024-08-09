using Godot;
using System;

public partial class JoyStick : Area2D
{
    private Sprite2D outerCircle;
    private Sprite2D innerCircle;
    private Vector2 touchStartPosition;
    private bool isDragging = false;
    private float maxRadius;
    private Vector2 Joystickposition;

    public override void _Ready()
    {
        outerCircle = GetNode<Sprite2D>("Outer_circle");
        innerCircle = GetNode<Sprite2D>("Outer_circle/Inside_circle");
        maxRadius = outerCircle.Texture.GetWidth() * outerCircle.Scale.X / 2;
        Joystickposition = GlobalPosition;

    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventScreenTouch touchEvent)
        {
            if (touchEvent.Pressed)
            {
                GlobalPosition = touchEvent.Position;
                Vector2 localTouchPosition = touchEvent.Position - GlobalPosition;
                if (IsPointInside(localTouchPosition))
                {
                    isDragging = true;
                    touchStartPosition = localTouchPosition - innerCircle.Position;
                }
            }
            else
            {
                isDragging = false;
                innerCircle.Position = Vector2.Zero;
                GlobalPosition = Joystickposition;
            }

        }
        else if (@event is InputEventScreenDrag dragEvent && isDragging)
        {
            Vector2 dragPosition = dragEvent.Position - GlobalPosition;
            innerCircle.Position = (dragPosition - touchStartPosition).LimitLength(maxRadius);
        }
    }

    private bool IsPointInside(Vector2 point)
    {
        return point.Length() <= maxRadius;
    }

    public Vector2 GetValue()
    {
        return innerCircle.Position / maxRadius;
    }
}