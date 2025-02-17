import React, { useEffect, useState } from "react";
import { useParams } from "react-router-dom";
import { useSendChatAnswer } from "../Components/ChatAnswer.jsx";

const ChatHistory = () => {
  const { chatId } = useParams();
  const [data, setData] = useState([]);

  useEffect(() => {
    const GetChats = async () => {
      const response = await fetch(`/api/Chat/${chatId}`);
      const responseData = await response.json();
      setData(responseData);
    };
    GetChats();
  }, [chatId]);

  const { message, setMessage, email, setEmail, csrep, setCsrep, sendToBackend } = useSendChatAnswer();

  return (
    <div>
      <ul>
        {data.map((chat, index) => (
          <li key={index}>
            <small>{chat.sender} skrev: </small><br />
            <small>{chat.message}</small><br />
            <small>{chat.timestamp}</small>
          </li>
        ))}
      </ul>

      <form onSubmit={sendToBackend}>
        <input
          id="message"
          value={message}
          placeholder="Enter your message..."
          onChange={(e) => setMessage(e.target.value)}
        />
        <input
          id="email"
          value={email}
          placeholder="Enter your Email..."
          onChange={(e) => setEmail(e.target.value)}
        />
        <p>Answer from a CS rep?</p>
        <input
          id="csrep"
          type="checkbox"
          checked={csrep}
          onChange={(e) => setCsrep(e.target.checked)}
        />
        <button type="submit">Submit</button>
      </form>
    </div>
  );
};

export default ChatHistory;
