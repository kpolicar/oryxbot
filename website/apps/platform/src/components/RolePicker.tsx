import { motion } from 'framer-motion'
import { Code, Gamepad2 } from 'lucide-react'

interface RolePickerProps {
    selected: 'developer' | 'user' | null
    onSelect: (role: 'developer' | 'user') => void
}

const roles = [
    {
        id: 'developer' as const,
        icon: <Code size={24} />,
        title: 'Developer',
        description: "I want to build scripts and sell on the marketplace",
    },
    {
        id: 'user' as const,
        icon: <Gamepad2 size={24} />,
        title: 'User',
        description: "I want to use Pilot and marketplace scripts",
    },
]

export function RolePicker({ selected, onSelect }: RolePickerProps) {
    return (
        <div className="grid grid-cols-1 gap-4">
            {roles.map((role) => (
                <motion.button
                    key={role.id}
                    onClick={() => onSelect(role.id)}
                    className={`relative p-6 rounded-xl border-2 text-left transition-all cursor-pointer ${selected === role.id
                            ? 'border-accent-gold bg-accent-gold/5'
                            : 'border-border-subtle bg-bg-card hover:border-accent-gold/30 hover:bg-bg-card-hover'
                        }`}
                    whileHover={{ scale: 1.02 }}
                    whileTap={{ scale: 0.98 }}
                >
                    {/* Selection radio */}
                    <div
                        className={`absolute top-4 right-4 w-5 h-5 rounded-full border-2 flex items-center justify-center ${selected === role.id ? 'border-accent-gold' : 'border-border-subtle'
                            }`}
                    >
                        {selected === role.id && (
                            <motion.div
                                className="w-3 h-3 rounded-full bg-accent-gold"
                                initial={{ scale: 0 }}
                                animate={{ scale: 1 }}
                                transition={{ type: 'spring', duration: 0.3 }}
                            />
                        )}
                    </div>

                    <div className={`p-2.5 rounded-lg inline-flex mb-3 ${selected === role.id ? 'bg-accent-gold/15 text-accent-gold' : 'bg-bg-card-hover text-text-secondary'
                        }`}>
                        {role.icon}
                    </div>
                    <h3 className="font-semibold text-text-primary mb-1">{role.title}</h3>
                    <p className="text-sm text-text-secondary">{role.description}</p>
                </motion.button>
            ))}
        </div>
    )
}
