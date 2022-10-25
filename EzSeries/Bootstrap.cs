namespace EzSeries
{
    using System.Reflection;
    using System.Reflection.Emit;
    using Base;
    using Helpers;
    using Oasys.Common.Menu;
    using Oasys.SDK.Events;
    using Oasys.SDK.Tools;

    public class Bootstrap
    {
        private static Tab _rootTab;
        private static List<Plugin> _loadedPlugins = new();
        
        /// <summary>
        ///     The Oasys module entry point
        /// </summary>
        [Oasys.SDK.OasysModuleEntryPoint]
        public static void Execute()
        {
            GameEvents.OnGameLoadComplete += OnLoad;
        }
        
        /// <summary>
        ///     Games events [on game load complete].
        /// </summary>
        private static async Task OnLoad()
        {            
            _rootTab = new Tab("EzSeries: Settings");
            GetTypesByGroup("Champions").ForEach(x => { NewPlugin((Plugin) NewInstance(x), _rootTab); });
        }

        private static List<Type> GetTypesByGroup(string nspace)
        {
            try
            {
                Type[] allowedTypes = new[] { typeof(Plugin) };
                Logger.Log("Fetching plugins....", LogSeverity.Warning);
                
                return
                    Assembly.GetExecutingAssembly()
                        .GetTypes()
                        .Where(
                            t =>
                                t.IsClass && t.Namespace == "EzSeries." + nspace &&
                                allowedTypes.Any(x => x.IsAssignableFrom(t)))
                        .ToList();
            }

            catch (Exception e)
            {
                Console.WriteLine(e);
                Logger.Log("Exception thrown at EzSeries.GetTypesByGroup", LogSeverity.Danger);
                return null;
            }
        }
        
        private static void NewPlugin(Plugin plugin, Tab parent)
        {
            try
            {
                if (_loadedPlugins.Contains(plugin) == false)
                    _loadedPlugins.Add(plugin.Init(parent, _rootTab));
            }

            catch (Exception e)
            {
                Console.WriteLine(e);
                Logger.Log("Exception thrown at EzSeries.NewPlugin", LogSeverity.Danger);
                throw;
            }
        }
        
        private static object NewInstance(Type type)
        {
            try
            {
                ConstructorInfo? target = type.GetConstructor(Type.EmptyTypes);
                DynamicMethod dynamic = new DynamicMethod(string.Empty, type, new Type[0], target.DeclaringType);
                ILGenerator il = dynamic.GetILGenerator();

                il.DeclareLocal(target.DeclaringType);
                il.Emit(OpCodes.Newobj, target);
                il.Emit(OpCodes.Stloc_0);
                il.Emit(OpCodes.Ldloc_0);
                il.Emit(OpCodes.Ret);

                Func<object> method = (Func<object>)dynamic.CreateDelegate(typeof(Func<object>));
                return method();
            }

            catch (Exception e)
            {
                Console.WriteLine(e);
                Logger.Log("Exception thrown at EzSeries.NewInstance", LogSeverity.Danger);
                return null;
            }
        }
    }
}