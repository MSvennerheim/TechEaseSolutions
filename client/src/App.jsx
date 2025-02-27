import { BrowserRouter, Route, Routes } from "react-router-dom";
import Login from './Pages/LoginUI.jsx';

import Adminsida from "./Pages/Adminsida.jsx";
import Arbetarsida from "./Pages/Arbetarsida.jsx";
import KontaktaOss from "./Pages/Kontaktaoss.jsx";
import Confirmationsida from "./Pages/confirmationsida.jsx";
import CaseEditor from "./Pages/CaseEditor.jsx";
import Redigeramedarbetare from "./Pages/redigeramedarbetare.jsx";
import ChatHistory from "./pages/Chat.jsx"
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
        <Route path="/redigeramall" element={<CaseEditor />} />
        <Route path="/kontaktaoss" element={<KontaktaOss />} />
        {/* Skyddad routes */}
        <Route path="/arbetarsida/:company" element={<ProtectedRoute><Arbetarsida /></ProtectedRoute>} />
        <Route path="/admin/:company" element={<ProtectedRoute adminOnly={true}><Adminsida /></ProtectedRoute>} />
        {/* vet inte rikigt än Routes */}

        <Route path="/redigeramedarbetare" element={<Redigeramedarbetare />} />
        
        <Route path="Chat/:chatId" element={<ChatHistory />} />
      </Routes>
    </BrowserRouter>
  );
}

