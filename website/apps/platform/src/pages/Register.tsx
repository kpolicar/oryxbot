import { SignUp } from '@clerk/clerk-react'
import { useState } from 'react'
import { motion } from 'framer-motion'
import { ShieldAlert } from 'lucide-react'
import { RolePicker } from '../components/RolePicker'
import { clerkAppearance } from '../lib/clerkAppearance'

export default function Register() {
    const [selectedRole, setSelectedRole] = useState<'developer' | 'user' | null>(null)

    return (
        <div className="min-h-screen pt-24 pb-16 px-6 flex items-center justify-center">
            <div className="w-full max-w-4xl">
                {/* Beta notice */}
                <motion.div
                    className="bg-accent-gold/5 border border-accent-gold/20 rounded-xl p-5 mb-10"
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

                {/* 2-column layout */}
                <div className="grid grid-cols-1 lg:grid-cols-2 gap-12 items-start">
                    {/* Left: Role selection */}
                    <motion.div
                        initial={{ opacity: 0, y: 20 }}
                        animate={{ opacity: 1, y: 0 }}
                        transition={{ duration: 0.5, delay: 0.1 }}
                    >
                        <h2 className="text-xl font-bold text-text-primary mb-2">State your intent</h2>
                        <p className="text-sm text-text-muted mb-6">Select how you plan to use OryxBot.</p>
                        <RolePicker selected={selectedRole} onSelect={setSelectedRole} />
                    </motion.div>

                    {/* Right: Clerk SignUp */}
                    <motion.div
                        initial={{ opacity: 0, y: 20 }}
                        animate={{ opacity: 1, y: 0 }}
                        transition={{ duration: 0.5, delay: 0.2 }}
                        className="flex flex-col items-center"
                    >
                        {selectedRole ? (
                            <motion.div
                                key="signup-form"
                                initial={{ opacity: 0, y: 10 }}
                                animate={{ opacity: 1, y: 0 }}
                                transition={{ duration: 0.3 }}
                            >
                                <SignUp routing="hash" appearance={clerkAppearance} signInUrl="/login" afterSignUpUrl="/register/pending" />
                            </motion.div>
                        ) : (
                            <div className="w-full rounded-xl border border-border-subtle bg-bg-card p-10 text-center text-text-muted text-sm">
                                Select a role on the left to continue.
                            </div>
                        )}
                    </motion.div>
                </div>
            </div>
        </div>
    )
}
