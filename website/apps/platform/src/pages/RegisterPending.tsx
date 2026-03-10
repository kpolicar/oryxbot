import { motion } from 'framer-motion'
import { CheckCircle, ArrowRight } from 'lucide-react'
import { Link } from 'react-router-dom'

export default function RegisterPending() {
    return (
        <div className="min-h-screen pt-24 pb-16 px-6 flex items-center justify-center">
            <motion.div
                className="max-w-md w-full text-center"
                initial={{ opacity: 0, scale: 0.95 }}
                animate={{ opacity: 1, scale: 1 }}
                transition={{ duration: 0.5 }}
            >
                <div className="mx-auto w-16 h-16 rounded-full bg-accent-gold/10 flex items-center justify-center mb-6">
                    <CheckCircle size={32} className="text-accent-gold" />
                </div>

                <h1 className="text-2xl font-bold text-text-primary mb-3">You're on the list!</h1>
                <p className="text-text-secondary leading-relaxed mb-6">
                    Your request to join OryxBot has been submitted for review. We're currently selective about who
                    gets access — we'll notify you by email once your application is approved.
                </p>

                <div className="flex flex-col sm:flex-row items-center justify-center gap-4">
                    <Link
                        to="/#developers"
                        className="inline-flex items-center gap-2 text-sm text-accent-gold hover:text-accent-gold-hover transition-colors"
                    >
                        Explore documentation <ArrowRight size={14} />
                    </Link>
                    <a
                        href="https://pilot.oryxbot.com"
                        target="_blank"
                        rel="noopener noreferrer"
                        className="inline-flex items-center gap-2 text-sm text-text-muted hover:text-text-secondary transition-colors"
                    >
                        Check out Pilot <ArrowRight size={14} />
                    </a>
                </div>
            </motion.div>
        </div>
    )
}
