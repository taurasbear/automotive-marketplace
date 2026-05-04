/// <reference types="vitest" />
import tailwindcss from "@tailwindcss/vite";
import { TanStackRouterVite } from "@tanstack/router-plugin/vite";
import react from "@vitejs/plugin-react";
import { fileURLToPath, URL } from "node:url";
import { defineConfig, loadEnv } from "vite";

// https://vitejs.dev/config/
export default defineConfig(({ mode }) => {
  const env = loadEnv(mode, process.cwd(), "");
  const target =
    env.VITE_APP_PROXY_API_TARGET ??
    "http://automotive.marketplace.server:8080";

  return {
    plugins: [
      TanStackRouterVite({
        target: "react",
        autoCodeSplitting: true,
        routesDirectory: "./src/app/routes",
      }),
      react(),
      tailwindcss(),
    ],
    resolve: {
      alias: {
        "@": fileURLToPath(new URL("./src", import.meta.url)),
      },
    },
    server: {
      proxy:
        mode === "development"
          ? {
              "^/api": {
                target,
                secure: false,
                changeOrigin: false,
                ws: true,
                rewrite: (path) => path.replace(/^\/api/, ""),
                cookieDomainRewrite: {
                  "*": "",
                },
              },
            }
          : undefined,
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
    test: {
      globals: true,
      environment: "jsdom",
      setupFiles: ["./src/test/setup.ts"],
      clearMocks: true,
      coverage: {
        provider: "v8",
        reporter: ["text", "html"],
        reportsDirectory: "./coverage",
      },
    },
  };
});
