import { motion } from 'framer-motion'
import { Star } from 'lucide-react'

export interface MarketplaceEntry {
    name: string
    author: string
    price: string
    description: string
    rating: number
    category: string
}

interface MarketplaceGridProps {
    entries: MarketplaceEntry[]
    onSubscribe?: (name: string) => void
}

export function MarketplaceGrid({ entries, onSubscribe }: MarketplaceGridProps) {
    return (
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-6">
            {entries.map((entry, i) => (
                <motion.div
                    key={entry.name}
                    className="bg-bg-card border border-border-subtle rounded-xl p-6 flex flex-col hover:border-accent-gold/30 hover:bg-bg-card-hover transition-all group"
                    initial={{ opacity: 0, y: 20 }}
                    animate={{ opacity: 1, y: 0 }}
                    transition={{ duration: 0.4, delay: i * 0.05 }}
                >
                    <div className="flex items-start justify-between mb-3">
                        <h3 className="font-semibold text-text-primary group-hover:text-accent-gold transition-colors">
                            {entry.name}
                        </h3>
                        <span className="text-accent-gold font-bold text-sm whitespace-nowrap ml-2">{entry.price}</span>
                    </div>

                    <p className="text-xs text-text-muted mb-1">by {entry.author}</p>

                    {/* Rating */}
                    <div className="flex items-center gap-0.5 mb-3">
                        {Array.from({ length: 5 }).map((_, idx) => (
                            <Star
                                key={idx}
                                size={12}
                                className={idx < entry.rating ? 'text-accent-gold fill-accent-gold' : 'text-border-subtle'}
                            />
                        ))}
                    </div>

                    <p className="text-sm text-text-secondary mb-6 flex-1 leading-relaxed">{entry.description}</p>

                    <button
                        onClick={() => onSubscribe?.(entry.name)}
                        className="w-full py-2.5 rounded-lg border border-accent-gold/30 text-accent-gold text-sm font-medium hover:bg-accent-gold/10 transition-colors cursor-pointer"
                    >
                        Subscribe
                    </button>
                </motion.div>
            ))}
        </div>
    )
}
