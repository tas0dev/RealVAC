using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;

namespace RealVAC;

public sealed class RealVac : BasePlugin
{
    public override string ModuleName => "RealVAC";
    public override string ModuleVersion => "0.1.0";

    public override void Load(bool hotReload)
    {
        RegisterListener<Listeners.CheckTransmit>(OnCheckTransmit);
    }

    private void OnCheckTransmit(CCheckTransmitInfoList infoList)
    {
        var players = Utilities.GetPlayers();

        foreach ((CCheckTransmitInfo info, CCSPlayerController? viewer) in infoList)
        {
            if (viewer == null || !viewer.IsValid)
                continue;

            foreach (var target in players)
            {
                if (!target.IsValid)
                    continue;

                if (target.Slot == viewer.Slot)
                    continue;

                if (target.Team == viewer.Team)
                    continue;

                if (!target.Pawn.IsValid)
                    continue;

                var pawn = target.PlayerPawn.Value;

                if (pawn == null || !pawn.IsValid) continue;

                info.TransmitEntities.Remove(pawn);
            }
        }
    }
}