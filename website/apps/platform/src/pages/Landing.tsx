import { HeroPilotDemo } from '../components/HeroPilotDemo'
import { HeroDeveloperPitch } from '../components/HeroDeveloperPitch'
import { RailripSection } from '../components/RailripSection'
import { DeveloperSection } from '../components/DeveloperSection'
import { UserSection } from '../components/UserSection'
import { PricingSection } from '../components/PricingSection'

export default function Landing() {
    return (
        <>
            {/* Hero — Side-by-Side */}
            <section className="min-h-screen pt-28 pb-20 px-6 relative overflow-hidden">
                {/* Background gradient effects */}
                <div className="absolute inset-0 pointer-events-none">
                    <div className="absolute top-0 left-1/2 -translate-x-1/2 w-[800px] h-[600px] opacity-[0.04]"
                        style={{ background: 'radial-gradient(ellipse, #d3c8a8 0%, transparent 70%)' }}
                    />
                    <div className="absolute bottom-0 left-0 w-[500px] h-[400px] opacity-[0.02]"
                        style={{ background: 'radial-gradient(ellipse, #aea480 0%, transparent 70%)' }}
                    />
                </div>

                <div className="max-w-5xl mx-auto grid grid-cols-1 lg:grid-cols-2 gap-16 items-start relative">
                    <HeroPilotDemo />
                    <HeroDeveloperPitch />
                </div>
            </section>

            {/* Horizontal divider */}
            <div className="max-w-5xl mx-auto px-6">
                <div className="h-px" style={{ background: 'linear-gradient(90deg, transparent, rgba(211, 200, 168, 0.15), transparent)' }} />
            </div>

            {/* Architecture Diagram */}
            <RailripSection />

            <div className="max-w-5xl mx-auto px-6">
                <div className="h-px" style={{ background: 'linear-gradient(90deg, transparent, rgba(211, 200, 168, 0.15), transparent)' }} />
            </div>

            {/* Developer Section */}
            <DeveloperSection />

            {/* User Section */}
            <UserSection />

            <div className="max-w-5xl mx-auto px-6">
                <div className="h-px" style={{ background: 'linear-gradient(90deg, transparent, rgba(211, 200, 168, 0.15), transparent)' }} />
            </div>

            {/* Pricing */}
            <PricingSection />
        </>
    )
}
