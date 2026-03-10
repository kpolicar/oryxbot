import { RailripDiagram } from '@shared/components/RailripDiagram'
import { motion } from 'framer-motion'
import { Shield } from 'lucide-react'

export function RailripSection() {
    return (
        <section className="py-24 px-6">
            <div className="max-w-6xl mx-auto">
                <motion.div
                    className="text-center mb-16"
                    initial={{ opacity: 0, y: 20 }}
                    whileInView={{ opacity: 1, y: 0 }}
                    viewport={{ once: true }}
                    transition={{ duration: 0.5 }}
                >
                    <div className="inline-flex items-center gap-2 px-4 py-1.5 rounded-full bg-accent-gold/10 border border-accent-gold/20 mb-6">
                        <Shield size={14} className="text-accent-gold" />
                        <span className="text-sm font-medium text-accent-gold">Undetectable by Design</span>
                    </div>
                    <h2 className="text-3xl lg:text-4xl font-bold text-text-primary">
                        Powered by{' '}
                        <a
                            href="https://railrip.com"
                            target="_blank"
                            rel="noopener noreferrer"
                            className="text-accent-gold hover:text-accent-gold-hover underline underline-offset-4 decoration-accent-gold/30"
                        >
                            RailRip
                        </a>
                    </h2>
                </motion.div>

                <RailripDiagram />
            </div>
        </section>
    )
}
