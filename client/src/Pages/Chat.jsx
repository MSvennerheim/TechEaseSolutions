import React, { useEffect, useState } from 'react';

export default function ShowChat() {
  const [chatData, setChatData] = useState([])
  const chatId = 1
  useEffect(() => {
    fetch(`http://localhost:5000/GetChatHistory/${chatId}`)


  })



  return (
    <div>
      <h2>Heres chat</h2>
    </div>
  )

}