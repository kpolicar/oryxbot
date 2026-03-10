import { useState } from 'react'
import { motion } from 'framer-motion'
import { ShieldAlert } from 'lucide-react'
import { RolePicker } from '../components/RolePicker'

export default function Register() {
    const [selectedRole, setSelectedRole] = useState<'developer' | 'user' | null>(null)

    return (
        <div className="min-h-screen pt-24 pb-16 px-6 flex items-center justify-center">
            <div className="max-w-lg w-full">
                {/* Beta notice */}
                <motion.div
                    className="bg-accent-gold/5 border border-accent-gold/20 rounded-xl p-5 mb-8"
                    initial={{ opacity: 0, y: 20 }}
                    animate={{ opacity: 1, y: 0 }}
                    transition={{ duration: 0.5 }}
                >
                    <div className="flex items-start gap-3">
                        <ShieldAlert size={20} className="text-accent-gold shrink-0 mt-0.5" />
                        <div>
                            <h3 className="font-semibold text-text-primary mb-1">OryxBot is currently in closed beta.</h3>
                            <p className="text-sm text-text-secondary leading-relaxed">
                                We're selective about who gets access. Registration submits a request to join — all
                                applications are reviewed by our team and you'll be notified by email if you're accepted.
                            </p>
                        </div>
                    </div>
                </motion.div>

                {/* Step 1: Role Selection */}
                <motion.div
                    initial={{ opacity: 0, y: 20 }}
                    animate={{ opacity: 1, y: 0 }}
                    transition={{ duration: 0.5, delay: 0.1 }}
                >
                    <h2 className="text-xl font-bold text-text-primary mb-2">Choose your role</h2>
                    <p className="text-sm text-text-muted mb-6">Select how you plan to use OryxBot.</p>

                    <RolePicker selected={selectedRole} onSelect={setSelectedRole} />
                </motion.div>

                {/* Step 2: Clerk SignUp placeholder */}
                {selectedRole && (
                    <motion.div
                        className="mt-8"
                        initial={{ opacity: 0, y: 20 }}
                        animate={{ opacity: 1, y: 0 }}
                        transition={{ duration: 0.4 }}
                    >
                        <div className="bg-bg-card border border-border-subtle rounded-xl p-8 text-center">
                            <p className="text-text-muted text-sm mb-2">Clerk SignUp component goes here</p>
                            <p className="text-xs text-text-muted">
                                Add <code className="text-accent-gold font-family-mono">VITE_CLERK_PUBLISHABLE_KEY</code> to{' '}
                                <code className="text-accent-gold font-family-mono">.env</code> to enable authentication.
                            </p>
                        </div>
                    </motion.div>
                )}
            </div>
        </div>
    )
}
