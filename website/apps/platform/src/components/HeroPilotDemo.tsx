import { AlbionMinimap } from '@shared/components/AlbionMinimap'
import { motion } from 'framer-motion'

export function HeroPilotDemo() {
    return (
        <motion.div
            className="flex flex-col"
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ duration: 0.7, ease: 'easeOut' }}
        >
            {/* Text first, minimap below */}
            <div className="mb-6">
                <div className="inline-flex items-center gap-2 px-3 py-1 rounded-full text-[11px] font-medium tracking-wide uppercase mb-5"
                    style={{ background: 'rgba(174, 164, 128, 0.08)', border: '1px solid rgba(174, 164, 128, 0.15)', color: '#d3c8a8' }}
                >
                    <span className="w-1.5 h-1.5 rounded-full bg-accent-green" />
                    Navigation Engine
                </div>
                <h2 className="text-2xl lg:text-3xl font-bold text-text-primary mb-3 leading-tight">
                    OryxBot <span className="text-gradient-gold">Pilot</span>
                </h2>
                <p className="text-text-secondary text-base leading-relaxed mb-4">
                    Click where you want to go. Pilot handles the rest — pathfinding, obstacle avoidance,
                    portal transitions, and death recovery.
                </p>
                <div className="flex flex-wrap gap-2 mb-5">
                    {['Pathfinding', 'Obstacle avoidance', 'Portal transitions', 'Death recovery'].map((f) => (
                        <span key={f}
                            className="text-[11px] font-medium px-2.5 py-1 rounded-md text-gold-300"
                            style={{ background: 'rgba(174, 164, 128, 0.06)', border: '1px solid rgba(174, 164, 128, 0.1)' }}
                        >
                            {f}
                        </span>
                    ))}
                </div>
                <a href="https://pilot.oryxbot.com" target="_blank" rel="noopener noreferrer"
                    className="btn-gold text-sm"
                >
                    Try Pilot →
                </a>
            </div>

            {/* Minimap — smaller, contained */}
            <div className="glow-card overflow-hidden">
                <AlbionMinimap className="w-full aspect-square max-w-[340px] mx-auto" />
            </div>
        </motion.div>
    )
}
