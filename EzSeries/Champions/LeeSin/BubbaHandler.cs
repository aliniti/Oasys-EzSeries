// ReSharper disable CollectionNeverQueried.Local
// ReSharper disable FieldCanBeMadeReadOnly.Local
// ReSharper disable ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
#pragma warning disable CS0414

namespace EzSeries.Champions.LeeSin
{
    using Oasys.Common;
    using Oasys.Common.Enums.GameEnums;
    using Oasys.Common.Extensions;
    using Oasys.Common.GameObject.Clients;
    using Oasys.Common.GameObject.Clients.ExtendedInstances.Spells;
    using Oasys.Common.GameObject.ObjectClass;
    using Oasys.Common.Logic;
    using Oasys.SDK;
    using SharpDX;

    public class BubbaHandler
    {
        private enum Mode
        {
            Default,
            Insec
        }
        
        private static float _wRange = 700f;
        private static float _qRange = 1100f;
        private static float _eRange = 350f;
        private static float _rRange = 375f;

        private static AIHeroClient _cachedInsecTarget;
        private static BubbaAI _bubbaSegmentCache;
        private static Vector3 _allyCachePosition;
        
        private static Dictionary<string, float> _timeStamps = new();
        private static Dictionary<float, AIBaseClient> _objWithQ = new();
        private static Dictionary<float, Vector3> _recentDashOrWardSpots = new();
        private static Dictionary<int, Turret> _turretCache = new();
        private static Dictionary<int, AIHeroClient> _heroCache = new();

        private static AIHeroClient _me() => ObjectManagerExport.LocalPlayer;
        private static int _flashDelay() => Math.Max(150, 300 - GameEngine.GamePing / 2);

        private static Dictionary<string, SpellClass> _spells = new ();

        public BubbaHandler()
        {
            _timeStamps["CachedInsecTargetTime"] = 0;
            _timeStamps["CachedBubbaAi"] = 0;
            _timeStamps["SonicWaveDelete"] = 0;
            _timeStamps["AllyCacheTime"] = 0;
            _timeStamps["TurretCacheTime"] = 0;
            _timeStamps["R"] = 0;
            _timeStamps["Flash"] = 0;
            _timeStamps["Ward"] = 0;
            _timeStamps["Insec"] = 0;
            _timeStamps["Q1"] = 0;
            _timeStamps["Q2"] = 0;
            _timeStamps["W1"] = 0;
        }
        
        private static bool _insecQ(AIBaseClient unit, AIHeroClient inSecTarget = null, Mode m = Mode.Default)
        {
            var qClass = _me().GetSpellBook().GetSpellClass(SpellSlot.Q);
            
            if (_recentDashOrWardSpots.Values.Any(x => x.Distance(inSecTarget.Position) <= _rRange) && inSecTarget != null)
            {
                BubbaUtils.Log("Blocked Insec Q Because we just ward jumped");
                return false;
            }

            if (unit.IsValidTarget())
            {
                if (qClass.IsSpellReady && qClass.IsOne() && GameEngine.GameTick - _timeStamps["Q1"] >= 1000)
                {
                    if (_me().Distance(unit) <= _qRange)
                    {
                        var input = new LS.PredictionInput
                        {
                            Delay = 0.25f,
                            Radius = 60f,
                            Range = 1100f,
                            Speed = 1800,
                            Aoe = false,
                            Collision = true,
                            CollisionObjects = new [] { LS.CollisionableObjects.Minions, LS.CollisionableObjects.Heroes },
                            From = _me().Position,
                            RangeCheckFrom = _me().Position,
                            Type = LS.SkillshotType.SkillshotLine
                        };

                        var pred = LS.Prediction.GetPrediction(input);
                        if (pred.Hitchance >= LS.HitChance.High)
                        {
                               
                        }
                    }
                }
            }
            
            return false;
        }
    }
}