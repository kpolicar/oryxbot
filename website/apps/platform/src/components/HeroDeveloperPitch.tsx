import { CodeBlock } from '@shared/components/CodeBlock'
import { ButtonArrow } from '@shared/components/ButtonArrow'
import { motion } from 'framer-motion'

const HERO_CODE = `from oryxbot import Pilot, Config

config = Config(
    on_attacked="return_to_city",
    route_preference="roads",
)

pilot = Pilot(config)

RESOURCE_ZONE = "Stonemouth Rills"
BANK_CITY = "Fort Sterling"

while True:
    pilot.move_to(RESOURCE_ZONE)
    pilot.wait_until_arrived()

    node = pilot.find("t6_ore")
    pilot.click(node.x, node.y)
    pilot.wait_for("gathering_complete")

    if pilot.find("inventory_full_indicator"):
        pilot.move_to(BANK_CITY)
        pilot.wait_until_arrived()
        pilot.click(482, 310)   # open bank NPC
        pilot.click(520, 440)   # deposit all`

export function HeroDeveloperPitch() {
    return (
        <motion.div
            className="flex flex-col"
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ duration: 0.7, ease: 'easeOut', delay: 0.15 }}
        >
            <div className="mb-6">
                <div className="inline-flex items-center gap-2 px-3 py-1 rounded-full text-[11px] font-medium tracking-wide uppercase mb-5"
                    style={{ background: 'rgba(174, 164, 128, 0.08)', border: '1px solid rgba(174, 164, 128, 0.15)', color: '#d3c8a8' }}
                >
                    Developer API
                </div>
                <h2 className="text-2xl lg:text-3xl font-bold text-text-primary mb-3 leading-tight">
                    Build with <span className="text-gradient-gold">OryxBot</span>
                </h2>
                <p className="text-text-secondary text-base leading-relaxed mb-5">
                    Use our Python SDK to script automated workflows on top of Pilot's navigation engine.
                    Sell your scripts on the Marketplace.
                </p>
            </div>

            <CodeBlock
                code={HERO_CODE}
                fontSize="10px"
                className="mb-5 max-w-[540px] [&_pre]:p-3"
            />

            <p className="text-sm text-text-muted italic mb-4">
                Set a monthly price for your scripts — we handle billing and distribution.
            </p>
            <a href="/#developers" className="btn-outline text-sm w-fit group">
                Read Developer Docs
                <ButtonArrow />
            </a>
        </motion.div>
    )
}
