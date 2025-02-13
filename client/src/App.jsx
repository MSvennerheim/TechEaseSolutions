/*import { Navbar, Nav } from 'rsuite';
import { BrowserRouter as Router, Route, Routes } from "react-router-dom";
import CogIcon from '@rsuite/icons/legacy/Cog';
import './index.css';

import Adminsida from "./Pages/adminsida";
import Inloggningssida from "./Pages/inloggningssida";
import KontaktaOss from "./Pages/kontaktaoss";
import Arbetarsida from "./Pages/arbetarsida";
import ConfirmationSida from "./Pages/confirmationsida";
import Redigeramedarbetare from "./Pages/redigerarmedarbetare";
import RedigeraForm from "./Pages/RedigeraForm";

function App() {
    const user = { role: 'admin' }; //

    return (
        <Router>
            <Navbar>
                <Navbar.Brand href="/">TechEaseSolutions</Navbar.Brand>
                <Nav>
                    <Nav.Item href="/inloggning">Inloggning</Nav.Item>
                    <Nav.Item href="/kontaktaoss">Kontakta oss</Nav.Item>
                    {user?.role === 'admin' && (
                        <>
                            <Nav.Item href="/admin">Admin</Nav.Item>
                            <Nav.Item href="/redigeramall">Redigera Mall</Nav.Item>
                            <Nav.Item href="/redigeramedarbetare">Redigera Medarbetare</Nav.Item>
                        </>
                    )}
                </Nav>
                <Nav pullRight>
                    <Nav.Item icon={<CogIcon />}>Inställningar</Nav.Item>
                </Nav>
            </Navbar>

            <Routes>
                <Route path="/inloggning" element={<Inloggningssida />} />
                <Route path="/confirmation" element={<ConfirmationSida />} />
                <Route path="/kontaktaoss" element={<KontaktaOss />} />
                <Route path="/arbetarsida" element={<Arbetarsida />} />
                <Route path="/redigera-formular" element={<RedigeraForm />} />
                
                {user?.role === 'admin' && (
                    <>
                        <Route path="/admin" element={<Adminsida />} />
                        <Route path="/redigeramedarbetare" element={<Redigeramedarbetare />} />
                        <Route path="/redigeramall" element={<Redigeramall />} />
                    </>
                )}
            </Routes>
        </Router>
    );
}

export default App;*/

import { BrowserRouter as Router, Route, Routes } from "react-router-dom";
import RedigeraForm from "./Pages/RedigeraForm"; // ✅ Behåll bara denna

function App() {
    return (
        <Router>
            <Routes>
                <Route path="/redigera-formular" element={<RedigeraForm />} />
            </Routes>
        </Router>
    );
}

export default App;