import { BrowserRouter, Route, Routes } from "react-router-dom";
import Login from './pages/LoginUI.jsx';

import Adminsida from "./pages/Adminsida.jsx";
import Arbetarsida from "./pages/Arbetarsida.jsx";
import KontaktaOss from "./pages/Kontaktaoss.jsx";
import Confirmationsida from "./pages/confirmationsida.jsx";
import Redigeramedarbetare from "./pages/redigeramedarbetare.jsx";
import ChatHistory from "./pages/Chat.jsx"
import EditForm from "./pages/CaseEditor.jsx"
import './index.css';
import ProtectedRoute from './Components/ProtectedRoute.jsx';


export default function App() {
  return (
    <BrowserRouter>
      <Routes>
        {/* Public Routes */}
        <Route path="/" element={<Login />} />
        <Route path="/login" element={<Login />} />
        <Route path="/confirmation" element={<Confirmationsida />} />

        <Route path="/kontaktaoss/:companyName" element={<KontaktaOss />} />

        <Route path="/kontaktaoss" element={<KontaktaOss />} />
        {/* Skyddad routes */}
        <Route path="/arbetarsida/:company" element={<ProtectedRoute><Arbetarsida /></ProtectedRoute>} />
        <Route path="/admin/:company" element={<ProtectedRoute adminOnly={true}><Adminsida /></ProtectedRoute>} />
        {/* vet inte rikigt än Routes */}
        
        <Route path="/redigeramedarbetare" element={<Redigeramedarbetare />} />
        <Route path="/edit/:companyId" element={<EditForm />} />
        <Route path="Chat/:chatId" element={<ChatHistory />} />
      </Routes>
    </BrowserRouter>
  );
}
