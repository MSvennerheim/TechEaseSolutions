import React, { useEffect, useState } from "react";
import { Link, useParams } from "react-router-dom";

const Arbetarsida = () => {
  const [data, setData] = useState([])
  const [updateTicker, setUpdateTicker] = useState(0)


  useEffect(() => {
    const GetAllChats = async () => {
      const response = await fetch(`/api/arbetarsida/`, {
        method: "POST",
            headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify({ checked: isChecked }),
      })
      const responseData = await response.json()
      setData(responseData)
      console.log(responseData)
    }
    GetAllChats()
  }, [updateTicker])

  const updateSite = () => {
    setTimeout(() => {
      setUpdateTicker(updateTicker + 1)
    }, 1000);
  }

  const handleCheckboxChange = (event) => {
    const isChecked = event.target.checked;
    updateSite()
  }

  

  return (
    <div>
      <div>
        <input type={"checkbox"} onChange={handleCheckboxChange}/>
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