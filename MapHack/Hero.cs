using SharpDX;

namespace MapHack
{
    internal class Hero
    {
        public string Name { get; set; }

        public bool Visible { get; set; }

        public bool Dead { get; set; }

        public Vector3 LastPosition { get; set; }
    }
}