// Thin fetch wrapper. All requests go to the same origin under /api and are
// proxied to the .NET host by Vite in dev (see vite.config.ts). As auth is added
// (Iteration 3), the bearer token will be attached here in one place.

const BASE_URL = "/api";

export interface PingResponse {
  status: string;
  service: string;
  timestampUtc: string;
}

async function request<T>(path: string, init?: RequestInit): Promise<T> {
  const response = await fetch(`${BASE_URL}${path}`, {
    headers: { "Content-Type": "application/json" },
    ...init,
  });

  if (!response.ok) {
    throw new Error(`Request failed: ${response.status} ${response.statusText}`);
  }

  return (await response.json()) as T;
}

export const api = {
  ping: () => request<PingResponse>("/ping"),
};
