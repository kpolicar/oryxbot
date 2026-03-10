import { useState, useEffect, useCallback, useRef } from 'react'
import { motion, AnimatePresence } from 'framer-motion'
import { ButtonArrow } from '@shared/components/ButtonArrow'
import { Star, Users, Clock, TrendingUp, ArrowLeft, Activity } from 'lucide-react'

/* ── Types & Constants ── */

type Phase = 'browse' | 'hover' | 'detail' | 'purchase' | 'dashboard'

const PHASE_SCHEDULE: [Phase, number][] = [
    ['browse', 0],
    ['hover', 3000],
    ['detail', 5500],
    ['purchase', 9000],
    ['dashboard', 11500],
]
const CYCLE_MS = 19000

const BOTS = [
    {
        title: 'Personal Island Farming',
        loop: 'Walk to a dirt square, plant seed, water, wait 24h, harvest. Repeat for every square.',
        why: 'Zero combat, zero risk, zero dynamic interaction. Just menus and clicking on the ground.',
        tag: 'Farming',
        price: '€10/mo',
        rating: 4.8
    },
    {
        title: 'Refining and Mass Crafting',
        loop: 'Overload mount, walk slowly to refining station, craft, watch progress bar. Walk back.',
        why: 'Incredibly monotonous idle loop converting Item A into Item B.',
        tag: 'Crafting',
        price: '€12/mo',
        rating: 4.5
    },
    {
        title: 'Safe Zone Fishing',
        loop: 'Cast line, keep bobber in the green zone until caught. Move down the shore.',
        why: 'Muscle memory mini-game in Blue/Yellow zones. Zone out and watch a second monitor.',
        tag: 'Fishing',
        price: '€8/mo',
        rating: 4.9
    },
    {
        title: 'Safe Zone Gathering',
        loop: 'Mount up, run in a circle. Click glowing node, wait, remount. Repeat.',
        why: 'Zero full-loot PvP threat. The backbone of the economy, easily automated.',
        tag: 'Gathering',
        price: '€9/mo',
        rating: 4.7
    },
    {
        title: 'Laborers Management',
        loop: 'Fill journals, hand them out, collect silver next day. Repeat across islands.',
        why: 'Pure menu navigation. No combat, no movement — just click and collect.',
        tag: 'Economy',
        price: '€7/mo',
        rating: 4.6
    },
    {
        title: 'Market Flipping',
        loop: 'Scan buy/sell orders, place undercuts, collect fulfilled orders. Relist.',
        why: 'Spreadsheet gameplay. Repetitive price checking and order placement.',
        tag: 'Trading',
        price: '€15/mo',
        rating: 4.4
    }
]

/* ── Main Component ── */

