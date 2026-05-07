# Octane-Bridge

The bridge for Octane that reads the raw TCP stream and converts it to WebSockets for the browser.

## Channels

The WS server (default `127.0.0.1:49124`) exposes three channels. Each is a separate WS endpoint clients can subscribe to.

| Path      | Direction | Payload                                                          |
|-----------|-----------|------------------------------------------------------------------|
| `/state`  | broadcast | `UpdateState` — current match state, fired on every tick         |
| `/events` | broadcast | `Event` — discrete events (goals, hits, stat feed, etc.)         |
| `/meta`   | broadcast | `OctaneMeta` — broadcaster-controlled overlay metadata           |

## Meta protocol

`/meta` carries broadcaster-controlled state (team names, logos, series wins, best-of). It is not derived from the game; it is set by an operator and persisted to disk.

### Schema

```jsonc
{
  "bestOf": 5,
  "blue":   { "name": "Team A", "logo": "<data url or empty>", "wins": 0 },
  "orange": { "name": "Team B", "logo": "<data url or empty>", "wins": 0 }
}
```

### Endpoints

- `GET http://127.0.0.1:49124/meta` — returns the current `OctaneMeta` as JSON. Use this to populate a control UI on load.
- `POST http://127.0.0.1:49124/meta` — body is a full `OctaneMeta`; replaces the current state, persists to disk (debounced ~200ms), and broadcasts to all WS subscribers.
- `ws://127.0.0.1:49124/meta` — clients receive the current `OctaneMeta` immediately on connect, then again on every successful POST.

CORS is wide open (`*`) so a browser-based control panel on a different origin can call the endpoints. Listener is bound to `127.0.0.1` only, so the surface is local-machine.

### Persistence

Stored in `meta.json` next to the bridge binary by default. Override via `META_FILE=...` in `app.ini`. Writes are atomic (`.tmp` + rename) and debounced.

## Config

`app.ini` next to the binary:

```ini
PORT=49123
WS_PORT=49124
META_FILE=meta.json
```
