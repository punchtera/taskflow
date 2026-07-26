import { useCallback, useEffect, useState } from "react";
import {
  api,
  ApiError,
  type CreateTaskRequest,
  type TaskResponse,
  type UpdateTaskRequest,
} from "../api/client";

interface UseTasks {
  tasks: TaskResponse[];
  loading: boolean;
  error: string | null;
  reload: () => Promise<void>;
  create: (body: CreateTaskRequest) => Promise<void>;
  update: (id: string, body: UpdateTaskRequest) => Promise<void>;
  remove: (id: string) => Promise<void>;
}

/** Loads and mutates the current user's tasks, keeping local state in sync. */
export function useTasks(): UseTasks {
  const [tasks, setTasks] = useState<TaskResponse[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const reload = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      setTasks(await api.tasks.list());
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Could not load tasks.");
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    void reload();
  }, [reload]);

  const create = useCallback(async (body: CreateTaskRequest) => {
    const created = await api.tasks.create(body);
    setTasks((prev) => [created, ...prev]);
  }, []);

  const update = useCallback(async (id: string, body: UpdateTaskRequest) => {
    const updated = await api.tasks.update(id, body);
    setTasks((prev) => prev.map((t) => (t.id === id ? updated : t)));
  }, []);

  const remove = useCallback(async (id: string) => {
    await api.tasks.remove(id);
    setTasks((prev) => prev.filter((t) => t.id !== id));
  }, []);

  return { tasks, loading, error, reload, create, update, remove };
}
