namespace EzSeries.Models
{
    using Oasys.Common.Menu;
    using Oasys.Common.Menu.ItemComponents;
    using Oasys.SDK;
    using Oasys.SDK.Tools;

    public delegate void OnChampionInitialize();
    public delegate void OnChampionDispose();

    public abstract class Champion
    {
        // fields
        private bool _disposed;
        private bool _initialized;

        protected Tab Config { get; set; } = null!;
        private InfoDisplay ChampionDisplay { get; set; } = null!;
        
        /// <summary>
        ///     The champion initializer
        /// </summary>
        /// <param name="parentTab"></param>
        public void Initialize(Tab? parentTab)
        {
            Config = parentTab;
            InitializeChampion();
        }

        /// <summary>
        ///     Initializes the champion module.
        /// </summary>
        private void InitializeChampion()
        { 
            var v = "EzSeries.Champions." + UnitManager.MyChampion.ModelName;

            switch (_initialized)
            {
                case false when string.Equals(this.GetType().Namespace, v, StringComparison.CurrentCultureIgnoreCase):
                    ChampionDisplay = new InfoDisplay { Title = UnitManager.MyChampion.ModelName };
                    Config.AddItem(ChampionDisplay);

                    OnLoad();
                    OnChampionInitialize?.Invoke();

                    _disposed = false;
                    _initialized = true;
                    
                    Logger.Log("Initialized " + UnitManager.MyChampion.ModelName.ToLower() + " plugin!", LogSeverity.Warning);
                    break;
                case false:
                    DisposeChampion();
                    break;
            }
        }

        /// <summary>
        ///     Disposes the champion module.
        /// </summary>
        private void DisposeChampion()
        {
            switch (_disposed)
            {
                case false:
                    OnChampionDispose?.Invoke();

                    _disposed = true;
                    _initialized = false;
                    Logger.Log("Couldn't load " + this.GetType().Name.ToLower() + " module.", LogSeverity.Warning);
                    break;
            }
        }

        protected abstract void OnLoad();
        public abstract Task OnGameUpdate();
        public abstract Task OnMainInput();
        public abstract Task OnHarassInput();
        public abstract Task OnLaneClearInput();

        public event OnChampionInitialize? OnChampionInitialize;
        public event OnChampionDispose? OnChampionDispose;
    }
}