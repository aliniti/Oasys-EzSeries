#pragma warning disable CS8618
namespace EzSeries.Champions.Elise
{
    using Oasys.Common.Menu;
    using Oasys.Common.Menu.ItemComponents;
    using Oasys.SDK;

    public abstract class Menu
    {
        private static Tab _config;
        private static readonly string [] HitChances = { "Low", "Medium", "High", "Very High" };
        private static Group _main, _clear, _draw, _misc;

        public static void Initialize(Tab parentTab)
        {
            _config = parentTab;
            _main = new Group("Main");
            _main.AddItem(new ModeDisplay { Title = "Cocoon HitChance", ModeNames = HitChances.ToList(), SelectedModeName = "High" });
            _main.AddItem(new Switch("Auto change forms", true));
            _config.AddGroup(_main);
            
            _draw = new Group("Drawings");
            _config.AddGroup(_draw);
            
            _misc = new Group("Miscellaneous");
            _misc.AddItem(new Switch("Debug Cooldowns", false));
            _config.AddGroup(_misc);
            
            _clear = new Group("Clearing: Advanced");
            _config.AddGroup(_clear);
        }

        public static bool Debug() => _main.GetItem<Switch>("Debug Cooldowns").IsOn;
        public static bool ChangeForms() => _main.GetItem<Switch>("Auto change forms").IsOn;

        public static Prediction.MenuSelected.HitChance GetHitChance()
        {
            return _main.GetItem<ModeDisplay>("Cocoon HitChance").SelectedModeName switch
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