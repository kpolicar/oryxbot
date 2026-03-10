# Shared Components & Configuration

> Design tokens, shared components, Clerk setup, and deployment config used by both sites.

---

## Project Structure

```
websites/
├── packages/
│   └── shared/                    # Shared component library (internal package)
│       ├── components/
│       │   ├── Navbar.tsx
│       │   ├── Footer.tsx
│       │   ├── RailripDiagram.tsx  # Animated network diagram
│       │   ├── AlbionMinimap.tsx   # SVG/Canvas minimap component
│       │   ├── CodeBlock.tsx       # Syntax-highlighted code viewer
│       │   ├── ClerkGuard.tsx      # Auth gate wrapper
│       │   ├── WindowsFrame.tsx    # Windows 11 window chrome (titlebar, min/max/close)
│       │   ├── BrowserFrame.tsx    # Browser window chrome (tab bar, address bar)
│       │   ├── LaptopFrame.tsx     # Laptop device frame (widescreen 16:9)
│       │   ├── PhoneFrame.tsx      # Mobile phone device frame
│       │   └── DataFlowLine.tsx    # Animated particle/pulse connection line
│       ├── hooks/
│       │   ├── useSimulatedPosition.ts   # Ticking character position
│       │   ├── useSimulatedNavState.ts   # Cycling navigation states
│       │   └── useSimulatedEventLog.ts   # Appending fake events
│       ├── assets/
│       │   ├── albion-world-map.svg      # Simplified Albion world map
│       │   ├── albion-minimap-cluster.svg # Single-cluster minimap
│       │   └── oryxbot-logo.svg
│       └── styles/
│           └── tokens.css                # Design tokens (CSS custom properties)
│
├── apps/
│   ├── platform/                  # oryxbot.com
│   │   ├── index.html
│   │   ├── vite.config.ts
│   │   ├── src/
│   │   │   ├── main.tsx
│   │   │   ├── App.tsx
│   │   │   ├── pages/
│   │   │   │   ├── Landing.tsx
│   │   │   │   ├── Marketplace.tsx
│   │   │   │   ├── Register.tsx
│   │   │   │   └── Login.tsx
│   │   │   └── components/
│   │   │       ├── HeroPilotDemo.tsx      # Left panel: animated minimap
│   │   │       ├── HeroDeveloperPitch.tsx  # Right panel: code snippet
│   │   │       ├── RailripSection.tsx      # Network diagram section
│   │   │       ├── DeveloperSection.tsx    # Code + bullet points
│   │   │       ├── UserSection.tsx         # "Just Click and Go"
│   │   │       ├── MarketplaceGrid.tsx     # Mock script cards
│   │   │       └── RolePicker.tsx          # Developer/User selector pre-signup
│   │   └── data/
│   │       └── mockMarketplace.ts          # Fake marketplace entries
│   │
│   └── pilot/                     # pilot.oryxbot.com
│       ├── index.html
│       ├── vite.config.ts
│       ├── src/
│       │   ├── main.tsx
│       │   ├── App.tsx
│       │   ├── pages/
│       │   │   └── Landing.tsx
│       │   └── components/
│       │       ├── PilotHero.tsx           # Split hero: Win11 game window + browser dashboard + data-flow line
│       │       ├── DashboardDemo.tsx       # Full mock dashboard in browser chrome
│       │       ├── WorldMapPanel.tsx       # Interactive SVG world map
│       │       ├── CharacterInfoPanel.tsx  # Ticking live stats
│       │       ├── NavigationStatePanel.tsx # State machine display
│       │       ├── RouteProgressPanel.tsx  # Cluster checklist
│       │       ├── EventLogPanel.tsx       # Scrolling event log
│       │       ├── DeviceShowcase.tsx      # Laptop frame (game) + phone frame (dashboard) side by side
│       │       ├── FeaturesGrid.tsx        # 11-feature grid (3 columns)
│       │       ├── HowItWorks.tsx          # 4-step architecture visual
│       │       ├── SetupSection.tsx        # "What You Need" — requirements + setup script + reassurance
│       │       └── PricingCard.tsx         # Early access card
│       └── data/
│           ├── mockRoute.ts               # Fake cluster route
│           ├── mockEvents.ts              # Fake event log entries
│           └── mockCharacter.ts           # Fake character data
│
├── package.json                   # Workspace root (pnpm workspaces)
├── pnpm-workspace.yaml
├── tailwind.config.ts             # Shared Tailwind config
└── tsconfig.base.json
```

---

## Design Tokens

