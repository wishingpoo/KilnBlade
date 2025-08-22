using UnityEngine;

public class PlayerSystem : ISystem
{
    public Vector2 Position { get; private set; }
    public float Speed = 4f;

    private PlayerControls controls;

    public PlayerSystem()
    {
        controls = new PlayerControls();
        controls.Enable();
    }

    public void Tick(float deltaTime)
    {
        Vector2 movement = controls.Player.Move.ReadValue<Vector2>();
        Position += movement.normalized * Speed * deltaTime;
    }
}
