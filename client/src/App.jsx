import { BrowserRouter, Route, Routes } from "react-router-dom";
import Login from './Pages/LoginUI.jsx';
import KontaktaOss from "./Pages/Kontaktaoss.jsx";
import Arbetarsida from "./Pages/Arbetarsida.jsx";
import Confirmationsida from "./Pages/confirmationsida.jsx";
import Redigeramall from "./Pages/redigeramall.jsx";
import Redigeramedarbetare from "./Pages/redigeramedarbetare.jsx";
import ProtectedRoute from './Components/ProtectedRoute.jsx';
import Adminsida from "./Pages/Adminsida.jsx";

export default function App() {
  return (
    <BrowserRouter>
      <Routes>
        {/* Public Routes */}
        <Route path="/" element={<Login />} />
        <Route path="/login" element={<Login />} />
        <Route path="/confirmation" element={<Confirmationsida />} />
        <Route path="/kontaktaoss" element={<KontaktaOss />} />

        {/* Skyddad routes */}
    <Route path="/arbetarsida" element={<ProtectedRoute><Arbetarsida /></ProtectedRoute>} />
<Route path="/admin" element={<ProtectedRoute><Adminsida /></ProtectedRoute>} />


        {/* vet inte rikigt än Routes */}
        <Route path="/redigeramedarbetare" element={<Redigeramedarbetare />} />
        <Route path="/redigeramall" element={<Redigeramall />} />
      </Routes>
    </BrowserRouter>
  );
}