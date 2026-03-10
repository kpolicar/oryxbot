import { useState, useCallback } from 'react'
import { motion } from 'framer-motion'
import { Search, Filter } from 'lucide-react'
import { MarketplaceGrid } from '../components/MarketplaceGrid'
import { MARKETPLACE_ENTRIES, MARKETPLACE_CATEGORIES } from '../data/mockMarketplace'

export default function Marketplace() {
    const [activeCategory, setActiveCategory] = useState('All')
    const [toast, setToast] = useState<string | null>(null)

    const filtered =
        activeCategory === 'All'
            ? MARKETPLACE_ENTRIES
            : MARKETPLACE_ENTRIES.filter((e) => e.category === activeCategory)

    const handleSubscribe = useCallback((name: string) => {
        setToast(`"${name}" — Coming soon!`)
        setTimeout(() => setToast(null), 3000)
    }, [])

    return (
        <div className="min-h-screen pt-24 pb-16 px-6">
            <div className="max-w-6xl mx-auto">
                {/* Header */}
                <motion.div
                    className="mb-10"
                    initial={{ opacity: 0, y: 20 }}
                    animate={{ opacity: 1, y: 0 }}
                    transition={{ duration: 0.5 }}
                >
                    <h1 className="text-3xl lg:text-4xl font-bold text-text-primary mb-3">
                        <span className="text-accent-gold">Marketplace</span>
                    </h1>
                    <p className="text-text-secondary">
                        Browse community-built scripts powered by OryxBot Pilot.
                    </p>
                </motion.div>

                {/* Filter bar */}
                <div className="flex flex-wrap items-center gap-3 mb-8">
                    <Filter size={16} className="text-text-muted" />
                    {MARKETPLACE_CATEGORIES.map((cat) => (
                        <button
                            key={cat}
                            onClick={() => setActiveCategory(cat)}
                            className={`text-sm px-4 py-1.5 rounded-full border transition-colors cursor-pointer ${activeCategory === cat
                                    ? 'bg-accent-gold/10 border-accent-gold/30 text-accent-gold'
                                    : 'border-border-subtle text-text-muted hover:text-text-secondary hover:border-text-muted'
                                }`}
                        >
                            {cat}
                        </button>
                    ))}
                </div>

                {/* Grid */}
                <MarketplaceGrid entries={filtered} onSubscribe={handleSubscribe} />

                {/* Toast */}
                {toast && (
                    <motion.div
                        className="fixed bottom-6 right-6 bg-bg-card border border-border-subtle rounded-xl px-5 py-3 shadow-lg"
                        initial={{ opacity: 0, y: 20 }}
                        animate={{ opacity: 1, y: 0 }}
                        exit={{ opacity: 0 }}
                    >
                        <span className="text-sm text-text-secondary">{toast}</span>
                    </motion.div>
                )}
            </div>
        </div>
    )
}
