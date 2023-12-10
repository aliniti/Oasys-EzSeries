namespace EzSeries.Models
{
    using Oasys.SDK;
    using Oasys.SDK.Tools;
    using Oasys.Common.Menu;
    
    /// <summary>
    /// Delegate for an event that is triggered when the champion module is initialized.
    /// </summary>
    public delegate void OnChampionInitialize();

    /// <summary>
    /// Delegate for an event that is triggered when the champion module is disposed.
    /// </summary>
    public delegate void OnChampionDispose();

    public abstract class Champion
    {
        /// <summary>
        /// A boolean field that indicates whether the champion module has been disposed.
        /// </summary>
        private bool _disposed;

        /// <summary>
        /// A boolean field that indicates whether the champion module has been initialized.
        /// </summary>
        private bool _initialized;

        /// <summary>
        /// A protected property that represents the configuration tab for the champion module.
        /// </summary>
        protected Tab Config { get; set; }

        /// <summary>
        /// This method initializes the champion module with the provided parent tab.
        /// It sets the Config property to the provided parent tab and calls the InitializeChampion method.
        /// </summary>
        /// <param name="parentTab">The parent tab to use for the champion module.</param>
        public void Initialize(Tab? parentTab)
        {
            Config = parentTab;
            InitializeChampion();
        }

        /// <summary>
        /// This method initializes the champion module.
        /// It checks if the module is not already initialized, and if not, it triggers the OnLoad method and the OnChampionInitialize event.
        /// If the module is already initialized, it disposes the champion module.
        /// </summary>
        private void InitializeChampion()
        {
            // Define the namespace for the champion module
            var v = "EzSeries.Champions." + UnitManager.MyChampion.ModelName;

            // Check if the module is not already initialized
            switch (_initialized)
            {
                // If the module is not initialized and the namespace matches the defined namespace
                case false when string.Equals(GetType().Namespace, v, StringComparison.CurrentCultureIgnoreCase):

                    // Call the OnLoad method
                    OnLoad();
                    
                    // Trigger the OnChampionInitialize event
                    OnChampionInitialize?.Invoke();

                    // Set the _disposed field to false
                    _disposed = false;
                    
                    // Set the _initialized field to true
                    _initialized = true;

                    // Log the initialization of the champion module
                    Logger.Log("Initialized " + UnitManager.MyChampion.ModelName + " plugin!", LogSeverity.Warning);
                    break;
                
                // If the module is not initialized and the namespace does not match the defined namespace
                case false:
                    
                    // Dispose the champion module
                    DisposeChampion();
                    break;
            }
        }

        /// <summary>
        /// This method disposes the champion module.
        /// It checks if the module is not already disposed, and if not, it triggers the OnChampionDispose event.
        /// </summary>
        private void DisposeChampion()
        {
            switch (_disposed)
            {
                case false:
                    
                    // Trigger the OnChampionDispose event
                    OnChampionDispose?.Invoke();

                    // Set the _disposed field to true
                    _disposed = true;
                    
                    // Set the _initialized field to false
                    _initialized = false;
                    break;
            }
        }

        /// <summary>
        /// This abstract method is called when the champion module is loaded.
        /// It should be overridden in derived classes to provide specific functionality.
        /// </summary>
        protected abstract void OnLoad();

        /// <summary>
        /// This abstract method is called on each game update.
        /// It should be overridden in derived classes to provide specific functionality.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public abstract Task OnGameUpdate();

        /// <summary>
        /// This abstract method is called when the main input is received.
        /// It should be overridden in derived classes to provide specific functionality.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public abstract Task OnMainInput();

        /// <summary>
        /// This abstract method is called when the harass input is received.
        /// It should be overridden in derived classes to provide specific functionality.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public abstract Task OnHarassInput();

        /// <summary>
        /// This abstract method is called when the lane clear input is received.
        /// It should be overridden in derived classes to provide specific functionality.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public abstract Task OnLaneClearInput();

        /// <summary>
        /// This event is triggered when the champion module is initialized.
        /// </summary>
        public event OnChampionInitialize? OnChampionInitialize;

        /// <summary>
        /// This event is triggered when the champion module is disposed.
        /// </summary>
        public event OnChampionDispose? OnChampionDispose;
    }
}