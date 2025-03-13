import React, { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import '../styles/Arbetarsida.css';

const Arbetarsida = () => {
  const [data, setData] = useState([]);
  const [showClosed, setShowClosed] = useState(false);
  const [searchTerm, setSearchTerm] = useState('');
  const [sortBy, setSortBy] = useState('newest');

  useEffect(() => {
    const GetAllChats = async () => {
      try {
        const response = await fetch('/api/arbetarsida/', {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify({ getAllChats: true })
        });
        if (!response.ok) throw new Error('Failed to fetch chats');
        const responseData = await response.json();
        setData(responseData);
      } catch (error) {
        console.error("Error:", error);
      }
    };
    GetAllChats();
  }, []);

  // Filtrera och sortera ärenden
  const filteredChats = data
    .filter(chat => showClosed || !chat.csrep)  // Visa alla eller bara öppna
    .filter(chat => 
      chat.message.toLowerCase().includes(searchTerm.toLowerCase()) ||
      chat.sender.toLowerCase().includes(searchTerm.toLowerCase())
    )
    .sort((a, b) => {
      if (sortBy === 'newest') {
        return new Date(b.timestamp) - new Date(a.timestamp);
      }
      return new Date(a.timestamp) - new Date(b.timestamp);
    });

  return (
    <div className="arbetarsida-container">
      <header className="arbetarsida-header">
        <h1>Kundärenden</h1>
        <div className="filter-section">
          <input
            type="text"
            placeholder="Sök ärenden..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            className="search-input"
          />
          <select 
            value={sortBy} 
            onChange={(e) => setSortBy(e.target.value)}
            className="sort-select"
          >
            <option value="newest">Nyast först</option>
            <option value="oldest">Äldst först</option>
          </select>
          <div className="filter-checkbox">
            <input
              type="checkbox"
              id="show-closed"
              checked={showClosed}
              onChange={(e) => setShowClosed(e.target.checked)}
            />
            <label htmlFor="show-closed">Visa stängda ärenden</label>
          </div>
        </div>
      </header>

      <div className="ticket-list">
        {filteredChats.map((chat, index) => (
          <div key={index} className={`ticket ${chat.csrep ? 'closed-ticket' : 'open-ticket'}`}>
            <div className="ticket-content">
              <div className="ticket-header">
                <span className="ticket-id">#{chat.chat}</span>
                <span className={`ticket-status ${chat.csrep ? 'status-closed' : 'status-open'}`}>
                  {chat.csrep ? 'Stängt' : 'Öppet'}
                </span>
              </div>
              <div className="ticket-body">
                <p className="ticket-message">{chat.message}</p>
                <div className="ticket-meta">
                  <span className="ticket-sender">{chat.sender}</span>
                  <span className="ticket-timestamp">
                    {new Date(chat.timestamp).toLocaleString('sv-SE')}
                  </span>
                </div>
              </div>
            </div>
            <Link to={`/Chat/${chat.chat}`} className="view-ticket-btn">
              Öppna ärende
            </Link>
          </div>
        ))}
      </div>
    </div>
  );
};

export default Arbetarsida;