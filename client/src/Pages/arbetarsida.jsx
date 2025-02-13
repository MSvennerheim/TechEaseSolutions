import React, { useEffect, useState } from "react";
import { useParams } from "react-router-dom";

const Arbetarsida = () => {
  const { company } = useParams()
  const [data, setData] = useState([])
  useEffect(() => {
    const GetAllChats = async () => {
      const response = await fetch(`http://localhost:5000/arbetarsida/${company}`)
      const responseData = await response.json()
      setData(responseData)
      console.log(responseData)
    }
    GetAllChats()
  }, [])
  return (
    <div>
      <ul>
        {data.map((chats, index) => (
          <li key={index}>
            <small>{chats.chat}</small><br />
            <small>{chats.message} </small><br />
            <small>{chats.timestamp}</small><br />
          </li>
        ))}
      </ul>
    </div>
  )
}

export default Arbetarsida;