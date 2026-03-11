import { motion } from 'framer-motion'
import { CheckCircle, ArrowRight } from 'lucide-react'
import { Link } from 'react-router-dom'
import { useEffect } from 'react'
import { trackSignUp } from '../lib/analytics'

export default function RegisterPending() {
    useEffect(() => {
        trackSignUp()
    }, [])
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
            </motion.div>
        </div>
    )
}
