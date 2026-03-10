# OryxBot Pilot — pilot.oryxbot.com

> Standalone product page + live dashboard demo for OryxBot Pilot.  
> SPA — Vite + React + TypeScript. Deployed to S3.

---

## Sitemap

```
/                  → Pilot landing + dashboard demo
```

Single-page app — the entire experience is one scrollable page with a sticky nav.

---

## Navbar

- Logo (left): "OryxBot Pilot" wordmark (with Pilot highlighted in accent color)
- Links: Features | Dashboard Demo | Pricing
- Right: "Get Pilot" CTA button (links to oryxbot.com/register)

---

## Section 1: Hero

Full-width dark hero with headline and a split visual showing the two halves of the system.

**Headline:**
> **Autopilot for Albion Online**  
> Set a destination. Pilot takes over your cursor and navigates your character through portals, around obstacles, across the entire world — all from the cloud, controlled from your browser.

**Hero visual** (center, wide):

Two panels side-by-side, connected by a subtle animated data-flow line:

**Left panel — "The Game" (Windows 11 window):**
- A mock Windows 11 window chrome (rounded corners, Mica-style titlebar with minimize/maximize/close buttons, title reads "Albion Online")
- Inside: a stylized Albion game viewport with the character moving along a path
- The mouse cursor is visibly moving on its own (animated) — clicking portals, navigating terrain
- Subtle label below: "Your Windows 11 machine"

**Right panel — "The Pilot" (browser window):**
- A mock browser chrome (tab bar showing "pilot.oryxbot.com", address bar)
- Inside: a compact version of the Pilot dashboard (minimap, status, route progress)
- Live stats ticking (mock data, updating in real-time):
  - Current cluster: `Martlock Steppe`
  - Status: `NAVIGATING`
  - ETA: `2m 34s`
  - Distance: `847 units`
- Subtle label below: "Control from any device"

**Animated data-flow line** connecting the two panels — small particles/pulses traveling from the browser panel to the game panel, with a subtle cloud icon in the middle. Reinforces the architecture: browser → OryxBot cloud → game machine.

CTA: [Get Started →] (links to oryxbot.com/register)

---

## Section 2: Dashboard Demo — "The Pilot Web UI"

Heading: **"Your Command Center"**

Subheading: *Pilot runs in the OryxBot cloud. You control it from your browser — no overlay, no injection, nothing installed on the game machine. The cloud connects to your PC and takes over the cursor externally — the game never knows it's there.*

A **full interactive mock dashboard** that pretends to be the real Pilot control panel. This is the core visual of the page — it should look like a real working product. Wrapped in a mock browser chrome (tab + address bar showing `pilot.oryxbot.com/dashboard`) to reinforce that the user interacts via a web app.

### Dashboard Layout (mock, all data simulated)

```
┌─────────────────────────────────────────────────────────────────┐
│  PILOT DASHBOARD                              Status: ● ONLINE │
├──────────────────────┬──────────────────────────────────────────┤
│                      │                                          │
│   WORLD MAP          │   CHARACTER INFO                         │
│   (interactive)      │   ────────────────                       │
│                      │   Name:     OrionTrader                  │
│   [Minimap with      │   Cluster:  Martlock Steppe              │
│    clickable zones,  │   Position: (412.3, 0.0, -187.6)        │
│    current position  │   Mount:    Swiftclaw (T5)               │
│    marker, and       │   Health:   ████████░░ 82%               │
│    destination       │                                          │
│    selector]         │   NAVIGATION STATE                       │
│                      │   ────────────────                       │
│                      │   State:    FOLLOWING_ROUTE               │
│                      │   Target:   Fort Sterling                │
│                      │   Via:      6 clusters remaining         │
│                      │   ETA:      4m 12s                       │
│                      │                                          │
│                      │   ROUTE PROGRESS                         │
│                      │   ────────────────                       │
│                      │   ✅ Martlock                            │
│                      │   ✅ Martlock Steppe                     │
│                      │   🔵 Creag Haulann (current)             │
│                      │   ⬜ Lewsdon Loch                        │
│                      │   ⬜ Stonemouth Rills                    │
│                      │   ⬜ Fort Sterling Surrounds             │
│                      │   ⬜ Fort Sterling                       │
│                      │                                          │
│                      │   EVENT LOG                              │
│                      │   ────────────────                       │
│                      │   14:23:07  Entered Creag Haulann        │
│                      │   14:22:45  Portal transition complete   │
│                      │   14:22:31  Approaching portal exit      │
│                      │   14:21:58  Course corrected (+12.4°)    │
│                      │   14:21:12  Entered Martlock Steppe      │
│                      │                                          │
└──────────────────────┴──────────────────────────────────────────┘
```

### Interactivity (all mocked/simulated)

- **World map panel (left):** A simplified Albion world map (SVG). User can click on any cluster to "set destination." On click, a route is instantly generated (fake) and the right panel updates.
- **Character info (right-top):** Position coordinates tick/animate to simulate live movement updates (random walk within cluster bounds, ~2 updates/second).
- **Navigation state (right-middle):** State name cycles through realistic transitions: `FOLLOWING_ROUTE → APPROACHING_PORTAL → WAITING_FOR_LOAD → FOLLOWING_ROUTE` on a timed loop.
- **Route progress (right):** Checkmarks animate from ⬜ to ✅ over time as the "character" progresses. Current cluster has a pulsing 🔵 indicator.
- **Event log (right-bottom):** New entries append at the top every few seconds with realistic messages: portal transitions, course corrections, cluster entries, obstacle avoidance events.

