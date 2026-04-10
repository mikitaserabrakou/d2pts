#  D2PTS

Very minimal app that stalks your dota GSI server for hero change then changes your dota2protracker tab to the new hero.

---

## How 
* **C# GSI Listener:**  A local console app that listens for game data from the Dota 2 client.
* **Local API:**  When a hero change is detected, the C# app sends the hero name to a local endpoint (http://localhost:5005/hero).
* **Firefox Extension:**  A background script polls this local API every 15 seconds. If your hero changes, it updates your existing D2PT tab (or opens a new one) and switches focus to it immediately.

---

## Setup Instructions
1. Dota 2 Game State Integration
*   Navigate to: `\steamapps\common\dota 2 beta\game\dota\cfg\gamestate_integration\`
*   Create a new file named `gamestate_integration_d2pts.cfg` with following:

```json
"Dota2GSI"
{
    "uri"           "http://localhost:3003/"
    "timeout"       "5.0"
    "buffer"        "0.1"
    "throttle"      "0.1"
    "heartbeat"     "30.0"
    "data"
    {
        "provider"      "1"
        "map"           "1"
        "player"        "1"
        "hero"          "1"
    }
}
```

---

2. C# GSI Program
```sh
dotnet add package Dota2GSI
dotnet run
```

---

3. Firefox Extension
Installation:
*    Go to `about:debugging` in Firefox.
*    Click `This Firefox` > Load Temporary Add-on.
*    Select your `manifest.json`.

## License

Distributed under the MIT License.

---
