import React, {use, useEffect, useState} from "react";
import { useParams } from "react-router-dom";
import { useSendChatAnswer } from "../Components/ChatAnswer.jsx";
import {useNavigate} from "react-router";

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
    }, 1000);
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
    if(nextChatResponseData != null){
    navigate(`/Chat/${nextChatResponseData}`)
    }
    else {
      window.alert("no more chats to assign! Press ok to go back")
      navigate(`/Arbetarsida/`)
    }
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

      <form onSubmit={(e) => e.preventDefault()} hidden={data.length === 0} >
        <input
          id="message"
          value={message}
          placeholder="Enter your message..."
          onChange={(e) => setMessage(e.target.value)}
        />
        <button type="submit" onClick={() => handleResponse(false)} disabled={message.length === 0}>Send</button>
        <button type="submit" onClick={() => handleResponse(true)} hidden={!isUserCsRep} disabled={message.length === 0} >Send and take next open ticket</button>
      </form>
    </div>
  );
};

export default ChatHistory;
