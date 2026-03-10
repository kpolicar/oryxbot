import { useState, useCallback } from 'react'
import { Link } from 'react-router-dom'
import { motion, AnimatePresence } from 'framer-motion'
import { MapPin, Navigation } from 'lucide-react'
import { AlbionMinimap, type MinimapState } from '@shared/components/AlbionMinimap'
import { ButtonArrow } from '@shared/components/ButtonArrow'

const ALL_STATES = [
    'Idle',
    'FollowingRoute',
    'CorrectingCourse',
    'Unsticking',
    'Transitioning',
    'Evading',
    'Lost',
    'Killed',
    'Disconnected',
] as const

const STATE_COLORS: Record<string, string> = {
    Idle: '#6b6b76',
    FollowingRoute: '#34d399',
    CorrectingCourse: '#fbbf24',
    Unsticking: '#f97316',
    Transitioning: '#60a5fa',
    Evading: '#f87171',
    Lost: '#f87171',
    Killed: '#ef4444',
    Disconnected: '#6b6b76',
}

function StatusPanel({ state }: { state: MinimapState }) {
    const stateColor = STATE_COLORS[state.state] || '#6b6b76'

    return (
        <div
            className="rounded-xl overflow-hidden h-full flex flex-col"
            style={{
                background: 'rgba(21, 23, 28, 0.95)',
                border: '1px solid rgba(255,255,255,0.06)',
            }}
        >
            {/* Header */}
            <div className="px-4 py-3 flex items-center gap-2"
                style={{ borderBottom: '1px solid rgba(255,255,255,0.06)' }}
            >
                <div className="w-2 h-2 rounded-full bg-accent-green animate-pulse" />
                <span className="text-[11px] font-semibold tracking-widest uppercase text-gold-400"
                    style={{ fontFamily: 'var(--font-mono)' }}
                >
                    Pilot Status
                </span>
            </div>

            {/* Body */}
            <div className="px-4 py-3 flex-1 space-y-3 overflow-hidden">
                {/* State */}
                <div>
                    <span className="text-[9px] font-medium uppercase tracking-wider text-text-muted block mb-1">
                        State
                    </span>
                    <AnimatePresence mode="wait">
                        <motion.div
                            key={state.state}
                            initial={{ opacity: 0, x: -8 }}
                            animate={{ opacity: 1, x: 0 }}
                            exit={{ opacity: 0, x: 8 }}
                            transition={{ duration: 0.15 }}
                            className="flex items-center gap-2"
                        >
                            <span className="w-1.5 h-1.5 rounded-full" style={{ background: stateColor }} />
                            <span className="text-sm font-semibold text-text-primary" style={{ fontFamily: 'var(--font-mono)' }}>
                                {state.state}
                            </span>
                        </motion.div>
                    </AnimatePresence>
                </div>

                {/* Position */}
                <div>
                    <span className="text-[9px] font-medium uppercase tracking-wider text-text-muted block mb-1">
                        Position
                    </span>
                    <span className="text-xs text-text-secondary" style={{ fontFamily: 'var(--font-mono)' }}>
                        ({state.position.x.toFixed(1)}, {state.position.y.toFixed(1)})
                    </span>
                </div>

                {/* Heading */}
                <div>
                    <span className="text-[9px] font-medium uppercase tracking-wider text-text-muted block mb-1">
                        Heading
                    </span>
                    <span className="text-xs text-text-secondary" style={{ fontFamily: 'var(--font-mono)' }}>
                        {state.heading.toFixed(0)}°
                    </span>
                </div>

                {/* Destination */}
                <div>
                    <span className="text-[9px] font-medium uppercase tracking-wider text-text-muted block mb-1">
                        Destination
                    </span>
                    <AnimatePresence mode="wait">
                        <motion.span
                            key={state.destination || 'none'}
                            initial={{ opacity: 0 }}
                            animate={{ opacity: 1 }}
                            exit={{ opacity: 0 }}
                            className="text-xs font-medium text-gold-300 block"
                            style={{ fontFamily: 'var(--font-mono)' }}
                        >
                            {state.destination || '—'}
                        </motion.span>
                    </AnimatePresence>
                </div>

                {/* Progress bar */}
                <div>
                    <span className="text-[9px] font-medium uppercase tracking-wider text-text-muted block mb-1">
                        Progress
                    </span>
                    <div className="h-1.5 rounded-full overflow-hidden" style={{ background: 'rgba(255,255,255,0.06)' }}>
                        <motion.div
                            className="h-full rounded-full"
                            style={{ background: stateColor }}
                            animate={{ width: `${Math.max(Math.round(state.progress * 100), 0)}%` }}
                            transition={{ duration: 0.1 }}
                        />
                    </div>
                    <span className="text-[9px] text-text-muted mt-0.5 block" style={{ fontFamily: 'var(--font-mono)' }}>
                        {Math.round(state.progress * 100)}%
                    </span>
                </div>

                {/* All states reference */}
                <div className="pt-2" style={{ borderTop: '1px solid rgba(255,255,255,0.04)' }}>
                    <span className="text-[9px] font-medium uppercase tracking-wider text-text-muted block mb-1.5">
                        All States
                    </span>
                    <div className="flex flex-wrap gap-1">
                        {ALL_STATES.map((s) => (
                            <span
                                key={s}
                                className="text-[8px] px-1.5 py-0.5 rounded"
                                style={{
                                    fontFamily: 'var(--font-mono)',
                                    background: s === state.state ? `${STATE_COLORS[s]}18` : 'rgba(255,255,255,0.03)',
                                    color: s === state.state ? STATE_COLORS[s] : '#4a4a54',
                                    border: s === state.state ? `1px solid ${STATE_COLORS[s]}30` : '1px solid transparent',
                                }}
                            >
                                {s}
                            </span>
                        ))}
                    </div>
                </div>
            </div>
        </div>
    )
}

