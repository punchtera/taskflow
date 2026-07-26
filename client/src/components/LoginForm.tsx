import { useState, type FormEvent } from "react";
import { useAuth } from "../context/AuthContext";
import { ApiError } from "../api/client";

type Mode = "login" | "register";

export default function LoginForm() {
  const { login, register } = useAuth();
  const [mode, setMode] = useState<Mode>("login");
  const [email, setEmail] = useState("demo@taskflow.dev");
  const [displayName, setDisplayName] = useState("");
  const [password, setPassword] = useState("Passw0rd!");
  const [error, setError] = useState<string | null>(null);
  const [busy, setBusy] = useState(false);

  const isRegister = mode === "register";

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();
    setError(null);
    setBusy(true);
    try {
      if (isRegister) await register(email, displayName, password);
      else await login(email, password);
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Something went wrong. Please try again.");
    } finally {
      setBusy(false);
    }
  }

  function switchMode() {
    setMode(isRegister ? "login" : "register");
    setError(null);
  }

  return (
    <div className="auth">
      <div className="card auth-card">
        <h1>TaskFlow</h1>
        <p className="muted">{isRegister ? "Create your account" : "Sign in to your tasks"}</p>

        <form onSubmit={handleSubmit} className="form">
          <label className="field">
            <span>Email</span>
            <input
              type="email"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              required
              autoComplete="email"
            />
          </label>

          {isRegister && (
            <label className="field">
              <span>Display name</span>
              <input
                type="text"
                value={displayName}
                onChange={(e) => setDisplayName(e.target.value)}
                required
                autoComplete="name"
              />
            </label>
          )}

          <label className="field">
            <span>Password</span>
            <input
              type="password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              required
              minLength={isRegister ? 8 : undefined}
              autoComplete={isRegister ? "new-password" : "current-password"}
            />
          </label>

          {error && <p className="error" role="alert">{error}</p>}

          <button type="submit" className="btn btn-primary" disabled={busy}>
            {busy ? "Please wait…" : isRegister ? "Create account" : "Sign in"}
          </button>
        </form>

        <p className="muted switch">
          {isRegister ? "Already have an account?" : "New here?"}{" "}
          <button type="button" className="linkbtn" onClick={switchMode}>
            {isRegister ? "Sign in" : "Create one"}
          </button>
        </p>

        {!isRegister && (
          <p className="muted hint">Demo: demo@taskflow.dev / Passw0rd!</p>
        )}
      </div>
    </div>
  );
}
