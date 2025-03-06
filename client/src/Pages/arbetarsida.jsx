import React, { useEffect, useState } from "react";
import { Link, useParams } from "react-router-dom";

const Arbetarsida = () => {
  const { company } = useParams();
  const [data, setData] = useState([]);
  const [showOnlyAvailable, setShowOnlyAvailable] = useState(false);

  useEffect(() => {
    const GetAllChats = async () => {
      const response = await fetch(`/api/arbetarsida/`);
      const responseData = await response.json();
      setData(responseData);
    };
    GetAllChats();
  }, []);

  const filteredData = showOnlyAvailable 
    ? data.filter(chat => !chat.csrep) 
    : data;

  return (
    <div className="worker-dashboard">
      <div className="filter-section">
        <div className="filter-checkbox">
          <input
            type="checkbox"
            id="available-filter"
            checked={showOnlyAvailable}
            onChange={(e) => setShowOnlyAvailable(e.target.checked)}
          />
          <label htmlFor="available-filter">Visa endast tillgängliga ärenden</label>
        </div>
      </div>

      <div className="tickets-grid">
        {filteredData.map((ticket, index) => (
          <div key={index} className={ticket.csrep ? "closedTicket" : "openTicket"}>
            <div className="ticket-content">
              <div className="ticket-message">{ticket.message}</div>
              <div className="ticket-timestamp">{ticket.timestamp}</div>
            </div>
            <Link to={`/Chat/${ticket.chat}`}>
              <button className="ticket-button">
                {ticket.csrep ? "Visa ärende" : "Ta ärende"}
              </button>
            </Link>
          </div>
        ))}
      </div>
    </div>
  );
};

export default Arbetarsida;