export function UserSection() {
    const [phase, setPhase] = useState<Phase>('browse')

    useEffect(() => {
        let cancelled = false
        let ids: ReturnType<typeof setTimeout>[] = []

        function runCycle() {
            ids.forEach(clearTimeout)
            ids = []
            for (const [p, delay] of PHASE_SCHEDULE) {
                ids.push(setTimeout(() => { if (!cancelled) setPhase(p) }, delay))
            }
            ids.push(setTimeout(() => { if (!cancelled) runCycle() }, CYCLE_MS))
        }

        runCycle()
        return () => { cancelled = true; ids.forEach(clearTimeout) }
    }, [])

    const showBrowse = phase === 'browse' || phase === 'hover'
    const showDetail = phase === 'detail' || phase === 'purchase'

    return (
        <section className="py-28 px-6 relative overflow-hidden">
            <div className="max-w-[90rem] mx-auto relative grid grid-cols-1 lg:grid-cols-[2fr_3fr] gap-16 items-start">
                {/* Header Text - Left Column */}
                <motion.div
                    initial={{ opacity: 0, x: -30 }}
                    whileInView={{ opacity: 1, x: 0 }}
                    viewport={{ once: true }}
                    transition={{ duration: 0.6 }}
                    className="text-left z-10"
                >
                    <span className="text-[11px] font-medium tracking-[0.15em] uppercase text-gold-400 block mb-3">
                        OryxBot Marketplace
                    </span>
                    <h2 className="text-3xl lg:text-5xl font-bold text-text-primary mb-6 leading-tight">
                        Discover, purchase, and <span className="text-gradient-gold">go</span>
                    </h2>
                    <p className="text-text-secondary text-lg leading-relaxed mb-8">
                        Browse community-built scripts on the marketplace. Find what you need,
                        hit purchase, and your bot is running in seconds. All executed safely in the cloud.
                    </p>
                    <ul className="space-y-4 mb-8">
                        {[
                            'Simple setup',
                            'Community scripts',
                            'Automatic updates',
                        ].map((item, i) => (
                            <li key={i} className="flex items-center gap-3 text-text-secondary text-sm">
                                <span className="w-1.5 h-1.5 rounded-full bg-accent-gold" />
                                {item}
                            </li>
                        ))}
                    </ul>
                </motion.div>

                {/* Animated Layout — Right Column */}
                <motion.div
                    initial={{ opacity: 0, x: 30 }}
                    whileInView={{ opacity: 1, x: 0 }}
                    viewport={{ once: true }}
                    transition={{ duration: 0.6, delay: 0.2 }}
                    className="relative hidden lg:block w-full"
                >
                    {/* Asymmetric 3-column layout: side cards offset & clipped, center card anchored */}
                    <div className="relative w-full overflow-visible" style={{ height: 720 }}>
                        <div
                            className="grid gap-4 w-full pointer-events-none items-start"
                            style={{
                                gridTemplateColumns: '1fr 1fr 1fr',
                            }}
                        >
                            {/* Left column — starts half a card above center, fades edges */}
                            <div
                                className="flex flex-col gap-4"
                                style={{ transform: 'translateY(-336px)', maskImage: 'linear-gradient(to bottom, transparent 0%, black 25%, black 75%, transparent 100%), linear-gradient(to right, transparent 0%, rgba(0,0,0,0.5) 40%, black 100%)', maskComposite: 'intersect' }}
                            >
                                <SideBotCard bot={BOTS[0]} />
                                <SideBotCard bot={BOTS[2]} />
                            </div>

                            {/* Center column — card above (clipped), animated primary, card below (clipped) */}
                            <div className="flex flex-col items-center z-10">
                                {/* Card peeking from above — pulled out of flow with negative margin, translated up */}
                                <div style={{ marginBottom: 0, height: 0, overflow: 'visible' }}>
                                    <div style={{
                                        transform: 'translateY(-100%)',
                                        maskImage: 'linear-gradient(to bottom, transparent 0%, transparent 40%, black 100%)',
                                    }}>
                                        <SideBotCard bot={BOTS[4]} />
                                    </div>
                                </div>
                                <div className="shadow-2xl shadow-black/60 rounded-lg pointer-events-auto relative z-10">
                                    <AppFrame glow>
                                        <BrowserBar phase={phase} />
                                        <div className="relative h-[calc(100%-24px)] overflow-hidden">
                                            <AnimatePresence mode="wait">
                                                {showBrowse && <BrowseView key="browse" isHovering={phase === 'hover'} />}
                                                {showDetail && <DetailView key="detail" isPurchasing={phase === 'purchase'} />}
                                                {phase === 'dashboard' && <DashboardView key="dashboard" />}
                                            </AnimatePresence>
                                            <CursorOverlay phase={phase} />
                                        </div>
                                    </AppFrame>
                                </div>
                                {/* Card peeking below — fades out at bottom */}
                                <div style={{ marginTop: -4 }}>
                                    <div style={{
                                        maskImage: 'linear-gradient(to bottom, black 0%, transparent 40%, transparent 100%)',
                                    }}>
                                        <SideBotCard bot={BOTS[5]} />
                                    </div>
                                </div>
                            </div>

                            {/* Right column — starts half a card above center, fades edges */}
                            <div
                                className="flex flex-col gap-4"
                                style={{ transform: 'translateY(-336px)', maskImage: 'linear-gradient(to bottom, transparent 0%, black 25%, black 75%, transparent 100%), linear-gradient(to left, transparent 0%, rgba(0,0,0,0.5) 40%, black 100%)', maskComposite: 'intersect' }}
                            >
                                <SideBotCard bot={BOTS[1]} />
                                <SideBotCard bot={BOTS[3]} />
                            </div>
                        </div>
                    </div>
                </motion.div>
            </div>
        </section>
    )
}

