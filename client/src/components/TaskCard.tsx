import { useState } from "react";
import { TaskStatus, type CreateTaskRequest, type TaskResponse } from "../api/client";
import { STATUS_LABELS, STATUS_ORDER, formatDueDate, isOverdue } from "../utils/format";
import TaskForm from "./TaskForm";

interface Props {
  task: TaskResponse;
  onUpdate: (id: string, body: CreateTaskRequest) => Promise<void>;
  onDelete: (id: string) => Promise<void>;
}

export default function TaskCard({ task, onUpdate, onDelete }: Props) {
  const [editing, setEditing] = useState(false);
  const [busy, setBusy] = useState(false);

  if (editing) {
    return (
      <li className="task-card">
        <TaskForm
          initial={task}
          submitLabel="Save changes"
          onCancel={() => setEditing(false)}
          onSubmit={async (body) => {
            await onUpdate(task.id, body);
            setEditing(false);
          }}
        />
      </li>
    );
  }

  const due = formatDueDate(task.dueDateUtc);
  const overdue = isOverdue(task.dueDateUtc, task.status);

  async function changeStatus(status: TaskStatus) {
    setBusy(true);
    try {
      await onUpdate(task.id, {
        title: task.title,
        description: task.description,
        status,
        dueDateUtc: task.dueDateUtc,
      });
    } finally {
      setBusy(false);
    }
  }

  async function handleDelete() {
    if (!confirm(`Delete “${task.title}”?`)) return;
    setBusy(true);
    try {
      await onDelete(task.id);
    } finally {
      setBusy(false);
    }
  }

  return (
    <li className="task-card">
      <h3 className="task-title">{task.title}</h3>
      {task.description && <p className="task-desc">{task.description}</p>}
      {due && (
        <p className={`task-due ${overdue ? "task-due--overdue" : ""}`}>
          Due {due}{overdue ? " · overdue" : ""}
        </p>
      )}

      <div className="task-actions">
        <select
          aria-label="Change status"
          value={task.status}
          disabled={busy}
          onChange={(e) => changeStatus(Number(e.target.value) as TaskStatus)}
        >
          {STATUS_ORDER.map((s) => (
            <option key={s} value={s}>{STATUS_LABELS[s]}</option>
          ))}
        </select>
        <button className="btn btn-sm" onClick={() => setEditing(true)} disabled={busy}>Edit</button>
        <button className="btn btn-sm btn-danger" onClick={handleDelete} disabled={busy}>Delete</button>
      </div>
    </li>
  );
}
