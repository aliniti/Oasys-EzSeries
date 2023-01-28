namespace EzSeries.Helpers
{
    using Oasys.Common.GameObject.Clients;
    
    public static class Utils
    {
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
    }

    public static class Unit
    {
        public static bool IsValidTarget(this AIBaseClient? unit)
        {
            return unit is { IsAlive: true, IsVisible: true, IsTargetable: true } ;
        }
    }
}