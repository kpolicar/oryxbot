import { AlbionMinimap } from '@shared/components/AlbionMinimap'
import { motion } from 'framer-motion'

export function HeroPilotDemo() {
    return (
        <motion.div
            className="flex flex-col h-full"
            initial={{ opacity: 0, x: -30 }}
            animate={{ opacity: 1, x: 0 }}
            transition={{ duration: 0.6, ease: 'easeOut' }}
        >
            {/* Minimap */}
            <div className="relative flex-1 min-h-[320px] lg:min-h-[420px]">
                <AlbionMinimap className="w-full h-full aspect-square" />
            </div>

            {/* Text overlay */}
            <div className="mt-6 space-y-3">
                <h2 className="text-2xl lg:text-3xl font-bold text-text-primary">
                    OryxBot <span className="text-accent-gold">Pilot</span>
                </h2>
                <p className="text-text-secondary text-lg">
                    Click where you want to go. Pilot handles the rest.
                </p>
                <div className="flex flex-wrap gap-2">
                    {['Pathfinding', 'Obstacle avoidance', 'Portal transitions', 'Death recovery'].map((feature) => (
                        <span
                            key={feature}
                            className="text-xs font-medium px-3 py-1 rounded-full bg-accent-gold/10 text-accent-gold border border-accent-gold/20"
                        >
                            {feature}
                        </span>
                    ))}
                </div>
                <a
                    href="https://pilot.oryxbot.com"
                    target="_blank"
                    rel="noopener noreferrer"
                    className="inline-flex items-center gap-2 text-accent-gold hover:text-accent-gold-hover font-medium mt-2 transition-colors"
                >
                    Try Pilot →
                </a>
            </div>
        </motion.div>
    )
}