function SideBotCard({ bot }: { bot: typeof BOTS[0] }) {
    const slug = bot.title.toLowerCase().replace(/\s+/g, '')
    return (
        <AppFrame>
            <div className="flex items-center h-6 px-3 border-b border-border-subtle bg-bg-card/30">
                <div className="flex items-center gap-1 flex-1 min-w-0">
                    <svg width="8" height="8" viewBox="0 0 16 16" className="shrink-0 text-accent-green">
                        <path d="M8 1a3.5 3.5 0 0 0-3.5 3.5V6H3a1 1 0 0 0-1 1v7a1 1 0 0 0 1 1h10a1 1 0 0 0 1-1V7a1 1 0 0 0-1-1h-1.5V4.5A3.5 3.5 0 0 0 8 1z" fill="currentColor" />
                    </svg>
                    <span className="text-[7px] text-text-muted truncate font-[family-name:var(--font-family-mono)] opacity-70">
                        dashboard.oryxbot.com/marketplace/{slug}
                    </span>
                </div>
            </div>

            <div className="absolute inset-0 top-6 p-4 pt-2">
                {/* Back */}
                <div className="flex items-center gap-1 mb-3 text-text-muted">
                    <ArrowLeft size={10} />
                    <span className="text-[8px]">Back</span>
                </div>

                {/* Header */}
                <div className="flex items-start justify-between mb-2">
                    <div>
                        <h3 className="text-[13px] font-bold text-accent-gold leading-tight">{bot.title}</h3>
                        <span className="text-[9px] text-text-muted">by @botsmith</span>
                    </div>
                </div>

                {/* Rating */}
                <div className="flex items-center gap-0.5 mb-3">
                    {[1, 2, 3, 4, 5].map((s) => (
                        <Star
                            key={s}
                            size={9}
                            className={s <= bot.rating ? 'text-accent-gold fill-accent-gold' : 'text-accent-gold/30 fill-accent-gold/30'}
                        />
                    ))}
                    <span className="text-[8px] text-text-muted ml-1">{bot.rating} · verified</span>
                </div>

                <div className="space-y-4 mt-6">
                    <div className="rounded-lg border border-border-subtle bg-bg-card/50 p-3">
                        <span className="text-[9px] font-bold text-text-muted uppercase tracking-[0.1em] block mb-1.5">The Loop</span>
                        <p className="text-[10px] text-text-secondary leading-relaxed">{bot.loop}</p>
                    </div>
                    <div className="rounded-lg border border-border-subtle bg-bg-card/50 p-3">
                        <span className="text-[9px] font-bold text-text-muted uppercase tracking-[0.1em] block mb-1.5">Why it fits</span>
                        <p className="text-[10px] text-text-secondary leading-relaxed">{bot.why}</p>
                    </div>
                </div>
            </div>
        </AppFrame>
    )
}

function AppFrame({ children, glow }: { children: React.ReactNode; glow?: boolean }) {
    return (
        <div
            className={`relative rounded-lg border border-border-subtle w-[336px] h-[672px] overflow-hidden ${glow
                ? 'shadow-[0_0_80px_rgba(174,164,128,0.15)] border-accent-gold/20'
                : ''
                }`}
            style={{ background: 'linear-gradient(145deg, #0f1014, #0a0b0d)' }}
        >
            {/* Content area */}
            <div className="relative h-full w-full">
                {children}
            </div>
        </div>
    )
}

/* ── Browser URL Bar ── */

const PHASE_URLS: Record<Phase, string> = {
    browse: 'dashboard.oryxbot.com/marketplace',
    hover: 'dashboard.oryxbot.com/marketplace',
    detail: 'dashboard.oryxbot.com/marketplace/harvestking',
    purchase: 'dashboard.oryxbot.com/marketplace/harvestking',
    dashboard: 'dashboard.oryxbot.com/scripts/harvestking',
}

