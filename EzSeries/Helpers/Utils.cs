namespace EzSeries.Helpers
{
    using Oasys.Common;
    using Oasys.Common.Extensions;
    using Oasys.Common.GameObject.Clients;
    using Oasys.Common.Logic;
    public static class Utils
    {
        public static bool IsLaneMinion(this AIBaseClient unit)
        {
            return unit.Name.StartsWith("Minion");
        }
        
        public static bool IsEpicMonster(this AIBaseClient unit)
        {
            var epic = new [] { "SRU_RiftHerald", "SRU_Baron", "SRU_Dragon" };
            return epic.Any(x => unit.Name.Contains(x));
        }
        
        public static bool IsLargeMonster(this AIBaseClient unit)
        {
            var large = new [] { "SRU_Blue", "SRU_Red", "Sru_Crab" };
            return large.Any(x => unit.Name.Contains(x));
        }
        
        public static bool IsSmallMonster(this AIBaseClient unit)
        {
            var small = new [] {  "SRU_Murkwolf", "SRU_Razorbeak", "SRU_Krug", "SRU_Gromp" };
            return small.Any(x => unit.Name.Contains(x));
        }

        public static bool CheckLineCollision(this AIHeroClient myHero, AIBaseClient unit)
        {
            var start = myHero.Position;
            var end = myHero.Position + (unit.Position - myHero.Position).Normalized() * 1100;
            var proj = unit.Position.ToVector2().ProjectOn(start.ToVector2(), end.ToVector2());
            var near = unit.Position.Distance(proj.SegmentPoint) <= 55;

            if (proj.IsOnSegment && near)
            {
                var collision = proj.GetEnemyUnitsOnSegment(55, true, true);
                if (collision.Any(x => x.NetworkID != unit.NetworkID))
                {
                    return true;
                }
            }

            return false;
        }
        
        public static List<AIBaseClient> GetEnemyUnitsOnSegment(this Geometry.ProjectionInfo proj, float radius, bool heroes, bool minions)
        {
            var objList = new List<AIBaseClient>();
            
            foreach (var u in ObjectManagerExport.HeroCollection)
            {
                var unit = u.Value;
                if (unit.IsValidTarget() && heroes)
                {
                    var nearit = unit.Position.Distance(proj.SegmentPoint) <= radius;
                    if (nearit)
                    {
                        objList.Add(unit);
                        break;
                    }
                }
            }
            
            foreach (var u in ObjectManagerExport.MinionCollection)
            {
                var minion = u.Value;
                if (minion.IsValidTarget() && minions)
                {
                    var nearit = minion.Position.Distance(proj.SegmentPoint) <= radius;
                    if (nearit)
                    {
                        objList.Add(minion);
                        break;
                    }
                }
            }
            
            return objList;
        }
    }
}