import React, { useEffect, useState } from "react";
import { useParams } from "react-router-dom";
import { useSendChatAnswer } from "../Components/ChatAnswer.jsx";

const ChatHistory = () => {
  const { chatId } = useParams();
  const [data, setData] = useState([]);
  const [updateTicker, setUpdateTicker] = useState(0)
  const [isUserCsRep, setIsUserCsRep] = useState(false)


  useEffect(() => {
    const GetChats = async () => {
      const response = await fetch(`/api/Chat/${chatId}`);
      const responseData = await response.json();
      setData(responseData);
    };
    GetChats();
  }, [updateTicker]);

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
    }, 1000);
  }
  
  const sendToNextOpenTicket = () => {
    
  }


  return (
    <div>
      <ul>
        {data.map((chat, index) => (
          <div key={index} className={chat.csrep === true ? 'fromCsRep' : 'fromCustomer'}>
            <small>{chat.sender} skrev: </small><br />
            <small>{chat.message}</small><br />
            <small>{chat.timestamp}</small>
          </div>
        ))}
      </ul>

      <form onSubmit={sendToBackend} hidden={data.length === 0} >
        <input
          id="message"
          value={message}
          placeholder="Enter your message..."
          onChange={(e) => setMessage(e.target.value)}
        />
        <button type="submit" onClick={updateSite}>Send</button>
        <button type="submit" onClick={sendToNextOpenTicket} hidden={!isUserCsRep} >Send and take next open ticket</button>
      </form>
    </div>
  );
};

export default ChatHistory;