**Purpose:** This mock dashboard should give potential customers a visceral feel for what Pilot's real-time control experience looks like. It should feel alive — data ticking, states transitioning, route progressing.

### Responsive presentation

The dashboard demo section uses a split visual to reinforce the two-device nature of the product:

- **Game viewport (left/top):** Shown inside a mock laptop frame (widescreen 16:9), displaying the Albion Online game window with the character moving. Reinforces that the game runs on a real Windows machine.
- **Pilot dashboard (right/bottom):** Shown inside a mock **mobile phone frame**, displaying the same dashboard UI in a compact mobile layout. Reinforces that you can control Pilot from any device — even your phone on the couch.

On desktop viewports, the laptop and phone float side-by-side (laptop larger, phone smaller beside it). On mobile viewports, they stack vertically.

The dashboard mock itself (Section 2 above) must be responsive: on narrow screens, the two-column layout (map + info panels) collapses to a single scrollable column with the map on top and info panels stacked below.

---

## Section 3: Features

Three-column feature grid:

| Feature | Description |
|---------|-------------|
| **Smart Pathfinding** | A* pathfinding across the entire Albion world graph. Finds optimal routes through hundreds of clusters — avoid PvP zones, cap tier, or blacklist specific areas. |
| **Portal Navigation** | Automatically handles portal transitions, loading screens, and cluster changes. Suppresses stale position data after zone loads so the bot never gets confused. |
| **Obstacle Avoidance** | Detects stuck states within 1.3 seconds and applies rotational unsticking. Escalates through multiple angles before declaring a failure — no infinite loops. |
| **Course Correction** | Continuously monitors drift from the planned path. If the character strays beyond threshold, Pilot steers back to the nearest upcoming waypoint automatically. |
| **Death Recovery** | If your character dies, Pilot respawns and resumes the route from where it left off. |
| **Route Recording & Replay** | Record your own routes as JSON waypoint sequences, then replay them anytime. Supports move waypoints, portal waypoints, and named markers. |
| **Route Skip-Ahead** | Pilot looks up to 4 waypoints ahead and skips past ones you've already passed — no backtracking if you overshoot. |
| **Position Prediction** | Between network updates (150–350ms apart), Pilot extrapolates your position using velocity history so decisions are never based on stale data. |
| **Real-Time Event Log** | Every decision, course correction, portal transition, and failure is logged with timestamps. Full post-mortem replay for debugging. |
| **Road Preference** | Configure Pilot to prefer roads for faster travel or wilderness paths for stealth. |
| **Fully External** | Runs in the OryxBot cloud, controlled from your browser. Takes over your game machine's cursor via VNC — nothing injected into the game process. |

---

## Section 4: How It Works

Four-step visual showing the fully external architecture:

```
1. GAME RUNS ON               2. SET DESTINATION          3. CLOUD TAKES OVER          4. ARRIVE
   YOUR WINDOWS PC                                           THE CURSOR
                                  Open pilot.oryxbot.com                                    Character arrives
   Albion Online runs             on any device — phone,     OryxBot cloud connects         at destination
   normally on your               tablet, another PC.        to your Windows machine
   Windows 11 machine.            Click the map or type      via VNC and moves the
   Nothing is installed            a zone name.              cursor directly — clicking,
   inside the game.                                          navigating, handling
                                                             portals and obstacles.
```

Each step has an icon/illustration and brief copy. Key emphasis: **nothing runs inside the game process** — Pilot operates the cursor from outside, the same way a human would.

---

## Section 5: What You Need

Heading: **"Setup in 5 Minutes"**

A single callout/card with a checklist:

> **Requirements:**
> - A Windows 11 machine running Albion Online
> - An OryxBot account (closed beta)
>
> **That's it.** Run our open-source setup script and you're done:
>
> ```
> irm https://setup.oryxbot.com | iex
> ```
>
> The script installs **TightVNC** (open-source remote desktop) and our **connection tunnel** — both are third-party, open-source applications. No OryxBot code runs on your machine. The tunnel securely connects your PC to OryxBot Cloud, and Pilot does the rest.

Below the card, three small reassurance bullets with Lucide icons:
- **Open source** — the setup script and tunnel are fully open-source. Inspect every line before you run it.
- **Nothing hidden** — only TightVNC and the tunnel run on your machine. No injectors, no hooks, no game modifications.
- **Uninstall anytime** — one command to remove everything cleanly.

---

## Section 6: Pricing (placeholder)

Heading: **"Pricing"**

Single card: 
> **Early Access**  
> Pilot is currently in closed beta.  
> [Request Access →] (links to oryxbot.com/register)

---

## Section 7: Footer

Same footer as platform site:
- Links: Platform | Pilot | Marketplace | Docs | Discord
- "Powered by [RailRip](https://railrip.com)"
- © OryxBot 2026
