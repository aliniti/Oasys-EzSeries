namespace EzSeries.Helpers
{
    using Oasys.Common.GameObject.Clients;
    using Oasys.Common.Extensions;
    using SharpDX;

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

    public static class Nav
    {
        public static Vector3 ExPosition(AIBaseClient unit, float delay = 0)
        {
            var unitPosition = Vector3.Zero;

            var nav = unit.AIManager.GetNavPoints();
            if (nav.Count == 0)
            {
                return unit.Position;
            }

            if (nav.First().Distance(nav.Last()) <= 50)
            {
                return unit.Position;
            }
            
            for (var i = 0; i < nav.Count - 1; i++)
            {
                var previousPath = nav[i];
                var currentPath = nav[i + 1];
                var direction = (currentPath - previousPath).Normalized();
                var velocity = direction * unit.UnitComponentInfo.UnitBaseMoveSpeed;

                unitPosition = unit.Position + velocity * delay;
            }
            
            return unitPosition;
        }
        
        public static void GetPositions(AIBaseClient unit, out List<Vector2> posList)
        {
            var posChecked = 0;
            var maxPosChecked = 90;
            var posRadius = 40;
            var radiusIndex = 0;
            
            posList = new List<Vector2>();
            posList.Clear();
            
            while (posChecked < maxPosChecked)
            {
                radiusIndex++;
            
                var curRadius = radiusIndex * 0x2 * posRadius;
                var curCircleChecks = (int) Math.Ceiling(0x2 * Math.PI * curRadius / (0x2 * (double) posRadius));
            
                for (var i = 1; i < curCircleChecks; i++)
                {
                    var cRadians = 0x2 * Math.PI / (curCircleChecks - 0x1) * i;
                    var xPos = (float) Math.Floor(unit.Position.X + curRadius * Math.Cos(cRadians));
                    var yPos = (float) Math.Floor(unit.Position.Z + curRadius * Math.Sin(cRadians));
                    
                    var position = new Vector2(xPos, yPos);
                    posList.Add(position);
                    posChecked++;
                }
            }
        }
    }
}
