import React, { useEffect, useState } from "react";
import { useParams } from "react-router-dom";
import { useSendChatAnswer } from "../Components/ChatAnswer.jsx";
import '../styles/Chat.css';

const ChatHistory = () => {
  const { chatId } = useParams();
  const [data, setData] = useState([]);
  const [updateTicker, setUpdateTicker] = useState(0);
  const [userEmail, setUserEmail] = useState('');

  useEffect(() => {
    const fetchUserEmail = async () => {
      try {
        const response = await fetch('/api/check-session');
        if (response.ok) {
          const userData = await response.json();
          setUserEmail(userData.email);
        }
      } catch (error) {
        console.error('Failed to fetch user email:', error);
      }
    };

    fetchUserEmail();
  }, []);

  useEffect(() => {
    const GetChats = async () => {
      try {
        const response = await fetch(`/api/Chat/${chatId}`);
        const responseText = await response.text();
        console.log('Raw response text:', responseText);
        
        try {
          const parsedData = JSON.parse(responseText);
          console.log('Parsed data:', parsedData);
          
          if (Array.isArray(parsedData)) {
            setData(parsedData);
          } else if (parsedData.error) {
            console.error('Server error:', parsedData.error);
            setData([]);
          } else {
            console.error('Unexpected response format:', parsedData);
            setData([]);
          }
        } catch (e) {
          console.error('Failed to parse response:', e);
          setData([]);
        }
      } catch (error) {
        console.error('Failed to fetch chat messages:', error);
        setData([]);
      }
    };
    GetChats();
  }, [updateTicker, chatId]);

  const { message, setMessage, sendToBackend } = useSendChatAnswer();

  const updateSite = () => {
    setTimeout(() => {
      setUpdateTicker(updateTicker + 1)
    }, 1000);
  }

  return (
    <div className="page-container">
      <header className="main-header">
        <h1>TechEaseSolutions</h1>
      </header>
      
      <div className="chat-container">
        <div className="chat-messages">
          {Array.isArray(data) && data.map((chat, index) => (
            <div key={index} className={`message ${chat.sender === userEmail ? 'my-message' : 'other-message'}`}>
              <div className="message-content">
                <small className="message-sender">{chat.sender}</small>
                <p>{chat.message}</p>
                <small className="message-timestamp">{chat.timestamp}</small>
              </div>
            </div>
          ))}
        </div>

        <form className="chat-form" onSubmit={sendToBackend}>
          <input
            className="chat-input"
            value={message}
            placeholder="Skriv ditt meddelande..."
            onChange={(e) => setMessage(e.target.value)}
          />
          <button className="send-button" type="submit" onClick={updateSite}>
            Skicka
          </button>
        </form>
      </div>
    </div>
  );
};

export default ChatHistory;
