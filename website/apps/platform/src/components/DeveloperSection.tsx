import { useRef, useState, useEffect } from 'react'
import Prism from 'prismjs'
import 'prismjs/components/prism-python'
import 'prismjs/themes/prism-tomorrow.css'
import { ButtonArrow } from '@shared/components/ButtonArrow'
import { motion, useInView } from 'framer-motion'
import { Terminal, Settings, Zap, ShoppingBag, TrendingDown } from 'lucide-react'
import { Link } from 'react-router-dom'

const DEV_CODE = `from oryxbot import (
    Pilot, Config, Screen,
    Zone, City, OnAttacked, RoutePreference,
)

# Configure navigation behavior
config = Config(
    on_attacked=OnAttacked.RETURN_TO_CITY,
    route_preference=RoutePreference.ROADS,
)

pilot = Pilot(config)
screen = Screen()

while True:
    pilot.move_to(Zone.STONEMOUTH_RILLS)
    pilot.wait_until_arrived()

    node = screen.find("t6_ore")
    screen.click(node.x, node.y)
    screen.wait_for("gathering_complete")

    if screen.find("inventory_full_indicator"):
        pilot.move_to(City.FORT_STERLING)
        pilot.wait_until_arrived()
        screen.click(482, 310)   # open bank NPC
        screen.click(520, 440)   # deposit all`

const FEATURES = [
    {
        icon: <Terminal size={18} />,
        title: 'Full Navigation API',
        description: 'move_to(), follow_route(), get_position()',
    },
    {
        icon: <Settings size={18} />,
        title: 'Configurable Behavior',
        description: 'Set responses to attacks, preferred route types, speed limits',
    },
    {
        icon: <Zap size={18} />,
        title: 'Event Hooks',
        description: 'on_arrived, on_attacked, on_stuck, on_cluster_changed',
    },
    {
        icon: <ShoppingBag size={18} />,
        title: 'Marketplace Income',
        description: 'Sell your scripts on the OryxBot Marketplace with monthly subscriptions',
    },
    {
        icon: <TrendingDown size={18} />,
        title: 'Grows with you',
        description: '€50/mo → €10/mo once any of your scripts reaches 5+ subscribers',
    },
]

function AnimatedCodeBlock() {
    const ref = useRef<HTMLDivElement>(null)
    const isInView = useInView(ref, { once: true, margin: '-80px' })
    const [revealed, setRevealed] = useState(0)
    const isDone = revealed >= DEV_CODE.length

    useEffect(() => {
        if (!isInView) return
        const total = DEV_CODE.length
        let current = 0
        const id = setInterval(() => {
            current = Math.min(current + 2, total)
            setRevealed(current)
            if (current >= total) clearInterval(id)
        }, 16)
        return () => clearInterval(id)
    }, [isInView])

    const visibleCode = DEV_CODE.slice(0, revealed)
    const highlighted = Prism.highlight(visibleCode, Prism.languages.python, 'python')

    return (
        <div
            ref={ref}
            className="rounded-xl overflow-hidden relative"
            style={{
                background: 'linear-gradient(145deg, #111318, #0d0e11)',
                border: '1px solid rgba(255, 255, 255, 0.06)',
                boxShadow: isDone ? '0 0 60px rgba(174, 164, 128, 0.06)' : undefined,
                transition: 'box-shadow 1s ease',
            }}
        >
            {/* Window bar */}
            <div className="flex items-center gap-2 px-4 py-2.5"
                style={{ borderBottom: '1px solid rgba(255, 255, 255, 0.04)' }}
            >
                <div className="flex gap-1.5">
                    <div className="w-2.5 h-2.5 rounded-full" style={{ background: 'rgba(248, 113, 113, 0.5)' }} />
                    <div className="w-2.5 h-2.5 rounded-full" style={{ background: 'rgba(211, 200, 168, 0.4)' }} />
                    <div className="w-2.5 h-2.5 rounded-full" style={{ background: 'rgba(52, 211, 153, 0.4)' }} />
                </div>
                <span className="ml-2 text-[11px] text-text-muted" style={{ fontFamily: 'var(--font-mono)' }}>bot.py</span>
                {!isDone && (
                    <span className="ml-auto text-[10px] text-gold-500 opacity-60" style={{ fontFamily: 'var(--font-mono)' }}>
                        typing…
                    </span>
                )}
            </div>

            {/* Code */}
            <pre
                className="p-4 leading-relaxed m-0 whitespace-pre-wrap break-words"
                style={{ background: 'transparent', fontSize: '13px', minHeight: '380px', whiteSpace: 'pre-wrap', overflowWrap: 'break-word' }}
            >
                <code
                    className="language-python"
                    style={{ fontFamily: 'var(--font-mono)', fontSize: '13px', whiteSpace: 'pre-wrap', overflowWrap: 'break-word' }}
                    dangerouslySetInnerHTML={{ __html: highlighted }}
                />
                <span
                    className="cursor-blink"
                    style={{ color: '#d3c8a8', fontFamily: 'var(--font-mono)' }}
                >▌</span>
            </pre>
        </div>
    )
}

