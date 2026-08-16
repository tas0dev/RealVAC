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

	private readonly MovementPredictor _movementPredictor;

	public float PredictionTime { get; set; } = 0.2f;

	public TraceService(MovementPredictor movementPredictor)
	{
		_movementPredictor = movementPredictor;
	}

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

		var currentViewerPosition = new Vector(
			viewerPawn.AbsOrigin.X,
			viewerPawn.AbsOrigin.Y,
			viewerPawn.AbsOrigin.Z
		);

		var currentTargetPosition = new Vector(
			targetPawn.AbsOrigin.X,
			targetPawn.AbsOrigin.Y,
			targetPawn.AbsOrigin.Z
		);

		if (HasLineOfSightAtPositions(
			currentViewerPosition,
			currentTargetPosition,
			viewerPawn,
			targetPawn
		))
		{
			return true;
		}

		var predictedViewerPosition =
			_movementPredictor.PredictPosition(
				viewer,
				PredictionTime
			);

		var predictedTargetPosition =
			_movementPredictor.PredictPosition(
				target,
				PredictionTime
			);

		return HasLineOfSightAtPositions(
			predictedViewerPosition,
			predictedTargetPosition,
			viewerPawn,
			targetPawn
		);
	}

	private bool HasLineOfSightAtPositions(
		Vector viewerPosition,
		Vector targetPosition,
		CCSPlayerPawn viewerPawn,
		CCSPlayerPawn targetPawn
	)
	{
		var start = new Vector(
			viewerPosition.X,
			viewerPosition.Y,
			viewerPosition.Z + viewerPawn.ViewOffset.Z
		);

		float yawRadians =
			targetPawn.EyeAngles.Y * (MathF.PI / 180.0f);

		float rightX = -MathF.Sin(yawRadians);
		float rightY = MathF.Cos(yawRadians);

		const float shoulderOffset = 16.0f;

		var points = new[]
		{
			new Vector(
				targetPosition.X,
				targetPosition.Y,
				targetPosition.Z + 64.0f
			),

			new Vector(
				targetPosition.X,
				targetPosition.Y,
				targetPosition.Z + 52.0f
			),

			new Vector(
				targetPosition.X,
				targetPosition.Y,
				targetPosition.Z + 40.0f
			),

			new Vector(
				targetPosition.X - rightX * shoulderOffset,
				targetPosition.Y - rightY * shoulderOffset,
				targetPosition.Z + 40.0f
			),

			new Vector(
				targetPosition.X + rightX * shoulderOffset,
				targetPosition.Y + rightY * shoulderOffset,
				targetPosition.Z + 40.0f
			),

			new Vector(
				targetPosition.X,
				targetPosition.Y,
				targetPosition.Z + 20.0f
			),

			new Vector(
				targetPosition.X,
				targetPosition.Y,
				targetPosition.Z + 8.0f
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