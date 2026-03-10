import { HeroPilotDemo } from '../components/HeroPilotDemo'
import { HeroDeveloperPitch } from '../components/HeroDeveloperPitch'
import { RailripSection } from '../components/RailripSection'
import { DeveloperSection } from '../components/DeveloperSection'
import { UserSection } from '../components/UserSection'
import { PricingSection } from '../components/PricingSection'

export default function Landing() {
    return (
        <>
            {/* Hero — Side-by-Side Product Showcase */}
            <section className="min-h-screen pt-24 pb-16 px-6">
                <div className="max-w-7xl mx-auto grid grid-cols-1 lg:grid-cols-2 gap-12 items-start">
                    <HeroPilotDemo />
                    <HeroDeveloperPitch />
                </div>
            </section>

            {/* Architecture Diagram */}
            <RailripSection />

            {/* Developer Section */}
            <DeveloperSection />

            {/* User Section */}
            <UserSection />

            {/* Pricing */}
            <PricingSection />
        </>
    )
}
