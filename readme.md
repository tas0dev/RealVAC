# RealVAC

A server-side add-on that disables wallhacks, replacing the lazy Valve.

Instead of trying to detect wallhacks on the client, RealVAC prevents hidden enemy player entities from being transmitted to clients when they should not be visible.

### Requirements

RealVAC currently requires:

- Counter-Strike 2 Dedicated Server
- Metamod
- CounterStrikeSharp
- Ray-Trace
- RayTraceImpl
- RayTraceApi

Recommended versions:

- CounterStrikeSharp: 1.0.371
- .NET: 10.0 

### Installation

Install Metamod and CounterStrikeSharp on your CS2 Dedicated Server first.

Then install the native Ray-Trace plugin.

The server should contain:

```
game/csgo/addons/
├── metamod/
├── RayTrace/
└── counterstrikesharp/
```

The Ray-Trace Metamod plugin must be loaded correctly.

Check it with:

`meta list`

You should see both CounterStrikeSharp and RayTrace.

Next, install the managed RayTrace components:
```
game/csgo/addons/counterstrikesharp/
├── plugins/
│   └── RayTraceImpl/
│       └── RayTraceImpl.dll
│
└── shared/
    └── RayTraceApi/
        └── RayTraceApi.dll
```

Finally, place RealVAC in:

`game/csgo/addons/counterstrikesharp/plugins/RealVAC/`

For example:

```
game/csgo/addons/counterstrikesharp/plugins/RealVAC/
├── RealVAC.dll
├── RealVAC.deps.json
└── RealVAC.pdb
```

Restart the server after installation.

Verifying the Installation

Run:

`meta list`

RayTrace and CounterStrikeSharp should both be loaded.

Then run:

`css_plugins list`

You should see:

RayTraceImpl
RealVAC

as loaded CounterStrikeSharp plugins.

### How It Works

For every player, RealVAC evaluates whether each enemy player can currently be seen.

If at least one trace reaches the target without being blocked by world geometry, the enemy is considered visible.

If every trace is blocked, RealVAC removes the enemy pawn from that client's transmission set.

RealVAC uses CounterStrikeSharp's CheckTransmit listener to perform the final transmission filtering.

Visibility Prediction

RealVAC also tracks player movement over time.

Velocity and estimated acceleration are used to predict future positions:

`p(t) = p + vt + 1/2 at^2`

This allows RealVAC to transmit an enemy slightly before they become directly visible.

The purpose of this prediction is to avoid visible pop-in when a player quickly peeks around a corner.

The prediction system is intended to classify players as effectively:

- Visible
- Potentially Visible
- Hidden

Both visible and potentially visible enemies are transmitted.

Only enemies considered safely hidden are suppressed.

### Testing

For development servers, sv_cheats can be enabled:

```
sv_cheats 1
```

CS2 entity debug commands can then be used to inspect player entities and verify whether hidden enemies are still available to the client.

RealVAC should only suppress enemy player entities when they are fully occluded.

When testing movement with bots, shooting can be disabled while keeping normal bot movement enabled:
```
bot_stop 0
bot_dont_shoot 1
```

### Important

RealVAC is designed for server-side use.

It does not inject code into CS2 clients, inspect client memory, or attempt to detect cheat software running on a player's machine.

Its purpose is to reduce the amount of hidden enemy information available to clients in the first place.