```css
:root {
  /* Backgrounds */
  --bg-primary: #25282d;       /* Dark charcoal */
  --bg-secondary: #2f3237;    /* Slightly lighter charcoal */
  --bg-card: #2f3237;
  --bg-card-hover: #3a3d42;

  /* Text */
  --text-primary: #ffffff;
  --text-secondary: #c8c8c8;
  --text-muted: #8a8a8a;

  /* Accents */
  --accent-gold: #aea480;
  --accent-gold-hover: #c4b98e;
  --accent-green: #22c55e;
  --accent-red: #ef4444;

  /* Fonts */
  --font-body: 'Inter', sans-serif;
  --font-mono: 'JetBrains Mono', 'Fira Code', monospace;

  /* Borders */
  --border-subtle: #3a3d42;
  --border-accent: #aea480;

  /* Animations */
  --transition-fast: 150ms ease;
  --transition-normal: 300ms ease;
}
```

---

## Clerk Configuration

### Environment Variables (per app)

```env
VITE_CLERK_PUBLISHABLE_KEY=pk_test_...
```

### Setup

- Use `@clerk/clerk-react` SDK with **self-hosted Clerk `<SignUp />` and `<SignIn />` components**
- Wrap both apps in `<ClerkProvider>` (needed for Clerk context)
- Clerk components are embedded in our own page layouts with custom surrounding UI (role picker, notices, messaging)
- Style Clerk components via `appearance` prop to match our dark theme + gold accents
- Registration: after Clerk signup, call `user.update({ unsafeMetadata: { intent: selectedRole, status: "pending" } })`
- Login: after Clerk signin, check `unsafeMetadata.status` for approval state
- Auth guard for `/marketplace`: redirect to `/login` if `!isSignedIn` or if `unsafeMetadata.status !== "approved"`

### Post-Registration Flow

After Clerk signup completes:
1. Set `unsafeMetadata.intent` = "developer" | "user", `unsafeMetadata.status` = "pending"
2. Redirect to `/register/pending`
3. Show confirmation: "Your request has been submitted. We're currently selective — we'll notify you by email."
4. No actual backend approval flow yet — just the UI. Admins approve manually in Clerk dashboard by setting `status: "approved"`

---

## Deployment

### S3 Static Hosting

- Each app builds to `dist/` via `vite build`
- Upload `dist/` contents to respective S3 buckets:
  - `s3://oryxbot-platform/` → oryxbot.com
  - `s3://oryxbot-pilot/` → pilot.oryxbot.com
- Configure S3 bucket for static website hosting
- Set error document to `index.html` (SPA fallback for client-side routing)
- CloudFront distribution in front with custom domain + SSL

### Build Commands

```bash
# From workspace root
pnpm --filter platform build    # builds oryxbot.com
pnpm --filter pilot build       # builds pilot.oryxbot.com
```

---

## Albion Minimap Asset Notes

The animated minimap component needs a stylized Albion-like cluster map. Options:

1. **Recreate from scratch** (recommended): SVG with terrain patches (green/brown), roads (tan lines), trees (small green circles), water (blue patches), portal exits (glowing circles at edges). Doesn't need to be accurate — just visually recognizable as an Albion minimap.
2. **Use real screenshot as base**: Take an Albion minimap screenshot, trace key features into SVG, style to match dark theme.

The character dot and waypoint marker are overlaid on top of the map SVG and animated with Framer Motion or CSS keyframes.

---

## Device Frame Components

Shared presentational components used by the Pilot site hero and dashboard demo sections to visually communicate the multi-device architecture.

### WindowsFrame
Mock Windows 11 window chrome. Rounded corners, Mica-style translucent titlebar, minimize/maximize/close buttons, customizable title text. Content is rendered as children.

### BrowserFrame
Mock browser window chrome. Tab bar with a single tab (customizable label), address bar (customizable URL), standard browser controls. Content is rendered as children.

### LaptopFrame
A laptop device shell (screen bezel + keyboard base). Widescreen 16:9 aspect ratio. Content fills the screen area. Used in the dashboard demo responsive showcase to frame the game viewport.

### PhoneFrame
A mobile phone device shell (rounded rectangle bezel, notch/dynamic island). Content fills the screen area. Used in the dashboard demo responsive showcase to frame the Pilot dashboard in a compact mobile layout.

### DataFlowLine
Animated SVG line with traveling particles/pulses. Connects two elements. Accepts a `middleIcon` prop — used with a cloud icon to show the browser → OryxBot Cloud → game machine flow in the Pilot hero.

---

## Simulated Data Hooks

All "live" data in the dashboard and hero animations is driven by React hooks that produce ticking/cycling mock data:

- `useSimulatedPosition(route)` — returns position coords that lerp along a predefined path, updates every 500ms
- `useSimulatedNavState()` — cycles through states: FOLLOWING_ROUTE → APPROACHING_PORTAL → WAITING_FOR_LOAD → FOLLOWING_ROUTE on timed intervals
- `useSimulatedEventLog()` — appends a new realistic event message every 2-4 seconds from a pool of templates (portal transitions, course corrections, cluster entries, obstacle avoidance events)
- `useSimulatedCursor(route)` — returns animated cursor position that moves across the game viewport, simulating VNC input. Used in the Pilot hero Windows 11 panel to show the cursor moving on its own.
