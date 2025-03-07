import React, { useEffect, useState } from "react";
import { Link, useParams } from "react-router-dom";

const Arbetarsida = () => {
  const [data, setData] = useState([])
  const [updateTicker, setUpdateTicker] = useState(0)
  const [allChats, setAllChats] = useState(false)


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
          <small>{chats.message} </small><br />
          <small>{chats.timestamp}</small><br />
          <Link to={`/Chat/${chats.chat}`}><button>Go to chat</button></Link>
        </div>
      ))}
    </div>
  )


}

export default Arbetarsida;