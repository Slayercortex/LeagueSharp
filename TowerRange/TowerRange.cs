using System;
using System.Linq;
using System.Reflection;
using LeagueSharp;
using SharpDX;
using Color = System.Drawing.Color;

namespace TowerRange
{
    internal class TowerRange
    {
        private const float TurretRange = 910;
        private const double MaxDistance = 1450;

        private readonly Action _onLoadAction;

        public TowerRange()
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
                foreach (
                    Obj_Turret tower in
                        ObjectManager.Get<Obj_Turret>()
                            .Where(tower => tower.IsValid && !tower.IsDead)
                            .Where(
                                tower => Vector3.Distance(ObjectManager.Player.Position, tower.Position) <= MaxDistance)
                    )
                {
                    Drawing.DrawCircle(tower.Position, TurretRange, tower.IsAlly ? Color.Green : Color.Red);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}