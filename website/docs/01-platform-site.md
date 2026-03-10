# OryxBot Platform — oryxbot.com

> Landing page for the OryxBot ecosystem. Sells both the Pilot product and the developer platform.  
> SPA — Vite + React + TypeScript. Deployed to S3.

---

## Sitemap

```
/                  → Landing page (hero + sections)
/marketplace       → Marketplace (auth-gated)
/register          → Registration flow (Clerk + role picker)
/login             → Clerk login redirect
```

---

## Page: Landing (`/`)

### Navbar
- Logo (left): "OryxBot" wordmark
- Links: Platform | Pilot | Developers | Marketplace | Pricing
- Right: Login / Register buttons (Clerk)

---

### Section 1: Hero — Side-by-Side Product Showcase

Two cards/panels displayed **side by side**, equal width, full viewport height.

#### Left Panel: "OryxBot Pilot" (product pitch)

Animated interactive demo:

1. A stylized **Albion Online minimap** is shown — must match **Albion's actual minimap style**: muted greens/browns for terrain, tan dirt roads, dark water, soft cluster boundary lines, small tree/rock icons, portal exits as glowing orbs at map edges. Use SVG/canvas. Reference real Albion minimaps for color palette and feel (earthy, desaturated, medieval cartography aesthetic).
2. An animated cursor appears and **clicks a destination** on the minimap — a waypoint marker (Albion-style golden pin) drops
3. A small character icon **smoothly animates along the roads** from its current position toward the waypoint
4. While moving, a small floating status badge shows: `NAVIGATING → Fort Sterling`
5. Loop: once arrived, pause, cursor clicks new destination, repeat

Visual style: Albion-authentic map colors, golden waypoint marker, smooth easing on character movement, subtle trail behind character.

**Text overlay on the left panel:**
> **OryxBot Pilot**  
> Click where you want to go. Pilot handles the rest.  
> Pathfinding • Obstacle avoidance • Portal transitions • Death recovery  
>  
> [Try Pilot →] (links to pilot.oryxbot.com)

#### Right Panel: "Build with OryxBot" (developer pitch)

Static but polished:

- Heading: **"Build with OryxBot"**
- Subtext: *"Use our API to script automated workflows on top of Pilot's navigation engine."*
- A code snippet preview (syntax-highlighted Python) — a **resource gathering bot** that uses Pilot:

```python
from oryxbot import Pilot, Config, Screen

config = Config(
    on_attacked="return_to_city",
    route_preference="roads",
)

pilot = Pilot(config)
screen = Screen()

RESOURCE_ZONE = "Stonemouth Rills"
BANK_CITY = "Fort Sterling"

while True:
    pilot.move_to(RESOURCE_ZONE)
    pilot.wait_until_arrived()

    # Gather ore — click resource nodes on screen
    node = screen.find("t6_ore")
    screen.click(node.x, node.y)
    screen.wait_for("gathering_complete")

    # Check if inventory is full
    if screen.find("inventory_full_indicator"):
        pilot.move_to(BANK_CITY)
        pilot.wait_until_arrived()
        screen.click(482, 310)   # open bank NPC
        screen.click(520, 440)   # deposit all button
```

- Below the code: *"Sell your scripts on the Marketplace. Set a monthly price — we handle billing."*
- CTA button: [Read Developer Docs →]

---

### Section 2: How It Works — RailRip Architecture Diagram

Full-width section with a heading:

