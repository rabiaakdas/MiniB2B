import { BrowserRouter } from 'react-router-dom';
import { AuthProvider } from './auth/AuthContext';
import { AppRouter } from './routes/AppRouter';
import './styles/global.css';

export default function App() {
  return (
    <BrowserRouter>
      <AuthProvider>
        <AppRouter />
      </AuthProvider>
    </BrowserRouter>
  );
}
