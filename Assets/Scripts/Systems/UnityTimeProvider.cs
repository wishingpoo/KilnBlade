using System;
using UnityEngine;

namespace RPG.Systems
{
    public interface ITimeProvider
    {
        float DeltaTime { get; }
    }

    public class UnityTimeProvider : ITimeProvider
    {
        public float DeltaTime => Time.deltaTime;
    }
}
