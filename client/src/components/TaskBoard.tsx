import { useState } from "react";
import { useAuth } from "../context/AuthContext";
import { useTasks } from "../hooks/useTasks";
import { STATUS_LABELS, STATUS_ORDER } from "../utils/format";
import TaskForm from "./TaskForm";
import TaskCard from "./TaskCard";

export default function TaskBoard() {
  const { user, logout } = useAuth();
  const { tasks, loading, error, reload, create, update, remove } = useTasks();
  const [showForm, setShowForm] = useState(false);

  return (
    <div className="board">
      <header className="topbar">
        <div>
          <h1>TaskFlow</h1>
          <p className="muted">{user?.email}</p>
        </div>
        <button className="btn" onClick={logout}>Sign out</button>
      </header>

      <section className="board-toolbar">
        <button className="btn btn-primary" onClick={() => setShowForm((v) => !v)}>
          {showForm ? "Close" : "+ New task"}
        </button>
      </section>

      {showForm && (
        <div className="card">
          <TaskForm
            submitLabel="Add task"
            onSubmit={async (body) => {
              await create(body);
              setShowForm(false);
            }}
          />
        </div>
      )}

      {loading && <p className="muted">Loading tasks…</p>}

      {error && (
        <div className="card error-card">
          <p className="error" role="alert">{error}</p>
          <button className="btn" onClick={() => void reload()}>Retry</button>
        </div>
      )}

      {!loading && !error && (
        <div className="columns">
          {STATUS_ORDER.map((status) => {
            const column = tasks.filter((t) => t.status === status);
            return (
              <div key={status} className="column">
                <h2 className="column-title">
                  {STATUS_LABELS[status]} <span className="count">{column.length}</span>
                </h2>
                {column.length === 0 ? (
                  <p className="muted empty">Nothing here.</p>
                ) : (
                  <ul className="task-list">
                    {column.map((task) => (
                      <TaskCard key={task.id} task={task} onUpdate={update} onDelete={remove} />
                    ))}
                  </ul>
                )}
              </div>
            );
          })}
        </div>
      )}
    </div>
  );
}
