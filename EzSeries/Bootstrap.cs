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
        ///     The initialized champions
        /// </summary>
        private static readonly List<Champion> Champions = new();

        /// <summary>
        ///     The Oasys module entry point
        /// </summary>
        [OasysModuleEntryPoint]
        public static void Execute()
        {
            GameEvents.OnGameLoadComplete += OnGameLoadComplete;
            GameEvents.OnGameMatchComplete += () => { Champions.Clear(); return Task.CompletedTask; };
        }

        /// <summary>
        ///     Called when game load is complete
        /// </summary>
        private static Task OnGameLoadComplete()
        {
            var root = new Tab("EzSeries");

            foreach (var hero in SupportedChampions("Module").Select(i => (Champion) NewInstance(i)))
            {
                hero.OnChampionInitialize += () => Champions.Add(hero);
                hero.OnChampionDispose += () => Champions.Remove(hero);
                hero.Initialize(root);
            }

            MenuManager.AddTab(root);
            
            foreach (var hero in Champions)
            {
                CoreEvents.OnCoreMainTick += () => hero.OnGameUpdate();
                CoreEvents.OnCoreMainInputAsync += () => hero.OnMainInput();
                CoreEvents.OnCoreHarassInputAsync += () => hero.OnHarassInput();
                CoreEvents.OnCoreLaneclearInputAsync += () => hero.OnLaneClearInput();
            }
            
            return Task.CompletedTask;
        }
        
        /// <summary>
        ///     Gets the supported champions
        /// </summary>
        /// <param name="str">The module name.</param>
        /// <returns></returns>
        private static List<Type> SupportedChampions(string str)
        {
            try
            {
                var allowedTypes = new[] { typeof(Champion) };
                Logger.Log("Fetching modules....", LogSeverity.Warning);
            
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
                Logger.Log("Exception thrown at EzSeries.SupportedChampions()..", LogSeverity.Danger);
                return null;
            }
        }
        /// <summary>
        ///     The alternative object generator
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        private static object NewInstance(Type type)
        {
            try
            {
                var target = type.GetConstructor(Type.EmptyTypes);
                var dynamic = new DynamicMethod(string.Empty, type, new Type[0], target.DeclaringType);
                var il = dynamic.GetILGenerator();

                il.DeclareLocal(target.DeclaringType);
                il.Emit(OpCodes.Newobj, target);
                il.Emit(OpCodes.Stloc_0);
                il.Emit(OpCodes.Ldloc_0);
                il.Emit(OpCodes.Ret);

                var method = (Func<object>)dynamic.CreateDelegate(typeof(Func<object>));
                return method();
            }

            catch (Exception e)
            {
                Logger.Log("Exception thrown at EzSeries.NewInstance()..", LogSeverity.Danger);
                return null;
            }
        }
    }
}