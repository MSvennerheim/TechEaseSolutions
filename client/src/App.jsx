import { BrowserRouter, Route, Routes } from "react-router-dom";
import Login from './Pages/LoginUI.jsx';
import Adminsida from "./Pages/Adminsida.jsx";
import KontaktaOss from "./Pages/Kontaktaoss.jsx";
import Arbetarsida from "./Pages/Arbetarsida.jsx";
import Confirmationsida from "./Pages/confirmationsida.jsx";
import Redigeramall from "./Pages/redigeramall.jsx";
import Redigeramedarbetare from "./Pages/redigeramedarbetare.jsx";
import './index.css';

export default function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/" element={<Login />} />
        <Route path="/login" element={<Login />} />
        <Route path="/admin" element={<Adminsida />} />
        <Route path="/confirmation" element={<Confirmationsida />} />
        <Route path="/kontaktaoss" element={<KontaktaOss />} />
        <Route path="/arbetarsida" element={<Arbetarsida />} />
        <Route path="/redigeramedarbetare" element={<Redigeramedarbetare />} />
        <Route path="/redigeramall" element={<Redigeramall />} />
      </Routes>
    </BrowserRouter>
  );
}
