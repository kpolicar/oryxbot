import { motion } from 'framer-motion'
import { ButtonArrow } from '@shared/components/ButtonArrow'
import { Star, Check } from 'lucide-react'

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

/**
 * Masonry column layout mimicking Clerk's equal-width homepage cards.
 */
export function MarketplaceGrid({ entries, onSubscribe }: MarketplaceGridProps) {
    // Break into 3 columns for masonry stagger matching Clerk's homepage
    const columns: MarketplaceEntry[][] = [[], [], []]
    entries.forEach((entry, i) => columns[i % 3].push(entry))

    return (
        <div className="w-full max-w-[1200px] mx-auto">
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6 items-start">
                {columns.map((col, colIndex) => (
                    <div
                        key={colIndex}
                        className={`flex flex-col gap-6 ${colIndex === 1 ? 'lg:mt-24' : ''
                            }`}
                    >
                        {col.map((entry, i) => {
                            const globalIndex = entries.indexOf(entry)
                            const priceParts = entry.price.split('/')
                            const priceAmount = priceParts[0]
                            const pricePeriod = priceParts[1] || 'mo'

                            // Splitting description to simulate a list of features for the checklist
                            const features = entry.description.split('. ').filter(f => f.trim().length > 0)

                            return (
                                <motion.div
                                    key={entry.name}
                                    className="bg-bg-card border border-border-subtle rounded-2xl p-6 sm:p-8 flex flex-col hover:border-accent-gold/40 transition-all duration-300 w-full group overflow-hidden relative shadow-sm hover:shadow-xl"
                                    initial={{ opacity: 0, y: 20 }}
                                    animate={{ opacity: 1, y: 0 }}
                                    transition={{ duration: 0.5, delay: globalIndex * 0.1 }}
                                >
                                    {/* Logo block */}
                                    <div className="w-12 h-12 rounded-xl bg-text-primary flex items-center justify-center mb-6 shadow-sm">
                                        <Star size={24} className="fill-bg-primary text-bg-primary" />
                                    </div>

                                    {/* Title & Subtitle */}
                                    <h3 className="text-xl font-bold text-text-primary tracking-tight mb-1">{entry.name}</h3>
                                    <p className="text-sm text-text-muted mb-6 mt-1">
                                        by {entry.author}
                                    </p>

                                    {/* Pricing block */}
                                    <div className="flex items-baseline gap-1 mb-6">
                                        <span className="text-4xl font-extrabold text-text-primary tracking-tight">
                                            {priceAmount}
                                        </span>
                                        <span className="text-sm font-medium text-text-muted">
                                            / {pricePeriod}
                                        </span>
                                    </div>

                                    {/* Subscribe button */}
                                    <button
                                        onClick={() => onSubscribe?.(entry.name)}
                                        className="w-full py-3 rounded-xl bg-text-primary text-bg-primary font-semibold text-sm hover:opacity-90 transition-opacity mb-8 shadow-md inline-flex items-center justify-center gap-1.5 group"
                                    >
                                        Subscribe
                                        <ButtonArrow />
                                    </button>

                                    {/* Divider */}
                                    <div className="h-px w-full bg-border-subtle mb-6" />

                                    {/* Features Checklist */}
                                    <div className="flex-1 flex flex-col gap-4 mb-8">
                                        {features.map((feature, idx) => (
                                            <div key={idx} className="flex items-start gap-3">
                                                <Check size={16} className="text-text-primary mt-1 shrink-0" />
                                                <span className="text-sm text-text-secondary leading-snug">
                                                    {feature}{!feature.endsWith('.') && idx === features.length - 1 ? '.' : ''}
                                                </span>
                                            </div>
                                        ))}
                                    </div>

                                    {/* Footer / Rating */}
                                    <div className="w-full py-2.5 mt-auto rounded-lg bg-bg-surface flex items-center justify-center border border-border-subtle">
                                        <div className="flex items-center gap-1.5">
                                            {Array.from({ length: 5 }).map((_, idx) => (
                                                <Star
                                                    key={idx}
                                                    size={12}
                                                    className={idx < entry.rating ? 'text-accent-gold fill-accent-gold' : 'text-border-subtle'}
                                                />
                                            ))}
                                            <span className="text-xs font-medium text-text-muted ml-2">{entry.rating}.0</span>
                                        </div>
                                    </div>
                                </motion.div>
                            )
                        })}
                    </div>
                ))}
            </div>
        </div>
    )
}
