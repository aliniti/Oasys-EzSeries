namespace EzSeries.Champions.LeeSin
{
    #region

    using Oasys.Common;
    using Oasys.Common.Enums.GameEnums;
    using Oasys.Common.GameObject.Clients;
    using Oasys.Common.GameObject.Clients.ExtendedInstances.Spells;
    using Oasys.Common.Logic;
    using Oasys.SDK;
    using Oasys.SDK.Rendering;
    using Oasys.SDK.Tools;
    using SharpDX;

    #endregion

    public static class BubbaUtils
    {
        #region Static Fields and Constants

        public static readonly Dictionary<string, float> TimeStamps = new();

        #endregion

        #region Public Methods and Operators


        public static bool HasFurryPassive()
        {
            var b = new Bubba();
            return b.UseFurryPassive.IsOn && 
                   ObjectManagerExport.LocalPlayer.BuffManager.HasActiveBuff("blindmonkpassive_cosmetic");
        }
        
        public static bool Recent(string str, int time = 500)
        {
            return GameEngine.GameTick - TimeStamps[str] <= time;
        }

        public static bool IsDashing()
        {
            return Recent("Q2")
                   || Recent("W1")
                   || ObjectManagerExport.LocalPlayer.BuffManager.HasActiveBuff("blindmonkqtwodash")
                   || ObjectManagerExport.LocalPlayer.BuffManager.HasActiveBuff("blindmonkonedash");
        }

        public static bool GetSpellSlotByName(string name, out SpellSlot slot)
        {
            foreach (SpellSlot v in Enum.GetValues(typeof(SpellSlot)))
            {
                var s = ObjectManagerExport.LocalPlayer.GetSpellBook().GetSpellClass(v);
                if (s.SpellData.SpellName.ToLower() == name.ToLower())
                {
                    slot = s.SpellSlot;
                    return  true;
                }
            }

            slot = SpellSlot.NullSpell;
            return false;
        }

        public static bool HasWard()
        {
            return GetSpellSlotByName("TrinketTotemLvl1", out _)
                   || GetSpellSlotByName("ItemGhostWard", out _)
                   || GetSpellSlotByName("JammerDevice", out _);
        }

        public static bool IsOne(this SpellClass spell)
        {
            return spell.SpellData.SpellName.ToLower().Contains("one");
        }

        public static bool HaveQ(this AIBaseClient? unit)
        {
            return unit != null && unit.BuffManager.HasActiveBuff("BlindMonkQOne");
        }

        public static bool HaveE(this AIBaseClient? unit)
        {
            return unit != null && unit.BuffManager.HasActiveBuff("BlindMonkEOne");
        }

        public static bool QAgain(this AIBaseClient target)
        {
            foreach (var b in target.BuffManager.ActiveBuffs)
                if (b.IsValid())
                    if (b.Name == "BlindMonkQOne")
                        if (b.EndTime - GameEngine.GameTime <= 0.5f)
                            return true;

            return false;
        }

        public static bool WAgain(this AIBaseClient target)
        {
            foreach (var b in target.BuffManager.ActiveBuffs)
                if (b.IsValid())
                    if (b.Name == "BlindMonkWOne")
                        if (b.EndTime - GameEngine.GameTime <= 0.5f)
                            return true;

            return false;
        }

        public static bool EAgain(this AIBaseClient target)
        {
            foreach (var b in target.BuffManager.ActiveBuffs)
                if (b.IsValid())
                    if (b.Name == "BlindMonkEOne")
                        if (b.EndTime - GameEngine.GameTime <= 0.5f)
                            return true;

            return false;
        }

        public static void DrawLinkFromPos(Vector3 posA, Vector3 posB, Color color, bool ignore = false)
        {
            var b = new Bubba();
            if (b.Debug.IsOn || ignore)
            {
                var pos1 = LeagueNativeRendererManager.WorldToScreenSpell(posA);
                var pos2 = LeagueNativeRendererManager.WorldToScreenSpell(posB);

                RenderFactory.DrawLine(pos1.X, pos1.Y, pos2.X, pos2.Y, 1, color);
                RenderFactory.DrawNativeCircle(posA, 35, color, 1f);
            }
        }

        public static void DrawLinkFromPos(Vector2 posA, Vector2 posB, Color color, bool ignore = false)
        {
            var b = new Bubba();
            if (b.Debug.IsOn || ignore)
            {
                DrawLinkFromPos(posA.To3D(), posB.To3D(), color, ignore);
            }
        }
        
        public static void DrawLinkFromMe(Vector3 posA, Color color, bool ignore = false)
        {
            var b = new Bubba();
            if (b.Debug.IsOn || ignore)
            {
                var pos1 = LeagueNativeRendererManager.WorldToScreenSpell(posA);
                var pos2 = LeagueNativeRendererManager.WorldToScreenSpell(ObjectManagerExport.LocalPlayer.Position);

                RenderFactory.DrawLine(pos1.X, pos1.Y, pos2.X, pos2.Y, 1, color);
                RenderFactory.DrawNativeCircle(posA, 35, color, 1f);
            }
        }

        public static void DrawLinkFromMe(Vector2 posA, Color color, bool ignore = false)
        {
            var b = new Bubba();
            if (b.Debug.IsOn || ignore)
            {
                DrawLinkFromMe(posA.To3D(), color, ignore);
            }
        }
        
        public static void WarningInGame(string str, Vector3 pos)
        {
            DrawLinkFromMe(pos, Color.OrangeRed);

            var pos1 = LeagueNativeRendererManager.WorldToScreenSpell(ObjectManagerExport.LocalPlayer.Position);
            RenderFactory.DrawText(str, 8, new Vector2(pos1.X + 59, pos.Y - 119), Color.OrangeRed );
        }

        public static void Log(string str)
        {
            var b = new Bubba();
            if (b.Debug.IsOn)
            {
                Logger.Log(str + " :: " + GameEngine.GameTick);                
            }
        }

        #endregion
    }
}