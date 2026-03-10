import { motion } from 'framer-motion'
import { ShieldAlert } from 'lucide-react'
import { Link } from 'react-router-dom'

export default function Login() {
    return (
        <div className="min-h-screen pt-24 pb-16 px-6 flex items-center justify-center">
            <motion.div
                className="max-w-md w-full"
                initial={{ opacity: 0, y: 20 }}
                animate={{ opacity: 1, y: 0 }}
                transition={{ duration: 0.5 }}
            >
                {/* Access notice */}
                <div className="bg-bg-secondary border border-border-subtle rounded-xl p-4 mb-8 flex items-start gap-3">
                    <ShieldAlert size={18} className="text-accent-gold shrink-0 mt-0.5" />
                    <p className="text-sm text-text-secondary">
                        <span className="font-medium text-text-primary">Access is currently limited.</span>{' '}
                        Only approved members can log in. If you haven't been accepted yet, you'll be redirected
                        after sign-in.
                    </p>
                </div>

                {/* Clerk SignIn placeholder */}
                <div className="bg-bg-card border border-border-subtle rounded-xl p-8 text-center">
                    <p className="text-text-muted text-sm mb-2">Clerk SignIn component goes here</p>
                    <p className="text-xs text-text-muted">
                        Add <code className="text-accent-gold font-family-mono">VITE_CLERK_PUBLISHABLE_KEY</code> to{' '}
                        <code className="text-accent-gold font-family-mono">.env</code> to enable authentication.
                    </p>
                </div>

                {/* Register link */}
                <p className="text-center mt-6 text-sm text-text-muted">
                    Don't have an account?{' '}
                    <Link to="/register" className="text-accent-gold hover:text-accent-gold-hover transition-colors">
                        Register →
                    </Link>
                </p>
            </motion.div>
        </div>
    )
}
