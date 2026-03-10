import { motion } from 'framer-motion'
import { Check } from 'lucide-react'
import { Link } from 'react-router-dom'

interface PricingCardProps {
    title: string
    price: string
    priceNote?: string
    features: string[]
    note?: string
    ctaText: string
    ctaLink: string
    highlighted?: boolean
}

function PricingCard({ title, price, priceNote, features, note, ctaText, ctaLink, highlighted }: PricingCardProps) {
    return (
        <motion.div
            className={`rounded-2xl border p-8 flex flex-col ${highlighted
                    ? 'border-accent-gold/40 bg-accent-gold/5 shadow-lg shadow-accent-gold/5'
                    : 'border-border-subtle bg-bg-card'
                }`}
            initial={{ opacity: 0, y: 30 }}
            whileInView={{ opacity: 1, y: 0 }}
            viewport={{ once: true }}
            transition={{ duration: 0.5 }}
            whileHover={{ y: -4, transition: { duration: 0.2 } }}
        >
            <h3 className="text-xl font-bold text-text-primary mb-2">{title}</h3>
            <div className="mb-6">
                <span className="text-3xl font-bold text-accent-gold">{price}</span>
                {priceNote && (
                    <span className="text-sm text-text-muted ml-2">{priceNote}</span>
                )}
            </div>

            <ul className="space-y-3 mb-8 flex-1">
                {features.map((feature) => (
                    <li key={feature} className="flex items-start gap-3 text-sm text-text-secondary">
                        <Check size={16} className="text-accent-gold shrink-0 mt-0.5" />
                        {feature}
                    </li>
                ))}
            </ul>

            {note && (
                <p className="text-xs text-text-muted mb-6 italic">{note}</p>
            )}

            <Link
                to={ctaLink}
                className={`text-center font-medium py-3 px-6 rounded-lg transition-colors ${highlighted
                        ? 'bg-accent-gold text-bg-primary hover:bg-accent-gold-hover'
                        : 'bg-bg-card-hover text-text-primary hover:bg-border-subtle'
                    }`}
            >
                {ctaText}
            </Link>
        </motion.div>
    )
}

export function PricingSection() {
    return (
        <section id="pricing" className="py-24 px-6 bg-bg-secondary/30">
            <div className="max-w-4xl mx-auto">
                <motion.h2
                    className="text-3xl lg:text-4xl font-bold text-text-primary text-center mb-16"
                    initial={{ opacity: 0, y: 20 }}
                    whileInView={{ opacity: 1, y: 0 }}
                    viewport={{ once: true }}
                    transition={{ duration: 0.5 }}
                >
                    Simple <span className="text-accent-gold">Pricing</span>
                </motion.h2>

                <div className="grid grid-cols-1 md:grid-cols-2 gap-8">
                    <PricingCard
                        title="User"
                        price="€50/month"
                        features={[
                            'OryxBot Pilot access',
                            'Access to Marketplace scripts',
                            'Cloud execution (no local cheat software)',
                            'Priority support',
                        ]}
                        ctaText="Get Started →"
                        ctaLink="/register"
                    />
                    <PricingCard
                        title="Developer"
                        price="€50/month → €10/month"
                        features={[
                            'Full API access',
                            'Publish scripts to Marketplace',
                            'Set your own monthly price for scripts',
                            'Developer dashboard',
                        ]}
                        note="Drops to €10/month once any of your published scripts has 5+ active subscribers"
                        ctaText="Start Building →"
                        ctaLink="/register"
                        highlighted
                    />
                </div>

                <motion.p
                    className="text-center text-sm text-text-muted mt-10 italic"
                    initial={{ opacity: 0 }}
                    whileInView={{ opacity: 1 }}
                    viewport={{ once: true }}
                    transition={{ delay: 0.5 }}
                >
                    Marketplace script subscriptions are billed separately by each script's author. OryxBot takes a
                    commission on marketplace transactions.
                </motion.p>
            </div>
        </section>
    )
}
