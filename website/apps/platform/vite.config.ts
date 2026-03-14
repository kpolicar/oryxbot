import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import tailwindcss from '@tailwindcss/vite'
import path from 'path'

export default defineConfig({
    plugins: [
        react(),
        tailwindcss(),
    ],
    resolve: {
        alias: {
            '@': path.resolve(__dirname, './src'),
            '@shared': path.resolve(__dirname, '../../packages/shared'),
        },
    },
    build: {
        rollupOptions: {
            output: {
                manualChunks: {
                    'vendor-react': ['react', 'react-dom', 'react-router-dom'],
                    'vendor-clerk': ['@clerk/clerk-react'],
                    'vendor-motion': ['framer-motion'],
                    'vendor-leaflet': ['leaflet'],
                },
            },
        },
    },
    server: {
        host: '0.0.0.0',
    },
})
