import { useAuth } from "./context/AuthContext";
import LoginForm from "./components/LoginForm";
import TaskBoard from "./components/TaskBoard";

export default function App() {
  const { isAuthenticated } = useAuth();
  return isAuthenticated ? <TaskBoard /> : <LoginForm />;
}
