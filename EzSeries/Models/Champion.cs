namespace EzSeries.Models
{
    using Oasys.SDK;
    using Oasys.SDK.Tools;
    using Oasys.Common.Menu;

    public delegate void OnChampionInitialize();
    public delegate void OnChampionDispose();

    public abstract class Champion
    {
        // fields
        private bool _disposed;
        private bool _initialized;
        
        protected Tab Config { get; set; }

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
                case false when string.Equals(GetType().Namespace, v, StringComparison.CurrentCultureIgnoreCase):

                    OnLoad();
                    OnChampionInitialize?.Invoke();

                    _disposed = false;
                    _initialized = true;
                    
                    Logger.Log("Initialized " + UnitManager.MyChampion.ModelName + " plugin!");
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
                    Logger.Log("Not Supported: " + UnitManager.MyChampion.ModelName, LogSeverity.Warning);
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