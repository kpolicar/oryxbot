import { motion } from 'framer-motion'
import { ButtonArrow } from '@shared/components/ButtonArrow'
import { Link } from 'react-router-dom'
import { AlbionMap } from './AlbionMap'

export function HeroSection() {
    return (
        <section className="relative min-h-screen flex items-center overflow-hidden">
            {/* Albion Map Background — fills entire hero, Royal Continent docked right */}
            <div className="absolute inset-0 z-0">
                <AlbionMap
                    zoom={3}
                    center={[-210, 90]}
                    interactive={false}
                    showOverlays={false}
                    showMarkers={false}
                    showEdges={false}
                    botCount={15}
                    style={{ width: '100%', height: '100%' }}
                />
                {/* Left fade gradient — blends map into dark bg so text is readable */}
                <div
                    className="absolute inset-0 pointer-events-none"
                    style={{
                        background: `linear-gradient(
                            to right,
                            #0a0b0d 0%,
                            #0a0b0d 25%,
                            rgba(10, 11, 13, 0.95) 35%,
                            rgba(10, 11, 13, 0.7) 50%,
                            rgba(10, 11, 13, 0.3) 65%,
                            transparent 80%
                        )`
                    }}
                />
                {/* Top fade */}
                <div
                    className="absolute inset-x-0 top-0 h-32 pointer-events-none"
                    style={{
                        background: 'linear-gradient(to bottom, #0a0b0d, transparent)'
                    }}
                />
                {/* Bottom fade */}
                <div
                    className="absolute inset-x-0 bottom-0 h-32 pointer-events-none"
                    style={{
                        background: 'linear-gradient(to top, #0a0b0d, transparent)'
                    }}
                />
            </div>

            {/* Hero Content — left-aligned */}
            <div className="relative z-10 w-full max-w-7xl mx-auto px-6 md:px-12 pt-24 pb-16">
                <motion.div
                    initial={{ opacity: 0, x: -30 }}
                    animate={{ opacity: 1, x: 0 }}
                    transition={{ duration: 0.6 }}
                    className="max-w-xl"
                >
                    <span className="inline-block py-1 px-3 rounded-full border border-border-subtle bg-bg-card/50 text-xs font-medium text-gold-300 mb-6 backdrop-blur-sm shadow-sm">
                        OryxBot Platform
                    </span>

                    <h1 className="text-4xl md:text-6xl lg:text-7xl font-extrabold text-text-primary mb-6 tracking-tight leading-tight">
                        Undetectable automation <br className="hidden md:block" />
                        built for <span className="text-gradient-gold">Albion.</span>
                    </h1>

                    <p className="text-lg md:text-xl text-text-secondary max-w-lg mb-10 leading-relaxed">
                        Pilot your character natively without client injection. Use the community scripts or write your own logic using our Python SDK.
                    </p>

                    <div className="flex flex-col sm:flex-row items-start gap-4">
                        <Link to="/register" className="btn-gold justify-center w-full sm:w-auto h-12 px-8 text-base group">
                            Get Started
                            <ButtonArrow />
                        </Link>
                    </div>
                </motion.div>
            </div>
        </section>
    )
}
