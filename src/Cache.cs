using System;
using System.Threading;
using System.Collections.Generic;
using ZzukBot.Objects;
using ZzukBot.Game.Statics;
using System.Linq;

namespace Zoth.Bot.CombatRoutine
{
    public class Cache
    {
        private static ObjectManager ObjectManager => ObjectManager.Instance;

        private IList<WoWItem> _totems = Array.Empty<WoWItem>();
        private Timer _timer;

        public static readonly Cache Instance = new Cache();

        public IEnumerable<WoWItem> Totems => _totems;

        private Cache()
        {
            _timer = new Timer(obj =>
            {
                if (!ObjectManager.IsIngame) return;

                UpdateTotems();
                
            }, null, 0, 1000);
        }

        private static string[] TotemList = new[] {
            ItemNames.Totems.AirTotem,
            ItemNames.Totems.FireTotem,
            ItemNames.Totems.EarthTotem,
            ItemNames.Totems.WaterTotem
        };

        private void UpdateTotems()
        {
            _totems = ObjectManager.Items.Where(item => TotemList.Contains(item.Name)).ToList();
        }
    }
}
