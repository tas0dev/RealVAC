using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Capabilities;
using CounterStrikeSharp.API.Modules.Utils;
using RayTraceAPI;
using TraceOptions = RayTraceAPI.TraceOptions;

namespace RealVAC.Visibility;

public sealed class TraceService
{
    private static PluginCapability<CRayTraceInterface> RayTraceInterface { get; }
        = new("raytrace:craytraceinterface");

    public bool HasLineOfSight(
        CCSPlayerController viewer,
        CCSPlayerController target
    )
    {
        var rayTrace = RayTraceInterface.Get();

        if (rayTrace == null)
        {
            Console.WriteLine("[RealVAC] RayTraceInterface is NULL");
            return true;
        }

        var viewerPawn = viewer.PlayerPawn.Value;
        var targetPawn = target.PlayerPawn.Value;

        if (viewerPawn == null || targetPawn == null)
            return true;

        if (viewerPawn.AbsOrigin == null || targetPawn.AbsOrigin == null)
            return true;

        var start = new Vector(
            viewerPawn.AbsOrigin.X,
            viewerPawn.AbsOrigin.Y,
            viewerPawn.AbsOrigin.Z + viewerPawn.ViewOffset.Z
        );

        var targetOrigin = targetPawn.AbsOrigin;

        var points = new[]
        {
            new Vector(
                targetOrigin.X,
                targetOrigin.Y,
                targetOrigin.Z + 64.0f
            ),

            new Vector(
                targetOrigin.X,
                targetOrigin.Y,
                targetOrigin.Z + 40.0f
            ),

            new Vector(
                targetOrigin.X,
                targetOrigin.Y,
                targetOrigin.Z + 8.0f
            )
        };

        foreach (var point in points)
        {
            if (CanTrace(start, point, viewerPawn))
                return true;
        }

        return false;
    }

    private bool CanTrace(
        Vector start,
        Vector end,
        CCSPlayerPawn viewerPawn
    )
    {
        var rayTrace = RayTraceInterface.Get();

        if (rayTrace == null)
            return true;

        RayTraceAPI.TraceOptions options = new(
            RayTraceAPI.InteractionLayers.MASK_WORLD_ONLY,
            drawBeam: true
        );

        bool hit = rayTrace.TraceEndShape(
            start,
            end,
            viewerPawn,
            options,
            out RayTraceAPI.TraceResult result
        );

        Console.WriteLine(
            $"[RealVAC] hit={hit} fraction={result.Fraction:F3} " +
            $"start=({start.X:F1},{start.Y:F1},{start.Z:F1}) " +
            $"end=({end.X:F1},{end.Y:F1},{end.Z:F1})"
        );

        if (!hit)
            return true;

        return result.Fraction >= 0.99f;
    }
}