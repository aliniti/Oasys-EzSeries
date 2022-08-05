namespace EzSeries.Helpers
{
    #region

    using Oasys.Common.GameObject.Clients;
    using Oasys.Common.Menu;
    using Oasys.SDK;
    using Oasys.SDK.Menu;
    using Oasys.SDK.Tools;

    #endregion

    public abstract class Plugin
    {
        #region Fields

        private readonly Dictionary<string, bool> pDict = new ();
        private int pNow;
        
        #endregion

        #region Properties and Encapsulation

        public AIHeroClient Me => UnitManager.MyChampion;
        public abstract string PluginName { get; set; }

        public Tab MainTab { get; set; }
        public Tab PluginTab { get; set; }

        #endregion

        #region Public Methods and Operators

        public Plugin Init(Tab pluginTab, Tab rootTab)
        {
            try
            {
                if (Me.ModelName?.ToLower() == PluginName?.ToLower())
                {
                    MainTab = rootTab;
                    PluginTab = new Tab("EzSeries: " + PluginName);

                    OnLoadPlugin();
                    MenuManager.AddTab(PluginTab);
                    MenuManager.AddTab(rootTab);

                    // check if plugin has been initialized
                    if (!pDict.ContainsKey("Init"))
                    {
                        Logger.Log("Initialized " + PluginName.ToLower() + " plugin!", LogSeverity.Neutral);
                        pDict["Init"] = true;
                    }
                }
                else
                {
                    Logger.Log("Found " + PluginName.ToLower() + " plugin but did not load!", LogSeverity.Warning);
                }

                return this;
            }
            catch (Exception e)
            {
                Logger.Log(e, LogSeverity.Danger);
                throw;
            }
        }

        public abstract void OnLoadPlugin();

        #endregion
    }
}