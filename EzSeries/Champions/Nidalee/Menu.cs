#pragma warning disable CS8618
namespace EzSeries.Champions.Nidalee
{
    using Oasys.Common.GameObject.Clients;
    using Oasys.Common.Menu;
    using Oasys.Common.Menu.ItemComponents;
    using Oasys.SDK;
    
    public abstract class Menu
    {
        private static Tab _config;
        private static readonly string [] HitChanceList = { "Low", "Medium", "High", "Very High" };
        private static Group _main, _clear, _draw, _misc;
        
        public static void Initialize(Tab parentTab)
        {
            _config = parentTab;
            _main = new Group("Main");
            _main.AddItem(new ModeDisplay { Title = "Javelin HitChance", ModeNames = HitChanceList.ToList(), SelectedModeName = "High" });
            _main.AddItem(new Switch("Auto change forms", true));
            _main.AddItem(new InfoDisplay { Information = "Auto heal allies" });
            
            var allies = UnitManager.AllyChampions.OrderBy(a => a.ModelName);
            foreach (var a in allies)
            {
                _main.AddItem(new Switch("Auto heal " + a.ModelName, true));
                _main.AddItem(new Counter(a.ModelName + " heal pct", 75, 1, 100));
            }
            _config.AddGroup(_main);
            
            _draw = new Group("Drawings");
            // _draw.AddItem(new Switch("Draw Javelin Toss range", true));
            // _draw.AddItem(new Switch("Draw Pounce range", true));
            _config.AddGroup(_draw);
            
            _misc = new Group("Miscellaneous");
            // _misc.AddItem(new Switch("Auto JavelinToss Immobile/Dashing", true));
            // _misc.AddItem(new Switch("Auto Bushwhack Immobile", true));
            _misc.AddItem(new Switch("Debug Cooldowns", false));
            _config.AddGroup(_misc);
            
            _clear = new Group("Clearing: Advanced");
            _clear.AddItem(new InfoDisplay { Title = "Don't change these if you don't know what your doing." });
            _clear.AddItem(new Switch("Require min auto attacks", true));
            _clear.AddItem(new Counter("Min auto attacks", 2, 1, 5));
            _clear.AddItem(new Counter("Min auto attacks w/ heal", 4, 1, 5));
            _clear.AddItem(new Counter("Fast clear at/after lvl", 11, 1, 17));
            //_clear.AddItem(new Switch("Experimental wave clear", false) );
            _config.AddGroup(_clear);
        }

        public static bool Debug()
        {
            return _misc.GetItem<Switch>("Debug Cooldowns").IsOn;
        }

        public static bool ChangeForms()
        {
            return _main.GetItem<Switch>("Auto change forms").IsOn;
        }

        public static bool AutoHealAlly(AIBaseClient unit)
        {
            return _main.GetItem<Switch>("Auto heal " + unit.ModelName).IsOn;
        }
        
        public static int AutoHealAllyPct(AIBaseClient unit)
        {
            return _main.GetItem<Counter>(unit.ModelName + " heal pct").Value;
        }

        public static bool SafeToFastClear()
        {
            return UnitManager.MyChampion.Level >= _clear.GetItem<Counter>("Fast clear at/after lvl").Value;
        }

        public static bool RequireMinAutoAttacks()
        {
            return _clear.GetItem<Switch>("Require min auto attacks").IsOn;
        }

        public static int ClearMinAttacks()
        {
            return _clear.GetItem<Counter>("Min auto attacks").Value;
        }
        
        public static int ClearMinAttacksWithHeal()
        {
            return _clear.GetItem<Counter>("Min auto attacks w/ heal").Value;
        }

        public static Prediction.MenuSelected.HitChance GetHitChance()
        {
            return _main.GetItem<ModeDisplay>("Javelin HitChance").SelectedModeName switch
            {
                "VeryHigh" => Prediction.MenuSelected.HitChance.VeryHigh,
                "High" => Prediction.MenuSelected.HitChance.High,
                "Medium" => Prediction.MenuSelected.HitChance.Medium,
                "Low" => Prediction.MenuSelected.HitChance.Low,
                _ => Prediction.MenuSelected.HitChance.Unknown
            };
        }
    }
}