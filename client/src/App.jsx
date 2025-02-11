import { Navbar, Nav } from 'rsuite';
import { BrowserRouter, Route, Router, Routes } from "react-router";
import CogIcon from '@rsuite/icons/legacy/Cog';
import './index.css';
import ShowChat from './pages/Chat.jsx'


export default function App() {

  return (<section>
    <h1>Hello</h1>
    <ShowChat />
  </section>)

}
