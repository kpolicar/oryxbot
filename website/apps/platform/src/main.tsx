import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import { BrowserRouter } from 'react-router-dom'
import App from './App'
import './index.css'

const CLERK_KEY = import.meta.env.VITE_CLERK_PUBLISHABLE_KEY

async function mount() {
    const root = createRoot(document.getElementById('root')!)

    // If Clerk key is available, wrap with ClerkProvider
    if (CLERK_KEY) {
        const { ClerkProvider } = await import('@clerk/clerk-react')
        root.render(
            <StrictMode>
                <ClerkProvider publishableKey={CLERK_KEY}>
                    <BrowserRouter>
                        <App />
                    </BrowserRouter>
                </ClerkProvider>
            </StrictMode>,
        )
    } else {
        // Dev mode without Clerk — render without auth
        root.render(
            <StrictMode>
                <BrowserRouter>
                    <App />
                </BrowserRouter>
            </StrictMode>,
        )
    }
}

mount()
