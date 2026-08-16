using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;

namespace RealVAC.Visibility;

public sealed class MovementPredictor
{
    private sealed class MovementState
    {
        public Vector Velocity { get; set; } = new();
        public Vector Acceleration { get; set; } = new();
        public double Time { get; set; }
    }

    private readonly Dictionary<int, MovementState> _states = new();

    public void Update(CCSPlayerController player, double now)
    {
        var pawn = player.PlayerPawn.Value;

        if (pawn == null || !pawn.IsValid)
            return;

        var velocity = pawn.AbsVelocity;

        if (!_states.TryGetValue(player.Slot, out var state))
        {
            _states[player.Slot] = new MovementState
            {
                Velocity = new Vector(
                    velocity.X,
                    velocity.Y,
                    velocity.Z
                ),
                Time = now
            };

            return;
        }

        double deltaTime = now - state.Time;

        if (deltaTime <= 0.0)
            return;

        state.Acceleration = new Vector(
            (float)((velocity.X - state.Velocity.X) / deltaTime),
            (float)((velocity.Y - state.Velocity.Y) / deltaTime),
            (float)((velocity.Z - state.Velocity.Z) / deltaTime)
        );

        state.Velocity = new Vector(
            velocity.X,
            velocity.Y,
            velocity.Z
        );

        state.Time = now;
    }

    public Vector PredictPosition(
        CCSPlayerController player,
        float predictionTime
    )
    {
        var pawn = player.PlayerPawn.Value;

        if (pawn?.AbsOrigin == null)
            return new Vector();

        var origin = pawn.AbsOrigin;

        if (!_states.TryGetValue(player.Slot, out var state))
        {
            return new Vector(
                origin.X,
                origin.Y,
                origin.Z
            );
        }

        float t2 = predictionTime * predictionTime;

        return new Vector(
            origin.X
            + state.Velocity.X * predictionTime
            + 0.5f * state.Acceleration.X * t2,

            origin.Y
            + state.Velocity.Y * predictionTime
            + 0.5f * state.Acceleration.Y * t2,

            origin.Z
            + state.Velocity.Z * predictionTime
            + 0.5f * state.Acceleration.Z * t2
        );
    }
}