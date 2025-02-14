import { BrowserRouter, Route, Routes } from "react-router-dom";
import Home from "./pages/Home";
import CaseEditor from "./pages/CaseEditor";
import './index.css';

export default function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/" element={<Home />} />
        <Route path="/edit/:companyId" element={<CaseEditor />} />
      </Routes>
    </BrowserRouter>
  );
}
