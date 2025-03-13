import React, {use, useEffect, useState} from "react";
import { useParams } from "react-router-dom";
import { useSendChatAnswer } from "../Components/ChatAnswer.jsx";
import {useNavigate} from "react-router";
import '../styles/Chat.css';

const ChatHistory = () => {
  const { chatId } = useParams();
  const [data, setData] = useState([]);
  const [updateTicker, setUpdateTicker] = useState(0)
  const [isUserCsRep, setIsUserCsRep] = useState(false)
  const navigate = useNavigate();
  


  useEffect(() => {
    const GetChats = async () => {
      const response = await fetch(`/api/Chat/${chatId}`);
      const responseData = await response.json();
      setData(responseData);
    };
    GetChats();
  }, [updateTicker, chatId]);

  useEffect(() => {
    const GetIsUserCsRep = async () => {
    const csRepResponse = await fetch(`/api/IsUserCsRep`);
    const csRepResponseData = await csRepResponse.json();
    csRepResponseData === 'True' ? setIsUserCsRep(true) : setIsUserCsRep(false)
    };
    GetIsUserCsRep();
  }, []);
  
  const { message, setMessage, sendToBackend } = useSendChatAnswer();
  
  

  const updateSite = () => {
    setTimeout(() => {
      setUpdateTicker(updateTicker + 1)
    }, 200);
  }
  
  const handleResponse = async (sendToNextTicket) => {
    await sendToBackend()
    if (sendToNextTicket){
      await sendToNextOpenTicket()
    }  else {
      updateSite()
    }
  }
  
  const sendToNextOpenTicket = async () => {
    const nextChatResponse = await fetch(`/api/assignNextTicket`)
    const nextChatResponseData = await nextChatResponse.json();
    console.log(nextChatResponseData)
    if(nextChatResponseData != ""){
    navigate(`/Chat/${nextChatResponseData}`)
    }
    else {
      window.alert("no more chats to assign! Press ok to go back")
      navigate(`/Arbetarsida/`)
    }
  }

  

  return (
    <div className="page-container">
      <div className="chat-container">
        <div className="chat-messages">
          {data.map((chat, index) => (
            <div key={index} className={`message ${isUserCsRep ? (chat.csrep ? 'my-message' : 'other-message') : (chat.csrep ? 'other-message' : 'my-message' )}`}>
              <div className="message-content">
                <div className="message-sender">Avsändare: {chat.sender}</div>
                <div>{chat.message}</div>
                <div className="message-timestamp">Skickat: {chat.timestamp}</div>
              </div>
            </div>
          ))}
        </div>

        <form className="chat-form" onSubmit={(e) => e.preventDefault()} hidden={data.length === 0}>
          <input
            className="chat-input"
            id="message"
            value={message}
            placeholder="Enter your message..."
            onChange={(e) => setMessage(e.target.value)}
          />
          <button 
            className="send-button"
            type="submit" 
            onClick={() => handleResponse(false)} 
            disabled={message.length === 0}
          >
            Send
          </button>
          <button 
            className="send-button"
            type="submit" 
            onClick={() => handleResponse(true)} 
            hidden={!isUserCsRep} 
            disabled={message.length === 0}
          >
            Send and take next open ticket
          </button>
        </form>
      </div>
    </div>
  );
};

export default ChatHistory;
