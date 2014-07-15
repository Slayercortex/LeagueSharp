using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using LeagueSharp;
using SharpDX;
using Color = System.Drawing.Color;

namespace LasthitMarker
{
    internal class LasthitMarker
    {
        private const int MaxMinionDistance = 1000;

        private readonly Action _onLoadAction;
        private List<Obj_AI_Minion> _killableMinions = new List<Obj_AI_Minion>();

        public LasthitMarker()
        {
            _onLoadAction = new CallOnce().A(OnLoad);
            Game.OnGameUpdate += OnGameUpdate;
            Drawing.OnDraw += OnDraw;
        }

        private void OnLoad()
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
                _killableMinions = (from minion in ObjectManager.Get<Obj_AI_Minion>()
                    where minion.IsValid && minion.IsVisible && minion.IsEnemy && !minion.IsDead
                    where Vector3.Distance(ObjectManager.Player.Position, minion.Position) <= MaxMinionDistance
                    where minion.Health <= DamageCalculator.Calculate(ObjectManager.Player, minion)
                    select minion).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private void OnDraw(EventArgs args)
        {
            try
            {
                Drawing.DrawCircle(ObjectManager.Player.Position,
                    ObjectManager.Player.AttackRange + ObjectManager.Player.BoundingRadius, Color.Gray);
                foreach (Obj_AI_Minion minion in _killableMinions)
                {
                    Drawing.DrawCircle(minion.Position, minion.BoundingRadius + 25, Color.Gray);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}