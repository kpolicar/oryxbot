# OryxBot Website Rebuild — Overview

> Two SPAs, both Vite + React/TypeScript, deployed as static files to S3.  
> No backend. Auth via Clerk (client-side only).

---

## Projects

| Project | Domain | URL |
|---------|--------|-----|
| **OryxBot Platform** | Landing page, developer docs, marketplace shell, auth | `oryxbot.com` |
| **OryxBot Pilot** | Pilot product page, live dashboard demo, setup guide | `pilot.oryxbot.com` |

## Tech Stack

- **Framework:** Vite + React + TypeScript
- **Styling:** Tailwind CSS + shadcn/ui components
- **Auth:** Clerk (client-side SDK only)
- **Routing:** React Router (SPA, hash or history mode with S3 fallback)
- **Animations:** Framer Motion (hero animations, minimap demo)
- **Deployment:** S3 static hosting + CloudFront
- **No backend** — marketplace data, user lists, and dashboard state are all mocked/stubbed

## Shared Design Language

- Dark theme — charcoal `#25282D` background, white text, warm gold `#AEA480` accents
- Primary palette: `#AEA480` (gold accent / CTAs), `#25282D` (dark bg), `#FFFFFF` (text / highlights)
- Monospace font for code snippets, sans-serif (Inter) for body
- Consistent navbar with OryxBot logo, links, login/register button via Clerk
- Footer with links to both sites, Discord, GitHub, railrip.com

## Design Decisions

- **No ASCII art** — all diagrams, icons, and visual elements on the actual site must use proper graphics. Use **Lucide React** (`lucide-react`) as the icon pack for all UI icons (monitor, server, cloud, code, gamepad, shield, etc.). Diagrams are built with SVG/Canvas + Framer Motion, not text art.
- **Albion-style minimap** — the minimap component must visually match Albion Online's actual minimap aesthetic: muted earthy tones (desaturated greens, browns, tans), medieval cartography feel, soft cluster boundary lines, small terrain icons for trees/rocks/water. Reference real Albion screenshots.
- **Self-hosted Clerk components** — use Clerk's `<SignUp />` and `<SignIn />` pre-built components but embed them in our own page layouts with custom messaging, role picker, and styling via Clerk's `appearance` prop. Pages are ours; auth widgets are Clerk's.
- **Marketplace pricing model** — developers set a monthly subscription price for their scripts. OryxBot takes a commission on all marketplace transactions.
- **Cloud-based architecture** — Pilot runs in OryxBot Cloud, not on the user's machine or in the browser. The user controls Pilot from the browser, but all bot logic executes server-side. The game runs on the user's Windows 11 machine; the cloud connects via VNC to move the cursor.
- **Device frame mockups** — Pilot site uses mock device chrome (Windows 11 titlebar for the game, browser chrome for the dashboard, laptop frame + mobile phone frame) to visually communicate the multi-device architecture.
- **Open-source setup** — the user installs only third-party open-source software (TightVNC + connection tunnel) via an open-source setup script. No OryxBot code runs on the user's machine.

## Detailed Plans

1. [01-platform-site.md](./01-platform-site.md) — oryxbot.com full page breakdown
2. [02-pilot-site.md](./02-pilot-site.md) — pilot.oryxbot.com full page breakdown
3. [03-shared-components.md](./03-shared-components.md) — shared component library, design tokens, Clerk config
