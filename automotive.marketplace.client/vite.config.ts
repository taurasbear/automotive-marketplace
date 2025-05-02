import path from "path";
import { TanStackRouterVite } from "@tanstack/router-plugin/vite";
import tailwindcss from "@tailwindcss/vite";
import { fileURLToPath, URL } from "node:url";

import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";
import { env } from "process";

const target = env.VITE_API_URL ?? "http://automotive.marketplace.server:8080";

// https://vitejs.dev/config/
export default defineConfig({
  plugins: [
    TanStackRouterVite({ target: "react", autoCodeSplitting: true }),
    react(),
    tailwindcss(),
  ],
  resolve: {
    alias: {
      "@": fileURLToPath(new URL("./src", import.meta.url)),
    },
  },
  server: {
    proxy: {
      "^/api": {
        target,
        secure: false,
        changeOrigin: false,
        cookieDomainRewrite: {
          "*": "",
        },
      },
    },
    port: 57263,
    host: true,
    watch: {
      usePolling: true,
      interval: 1000,
    },
    hmr: {
      clientPort: 57263,
    },
  },
});
