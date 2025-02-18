import React, { useEffect, useState } from "react";
import { Link, useParams } from "react-router-dom";

const Arbetarsida = () => {
  const { company } = useParams();
  const [data, setData] = useState([]);

  useEffect(() => {
    const GetAllChats = async () => {
      try {
        const response = await fetch(`/api/arbetarsida/${company}`);
        if (!response.ok) {
          throw new Error(`Failed to fetch chats: ${response.statusText}`);
        }
        const responseData = await response.json();
        console.log("Chats Response:", responseData);
        setData(responseData);
      } catch (error) {
        console.error("Error fetching chats:", error);
      }
    };
    GetAllChats();
  }, [company]);

  return (
    <div>
      {Array.isArray(data) && data.map((chats, index) => (
        <div key={index} className={chats.csrep ? "openTicket" : "closedTicket"}>
          <small>{chats.message} </small>
          <br />
          <small>{chats.timestamp}</small>
          <br />
          <Link to={`/Chat/${chats.chat}`}>
            <button>Gå till chatten</button>
          </Link>
          <br />
          <a href={`http://localhost:5173/case/${chats.token}`} target="_blank" rel="noopener noreferrer">
            Öppna ärende i ny flik
          </a>
        </div>
      ))}
    </div>
  );
};

export default Arbetarsida;