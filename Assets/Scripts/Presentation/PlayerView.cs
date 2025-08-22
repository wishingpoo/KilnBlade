using UnityEngine;

public class PlayerView : MonoBehaviour
{
    private PlayerSystem player;

    public void Initialize(PlayerSystem playerSystem)
    {
        player = playerSystem;
    }

    void LateUpdate()
    {
        if (player != null)
            transform.position = player.Position;
    }
}
