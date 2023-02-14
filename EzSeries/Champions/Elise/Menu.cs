namespace EzSeries.Champions.Elise
{
    using Oasys.Common.Menu;
    using Oasys.Common.Menu.ItemComponents;
    using Oasys.SDK;

    public class Menu
    {
        private static Tab _config;
        private static readonly string [] HitChanceList = { "Low", "Medium", "High", "Very High" };
        private static Group _main;

        public static void Initialize(Tab parentTab)
        {
            _config = parentTab;
            _main = new Group("Main");
            _main.AddItem(new ModeDisplay { Title = "Cocoon HitChance", ModeNames = HitChanceList.ToList(), SelectedModeName = "High" });
            _main.AddItem(new Switch("Auto change forms", true));
            _config.AddGroup(_main);
        }
        
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