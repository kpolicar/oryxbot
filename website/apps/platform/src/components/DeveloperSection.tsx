import { CodeBlock } from '@shared/components/CodeBlock'
import { motion } from 'framer-motion'
import { Terminal, Settings, Zap, ShoppingBag, TrendingDown } from 'lucide-react'
import { Link } from 'react-router-dom'

const DEV_CODE = `from oryxbot import Pilot, Config, Screen

# Configure navigation behavior
config = Config(
    on_attacked="return_to_city",
    route_preference="roads",
)

pilot = Pilot(config)
screen = Screen()

RESOURCE_ZONE = "Stonemouth Rills"
BANK_CITY = "Fort Sterling"

while True:
    pilot.move_to(RESOURCE_ZONE)
    pilot.wait_until_arrived()

    node = screen.find("t6_ore")
    screen.click(node.x, node.y)
    screen.wait_for("gathering_complete")

    if screen.find("inventory_full_indicator"):
        pilot.move_to(BANK_CITY)
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

export function DeveloperSection() {
    return (
        <section id="developers" className="py-28 px-6 relative">
            <div className="absolute inset-0 pointer-events-none section-gradient" />

            <div className="max-w-5xl mx-auto relative">
                <motion.div
                    className="mb-14"
                    initial={{ opacity: 0, y: 20 }}
                    whileInView={{ opacity: 1, y: 0 }}
                    viewport={{ once: true }}
                    transition={{ duration: 0.5 }}
                >
                    <span className="text-[11px] font-medium tracking-[0.15em] uppercase text-gold-400 block mb-3">
                        For Developers
                    </span>
                    <h2 className="text-3xl lg:text-4xl font-bold text-text-primary leading-tight">
                        Script anything on top of <span className="text-gradient-gold">Pilot</span>
                    </h2>
                </motion.div>

                <div className="grid grid-cols-1 lg:grid-cols-2 gap-12 items-start">
                    {/* Code */}
                    <motion.div
                        initial={{ opacity: 0, x: -20 }}
                        whileInView={{ opacity: 1, x: 0 }}
                        viewport={{ once: true }}
                        transition={{ duration: 0.6 }}
                    >
                        <CodeBlock code={DEV_CODE} />
                    </motion.div>

                    {/* Features */}
                    <motion.div
                        className="space-y-5"
                        initial={{ opacity: 0, x: 20 }}
                        whileInView={{ opacity: 1, x: 0 }}
                        viewport={{ once: true }}
                        transition={{ duration: 0.6, delay: 0.15 }}
                    >
                        {FEATURES.map((feature, i) => (
                            <motion.div
                                key={feature.title}
                                className="flex items-start gap-4 group"
                                initial={{ opacity: 0, y: 10 }}
                                whileInView={{ opacity: 1, y: 0 }}
                                viewport={{ once: true }}
                                transition={{ duration: 0.4, delay: 0.2 + i * 0.08 }}
                            >
                                <div className="p-2 rounded-lg shrink-0 text-gold-300 group-hover:text-gold-200 transition-colors"
                                    style={{ background: 'rgba(174, 164, 128, 0.06)' }}
                                >
                                    {feature.icon}
                                </div>
                                <div>
                                    <h4 className="text-sm font-semibold text-text-primary mb-0.5">{feature.title}</h4>
                                    <p className="text-sm text-text-muted leading-relaxed">{feature.description}</p>
                                </div>
                            </motion.div>
                        ))}

                        <motion.div
                            initial={{ opacity: 0 }}
                            whileInView={{ opacity: 1 }}
                            viewport={{ once: true }}
                            transition={{ delay: 0.6 }}
                            className="pt-3"
                        >
                            <Link to="/register" className="btn-gold text-sm">
                                Get API Key →
                            </Link>
                        </motion.div>
                    </motion.div>
                </div>
            </div>
        </section>
    )
}
