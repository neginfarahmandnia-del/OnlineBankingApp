/// <reference types="vitest" />
import { defineConfig } from 'vitest/config'
import react from '@vitejs/plugin-react'

export default defineConfig({
    plugins: [react()],
    server: {
        proxy: {
            // alles unter /api an die ASP.NET-API weiterleiten
            '/api': {
                target: 'https://localhost:7202', // <- dein Kestrel-HTTPS-Port
                changeOrigin: true,
                secure: false,                     // dev-Zertifikat akzeptieren
            },
        },
    },
    test: {
        environment: 'jsdom',
        setupFiles: './src/test/setup.ts',
        css: true,
    },
})
