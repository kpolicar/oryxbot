import { CodeBlock } from '@shared/components/CodeBlock'
import { motion } from 'framer-motion'

const HERO_CODE = `from oryxbot import Pilot, Config, Screen

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

    # Gather ore — click resource nodes on screen
    node = screen.find("t6_ore")
    screen.click(node.x, node.y)
    screen.wait_for("gathering_complete")

    # Check if inventory is full
    if screen.find("inventory_full_indicator"):
        pilot.move_to(BANK_CITY)
        pilot.wait_until_arrived()
        screen.click(482, 310)   # open bank NPC
        screen.click(520, 440)   # deposit all button`

export function HeroDeveloperPitch() {
    return (
        <motion.div
            className="flex flex-col h-full"
            initial={{ opacity: 0, x: 30 }}
            animate={{ opacity: 1, x: 0 }}
            transition={{ duration: 0.6, ease: 'easeOut', delay: 0.2 }}
        >
            <h2 className="text-2xl lg:text-3xl font-bold text-text-primary mb-2">
                Build with <span className="text-accent-gold">OryxBot</span>
            </h2>
            <p className="text-text-secondary mb-6">
                Use our API to script automated workflows on top of Pilot's navigation engine.
            </p>

            <CodeBlock code={HERO_CODE} className="flex-1" />

            <div className="mt-6 space-y-3">
                <p className="text-sm text-text-muted italic">
                    Sell your scripts on the Marketplace. Set a monthly price — we handle billing.
                </p>
                <a
                    href="/#developers"
                    className="inline-flex items-center gap-2 text-accent-gold hover:text-accent-gold-hover font-medium transition-colors"
                >
                    Read Developer Docs →
                </a>
            </div>
        </motion.div>
    )
}
