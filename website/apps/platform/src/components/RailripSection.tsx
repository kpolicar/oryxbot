import { RailripDiagram } from '@shared/components/RailripDiagram'
import { motion } from 'framer-motion'

export function RailripSection() {
    return (
        <section className="py-28 px-6 relative overflow-hidden">
            {/* Subtle radial gradient background */}
            <div className="absolute inset-0 pointer-events-none"
                style={{ background: 'radial-gradient(ellipse at 30% 50%, rgba(174, 164, 128, 0.03) 0%, transparent 60%)' }}
            />

            <div className="max-w-5xl mx-auto relative">
                <motion.div
                    className="mb-14"
                    initial={{ opacity: 0, y: 20 }}
                    whileInView={{ opacity: 1, y: 0 }}
                    viewport={{ once: true }}
                    transition={{ duration: 0.5 }}
                >
                    <span className="text-[11px] font-medium tracking-[0.15em] uppercase text-gold-400 block mb-3">
                        Architecture
                    </span>
                    <h2 className="text-3xl lg:text-4xl font-bold text-text-primary leading-tight">
                        Undetectable by design
                    </h2>
                </motion.div>

                <RailripDiagram />
            </div>
        </section>
    )
}