> **Undetectable by Design — Powered by [RailRip](https://railrip.com)**

A polished **architecture diagram** (use proper icons from Lucide — see Design Decisions). Layout: two boxes on top row, OryxBot Cloud below and centered (visually "out of the way").

**Diagram layout:**

```
   ┌──────────────────────────┐                    ┌──────────────────────────┐
   │      User's Machine      │                    │       Game Server        │
   │                          │ ── Game traffic ──→│       (Albion Online)    │
   │  Game Client             │    (normal UDP)    │                          │
   │  TightVNC Server         │                    │                          │
   │  EasyAntiCheat           │                    └──────────────────────────┘
   │                          │
   │  Tunnel endpoint          │
   └────────────┬─────────────┘
                │
                │  Encrypted tunnel
                │  + VNC input commands
                │  + game traffic (rerouted copy)
                │
   ┌────────────▼──────────────────────────────────┐
   │              OryxBot Cloud                    │
   │                                               │
   │  Bot software (Pilot / marketplace scripts)   │
   │  Packet sniffer (reads game state)            │
   │  TightVNC Client (sends input to user's PC)  │
   │                                               │
   │  Game traffic is rerouted here via tunnel     │
   └───────────────────────────────────────────────┘
```

**Key points to communicate visually (use icons, NOT ASCII art in the actual site):**

- The **cheat software runs entirely on OryxBot Cloud** — nothing suspicious runs on the user's machine
- User's machine runs only: Game Client, TightVNC Server, EasyAntiCheat (all legitimate software)
- Game traffic flows from user's machine directly to the game server (normal connection)
- A copy of the game traffic is rerouted to OryxBot Cloud via an encrypted tunnel
- OryxBot Cloud reads the game state from the rerouted packets and sends input commands back via VNC
- **The game client never sees anything wrong** — EasyAntiCheat only scans the user's machine, where no cheat software exists
- OryxBot Cloud is positioned **below/underneath** the main flow in the diagram — visually communicating it is out of the way, not in the middle

**Process summary (3 labeled nodes with icons):**

| Node | Software Running | Icon |
|------|-----------------|------|
| **User's Machine** | Game Client, TightVNC Server, EasyAntiCheat, tunnel endpoint | Monitor icon |
| **Game Server** | Albion Online server | Server icon |
| **OryxBot Cloud** | Bot software, packet sniffer, TightVNC Client | Cloud icon |

Caption: *"The cheat software runs on OryxBot Cloud — your game client stays clean. EasyAntiCheat sees nothing because there's nothing to see. Powered by [railrip.com](https://railrip.com)"*

---

### Section 3: For Developers

Heading: **"Script Anything on Top of Pilot"**

Two-column layout:

**Left column — code example (larger, syntax-highlighted):**

A **resource gathering bot** — a realistic, compelling example that shows the power of the API:

```python
from oryxbot import Pilot, Config, Screen

# Configure navigation behavior
config = Config(
    on_attacked="return_to_city",   # flee to nearest city if attacked
    route_preference="roads",       # prefer road paths over wilderness
)

pilot = Pilot(config)
screen = Screen()

# Gathering loop: mine ore → deposit at bank → repeat
RESOURCE_ZONE = "Stonemouth Rills"   # T6 ore node location
BANK_CITY = "Fort Sterling"

while True:
    # Navigate to resource zone
    pilot.move_to(RESOURCE_ZONE)
    pilot.wait_until_arrived()

    # Gather ore — click resource nodes on screen
    node = screen.find("t6_ore")
    screen.click(node.x, node.y)
    screen.wait_for("gathering_complete")

    if screen.find("inventory_full_indicator"):
        # Return to city and deposit at bank
        pilot.move_to(BANK_CITY)
        pilot.wait_until_arrived()
        screen.click(482, 310)   # open bank NPC
        screen.click(520, 440)   # deposit all
```

**Right column — feature bullets:**

- **Full Navigation API** — `move_to()`, `follow_route()`, `get_position()`
- **Configurable Behavior** — set responses to attacks, preferred route types, speed limits
- **Event Hooks** — `on_arrived`, `on_attacked`, `on_stuck`, `on_cluster_changed`
- **Marketplace Income** — Sell your scripts on the OryxBot Marketplace. Set a monthly subscription price for users.
- **Grows with you** — Developer subscription starts at €50/mo, drops to just €10/mo once any of your published scripts reaches 5+ active subscribers
- CTA: [Get API Key →] (links to /register)

---

### Section 4: For Users

Heading: **"Just Click and Go"**

- Brief description: *"Don't want to code? OryxBot Pilot gives you a simple dashboard to autopilot your character anywhere in the world."*
- Feature highlights:
  - Set a destination on the map
  - Watch real-time status as your character navigates
  - Automatic obstacle avoidance and death recovery
  - No coding required
- CTA: [Launch Pilot →] (links to pilot.oryxbot.com)

---

### Section 5: Pricing

Heading: **"Simple Pricing"**

Two pricing cards side by side:

| | **User** | **Developer** |
|---|---|---|
| **Price** | **€50/month** | **€50/month → €10/month** |
| **Includes** | OryxBot Pilot access | Full API access |
| | Access to Marketplace scripts | Publish scripts to Marketplace |
| | Cloud execution (no local cheat software) | Set your own monthly price for scripts |
| | Priority support | Developer dashboard |
| **Note** | — | Drops to €10/month once any of your published scripts has 5+ active subscribers |
| **CTA** | [Get Started →](/register) | [Start Building →](/register) |

Below the cards, a small note:
> *Marketplace script subscriptions are billed separately by each script's author. OryxBot takes a commission on marketplace transactions.*

---

### Section 6: Footer

- Links: Platform | Pilot | Marketplace | Developer Docs | Pricing | Discord
- "Powered by [RailRip](https://railrip.com)"
- © OryxBot 2026

---

## Page: Marketplace (`/marketplace`)

**Auth-gated** — redirect to /login if not authenticated via Clerk.

### Layout

- Sidebar or top filter bar: Categories (Navigation Scripts, Gathering Bots, Fishing Scripts, Utility Scripts)
- Grid of script cards, each showing:
  - Script name
  - Author
  - Rating (stars)
  - Monthly price (set by developer)
  - Short description
  - "Subscribe" button (disabled/mock — shows toast "Coming soon")

**Example mock marketplace entries:**

| Script | Author | Price | Description |
|--------|--------|-------|-------------|
| **FortFisher Pro** | @anglerdev | €8/mo | Automated fishing at Fort Sterling docks. Auto-sells trash fish, banks rare catches. |
| **Stonemouth Miner** | @oreminer42 | €12/mo | T6 ore gathering in Stonemouth Rills. Fills inventory → deposits at Fort Sterling → repeats. |
| **Caerleon Courier** | @fastroutes | €5/mo | Optimized trade route runner between Caerleon and all royal cities. |
| **HideHunter** | @skinnerbot | €10/mo | T5-T7 hide gathering across multiple Steppe zones with PvP flee logic. |
| **WoodCutter Elite** | @lumberai | €7/mo | Logs T5-T8 wood in rotating forest zones. Smart tree selection. |
| **GatherAll** | @multipurpose | €15/mo | All-resource gatherer. Configurable target resource, tier, and zone. |

**All data is mocked** — hardcoded array of fake marketplace entries.

---

## Page: Register (`/register`)

> **Self-hosted page with Clerk's `<SignUp />` component** — the Clerk component is embedded in our own page layout, styled to match our design system via Clerk's `appearance` prop. The surrounding page, messaging, and role picker are fully custom.

At the top of the page, a prominent notice:

> **OryxBot is currently in closed beta.**  
> We're selective about who gets access. Registration submits a request to join — all applications are reviewed by our team and you'll be notified by email if you're accepted.

### Flow

1. **Step 1: Role Selection** (custom UI, above Clerk component)
   - Two selectable cards (radio-style, one active at a time):
     - **Developer** — "I want to build scripts and sell on the marketplace"
     - **User** — "I want to use Pilot and marketplace scripts"
   - Icons from Lucide (Code icon for Developer, Gamepad2 icon for User)

2. **Step 2: Clerk `<SignUp />` Component**
   - Clerk's pre-built `<SignUp />` component, self-hosted on our page
   - Styled via Clerk `appearance` prop to match our dark theme + gold accents
   - On successful signup, use `clerk.user.update({ unsafeMetadata: { intent: selectedRole, status: "pending" } })`

3. **Step 3: Confirmation Page (`/register/pending`)**
   - After successful Clerk user creation, redirect here:
   > **You're on the list!**  
   > Your request to join OryxBot has been submitted for review.  
   > We're currently selective about who gets access — we'll notify you by email once your application is approved.  
   >
   > In the meantime, explore our [documentation](#) or check out [Pilot](https://pilot.oryxbot.com).

---

## Page: Login (`/login`)

> **Self-hosted page with Clerk's `<SignIn />` component** — Clerk component embedded in our own page layout with custom messaging.

At the top, a small notice:

> **Access is currently limited.** Only approved members can log in. If you haven't been accepted yet, you'll be redirected after sign-in.

- Clerk `<SignIn />` component, styled via `appearance` prop to match site theme
- On success: check `user.unsafeMetadata.status`
  - If `"approved"` → redirect to `/marketplace`
  - If `"pending"` → redirect to `/register/pending` (show "still under review" message)
- Link below: "Don't have an account? [Register →](/register)"
