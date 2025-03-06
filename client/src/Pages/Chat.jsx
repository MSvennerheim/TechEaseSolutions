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
    <>
    <div id="header">
      <h1>TechEaseSolutions</h1>
    </div>
    <div id="chatwrap">
      <ul>
        {data.map((chat, index) => (
          <li key={index} id="message">
            <small>{chat.sender} skrev: </small><br />
            <small>{chat.message}</small><br />
            <small>{chat.timestamp}</small>
          </li>
        ))}
      </ul>

      <form onSubmit={sendToBackend}>
        <input
          id="entermessage"
          value={message}
          placeholder="Enter your message..."
          onChange={(e) => setMessage(e.target.value)}
        />
        <button id="submitbutton" type="submit" onClick={updateSite} >Submit</button>
      </form>
    </div>
    </>
  );
};

export default ChatHistory;
