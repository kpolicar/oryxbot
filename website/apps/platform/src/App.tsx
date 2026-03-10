import { Routes, Route, useLocation } from 'react-router-dom'
import { useEffect } from 'react'
import { Navbar } from '@shared/components/Navbar'
import { Footer } from '@shared/components/Footer'
import Landing from './pages/Landing'
import Marketplace from './pages/Marketplace'
import Docs from './pages/Docs'
import Register from './pages/Register'
import RegisterPending from './pages/RegisterPending'
import Login from './pages/Login'

function ScrollToHash() {
    const location = useLocation()
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
                <Routes>
                    <Route path="/" element={<Landing />} />
                    <Route path="/marketplace" element={<Marketplace />} />
                    <Route path="/docs" element={<Docs />} />
                    <Route path="/register" element={<Register />} />
                    <Route path="/register/pending" element={<RegisterPending />} />
                    <Route path="/login" element={<Login />} />
                </Routes>
            </main>
            <Footer />
        </div>
    )
}
