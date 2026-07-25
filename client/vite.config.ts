import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";

// The Vite dev server runs on :5173 and proxies API calls to the .NET host on
// :5080, so the browser only ever talks to one origin during development.
// `build.outDir` points at the API's wwwroot so a production build is served
// by the .NET SPA fallback (see Program.cs).
export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    proxy: {
      "/api": {
        target: "http://localhost:5080",
        changeOrigin: true,
      },
    },
  },
  build: {
    outDir: "../src/TaskFlow.Api/wwwroot",
    emptyOutDir: true,
  },
});
