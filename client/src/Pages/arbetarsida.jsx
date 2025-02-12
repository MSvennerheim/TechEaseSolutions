import React, { useEffect, useState } from "react";
import { useParams } from "react-router-dom";

const CompanyChats = () => {
  const { company } = useParams()
  const { data, setData } = useState([])
  useEffect(() => {
    const GetAllChats = async () => {
      const response = await fetch(`http://localhost/arbetarsida/${company}`)
      const responseData = await response.json()
      setData(responseData)
    }
    CompanyChats()
  }, [])
  return (
    <div>
      <ul>
        {data.map((chats, index) => (
          <li key={index}>
            <small>{chats.sender} skrev: </small><br />
            <small>{chats.timestamp}</small>
          </li>
        ))}
      </ul>
    </div>
  )
}

export default arbetarsida;