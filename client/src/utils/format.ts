import { TaskStatus } from "../api/client";

export const STATUS_LABELS: Record<TaskStatus, string> = {
  [TaskStatus.Todo]: "To do",
  [TaskStatus.InProgress]: "In progress",
  [TaskStatus.Done]: "Done",
};

export const STATUS_ORDER: TaskStatus[] = [TaskStatus.Todo, TaskStatus.InProgress, TaskStatus.Done];

/** ISO string → "YYYY-MM-DD" for <input type="date">, or "" when null. */
export function toDateInputValue(iso: string | null): string {
  return iso ? iso.slice(0, 10) : "";
}

/** "YYYY-MM-DD" from a date input → ISO at UTC midnight, or null when empty. */
export function fromDateInputValue(value: string): string | null {
  return value ? new Date(`${value}T00:00:00Z`).toISOString() : null;
}

/** Human-friendly due date, or null when none. */
export function formatDueDate(iso: string | null): string | null {
  if (!iso) return null;
  return new Date(iso).toLocaleDateString(undefined, { year: "numeric", month: "short", day: "numeric" });
}

export function isOverdue(iso: string | null, status: TaskStatus): boolean {
  if (!iso || status === TaskStatus.Done) return false;
  const due = new Date(iso);
  const today = new Date();
  today.setHours(0, 0, 0, 0);
  return due < today;
}
