import React, { useEffect, useState } from "react";
import { Link, useParams } from "react-router-dom";

const Arbetarsida = () => {
  const { company } = useParams()
  const [data, setData] = useState([])
  useEffect(() => {
    const GetAllChats = async () => {
      const response = await fetch(`/api/Arbetarsida`)
      const responseData = await response.json()
      setData(responseData)
      console.log(responseData)
      console.log(data)
    }
    GetAllChats()
  }, [])


  





  return (
    <div>
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