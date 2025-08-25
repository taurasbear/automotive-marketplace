import { TanStackRouterVite } from "@tanstack/router-plugin/vite";
import tailwindcss from "@tailwindcss/vite";
import { fileURLToPath, URL } from "node:url";
import { defineConfig, loadEnv } from "vite";
import react from "@vitejs/plugin-react";

// https://vitejs.dev/config/
export default defineConfig(({ mode }) => {
  const env = loadEnv(mode, process.cwd(), "");
  const target =
    env.VITE_APP_PROXY_API_TARGET ??
    "http://automotive.marketplace.server:8080";

  return {
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
      proxy:
        mode === "development"
          ? {
              "^/api": {
                target,
                secure: false,
                changeOrigin: false,
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
  };
});
