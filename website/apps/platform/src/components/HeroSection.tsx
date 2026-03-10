import { motion } from 'framer-motion'
import { Link } from 'react-router-dom'

export function HeroSection() {
    return (
        <section className="relative min-h-screen flex items-center justify-center pt-24 pb-16 px-6 overflow-hidden">
            {/* Animated Blob Gradients (background) */}
            <div className="moving-gradient-wrapper">
                <div className="blob bg-gold-400 w-[500px] h-[500px] animate-blob top-0 left-1/4" />
                <div className="blob bg-gold-200 w-[400px] h-[400px] animate-blob animation-delay-2000 bottom-[-100px] right-1/4" />
                <div className="blob bg-accent-gold w-[600px] h-[600px] animate-blob animation-delay-4000 top-1/2 left-1/2 -translate-x-1/2" />
            </div>

            <div className="max-w-4xl mx-auto relative z-10 text-center">
                <motion.div
                    initial={{ opacity: 0, y: 30 }}
                    animate={{ opacity: 1, y: 0 }}
                    transition={{ duration: 0.6 }}
                >
                    <span className="inline-block py-1 px-3 rounded-full border border-border-subtle bg-bg-card/50 text-xs font-medium text-gold-300 mb-6 backdrop-blur-sm shadow-sm">
                        OryxBot System v2.0
                    </span>

                    <h1 className="text-4xl md:text-6xl lg:text-7xl font-extrabold text-text-primary mb-6 tracking-tight leading-tight">
                        Undetectable automation <br className="hidden md:block" />
                        built for <span className="text-gradient-gold">Albion.</span>
                    </h1>

                    <p className="text-lg md:text-xl text-text-secondary max-w-2xl mx-auto mb-10 leading-relaxed">
                        Pilot your character natively without client injection. Use the standalone client for automated tasks or script your own logic using the Python API.
                    </p>

                    <div className="flex flex-col sm:flex-row items-center justify-center gap-4">
                        <Link to="/register" className="btn-gold justify-center w-full sm:w-auto h-12 px-8 text-base">
                            Get Started
                        </Link>
                        <a href="#developers" className="btn-outline justify-center w-full sm:w-auto h-12 px-8 text-base">
                            Documentation
                        </a>
                    </div>
                </motion.div>
            </div>
        </section>
    )
}
