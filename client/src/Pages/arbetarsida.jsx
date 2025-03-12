import React, { useEffect, useState } from "react";
import { Link, useParams } from "react-router-dom";
import assignTicket from "../Components/AssignTicket.jsx";
import {useNavigate} from "react-router";

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
    <div>
      <div>
        <input type={"checkbox"} onChange={getAllChats}/> <p>Get all chats?</p>
      </div>
      {data.map((chats, index) => (
        <div key={index} className={chats.csrep ? "openTicket" : "closedTicket"}> {/*Should be some kind of marker for when a ticket is closed here(grayed out?)*/}
          <small>Case Type: {chats.casetype}</small><br/>
          <small>Last message from: {chats.sender}</small><br/>
          <small>{chats.message} </small><br />
          <small>Sent: {chats.timestamp}</small><br />
          {chats.assignedCsRep != null && (
              <>
              <small>User assigned to this ticket: {chats.assignedCsRep}</small><br/>
              </>
            )}
          <Link to={`/Chat/${chats.chat}`}><button>Go to chat</button></Link>
          <button onClick={() => assignTicket(chats.chat, navigate)}>Go to chat and assign ticket</button>
        </div>
      ))}
    </div>  
  )
}

export default Arbetarsida;