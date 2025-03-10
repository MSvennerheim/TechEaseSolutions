import React, { useEffect, useState } from "react";
import { useParams } from "react-router-dom";
import { useSendChatAnswer } from "../Components/ChatAnswer.jsx";

const ChatHistory = () => {
  const { chatId } = useParams();
  const [data, setData] = useState([]);
  const [updateTicker, setUpdateTicker] = useState(0)


  useEffect(() => {
    const GetChats = async () => {
      const response = await fetch(`/api/Chat/${chatId}`);
      const responseData = await response.json();
      setData(responseData);
    };
    GetChats();
  }, [updateTicker]);

  const { message, setMessage, sendToBackend } = useSendChatAnswer();


  const updateSite = () => {
    setTimeout(() => {
      setUpdateTicker(updateTicker + 1)
    }, 1000);
  }

  return (
    <div>
      <ul>
        {data.map((chat, index) => (
          <div key={index}>
            <small>{chat.sender} skrev: </small><br />
            <small>{chat.message}</small><br />
            <small>{chat.timestamp}</small>
          </div>
        ))}
      </ul>

      <form onSubmit={sendToBackend} style={{ opacity: data.length === 0 ? 0.5 : 1 }} >
        <input
          id="message"
          value={message}
          placeholder="Enter your message..."
          onChange={(e) => setMessage(e.target.value)}
        />
        <button type="submit" onClick={updateSite} disabled={data.length === 0} >Submit</button>
      </form>
    </div>
  );
};

export default ChatHistory;
