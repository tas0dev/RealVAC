using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using Microsoft.Extensions.Logging;
using RealVAC.Visibility;

namespace RealVAC;

public sealed class RealVac : BasePlugin
{
    private readonly TraceService _traceService = new();
    private readonly Dictionary<(int Viewer, int Target), bool> _lastVisibility = new();

    public override string ModuleName => "RealVAC";
    public override string ModuleVersion => "0.1.0";

    public override void Load(bool hotReload)
    {
        RegisterListener<Listeners.CheckTransmit>(
            (infoList) =>
            {
                var players = Utilities.GetPlayers();

                foreach ((CCheckTransmitInfo info, CCSPlayerController? viewer) in infoList)
                {
                    if (viewer == null || !viewer.IsValid)
                        continue;

                    var transmitEntities = info.TransmitEntities;

                    foreach (var target in players)
                    {
                        if (!target.IsValid)
                            continue;

                        if (!target.Pawn.IsValid)
                            continue;

                        if (target.Slot == viewer.Slot)
                            continue;

                        if (target.Team == viewer.Team)
                            continue;

                        bool visible = _traceService.HasLineOfSight(
                            viewer,
                            target
                        );

                        var key = (
                            Viewer: viewer.Slot,
                            Target: target.Slot
                        );

                        if (
                            !_lastVisibility.TryGetValue(
                                key,
                                out bool lastVisible
                            )
                            || lastVisible != visible
                        )
                        {
                            Logger.LogInformation(
                                "{Viewer} -> {Target}: {State}",
                                viewer.PlayerName,
                                target.PlayerName,
                                visible ? "VISIBLE" : "HIDDEN"
                            );

                            _lastVisibility[key] = visible;
                        }

                        if (!visible)
                            transmitEntities.Remove(target.Pawn.Index);
                    }
                }
            }
        );
    }
}