namespace EzSeries.Helpers
{
    using Oasys.Common;
    using Oasys.Common.Extensions;
    using Oasys.Common.GameObject.Clients;
    
    public static class Utils
    {
        public static bool CheckLineCollision(this AIHeroClient myHero, AIBaseClient unit)
        {
            var start = myHero.Position;
            var end = myHero.Position + (unit.Position - myHero.Position).Normalized() * 1100;
            var proj = unit.Position.ProjectOn(start, end);
            var nearit = unit.Position.Distance(proj.SegmentPoint) <= 55;

            if (proj.IsOnSegment && nearit)
            {
                var collision = proj.GetEnemyUnitsOnSegment(55, true, true);
                if (collision.Any(x => x.NetworkID != unit.NetworkID))
                {
                    return true;
                }
            }

            return false;
        }
        
        public static List<AIBaseClient> GetEnemyUnitsOnSegment(this ProjectionInfo proj, float radius, bool heroes, bool minions)
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
                    }
                }
            }
            
            return objList;
        }
    }
}