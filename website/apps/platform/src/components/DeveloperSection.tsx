import { CodeBlock } from '@shared/components/CodeBlock'
import { motion } from 'framer-motion'
import { Terminal, Settings, Zap, ShoppingBag, TrendingDown } from 'lucide-react'
import { Link } from 'react-router-dom'

const DEV_CODE = `from oryxbot import Pilot, Config, Screen

# Configure navigation behavior
config = Config(
    on_attacked="return_to_city",   # flee to nearest city if attacked
    route_preference="roads",       # prefer road paths over wilderness
)

pilot = Pilot(config)
screen = Screen()

# Gathering loop: mine ore → deposit at bank → repeat
RESOURCE_ZONE = "Stonemouth Rills"   # T6 ore node location
BANK_CITY = "Fort Sterling"

while True:
    # Navigate to resource zone
    pilot.move_to(RESOURCE_ZONE)
    pilot.wait_until_arrived()

    # Gather ore — click resource nodes on screen
    node = screen.find("t6_ore")
    screen.click(node.x, node.y)
    screen.wait_for("gathering_complete")

    if screen.find("inventory_full_indicator"):
        # Return to city and deposit at bank
        pilot.move_to(BANK_CITY)
        pilot.wait_until_arrived()
        screen.click(482, 310)   # open bank NPC
        screen.click(520, 440)   # deposit all`

const FEATURES = [
    {
        icon: <Terminal size={20} />,
        title: 'Full Navigation API',
        description: 'move_to(), follow_route(), get_position()',
    },
    {
        icon: <Settings size={20} />,
        title: 'Configurable Behavior',
        description: 'Set responses to attacks, preferred route types, speed limits',
    },
    {
        icon: <Zap size={20} />,
        title: 'Event Hooks',
        description: 'on_arrived, on_attacked, on_stuck, on_cluster_changed',
    },
    {
        icon: <ShoppingBag size={20} />,
        title: 'Marketplace Income',
        description: 'Sell your scripts on the OryxBot Marketplace. Set a monthly subscription price for users.',
    },
    {
        icon: <TrendingDown size={20} />,
        title: 'Grows with you',
        description: 'Developer subscription starts at €50/mo, drops to just €10/mo once any of your published scripts reaches 5+ active subscribers',
    },
]

export function DeveloperSection() {
    return (
        <section id="developers" className="py-24 px-6 bg-bg-secondary/30">
            <div className="max-w-6xl mx-auto">
                <motion.h2
                    className="text-3xl lg:text-4xl font-bold text-text-primary text-center mb-16"
                    initial={{ opacity: 0, y: 20 }}
                    whileInView={{ opacity: 1, y: 0 }}
                    viewport={{ once: true }}
                    transition={{ duration: 0.5 }}
                >
                    Script Anything on Top of <span className="text-accent-gold">Pilot</span>
                </motion.h2>

                <div className="grid grid-cols-1 lg:grid-cols-2 gap-12 items-start">
                    {/* Code column */}
                    <motion.div
                        initial={{ opacity: 0, x: -30 }}
                        whileInView={{ opacity: 1, x: 0 }}
                        viewport={{ once: true }}
                        transition={{ duration: 0.6 }}
                    >
                        <CodeBlock code={DEV_CODE} />
                    </motion.div>

                    {/* Features column */}
                    <motion.div
                        className="space-y-6"
                        initial={{ opacity: 0, x: 30 }}
                        whileInView={{ opacity: 1, x: 0 }}
                        viewport={{ once: true }}
                        transition={{ duration: 0.6, delay: 0.2 }}
                    >
                        {FEATURES.map((feature) => (
                            <div key={feature.title} className="flex items-start gap-4 group">
                                <div className="p-2.5 rounded-lg bg-bg-card border border-border-subtle text-accent-gold group-hover:bg-accent-gold/10 transition-colors shrink-0">
                                    {feature.icon}
                                </div>
                                <div>
                                    <h4 className="font-semibold text-text-primary mb-1">{feature.title}</h4>
                                    <p className="text-sm text-text-secondary leading-relaxed">{feature.description}</p>
                                </div>
                            </div>
                        ))}

                        <Link
                            to="/register"
                            className="inline-flex items-center gap-2 bg-accent-gold text-bg-primary font-medium px-6 py-3 rounded-lg hover:bg-accent-gold-hover transition-colors mt-4"
                        >
                            Get API Key →
                        </Link>
                    </motion.div>
                </div>
            </div>
        </section>
    )
}
