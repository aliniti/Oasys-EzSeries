namespace EzSeries
{
    using System.Reflection;
    using System.Reflection.Emit;
    using Models;
    using Oasys.Common.EventsProvider;
    using Oasys.Common.Menu;
    using Oasys.SDK;
    using Oasys.SDK.Menu;
    using Oasys.SDK.Tools;

    public class Bootstrap
    {
        /// <summary>
        /// A static list of Champion objects that are currently active in the game.
        /// This list is used to manage and interact with the champions throughout the game.
        /// </summary>
        private static readonly List<Champion> Champions = new();
        
        /// <summary>
        /// This is the entry point for the Oasys module.
        /// It sets up the game events for when the game load is complete and when the game match is complete.
        /// </summary>
        [OasysModuleEntryPoint]
        public static void Execute()
        {
            // Set up the event for when the game load is complete
            // This event will call the OnGameLoadComplete method
            GameEvents.OnGameLoadComplete += OnGameLoadComplete;

            // Set up the event for when the game match is complete
            // This event will clear the list of champions and return a completed task
            GameEvents.OnGameMatchComplete += () => { Champions.Clear(); return Task.CompletedTask; };
        }

        /// <summary>
        /// This method is called when the game load is complete.
        /// It initializes the champions and adds them to the game menu.
        /// It also sets up the game events for each champion.
        /// </summary>
        private static Task OnGameLoadComplete()
        {
            // Create a new tab for the game menu
            var root = new Tab("EzSeries");

            // For each supported champion, create a new instance, initialize it and add it to the champions list
            foreach (var hero in SupportedChampions("Module").Select(i => (Champion) NewInstance(i)))
            {
                // Add the champion to the champions list when it is initialized
                hero.OnChampionInitialize += () => Champions.Add(hero);
                
                // Remove the champion from the champions list when it is disposed
                hero.OnChampionDispose += () => Champions.Remove(hero);
                
                // Initialize the champion
                hero.Initialize(root);
            }

            // Add the tab to the game menu
            MenuManager.AddTab(root);

            // For each champion, set up the game events
            foreach (var hero in Champions)
            {
                // Update the game state for the champion on each main tick of the game
                CoreEvents.OnCoreMainTick += () => hero.OnGameUpdate();
                
                // Handle the main input for the champion
                CoreEvents.OnCoreMainInputAsync += () => hero.OnMainInput();
                
                // Handle the harass input for the champion
                CoreEvents.OnCoreHarassInputAsync += () => hero.OnHarassInput();
                
                // Handle the lane clear input for the champion
                CoreEvents.OnCoreLaneclearInputAsync += () => hero.OnLaneClearInput();
            }

            // Return a completed task
            return Task.CompletedTask;
        }
        

        /// <summary>
        /// This method retrieves a list of types that are subclasses of the Champion class.
        /// It uses reflection to get all types in the current assembly that match the specified criteria.
        /// </summary>
        /// <param name="str">The name of the class to match.</param>
        /// <returns>A list of types that are subclasses of the Champion class and match the specified class name. If an exception occurs, it returns null.</returns>
        private static List<Type> SupportedChampions(string str)
        {
            try
            {
                // Define the types that are allowed to be returned
                var allowedTypes = new[] { typeof(Champion) };

                // Log the start of the module fetching process
                Logger.Log("Fetching modules....", LogSeverity.Warning);

                // Get all types in the current assembly
                // Filter the types to only include classes that match the specified name and are subclasses of the allowed types
                // Convert the result to a list and return it
                return
                    Assembly.GetExecutingAssembly()
                        .GetTypes()
                        .Where(
                            t =>
                                t.IsClass && t.Name == str
                                          && allowedTypes.Any(x => x.IsAssignableFrom(t)))
                        .ToList();
            }
            catch (Exception e)
            {
                // Log any exceptions that occur during the process
                Logger.Log("Exception thrown at EzSeries.SupportedChampions()..", LogSeverity.Danger);

                // Return null if an exception occurs
                return null;
            }
        }
        
        /// <summary>
        /// This method creates a new instance of the specified type using its parameterless constructor.
        /// It uses the DynamicMethod class to create a dynamic method that calls the constructor and returns the new instance.
        /// </summary>
        /// <param name="type">The type of the object to create.</param>
        /// <returns>A new instance of the specified type.</returns>
        private static object NewInstance(Type type)
        {
            // Get the parameterless constructor of the specified type
            var target = type.GetConstructor(Type.EmptyTypes);

            // Create a dynamic method that returns an instance of the specified type
            var dynamic = new DynamicMethod(string.Empty, type, new Type[0], target.DeclaringType);

            // Get an ILGenerator for the dynamic method
            var il = dynamic.GetILGenerator();

            // Declare a local variable of the specified type
            il.DeclareLocal(target.DeclaringType);

            // Emit IL instructions to create a new instance of the specified type and store it in the local variable
            il.Emit(OpCodes.Newobj, target);
            il.Emit(OpCodes.Stloc_0);

            // Emit IL instructions to load the new instance onto the evaluation stack and return it
            il.Emit(OpCodes.Ldloc_0);
            il.Emit(OpCodes.Ret);

            // Create a delegate for the dynamic method and invoke it to create a new instance of the specified type
            var method = (Func<object>)dynamic.CreateDelegate(typeof(Func<object>));
            return method();
        }
    }
}