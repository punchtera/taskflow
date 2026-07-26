import { useState, type FormEvent } from "react";
import { ApiError, TaskStatus, type CreateTaskRequest, type TaskResponse } from "../api/client";
import { STATUS_LABELS, STATUS_ORDER, fromDateInputValue, toDateInputValue } from "../utils/format";

interface Props {
  initial?: TaskResponse;
  submitLabel: string;
  onSubmit: (body: CreateTaskRequest) => Promise<void>;
  onCancel?: () => void;
}

export default function TaskForm({ initial, submitLabel, onSubmit, onCancel }: Props) {
  const [title, setTitle] = useState(initial?.title ?? "");
  const [description, setDescription] = useState(initial?.description ?? "");
  const [status, setStatus] = useState<TaskStatus>(initial?.status ?? TaskStatus.Todo);
  const [dueDate, setDueDate] = useState(toDateInputValue(initial?.dueDateUtc ?? null));
  const [error, setError] = useState<string | null>(null);
  const [busy, setBusy] = useState(false);

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();
    setError(null);
    setBusy(true);
    try {
      await onSubmit({
        title: title.trim(),
        description: description.trim() ? description.trim() : null,
        status,
        dueDateUtc: fromDateInputValue(dueDate),
      });
      if (!initial) {
        setTitle("");
        setDescription("");
        setStatus(TaskStatus.Todo);
        setDueDate("");
      }
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Could not save the task.");
    } finally {
      setBusy(false);
    }
  }

  return (
    <form onSubmit={handleSubmit} className="form task-form">
      <label className="field">
        <span>Title</span>
        <input value={title} onChange={(e) => setTitle(e.target.value)} required maxLength={200} />
      </label>

      <label className="field">
        <span>Description</span>
        <textarea
          value={description}
          onChange={(e) => setDescription(e.target.value)}
          rows={2}
          maxLength={2000}
        />
      </label>

      <div className="form-row">
        <label className="field">
          <span>Status</span>
          <select value={status} onChange={(e) => setStatus(Number(e.target.value) as TaskStatus)}>
            {STATUS_ORDER.map((s) => (
              <option key={s} value={s}>{STATUS_LABELS[s]}</option>
            ))}
          </select>
        </label>

        <label className="field">
          <span>Due date</span>
          <input type="date" value={dueDate} onChange={(e) => setDueDate(e.target.value)} />
        </label>
      </div>

      {error && <p className="error" role="alert">{error}</p>}

      <div className="form-actions">
        <button type="submit" className="btn btn-primary" disabled={busy}>
          {busy ? "Saving…" : submitLabel}
        </button>
        {onCancel && (
          <button type="button" className="btn" onClick={onCancel} disabled={busy}>
            Cancel
          </button>
        )}
      </div>
    </form>
  );
}
