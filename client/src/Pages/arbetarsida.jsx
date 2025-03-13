import React, { useEffect, useState } from "react";
import { Link, useParams } from "react-router-dom";
import assignTicket from "../Components/AssignTicket.jsx";
import {useNavigate} from "react-router";
import "../Styles/Arbetarsida.css";

const Arbetarsida = () => {
  const [data, setData] = useState([])
  const [updateTicker, setUpdateTicker] = useState(0)
  const [allChats, setAllChats] = useState(false)
  const navigate = useNavigate();

  useEffect(() => {
    const GetChats = async () => {
      const response = await fetch(`/api/arbetarsida/`, {
        method: "POST",
            headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify({ getAllChats: allChats }),
      })
      const responseData = await response.json()
      setData(responseData)
      console.log(responseData)
    }
    GetChats()
  }, [updateTicker])

  const updateSite = () => {
    setTimeout(() => {
      setUpdateTicker(updateTicker + 1)
    }, 1000);
  }

  const getAllChats = (event) => {
    setAllChats(event.target.checked);
    updateSite()
  }

  
  return (
    <div className="arbetarsida-container">
      <div className="filter-section">
        <div className="filter-checkbox">
          <input type={"checkbox"} onChange={getAllChats}/>
          <p>Get all chats?</p>
        </div>
      </div>
      <div className="ticket-list">
        {data.map((chats, index) => (
          <div key={index} className={`ticket ${chats.csrep ? "open-ticket" : "closed-ticket"}`}>
            <div className="ticket-content">
              <div className="ticket-body">
                <small className="ticket-meta">Case Type: {chats.casetype}</small>
                <small className="ticket-meta">Last message from: {chats.sender}</small>
                <small className="ticket-message">{chats.message}</small>
                <small className="ticket-meta">Sent: {chats.timestamp}</small>
                {chats.assignedCsRep != null && (
                  <small className="ticket-meta">User assigned to this ticket: {chats.assignedCsRep}</small>
                )}
              </div>
              <div className="ticket-actions">
                <Link to={`/Chat/${chats.chat}`} className="view-ticket-btn">Go to chat</Link>
                <button onClick={() => assignTicket(chats.chat, navigate)} className="view-ticket-btn">
                  Go to chat and assign ticket
                </button>
              </div>
            </div>
          </div>
        ))}
      </div>
    </div>  
  )
}

export default Arbetarsida;