export function PilotSection() {
    const [minimapState, setMinimapState] = useState<MinimapState>({
        position: { x: 72, y: 22 },
        heading: 0,
        state: 'Idle',
        destination: '',
        progress: 0,
    })

    const handleStateChange = useCallback((s: MinimapState) => {
        setMinimapState(s)
    }, [])

    return (
        <section className="py-28 px-6 relative overflow-hidden bg-bg-secondary/30">
            <div className="max-w-[90rem] mx-auto grid grid-cols-1 lg:grid-cols-2 gap-16 items-center">

                {/* Minimap + Status Panel (Left) */}
                <motion.div
                    initial={{ opacity: 0, x: -30 }}
                    whileInView={{ opacity: 1, x: 0 }}
                    viewport={{ once: true }}
                    transition={{ duration: 0.6, delay: 0.2 }}
                    className="order-2 lg:order-1"
                >
                    <div className="flex items-start">
                        {/* Status panel (left, overlapping minimap) */}
                        <div className="w-64 shrink-0 -mr-8 mt-10 relative z-10">
                            <StatusPanel state={minimapState} />
                        </div>
                        {/* Diamond minimap */}
                        <div className="flex-1 min-w-0">
                            <AlbionMinimap
                                className="w-full"
                                onStateChange={handleStateChange}
                            />
                        </div>
                    </div>
                </motion.div>

                {/* Text Column (Right) */}
                <motion.div
                    initial={{ opacity: 0, x: 30 }}
                    whileInView={{ opacity: 1, x: 0 }}
                    viewport={{ once: true }}
                    transition={{ duration: 0.6 }}
                    className="order-1 lg:order-2 text-left"
                >
                    <span className="text-[11px] font-medium tracking-[0.15em] uppercase text-gold-400 block mb-3">
                        Pilot Standalone
                    </span>
                    <h2 className="text-3xl lg:text-5xl font-bold text-text-primary mb-6 leading-tight">
                        Navigate the world <br /> <span className="text-gradient-gold">hands-free.</span>
                    </h2>
                    <p className="text-text-secondary text-lg leading-relaxed mb-6">
                        OryxBot Pilot provides seamless, intelligent navigation across all Royal Continents.
                        Simply select your destination and let the standalone client handle the entire journey.
                    </p>

                    <ul className="space-y-4">
                        <li className="flex items-start gap-4">
                            <div className="p-2 rounded-lg bg-accent-gold/10 text-gold-300 mt-1">
                                <MapPin size={16} />
                            </div>
                            <div>
                                <h4 className="font-semibold text-text-primary text-sm mb-1">Smart Routing</h4>
                                <p className="text-xs text-text-muted leading-relaxed">
                                    Avoid dangerous zones or calculate the fastest path based on your exact routing preferences.
                                </p>
                            </div>
                        </li>
                        <li className="flex items-start gap-4">
                            <div className="p-2 rounded-lg bg-accent-gold/10 text-gold-300 mt-1">
                                <Navigation size={16} />
                            </div>
                            <div>
                                <h4 className="font-semibold text-text-primary text-sm mb-1">Obstacle Avoidance</h4>
                                <p className="text-xs text-text-muted leading-relaxed">
                                    Dynamic pathfinding automatically navigates around terrain, mobs, and other players in real-time.
                                </p>
                            </div>
                        </li>
                    </ul>

                    <div className="mt-8">
                        <Link
                            to="/pilot"
                            className="btn-gold text-sm group"
                        >
                            Try out Pilot
                            <ButtonArrow />
                        </Link>
                    </div>
                </motion.div>

            </div>
        </section>
    )
}
