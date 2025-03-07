import { BrowserRouter, Route, Routes } from "react-router-dom";
import Login from './pages/LoginUI.jsx';
import Redigeramedarbetare from "./pages/redigeramedarbetare.jsx";
import Adminsida from "./Pages/Adminsida.jsx";
import Arbetarsida from "./Pages/Arbetarsida.jsx";
import KontaktaOss from "./Pages/kontaktaoss.jsx";
import Confirmationsida from "./Pages/confirmationsida.jsx";
import ChatHistory from "./pages/Chat.jsx"
import PasswordresetForm from "./Pages/PasswordresetForm.jsx";
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
        <Route path="/reset-password" element={<PasswordresetForm />} />
        {/* Skyddad routes */}
        <Route path="/Arbetarsida" element={<ProtectedRoute><Arbetarsida /></ProtectedRoute>} />
        <Route path="/admin" element={<ProtectedRoute adminOnly={true}><Adminsida /></ProtectedRoute>} />
        {/* vet inte rikigt än Routes */}
          

        <Route path="/CaseEditor" element={<EditForm />} />
        

        <Route path="/redigeramedarbetare" element={<Redigeramedarbetare />} />
          
        <Route path="Chat/:chatId" element={<ChatHistory />} />
      </Routes>
    </BrowserRouter>
  );
}

