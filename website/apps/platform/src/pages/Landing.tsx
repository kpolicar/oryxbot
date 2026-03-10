import { HeroSection } from '../components/HeroSection'
import { UserSection } from '../components/UserSection'
import { PilotSection } from '../components/PilotSection'
import { DeveloperSection } from '../components/DeveloperSection'
import { ArchitectureSection } from '../components/ArchitectureSection'
import { PricingSection } from '../components/PricingSection'

export default function Landing() {
    return (
        <>
            <HeroSection />

            <div className="max-w-6xl mx-auto px-6">
                <div className="h-px" style={{ background: 'linear-gradient(90deg, transparent, rgba(211, 200, 168, 0.15), transparent)' }} />
            </div>

            <UserSection />

            <div className="max-w-6xl mx-auto px-6">
                <div className="h-px" style={{ background: 'linear-gradient(90deg, transparent, rgba(211, 200, 168, 0.15), transparent)' }} />
            </div>

            <PilotSection />

            <div className="max-w-6xl mx-auto px-6">
                <div className="h-px" style={{ background: 'linear-gradient(90deg, transparent, rgba(211, 200, 168, 0.15), transparent)' }} />
            </div>

            <DeveloperSection />

            <div className="max-w-6xl mx-auto px-6">
                <div className="h-px" style={{ background: 'linear-gradient(90deg, transparent, rgba(211, 200, 168, 0.15), transparent)' }} />
            </div>

            <ArchitectureSection />

            <div className="max-w-6xl mx-auto px-6">
                <div className="h-px" style={{ background: 'linear-gradient(90deg, transparent, rgba(211, 200, 168, 0.15), transparent)' }} />
            </div>

            <PricingSection />
        </>
    )
}
