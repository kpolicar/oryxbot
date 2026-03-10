import { motion } from 'framer-motion'
import { ButtonArrow } from '@shared/components/ButtonArrow'
import { BookLock } from 'lucide-react'
import { Link } from 'react-router-dom'

export default function Docs() {
    return (
        <div className="min-h-screen pt-32 pb-16 px-6 relative overflow-hidden flex flex-col items-center justify-center">
            {/* Background Gradients */}
            <div className="absolute inset-0 pointer-events-none">
                <div className="absolute top-1/4 left-1/4 w-96 h-96 bg-accent-gold/5 rounded-full blur-[120px] mix-blend-screen" />
                <div className="absolute bottom-1/4 right-1/4 w-[500px] h-[500px] bg-accent-gold/5 rounded-full blur-[150px] mix-blend-screen" />
            </div>

            <motion.div
                className="relative z-10 max-w-lg w-full text-center"
                initial={{ opacity: 0, scale: 0.95, y: 20 }}
                animate={{ opacity: 1, scale: 1, y: 0 }}
                transition={{ duration: 0.5 }}
            >
                <div className="w-20 h-20 mx-auto rounded-3xl bg-bg-card border border-border-subtle flex items-center justify-center mb-8 shadow-2xl relative overflow-hidden">
                    <div className="absolute inset-0 bg-gradient-to-br from-accent-gold/20 to-transparent opacity-50" />
                    <BookLock className="text-accent-gold relative z-10" size={32} />
                </div>

                <h1 className="text-4xl font-bold text-text-primary mb-4 tracking-tight">
                    Restricted Area
                </h1>

                <p className="text-text-secondary text-lg mb-10 leading-relaxed">
                    Documentation is available to active subscribers only. Please log in or subscribe to access the developer docs and API reference.
                </p>

                <div className="flex items-center justify-center gap-4">
                    <Link to="/login" className="btn-outline group">
                        Sign In
                        <ButtonArrow />
                    </Link>
                    <Link to="/register" className="btn-gold group">
                        Get Access
                        <ButtonArrow />
                    </Link>
                </div>
            </motion.div>
        </div>
    )
}
