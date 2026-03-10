import { Routes, Route } from 'react-router-dom'
import { Navbar } from '@shared/components/Navbar'
import { Footer } from '@shared/components/Footer'
import Landing from './pages/Landing'
import Marketplace from './pages/Marketplace'
import Register from './pages/Register'
import RegisterPending from './pages/RegisterPending'
import Login from './pages/Login'

export default function App() {
    return (
        <div className="min-h-screen flex flex-col bg-bg-primary">
            <Navbar />
            <main className="flex-1">
                <Routes>
                    <Route path="/" element={<Landing />} />
                    <Route path="/marketplace" element={<Marketplace />} />
                    <Route path="/register" element={<Register />} />
                    <Route path="/register/pending" element={<RegisterPending />} />
                    <Route path="/login" element={<Login />} />
                </Routes>
            </main>
            <Footer />
        </div>
    )
}