function BrowserBar({ phase }: { phase: Phase }) {
    const url = PHASE_URLS[phase]
    return (
        <div className="flex items-center h-6 px-3 border-b border-border-subtle bg-bg-card/30">
            <div className="flex items-center gap-1 flex-1 min-w-0">
                <svg width="8" height="8" viewBox="0 0 16 16" className="shrink-0 text-accent-green">
                    <path d="M8 1a3.5 3.5 0 0 0-3.5 3.5V6H3a1 1 0 0 0-1 1v7a1 1 0 0 0 1 1h10a1 1 0 0 0 1-1V7a1 1 0 0 0-1-1h-1.5V4.5A3.5 3.5 0 0 0 8 1z" fill="currentColor" />
                </svg>
                <AnimatePresence mode="wait">
                    <motion.span
                        key={url}
                        initial={{ opacity: 0 }}
                        animate={{ opacity: 1 }}
                        exit={{ opacity: 0 }}
                        transition={{ duration: 0.2 }}
                        className="text-[7px] text-text-muted truncate font-[family-name:var(--font-family-mono)]"
                    >
                        {url}
                    </motion.span>
                </AnimatePresence>
            </div>
        </div>
    )
}

/* ── Browse View ── */

function BrowseView({ isHovering }: { isHovering: boolean }) {
    const scripts = [
        { name: 'Stonemouth Miner', author: '@oreminer42', price: '€12', tag: 'Gathering', rating: 5 },
        { name: 'HarvestKing', author: '@botsmith', price: '€12', tag: 'Gathering', rating: 4, featured: true },
        { name: 'FortFisher Pro', author: '@anglerdev', price: '€8', tag: 'Fishing', rating: 4 },
    ]

    return (
        <motion.div
            initial={{ opacity: 0 }}
            animate={{ opacity: 1 }}
            exit={{ opacity: 0, x: -15 }}
            transition={{ duration: 0.3 }}
            className="absolute inset-0 p-4 pt-2 flex flex-col"
        >
            {/* Header */}
            <div className="text-[10px] font-semibold text-text-primary mb-3">Marketplace</div>

            {/* Search */}
            <div className="h-7 rounded-lg border border-border-subtle bg-bg-card/50 px-3 flex items-center mb-3">
                <span className="text-[9px] text-text-muted">Search scripts...</span>
            </div>

            {/* Categories */}
            <div className="flex gap-1.5 mb-4">
                {['All', 'Gathering', 'Fishing'].map((cat, i) => (
                    <span
                        key={cat}
                        className={`text-[8px] px-2 py-0.5 rounded-full border whitespace-nowrap ${i === 0
                            ? 'border-accent-gold/40 text-accent-gold bg-accent-gold/5'
                            : 'border-border-subtle text-text-muted'
                            }`}
                    >
                        {cat}
                    </span>
                ))}
            </div>

            {/* Script list */}
            <div className="flex flex-col gap-2.5 flex-1">
                {scripts.map((script) => {
                    const isFeatured = !!script.featured
                    const dimmed = isHovering && !isFeatured
                    const highlighted = isHovering && isFeatured
                    return (
                        <div
                            key={script.name}
                            className={[
                                'rounded-lg border p-3 transition-all duration-500',
                                highlighted
                                    ? 'border-accent-gold/40 bg-accent-gold/[0.04] shadow-[0_0_24px_rgba(174,164,128,0.06)]'
                                    : 'border-border-subtle bg-bg-card/40',
                                dimmed ? 'opacity-40' : '',
                            ].join(' ')}
                            style={highlighted ? { transform: 'scale(1.01)' } : undefined}
                        >
                            <div className="flex items-start justify-between mb-1">
                                <span className="text-[10px] font-semibold text-text-primary">{script.name}</span>
                                <span className="text-[9px] font-bold text-accent-gold">{script.price}</span>
                            </div>
                            <span className="text-[8px] text-text-muted block mb-1.5">{script.author}</span>
                            <div className="flex items-center justify-between">
                                <div className="flex items-center gap-0.5">
                                    {[1, 2, 3, 4, 5].map((s) => (
                                        <Star
                                            key={s}
                                            size={7}
                                            className={s <= script.rating ? 'text-accent-gold fill-accent-gold' : 'text-border-subtle'}
                                        />
                                    ))}
                                </div>
                                <span className="text-[8px] px-1.5 py-0.5 rounded bg-bg-surface text-text-muted">
                                    {script.tag}
                                </span>
                            </div>
                        </div>
                    )
                })}
            </div>
        </motion.div>
    )
}

