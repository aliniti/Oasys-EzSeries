namespace EzSeries.Champions.LeeSin
{
    using Helpers;
    using Oasys.Common.Menu.ItemComponents;

    public class Bubba : Plugin
    {
        public readonly Switch UseFurryPassive = new ();
        public readonly Switch Debug = new ();
    
        public override string PluginName { get; set; } = "LeeSin";
        public override void OnLoadPlugin()
        {

        }
    }
}