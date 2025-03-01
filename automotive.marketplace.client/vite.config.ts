import { fileURLToPath, URL } from 'node:url';

import { defineConfig } from 'vite';
import plugin from '@vitejs/plugin-react';
import { env } from 'process';

const target = env.VITE_API_URL ?? 'http://automotive.marketplace.server:8080';

// https://vitejs.dev/config/
export default defineConfig({
    plugins: [plugin()],
    resolve: {
        alias: {
            '@': fileURLToPath(new URL('./src', import.meta.url))
        }
    },
    server: {
        proxy: {
            '^/weatherforecast': {
                target,
                secure: false
            }
        },
        port: 57263,
        host: true,
        watch: {
            usePolling: true,
            interval: 1000
        },
        hmr: {
            clientPort: 57263
        }
    }
})