/* ── Detail View ── */

function DetailView({ isPurchasing }: { isPurchasing: boolean }) {
    const [clicked, setClicked] = useState(false)

    useEffect(() => {
        if (isPurchasing) {
            const t = setTimeout(() => setClicked(true), 600)
            return () => clearTimeout(t)
        }
        setClicked(false)
    }, [isPurchasing])

    return (
        <motion.div
            initial={{ opacity: 0, x: 15 }}
            animate={{ opacity: 1, x: 0 }}
            exit={{ opacity: 0, scale: 0.98 }}
            transition={{ duration: 0.35 }}
            className="absolute inset-0 p-4 pt-2"
        >
            {/* Back */}
            <div className="flex items-center gap-1 mb-3 text-text-muted">
                <ArrowLeft size={10} />
                <span className="text-[8px]">Back</span>
            </div>

            {/* Header */}
            <div className="flex items-start justify-between mb-2">
                <div>
                    <h3 className="text-[13px] font-bold text-text-primary leading-tight">HarvestKing</h3>
                    <span className="text-[9px] text-text-muted">by @botsmith</span>
                </div>
                <div className="text-right">
                    <span className="text-[15px] font-bold text-accent-gold">
                        €12<span className="text-[9px] font-normal text-text-muted">/mo</span>
                    </span>
                </div>
            </div>

            {/* Rating */}
            <div className="flex items-center gap-0.5 mb-3">
                {[1, 2, 3, 4, 5].map((s) => (
                    <Star
                        key={s}
                        size={9}
                        className={s <= 4 ? 'text-accent-gold fill-accent-gold' : 'text-accent-gold/30 fill-accent-gold/30'}
                    />
                ))}
                <span className="text-[8px] text-text-muted ml-1">4.8 · 2.3k users</span>
            </div>

            <p className="text-[9px] text-text-secondary leading-relaxed mb-4">
                T5–T8 resource harvesting across all royal continent zones. Smart node detection,
                inventory management, and automatic return-to-bank.
            </p>

            {/* Metrics */}
            <div className="grid grid-cols-2 gap-2 mb-4">
                {[
                    { value: '847', label: 'Active Now', Icon: Users },
                    { value: '2.3k', label: 'Total Users', Icon: TrendingUp },
                    { value: '12.4k', label: 'Hours Run', Icon: Clock },
                    { value: '4.8', label: 'Rating', Icon: Star },
                ].map(({ value, label, Icon }) => (
                    <div key={label} className="rounded-lg border border-border-subtle bg-bg-card/50 p-2 text-center">
                        <Icon size={9} className="mx-auto text-accent-gold mb-0.5" />
                        <div className="text-[11px] font-bold text-text-primary">{value}</div>
                        <div className="text-[7px] text-text-muted">{label}</div>
                    </div>
                ))}
            </div>

            {/* Purchase button */}
            <motion.div
                className={`w-full py-2.5 rounded-xl text-[11px] font-semibold text-center transition-colors duration-300 inline-flex items-center justify-center gap-1.5 group ${clicked ? 'bg-accent-green text-bg-primary' : 'text-bg-primary'
                    }`}
                style={!clicked ? { background: 'linear-gradient(135deg, #aea480, #d3c8a8)' } : undefined}
                animate={
                    isPurchasing && !clicked
                        ? { scale: [1, 0.96, 1] }
                        : clicked
                            ? { scale: [0.96, 1.02, 1] }
                            : {}
                }
                transition={{ duration: 0.3 }}
            >
                {clicked ? '✓ Launching...' : (
                    <>
                        Purchase & Go
                        <ButtonArrow size={11} />
                    </>
                )}
            </motion.div>
        </motion.div>
    )
}

/* ── Dashboard View with live metrics ── */

function useTick(intervalMs: number) {
    const [tick, setTick] = useState(0)
    useEffect(() => {
        const id = setInterval(() => setTick((t) => t + 1), intervalMs)
        return () => clearInterval(id)
    }, [intervalMs])
    return tick
}

function formatTime(totalSeconds: number) {
    const h = Math.floor(totalSeconds / 3600)
    const m = Math.floor((totalSeconds % 3600) / 60)
    const s = totalSeconds % 60
    return `${String(h).padStart(2, '0')}:${String(m).padStart(2, '0')}:${String(s).padStart(2, '0')}`
}

