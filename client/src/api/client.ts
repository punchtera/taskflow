// Single place for all backend calls. Requests go to /api and are proxied to the
// .NET host by Vite in dev (see vite.config.ts). The bearer token is injected
// here so components and hooks never deal with headers directly.

const BASE_URL = "/api";

let authToken: string | null = null;

/** Set (or clear) the bearer token used for subsequent requests. */
export function setAuthToken(token: string | null): void {
  authToken = token;
}

// ---- Types (mirror the server DTOs) ----

export const TaskStatus = { Todo: 0, InProgress: 1, Done: 2 } as const;
export type TaskStatus = (typeof TaskStatus)[keyof typeof TaskStatus];

export interface PingResponse {
  status: string;
  service: string;
  timestampUtc: string;
}

export interface AuthResponse {
  token: string;
  expiresAtUtc: string;
  userId: string;
  email: string;
  displayName: string;
}

export interface TaskResponse {
  id: string;
  title: string;
  description: string | null;
  status: TaskStatus;
  dueDateUtc: string | null;
  createdAtUtc: string;
  updatedAtUtc: string;
}

export interface CreateTaskRequest {
  title: string;
  description: string | null;
  status: TaskStatus;
  dueDateUtc: string | null;
}

export type UpdateTaskRequest = CreateTaskRequest;

export interface RegisterRequest {
  email: string;
  displayName: string;
  password: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

/** Thrown for any non-2xx response, carrying the HTTP status and a friendly message. */
export class ApiError extends Error {
  constructor(public status: number, message: string) {
    super(message);
    this.name = "ApiError";
  }
}

async function request<T>(path: string, init?: RequestInit): Promise<T> {
  const headers: Record<string, string> = { "Content-Type": "application/json" };
  if (authToken) headers.Authorization = `Bearer ${authToken}`;

  const response = await fetch(`${BASE_URL}${path}`, { ...init, headers: { ...headers, ...init?.headers } });

  if (!response.ok) {
    throw new ApiError(response.status, await extractError(response));
  }

  // 204 No Content (e.g. DELETE) has no body.
  if (response.status === 204) return undefined as T;
  return (await response.json()) as T;
}

async function extractError(response: Response): Promise<string> {
  try {
    const problem = await response.json();
    return problem.detail || problem.title || `Request failed (${response.status}).`;
  } catch {
    return `Request failed (${response.status}).`;
  }
}

export const api = {
  ping: () => request<PingResponse>("/ping"),

  auth: {
    login: (body: LoginRequest) =>
      request<AuthResponse>("/auth/login", { method: "POST", body: JSON.stringify(body) }),
    register: (body: RegisterRequest) =>
      request<AuthResponse>("/auth/register", { method: "POST", body: JSON.stringify(body) }),
  },

  tasks: {
    list: () => request<TaskResponse[]>("/tasks"),
    create: (body: CreateTaskRequest) =>
      request<TaskResponse>("/tasks", { method: "POST", body: JSON.stringify(body) }),
    update: (id: string, body: UpdateTaskRequest) =>
      request<TaskResponse>(`/tasks/${id}`, { method: "PUT", body: JSON.stringify(body) }),
    remove: (id: string) => request<void>(`/tasks/${id}`, { method: "DELETE" }),
  },
};
