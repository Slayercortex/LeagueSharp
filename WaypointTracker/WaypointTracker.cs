using System;
using System.Drawing;
using System.Reflection;
using LeagueSharp;

namespace WaypointTracker
{
    internal class WaypointTracker
    {
        private readonly Action _onLoadAction;

        public WaypointTracker()
        {
            _onLoadAction = new CallOnce().A(OnLoad);
            Game.OnGameUpdate += OnGameUpdate;
            Drawing.OnDraw += OnDraw;
        }

        public void OnLoad()
        {
            Game.PrintChat(
                string.Format(
                    "<font color='#F7A100'>{0} v{1} loaded.</font>",
                    Assembly.GetExecutingAssembly().GetName().Name,
                    Assembly.GetExecutingAssembly().GetName().Version
                    )
                );
        }

        private void OnGameUpdate(EventArgs args)
        {
            try
            {
                _onLoadAction();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private void OnDraw(EventArgs args)
        {
            foreach (Obj_AI_Hero hero in ObjectManager.Get<Obj_AI_Hero>())
            {
                if (!hero.IsValid || hero.IsDead || hero.IsBot || hero.Path.Length == 0)
                    continue;

                float[] lastPathPos = Drawing.WorldToScreen(hero.Path[hero.Path.Length > 1 ? hero.Path.Length - 1 : 0]);
                float[] heroPos = Drawing.WorldToScreen(hero.Position);
                for (int index = 0; index < hero.Path.Length; index++)
                {
                    float[] curr = Drawing.WorldToScreen(hero.Path[index]);
                    if (index > 0)
                    {
                        float[] prev = Drawing.WorldToScreen(hero.Path[index - 1]);
                        Drawing.DrawLine(prev[0], prev[1], curr[0], curr[1], 3, Color.Green);
                    }
                    else
                    {
                        Drawing.DrawLine(heroPos[0], heroPos[1], curr[0], curr[1], 3, Color.Green);
                    }
                }
                Drawing.DrawText(lastPathPos[0], lastPathPos[1], Color.Orange, hero.BaseSkinName);
            }
        }
    }
}