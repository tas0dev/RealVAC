using CounterStrikeSharp.API.Core;

namespace RealVAC.Visibility;

public sealed class TraceService
{
    public bool HasLineOfSight(
        CCSPlayerController viewer,
        CCSPlayerController target
    )
    {
        var viewerPawn = viewer.PlayerPawn.Value;
        var targetPawn = target.PlayerPawn.Value;

        if (viewerPawn == null || targetPawn == null)
            return true;

        // TODO: traceray

        return true;
    }
}