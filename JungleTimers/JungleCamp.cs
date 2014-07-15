using System;
using System.Collections.Generic;
using SharpDX;

namespace JungleTimers
{
    internal class JungleCamp
    {
        public TimeSpan SpawnTime { get; set; }
        public TimeSpan RespawnTimer { get; set; }
        public Vector3 Position { get; set; }
        public List<JungleMinion> Minions { get; set; }
        public JungleCampState State { get; set; }
        public float ClearTick { get; set; }
    }
}