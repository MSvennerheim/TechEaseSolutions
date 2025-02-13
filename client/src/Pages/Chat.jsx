import React, { useEffect, useState } from "react";
import { useParams } from "react-router-dom";

const ChatHistory = () => {
  const { chatId } = useParams()
  const [data, setData] = useState([])
  useEffect(() => {
    const GetChats = async () => {
      const response = await fetch(`http://localhost:5000/Chat/${chatId}`)
      const responseData = await response.json()
      setData(responseData)
      //console.log(responseData)
    }

    GetChats()

  }, [])
  return (
    <div>
      <ul>
        {data.map((chat, index) => (
          <li key={index}>
            <small>{chat.sender} skrev: </small><br /> {/*fix this to name later, but email is good enough for now... Also need to add check if it's a cs reprasentative */}
            <small>{chat.message}  </small><br />
            <small>{chat.timestamp}</small>
          </li>
        ))}
      </ul>
    </div>
  );


}
export default ChatHistory;
