using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Capabilities;
using CounterStrikeSharp.API.Modules.Utils;
using RayTraceAPI;
using TraceOptions = RayTraceAPI.TraceOptions;

namespace RealVAC.Visibility;

public sealed class TraceService
{
	private static PluginCapability<CRayTraceInterface> RayTraceInterface { get; } =
		new("raytrace:craytraceinterface");

	public bool HasLineOfSight(
		CCSPlayerController viewer,
		CCSPlayerController target
	)
	{
		var rayTrace = RayTraceInterface.Get();

		if (rayTrace == null)
			return true;

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

		float yawRadians = targetPawn.EyeAngles.Y * (MathF.PI / 180.0f);

		float rightX = -MathF.Sin(yawRadians);
		float rightY = MathF.Cos(yawRadians);

		const float shoulderOffset = 16.0f;

		var points = new[]
		{
			// Head
			new Vector(
				targetOrigin.X,
				targetOrigin.Y,
				targetOrigin.Z + 64.0f
			),

			// Upper body
			new Vector(
				targetOrigin.X,
				targetOrigin.Y,
				targetOrigin.Z + 52.0f
			),

			// Chest
			new Vector(
				targetOrigin.X,
				targetOrigin.Y,
				targetOrigin.Z + 40.0f
			),

			// Left shoulder
			new Vector(
				targetOrigin.X - rightX * shoulderOffset,
				targetOrigin.Y - rightY * shoulderOffset,
				targetOrigin.Z + 40.0f
			),

			// Right shoulder
			new Vector(
				targetOrigin.X + rightX * shoulderOffset,
				targetOrigin.Y + rightY * shoulderOffset,
				targetOrigin.Z + 40.0f
			),

			// Lower body
			new Vector(
				targetOrigin.X,
				targetOrigin.Y,
				targetOrigin.Z + 20.0f
			),

			// Feet
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

		TraceOptions options = new(
			InteractionLayers.MASK_WORLD_ONLY
		);

		bool hit = rayTrace.TraceEndShape(
			start,
			end,
			viewerPawn,
			options,
			out TraceResult result
		);

		if (!hit)
			return true;

		return result.Fraction >= 0.99f;
	}
}