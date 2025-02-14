import { BrowserRouter, Route, Routes } from "react-router-dom";
import Login from './Pages/LoginUI.jsx';
import KontaktaOss from "./Pages/Kontaktaoss.jsx";
import Arbetarsida from "./pages/Arbetarsida.jsx";
import Confirmationsida from "./Pages/confirmationsida.jsx";
import Redigeramall from "./Pages/redigeramall.jsx";
import Redigeramedarbetare from "./Pages/redigeramedarbetare.jsx";
<<<<<<< HEAD
import ChatHistory from "./pages/Chat.jsx"
import './index.css';
=======
import ProtectedRoute from './Components/ProtectedRoute.jsx';
import Adminsida from "./Pages/Adminsida.jsx";
>>>>>>> feature/Routing

export default function App() {
  return (
    <BrowserRouter>
      <Routes>
        {/* Public Routes */}
        <Route path="/" element={<Login />} />
        <Route path="/login" element={<Login />} />
        <Route path="/confirmation" element={<Confirmationsida />} />
        <Route path="/kontaktaoss" element={<KontaktaOss />} />
<<<<<<< HEAD
        <Route path="/arbetarsida/:company" element={<Arbetarsida />} />
=======

        {/* Skyddad routes */}
    <Route path="/arbetarsida" element={<ProtectedRoute><Arbetarsida /></ProtectedRoute>} />
    <Route path="/admin" element={<ProtectedRoute adminOnly={true}><Adminsida /></ProtectedRoute>} />



        {/* vet inte rikigt än Routes */}
>>>>>>> feature/Routing
        <Route path="/redigeramedarbetare" element={<Redigeramedarbetare />} />
        <Route path="/redigeramall" element={<Redigeramall />} />
        <Route path="Chat/:chatId" element={<ChatHistory />} />
      </Routes>
    </BrowserRouter>
  );
}
import Arbetarsida from "./pages/Arbetarsida.jsx";
import ProtectedRoute from './Components/ProtectedRoute.jsx';
import Adminsida from "./Pages/Adminsida.jsx";
import ChatHistory from "./pages/Chat.jsx"

        {/* Public Routes */}

        {/* Skyddad routes */}
    <Route path="/arbetarsida/:company" element={<ProtectedRoute><Arbetarsida /></ProtectedRoute>} />
    <Route path="/admin" element={<ProtectedRoute adminOnly={true}><Adminsida /></ProtectedRoute>} />



        {/* vet inte rikigt än Routes */}
        <Route path="Chat/:chatId" element={<ChatHistory />} />