import { motion } from 'framer-motion'
import { ButtonArrow } from '@shared/components/ButtonArrow'
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
            className={`rounded-2xl p-7 flex flex-col relative overflow-hidden ${highlighted
                ? 'glow-card'
                : 'glow-card'
                }`}
            style={highlighted ? {
                borderColor: 'rgba(211, 200, 168, 0.2)',
                background: 'linear-gradient(145deg, rgba(174, 164, 128, 0.06), rgba(21, 23, 28, 0.8))',
            } : {}}
            initial={{ opacity: 0, y: 20 }}
            whileInView={{ opacity: 1, y: 0 }}
            viewport={{ once: true }}
            transition={{ duration: 0.5 }}
            whileHover={{ y: -3, transition: { duration: 0.2 } }}
        >
            {highlighted && (
                <div className="absolute top-0 left-0 right-0 h-px"
                    style={{ background: 'linear-gradient(90deg, transparent, #d3c8a8, transparent)' }}
                />
            )}

            <h3 className="text-lg font-bold text-text-primary mb-2">{title}</h3>
            <div className="mb-6">
                <span className="text-2xl font-bold text-gradient-gold-bright">{price}</span>
                {priceNote && (
                    <span className="text-xs text-text-muted ml-2">{priceNote}</span>
                )}
            </div>

            <ul className="space-y-3 mb-6 flex-1">
                {features.map((feature) => (
                    <li key={feature} className="flex items-start gap-2.5 text-sm text-text-secondary">
                        <Check size={14} className="text-gold-300 shrink-0 mt-0.5" />
                        {feature}
                    </li>
                ))}
            </ul>

            {note && (
                <p className="text-[11px] text-text-muted mb-5 italic">{note}</p>
            )}

            <Link
                to={ctaLink}
                className={highlighted ? 'btn-gold text-sm text-center justify-center group' : 'btn-outline text-sm text-center justify-center group'}
            >
                {ctaText}
                <ButtonArrow />
            </Link>
        </motion.div>
    )
}

export function PricingSection() {
    return (
        <section id="pricing" className="py-28 px-6 relative">
            <div className="max-w-3xl mx-auto relative">
                <motion.div
                    className="text-center mb-14"
                    initial={{ opacity: 0, y: 20 }}
                    whileInView={{ opacity: 1, y: 0 }}
                    viewport={{ once: true }}
                    transition={{ duration: 0.5 }}
                >
                    <span className="text-[11px] font-medium tracking-[0.15em] uppercase text-gold-400 block mb-3">
                        Pricing
                    </span>
                    <h2 className="text-3xl lg:text-4xl font-bold text-text-primary">
                        Simple <span className="text-gradient-gold">pricing</span>
                    </h2>
                </motion.div>

                <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                    <PricingCard
                        title="User"
                        price="€50/month"
                        features={[
                            'OryxBot Pilot access',
                            'Access to Marketplace scripts',
                            'Cloud execution — nothing on your machine',
                            'Priority support',
                        ]}
                        ctaText="Get Started"
                        ctaLink="/register"
                    />
                    <PricingCard
                        title="Developer"
                        price="€10/mo*"
                        features={[
                            'Full SDK access',
                            'Publish scripts to Marketplace',
                            'Set your own script pricing',
                            'Developer dashboard',
                        ]}
                        note="* Drops to €10/month once any published script has 5+ paying subscribers"
                        ctaText="Start Building"
                        ctaLink="/register"
                        highlighted
                    />
                </div>

                <motion.p
                    className="text-center text-xs text-text-muted mt-8"
                    initial={{ opacity: 0 }}
                    whileInView={{ opacity: 1 }}
                    viewport={{ once: true }}
                    transition={{ delay: 0.5 }}
                >
                    Marketplace script subscriptions billed separately. OryxBot takes a 20% commission on transactions.
                </motion.p>
            </div>
        </section>
    )
}
