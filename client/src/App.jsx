import { Navbar, Nav } from 'rsuite';
import { BrowserRouter, Route, Router, Routes } from "react-router";
import CogIcon from '@rsuite/icons/legacy/Cog';
import './index.css';
import Adminsida from "./adminsida";
import Inloggningssida from "./inloggningssida";
import KontaktaOss from "./kontaktaoss";
import Arbetarsida from "./arbetarsida";
import ConfirmationSida from "./confirmationsida";
import Redigeramall from "./redigeramall";
import Redigeramedarbetare from "./redigerarmedarbetare";

function App() {
  return
  <Router>
<Routes>
      <Route path="/inloggning" element={<Inloggningssida />} />
      {user?.role === 'admin' && (
        <>      
          <Route path="/admin" element={<Adminsida />} />
          <Route path="/Redigerarmedarbetare" element={<Redigerarmedarbetare />} />
          <Route path="/Redigeramall" element={<Redigeramall />} />
     </>
      )}
<Route path="/admin" element={<Adminsida />} />
<Route path="/confirmation" element={<ConfirmationSida />} />
<Route path="/Kontakta oss" element={<KontaktaOss />} />
<Route path="/arbetarsida" element={<Arbetarsida />} />
<Route path="/Redigerarmedarbetare" element={<Redigerarmedarbetare/>} />
      <Route path="/Redigeramall" element={<Redigeramall />} /> 
      <Route path="/Redigeramall" element={<Redigeramall />} />


</Routes>
</Router>
}











/*
const App = () => (
  <Navbar>
    <Navbar.Brand href="#">TechEaseSolutions</Navbar.Brand>
    <Nav>
      <Nav.Item>Home</Nav.Item>
      <Nav.Item>News</Nav.Item>
      <Nav.Item>Products</Nav.Item>
      <Nav.Menu title="About">
        <Nav.Item>Company</Nav.Item>
        <Nav.Item>Team</Nav.Item>
        <Nav.Menu title="Contact">
          <Nav.Item>Via email</Nav.Item>
          <Nav.Item>Via telephone</Nav.Item>
        </Nav.Menu>
      </Nav.Menu>
    </Nav>
    <Nav pullRight>
      <Nav.Item icon={<CogIcon />}>Settings</Nav.Item>
    </Nav>
  </Navbar>
  );
*/