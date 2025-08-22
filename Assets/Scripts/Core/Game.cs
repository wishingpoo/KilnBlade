using System.Collections.Generic;

public class Game
{
    private readonly List<ISystem> systems = new();

    public void AddSystem(ISystem system) => systems.Add(system);

    public void Tick(float deltaTime)
    {
        foreach (var s in systems)
            s.Tick(deltaTime);
    }
}
