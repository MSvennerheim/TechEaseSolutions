import React, { useState, useEffect } from 'react';
import { useParams } from 'react-router-dom';
import '../styles/KontaktaOss.css';

const KontaktaOss = () => {
  const { company: preselectedCompany } = useParams();
  const [email, setEmail] = useState('');
  const [message, setMessage] = useState('');
  const [company, setCompany] = useState(preselectedCompany || '');
  const [companies, setCompanies] = useState([]);
  const [error, setError] = useState(null);
  const [success, setSuccess] = useState(false);
  const [loading, setLoading] = useState(false);

  // Hämta företagslistan
  useEffect(() => {
    const fetchCompanies = async () => {
      try {
        console.log('Fetching companies...');  // Debug-loggning
        const response = await fetch('/api/companies');
        console.log('Response status:', response.status);  // Debug-loggning
        
        if (!response.ok) {
          throw new Error(`HTTP error! status: ${response.status}`);
        }
        const data = await response.json();
        console.log('Received companies:', data);  // Debug-loggning
        setCompanies(data);
      } catch (err) {
        console.error('Failed to fetch companies:', err);
        setError('Kunde inte ladda företagslistan');
      }
    };

    fetchCompanies();
  }, []);

  const handleSubmit = async (e) => {
    e.preventDefault();
    setLoading(true);
    setError(null);

    try {
      const response = await fetch('/api/kontaktaoss', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ email, message, company })
      });

      if (!response.ok) throw new Error('Något gick fel');
      
      setSuccess(true);
      setEmail('');
      setMessage('');
    } catch (err) {
      setError('Kunde inte skicka meddelandet. Försök igen senare.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="kontakt-container">
      <div className="kontakt-card">
        <div className="kontakt-header">
          <h1>Kontakta oss</h1>
          <p>Beskriv ditt ärende så återkommer vi så snart som möjligt</p>
        </div>

        {success ? (
          <div className="success-message">
            <h2>Tack för ditt meddelande!</h2>
            <p>Vi har skickat en bekräftelse till din e-post. Vi återkommer så snart vi kan.</p>
          </div>
        ) : (
          <form onSubmit={handleSubmit} className="kontakt-form">
            {error && <div className="error-message">{error}</div>}
            
            <div className="input-group">
              <label htmlFor="company">Företag</label>
              <select
                id="company"
                value={company}
                onChange={(e) => setCompany(e.target.value)}
                required
              >
                <option value="">Välj företag</option>
                {companies.map(comp => (
                  <option key={comp.id} value={comp.name}>
                    {comp.name}
                  </option>
                ))}
              </select>
            </div>

            <div className="input-group">
              <label htmlFor="email">Din e-postadress</label>
              <input
                type="email"
                id="email"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                placeholder="namn@exempel.se"
                required
              />
            </div>

            <div className="input-group">
              <label htmlFor="message">Beskriv ditt ärende</label>
              <textarea
                id="message"
                value={message}
                onChange={(e) => setMessage(e.target.value)}
                placeholder="Beskriv ditt problem här..."
                rows="4"
                required
              />
            </div>

            <button type="submit" disabled={loading}>
              {loading ? 'Skickar...' : 'Skicka meddelande'}
            </button>
          </form>
        )}
      </div>
    </div>
  );
};

export default KontaktaOss;
