import { useEffect, useState } from "react";
import { api, type PingResponse } from "./api/client";

type Health = "pending" | "ok" | "down";

export default function App() {
  const [health, setHealth] = useState<Health>("pending");
  const [ping, setPing] = useState<PingResponse | null>(null);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    api
      .ping()
      .then((res) => {
        setPing(res);
        setHealth("ok");
      })
      .catch((err: unknown) => {
        setError(err instanceof Error ? err.message : "Unknown error");
        setHealth("down");
      });
  }, []);

  const label =
    health === "ok" ? "API online" : health === "down" ? "API unreachable" : "Checking…";

  return (
    <main className="app">
      <h1>TaskFlow</h1>
      <p className="muted">
        Full-stack task manager — .NET 10 Clean Architecture + React. This starter
        screen verifies the frontend can reach the backend. Task CRUD and auth
        arrive in the next iterations (see docs/PLAN.md).
      </p>

      <div className="card">
        <div className="status">
          <span className={`dot dot--${health === "ok" ? "ok" : health === "down" ? "down" : "pending"}`} />
          {label}
        </div>

        {ping && (
          <p className="muted" style={{ marginBottom: 0 }}>
            {ping.service} responded at {new Date(ping.timestampUtc).toLocaleTimeString()}
          </p>
        )}

        {error && (
          <p className="muted" style={{ marginBottom: 0 }}>
            {error} — is the .NET API running on http://localhost:5080?
          </p>
        )}
      </div>
    </main>
  );
}