export function DeveloperSection() {
    return (
        <section id="developers" className="py-28 px-6 relative">

            <div className="max-w-5xl mx-auto relative">
                <div className="grid grid-cols-1 lg:grid-cols-2 gap-16 items-center">
                    {/* Code Column (Left) */}
                    <motion.div
                        initial={{ opacity: 0, x: -30 }}
                        whileInView={{ opacity: 1, x: 0 }}
                        viewport={{ once: true }}
                        transition={{ duration: 0.6 }}
                        className="order-2 lg:order-1"
                    >
                        <AnimatedCodeBlock />
                    </motion.div>

                    {/* Text Features Column (Right) */}
                    <motion.div
                        className="order-1 lg:order-2 space-y-6"
                        initial={{ opacity: 0, x: 30 }}
                        whileInView={{ opacity: 1, x: 0 }}
                        viewport={{ once: true }}
                        transition={{ duration: 0.6, delay: 0.15 }}
                    >
                        <div className="mb-10">
                            <span className="text-[11px] font-medium tracking-[0.15em] uppercase text-gold-400 block mb-3">
                                For Developers
                            </span>
                            <h2 className="text-3xl lg:text-5xl font-bold text-text-primary leading-tight mb-6">
                                Script anything on top of <span className="text-gradient-gold">Pilot</span>
                            </h2>
                            <p className="text-text-secondary text-lg leading-relaxed mb-8">
                                Use the powerful Python SDK to write complex logic. From simple gatherers to marketplace tycoons, Pilot exposes everything you need.
                            </p>
                        </div>

                        <div className="space-y-6">
                            {FEATURES.map((feature, i) => (
                                <motion.div
                                    key={feature.title}
                                    className="flex items-start gap-4 group"
                                    initial={{ opacity: 0, y: 10 }}
                                    whileInView={{ opacity: 1, y: 0 }}
                                    viewport={{ once: true }}
                                    transition={{ duration: 0.4, delay: 0.2 + i * 0.08 }}
                                >
                                    <div className="p-2 rounded-lg shrink-0 bg-accent-gold/10 text-gold-300 group-hover:text-gold-200 transition-colors">
                                        {feature.icon}
                                    </div>
                                    <div>
                                        <h4 className="text-sm font-semibold text-text-primary mb-0.5">{feature.title}</h4>
                                        <p className="text-sm text-text-muted leading-relaxed">{feature.description}</p>
                                    </div>
                                </motion.div>
                            ))}
                        </div>

                        <motion.div
                            initial={{ opacity: 0 }}
                            whileInView={{ opacity: 1 }}
                            viewport={{ once: true }}
                            transition={{ delay: 0.6 }}
                            className="pt-6"
                        >
                            <Link to="/register" className="btn-gold text-base px-8 h-12 justify-center w-full sm:w-auto group">
                                Start Building
                                <ButtonArrow />
                            </Link>
                        </motion.div>
                    </motion.div>
                </div>
            </div>
        </section>
    )
}
