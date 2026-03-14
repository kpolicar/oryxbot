import { ClerkProvider } from '@clerk/clerk-react'
import { Outlet } from 'react-router-dom'

const CLERK_KEY = import.meta.env.VITE_CLERK_PUBLISHABLE_KEY

export default function ClerkLayout() {
    if (!CLERK_KEY) {
        return <Outlet />
    }

    return (
        <ClerkProvider publishableKey={CLERK_KEY}>
            <Outlet />
        </ClerkProvider>
    )
}
