import { Routes, Route, useLocation } from 'react-router-dom'
import { Suspense, lazy, useEffect } from 'react'
import { Navbar } from '@shared/components/Navbar'
import { trackPageview } from './lib/analytics'
import { Footer } from '@shared/components/Footer'

const Landing = lazy(() => import('./pages/Landing'))
const Marketplace = lazy(() => import('./pages/Marketplace'))
const Docs = lazy(() => import('./pages/Docs'))
const ClerkLayout = lazy(() => import('./components/ClerkLayout'))
const Register = lazy(() => import('./pages/Register'))
const RegisterPending = lazy(() => import('./pages/RegisterPending'))
const Login = lazy(() => import('./pages/Login'))
const Pilot = lazy(() => import('./pages/Pilot'))

function ScrollToHash() {
    const location = useLocation()
    useEffect(() => {
        trackPageview(location.pathname + location.search + location.hash)
    }, [location.pathname, location.search, location.hash])
    useEffect(() => {
        if (!location.hash) return
        const id = location.hash.slice(1)
        const timer = setTimeout(() => {
            const el = document.getElementById(id)
            if (el) el.scrollIntoView({ behavior: 'smooth' })
        }, 80)
        return () => clearTimeout(timer)
    }, [location.hash])
    return null
}

export default function App() {
    return (
        <div className="min-h-screen flex flex-col bg-bg-primary">
            <Navbar />
            <ScrollToHash />
            <main className="flex-1">
                <Suspense fallback={null}>
                    <Routes>
                        <Route path="/" element={<Landing />} />
                        <Route path="/marketplace" element={<Marketplace />} />
                        <Route path="/docs" element={<Docs />} />
                        <Route element={<ClerkLayout />}>
                            <Route path="/register" element={<Register />} />
                            <Route path="/register/pending" element={<RegisterPending />} />
                            <Route path="/login" element={<Login />} />
                        </Route>
                        <Route path="/pilot" element={<Pilot />} />
                    </Routes>
                </Suspense>
            </main>
            <Footer />
        </div>
    )
}
