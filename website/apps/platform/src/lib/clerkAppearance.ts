import type { Appearance } from '@clerk/clerk-react'

export const clerkAppearance: Appearance = {
    variables: {
        colorBackground: '#15171c',
        colorInputBackground: '#1a1c22',
        colorInputText: '#f5f5f7',
        colorText: '#f5f5f7',
        colorTextSecondary: '#a1a1aa',
        colorPrimary: '#d3c8a8',
        colorDanger: '#f87171',
        colorNeutral: '#6b6b76',
        borderRadius: '0.75rem',
        fontFamily: "'Inter', system-ui, sans-serif",
    },
    elements: {
        card: {
            backgroundColor: '#15171c',
            border: '1px solid rgba(255, 255, 255, 0.06)',
            boxShadow: '0 8px 40px rgba(0, 0, 0, 0.5)',
        },
        headerTitle: {
            color: '#f5f5f7',
        },
        headerSubtitle: {
            color: '#a1a1aa',
        },
        formButtonPrimary: {
            background: 'linear-gradient(135deg, #aea480, #d3c8a8)',
            color: '#0a0b0d',
            fontWeight: '600',
            '&:hover': {
                background: 'linear-gradient(135deg, #c4b98e, #e4d8a8)',
            },
        },
        formFieldInput: {
            backgroundColor: '#1a1c22',
            borderColor: 'rgba(255, 255, 255, 0.06)',
            color: '#f5f5f7',
            '&:focus': {
                borderColor: 'rgba(211, 200, 168, 0.4)',
                boxShadow: '0 0 0 2px rgba(211, 200, 168, 0.1)',
            },
        },
        formFieldLabel: {
            color: '#a1a1aa',
        },
        dividerLine: {
            backgroundColor: 'rgba(255, 255, 255, 0.06)',
        },
        dividerText: {
            color: '#6b6b76',
        },
        footerActionLink: {
            color: '#d3c8a8',
            '&:hover': {
                color: '#e4d8a8',
            },
        },
        identityPreviewText: {
            color: '#a1a1aa',
        },
        identityPreviewEditButton: {
            color: '#d3c8a8',
        },
        socialButtonsBlockButton: {
            backgroundColor: '#1a1c22',
            border: '1px solid rgba(255, 255, 255, 0.06)',
            color: '#f5f5f7',
            '&:hover': {
                backgroundColor: '#1c1f26',
            },
        },
        socialButtonsBlockButtonText: {
            color: '#f5f5f7',
        },
        alternativeMethodsBlockButton: {
            backgroundColor: '#1a1c22',
            border: '1px solid rgba(255, 255, 255, 0.06)',
            color: '#f5f5f7',
        },
        otpCodeFieldInput: {
            backgroundColor: '#1a1c22',
            borderColor: 'rgba(255, 255, 255, 0.06)',
            color: '#f5f5f7',
        },
    },
}