function DashboardView() {
    const tick = useTick(1000)
    const baseSeconds = 9257 // 02:34:17
    const sessionTime = formatTime(baseSeconds + tick)
    const resources = 1247 + tick * 3
    const bankTrips = 7 + Math.floor(tick / 4)
    const inventoryUsed = 733 + (tick * 3) % 367
    const inventoryCap = 1100
    const invPct = Math.min((inventoryUsed / inventoryCap) * 100, 99)

    const logEntries = useRef([
        { time: '14:32', msg: 'Gathered T7 Ore ×24', style: 'text-accent-gold' },
        { time: '14:31', msg: 'Navigating to cluster', style: 'text-text-secondary' },
        { time: '14:28', msg: `Bank deposit — Trip #7`, style: 'text-accent-green' },
    ])

    const newMessages = [
        'Gathered T6 Sandstone ×18',
        'Node depleted, moving',
        'Gathered T8 Slate ×4',
        'Navigating to bank',
        `Bank deposit — Trip #${bankTrips}`,
        'Returning to zone',
        'Gathered T7 Travertine ×12',
    ]
    const styles = [
        'text-accent-gold',
        'text-text-secondary',
        'text-accent-gold',
        'text-text-secondary',
        'text-accent-green',
        'text-text-secondary',
        'text-accent-gold',
    ]

    const getLog = useCallback(() => {
        if (tick > 0 && tick % 2 === 0) {
            const idx = Math.floor(tick / 2) % newMessages.length
            const now = new Date()
            now.setMinutes(now.getMinutes() + tick)
            const time = `${String(now.getHours()).padStart(2, '0')}:${String(now.getMinutes()).padStart(2, '0')}`
            return [{ time, msg: newMessages[idx], style: styles[idx] }, ...logEntries.current.slice(0, 2)]
        }
        return logEntries.current
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [tick])

    const logs = getLog()

    return (
        <motion.div
            initial={{ opacity: 0, y: 8 }}
            animate={{ opacity: 1, y: 0 }}
            exit={{ opacity: 0 }}
            transition={{ duration: 0.4 }}
            className="absolute inset-0 p-4 pt-2"
        >
            {/* Header */}
            <div className="flex items-center justify-between mb-3">
                <div className="flex items-center gap-2">
                    <div className="w-6 h-6 rounded-md bg-accent-gold/10 flex items-center justify-center">
                        <Activity size={11} className="text-accent-gold" />
                    </div>
                    <div>
                        <span className="text-[10px] font-bold text-text-primary block leading-tight">HarvestKing</span>
                        <span className="text-[8px] text-text-muted">by @botsmith</span>
                    </div>
                </div>
                <div className="flex items-center gap-1 px-2 py-0.5 rounded-full bg-accent-green/10 border border-accent-green/20">
                    <span className="w-1.5 h-1.5 rounded-full bg-accent-green animate-pulse" />
                    <span className="text-[8px] text-accent-green font-medium">Running</span>
                </div>
            </div>

            {/* Stats */}
            <div className="grid grid-cols-2 gap-1.5 mb-3">
                {[
                    { label: 'Session', value: sessionTime },
                    { label: 'Resources', value: resources.toLocaleString() },
                    { label: 'Bank Trips', value: String(bankTrips) },
                    { label: 'Deaths', value: '0', color: 'text-accent-green' },
                ].map(({ label, value, color }) => (
                    <div key={label} className="rounded-lg border border-border-subtle bg-bg-card/50 p-2">
                        <div className="text-[7px] text-text-muted mb-0.5">{label}</div>
                        <div className={`text-[11px] font-bold font-[family-name:var(--font-family-mono)] ${color ?? 'text-text-primary'}`}>
                            {value}
                        </div>
                    </div>
                ))}
            </div>

            {/* Inventory + Log */}
            <div className="grid grid-cols-2 gap-1.5 mb-3">
                <div className="rounded-lg border border-border-subtle bg-bg-card/50 p-2.5">
                    <div className="text-[8px] text-text-muted mb-1.5 font-medium">Inventory</div>
                    <div className="space-y-1">
                        {[
                            { item: 'T6 Sandstone', qty: 487 + tick * 2 },
                            { item: 'T7 Travertine', qty: 234 + Math.floor(tick * 0.8) },
                            { item: 'T8 Slate', qty: 12 + Math.floor(tick * 0.2) },
                        ].map(({ item, qty }) => (
                            <div key={item} className="flex justify-between text-[8px]">
                                <span className="text-text-secondary">{item}</span>
                                <span className="text-text-primary font-[family-name:var(--font-family-mono)] font-medium">
                                    {qty}
                                </span>
                            </div>
                        ))}
                    </div>
                </div>

                <div className="rounded-lg border border-border-subtle bg-bg-card/50 p-2.5">
                    <div className="text-[8px] text-text-muted mb-1.5 font-medium">Activity</div>
                    <div className="space-y-1">
                        {logs.map((log, i) => (
                            <div key={`${log.time}-${i}`} className="flex items-start gap-1 text-[7px]">
                                <span className="text-text-muted font-[family-name:var(--font-family-mono)] shrink-0">
                                    {log.time}
                                </span>
                                <span className={log.style}>{log.msg}</span>
                            </div>
                        ))}
                    </div>
                </div>
            </div>

            {/* Progress bar */}
            <div className="rounded-lg border border-border-subtle bg-bg-card/50 p-2">
                <div className="flex justify-between text-[7px] mb-1">
                    <span className="text-text-muted">Inventory</span>
                    <span className="text-text-secondary font-[family-name:var(--font-family-mono)]">
                        {Math.min(inventoryUsed, inventoryCap).toLocaleString()} / {inventoryCap.toLocaleString()}
                    </span>
                </div>
                <div className="h-1.5 rounded-full bg-bg-surface overflow-hidden">
                    <motion.div
                        className="h-full rounded-full"
                        style={{ background: 'linear-gradient(90deg, #aea480, #d3c8a8)' }}
                        animate={{ width: `${invPct}%` }}
                        transition={{ duration: 0.8, ease: 'easeOut' }}
                    />
                </div>
            </div>
        </motion.div>
    )
}

/* ── Animated Cursor ── */

const CURSOR_POSITIONS: Record<Phase, { left: string; top: string }> = {
    browse: { left: '60%', top: '20%' },
    hover: { left: '50%', top: '50%' },
    detail: { left: '50%', top: '42%' },
    purchase: { left: '50%', top: '86%' },
    dashboard: { left: '55%', top: '32%' },
}

function CursorOverlay({ phase }: { phase: Phase }) {
    const pos = CURSOR_POSITIONS[phase]
    const isClicking = phase === 'purchase'
    const hideCursor = phase === 'dashboard'

    return (
        <motion.div
            className="absolute z-20 pointer-events-none"
            animate={{
                left: pos.left,
                top: pos.top,
                scale: isClicking ? 0.9 : 1,
                opacity: hideCursor ? 0 : 1,
            }}
            transition={{
                left: { duration: 1, ease: [0.25, 0.1, 0.25, 1] },
                top: { duration: 1, ease: [0.25, 0.1, 0.25, 1] },
                scale: { duration: 0.15, delay: isClicking ? 0.5 : 0 },
                opacity: { duration: 0.4 },
            }}
            style={{ filter: 'drop-shadow(0 2px 6px rgba(0,0,0,0.5))' }}
        >
            <svg width="16" height="20" viewBox="0 0 18 22" fill="none">
                <path
                    d="M1.5 1L1.5 17.5L6 13L9.5 20L12.5 18.5L9 12L15 12L1.5 1Z"
                    fill="white"
                    stroke="#111"
                    strokeWidth="1.2"
                    strokeLinejoin="round"
                />
            </svg>
            <AnimatePresence>
                {isClicking && (
                    <motion.div
                        className="absolute top-0 left-0 w-4 h-4 rounded-full border-2 border-accent-gold/60"
                        initial={{ scale: 0.3, opacity: 1 }}
                        animate={{ scale: 2.5, opacity: 0 }}
                        exit={{ opacity: 0 }}
                        transition={{ duration: 0.6, delay: 0.5 }}
                    />
                )}
            </AnimatePresence>
        </motion.div>
    )
}
