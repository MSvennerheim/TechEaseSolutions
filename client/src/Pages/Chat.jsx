import React, { useEffect, useState } from "react";
import { useParams } from "react-router-dom";

const ChatHistory = () => {
  const { chatId } = useParams();
  const [data, setData] = useState([]);

  useEffect(() => {
    const GetChats = async () => {
      try {
        const response = await fetch(`/api/Chat/${chatId}`);
        if (!response.ok) {
          throw new Error("Failed to fetch chat history");
        }
        const responseData = await response.json();
        console.log("Chat History Response:", responseData); 
        setData(responseData);
      } catch (error) {
        console.error("Error fetching chat history:", error);
      }
    };

    GetChats();
  }, [chatId]);

  return (
    <div>
      <ul>
        {data.map((chat, index) => (
          <li key={index}>
            <small>{chat.sender} skrev: </small>
            <br />
            <small>{chat.message}</small>
            <br />
            <small>{chat.timestamp}</small>
          </li>
        ))}
      </ul>
    </div>
  );
};

export default ChatHistory;