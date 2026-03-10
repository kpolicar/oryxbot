import { motion } from 'framer-motion'
import { MapPin, Navigation } from 'lucide-react'

export function PilotSection() {
    return (
        <section className="py-28 px-6 relative overflow-hidden bg-bg-secondary/30">
            <div className="max-w-6xl mx-auto grid grid-cols-1 lg:grid-cols-2 gap-16 items-center">

                {/* Image/Animation Column (Left) */}
                <motion.div
                    initial={{ opacity: 0, x: -30 }}
                    whileInView={{ opacity: 1, x: 0 }}
                    viewport={{ once: true }}
                    transition={{ duration: 0.6, delay: 0.2 }}
                    className="order-2 lg:order-1 relative rounded-2xl border border-border-subtle bg-bg-card p-6 shadow-2xl overflow-hidden aspect-video flex flex-col justify-between"
                >
                    {/* Simplified subtle abstract representation of a map */}
                    <div className="absolute inset-0 opacity-20 pointer-events-none"
                        style={{
                            backgroundImage: 'radial-gradient(circle at 20% 30%, rgba(211,200,168,0.4) 0%, transparent 40%), radial-gradient(circle at 80% 70%, rgba(174,164,128,0.3) 0%, transparent 50%)',
                            filter: 'blur(30px)'
                        }}
                    />

                    {/* Route line */}
                    <div className="absolute top-1/2 left-8 right-8 h-0.5 bg-border-subtle overflow-hidden lg:top-[55%]">
                        <motion.div
                            className="h-full w-full bg-accent-gold"
                            initial={{ x: '-100%' }}
                            animate={{ x: '100%' }}
                            transition={{ repeat: Infinity, duration: 3, ease: 'linear' }}
                        />
                    </div>

                    {/* Nodes */}
                    <div className="relative z-10 flex justify-between items-center h-full pt-12 text-center px-4">
                        <div className="flex flex-col items-center">
                            <div className="w-4 h-4 rounded-full bg-accent-green mb-2 ring-4 ring-accent-green/20" />
                            <span className="text-xs font-medium text-text-secondary font-mono">T5 Zone</span>
                        </div>
                        <div className="relative flex flex-col items-center -top-8">
                            <div className="w-8 h-8 rounded-full bg-accent-gold/20 flex items-center justify-center animate-pulse">
                                <Navigation size={14} className="text-accent-gold rotate-90" />
                            </div>
                        </div>
                        <div className="flex flex-col items-center">
                            <div className="w-4 h-4 rounded-full bg-border-subtle mb-2" />
                            <span className="text-xs font-medium text-text-secondary font-mono">Fort Sterling</span>
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
                </motion.div>

            </div>
        </section>
    )
}
