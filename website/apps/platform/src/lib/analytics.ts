declare global {
    interface Window {
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
        gtag?: (...args: any[]) => void
    }
}

function gtag(...args: Parameters<NonNullable<Window['gtag']>>) {
    window.gtag?.(...args)
}

export function trackPageview(path: string) {
    gtag('event', 'page_view', {
        page_location: window.location.origin + path,
        page_path: path,
    })
}

export function trackSignUp(method: string = 'clerk') {
    gtag('event', 'sign_up', { method })
}
