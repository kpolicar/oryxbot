import { motion } from 'framer-motion'
import { MapPin, Activity, ShieldCheck, MousePointer } from 'lucide-react'

const USER_FEATURES = [
    { icon: <MapPin size={20} />, text: 'Set a destination on the map' },
    { icon: <Activity size={20} />, text: 'Watch real-time status as your character navigates' },
    { icon: <ShieldCheck size={20} />, text: 'Automatic obstacle avoidance and death recovery' },
    { icon: <MousePointer size={20} />, text: 'No coding required' },
]

export function UserSection() {
    return (
        <section className="py-24 px-6">
            <div className="max-w-4xl mx-auto text-center">
                <motion.div
                    initial={{ opacity: 0, y: 20 }}
                    whileInView={{ opacity: 1, y: 0 }}
                    viewport={{ once: true }}
                    transition={{ duration: 0.5 }}
                >
                    <h2 className="text-3xl lg:text-4xl font-bold text-text-primary mb-4">
                        Just Click and <span className="text-accent-gold">Go</span>
                    </h2>
                    <p className="text-lg text-text-secondary mb-12 max-w-2xl mx-auto">
                        Don't want to code? OryxBot Pilot gives you a simple dashboard to autopilot your character
                        anywhere in the world.
                    </p>
                </motion.div>

                <div className="grid grid-cols-1 sm:grid-cols-2 gap-6 max-w-2xl mx-auto">
                    {USER_FEATURES.map((feature, i) => (
                        <motion.div
                            key={feature.text}
                            className="flex items-center gap-4 bg-bg-card border border-border-subtle rounded-xl p-5 hover:border-accent-gold/30 hover:bg-bg-card-hover transition-all"
                            initial={{ opacity: 0, y: 20 }}
                            whileInView={{ opacity: 1, y: 0 }}
                            viewport={{ once: true }}
                            transition={{ duration: 0.4, delay: i * 0.1 }}
                        >
                            <div className="p-2 rounded-lg bg-accent-gold/10 text-accent-gold shrink-0">
                                {feature.icon}
                            </div>
                            <span className="text-sm text-text-secondary text-left">{feature.text}</span>
                        </motion.div>
                    ))}
                </div>

                <motion.div
                    className="mt-12"
                    initial={{ opacity: 0 }}
                    whileInView={{ opacity: 1 }}
                    viewport={{ once: true }}
                    transition={{ delay: 0.5 }}
                >
                    <a
                        href="https://pilot.oryxbot.com"
                        target="_blank"
                        rel="noopener noreferrer"
                        className="inline-flex items-center gap-2 bg-accent-gold text-bg-primary font-medium px-6 py-3 rounded-lg hover:bg-accent-gold-hover transition-colors"
                    >
                        Launch Pilot →
                    </a>
                </motion.div>
            </div>
        </section>
    )
}
