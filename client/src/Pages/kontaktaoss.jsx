import React, { useEffect, useState } from "react";
import { userInformation } from "../Components/Form.jsx";
import { useParams, useNavigate } from "react-router-dom";


function Home() {
  const { companyName } = useParams();
  const [data, setData] = useState([]);
  const navigate = useNavigate();

  useEffect(() => {
    const getCompanyCaseTypes = async () => {
      try {
        const response = await fetch(`/api/kontaktaoss/${companyName}`);
        if (!response.ok) throw new Error("Failed to fetch case types");
        const responseData = await response.json();
        setData(responseData);
      } catch (error) {
        console.error("An error has occurred:", error);
      }
    };
    getCompanyCaseTypes();
  }, [companyName]);

  const {
    email,
    setEmail,
    selectedOption,
    setOption,
    description,
    setDescription,
    error,
    submitTicket,
  } = userInformation();

  const handleSubmit = async (e) => {
    e.preventDefault();
    const success = await submitTicket(e);
    
    if (!success) {
      navigate('/confirmation');
    }
  };

  return (
    <div className="form-container">
      <form className="ticket-form" onSubmit={handleSubmit}>
        <div className="form-group">
          <label htmlFor="options">Välj ett ämne</label>
          <select
            id="options"
            className="form-control"
            value={selectedOption}
            onChange={(e) => setOption(e.target.value)}
            required
          >
            <option value="">--Välj ett ämne--</option>
            {data.length > 0 ? (
              data.map((caseType, index) => (
                <option key={index} value={caseType.caseId}>
                  {caseType.caseType}
                </option>
              ))
            ) : (
              <option disabled>Laddar...</option>
            )}
          </select>
        </div>
        
        <div className="form-group">
          <label htmlFor="email">E-post</label>
          <input
            id="email"
            className="form-control"
            type="email"
            value={email}
            placeholder="Enter your Email..."
            onChange={(e) => setEmail(e.target.value)}
            required
          />
        </div>
        
        <div className="form-group">
          <label htmlFor="description">Beskrivning</label>
          <textarea
            id="description"
            className="form-control description-field"
            value={description}
            placeholder="Describe your issue..."
            onChange={(e) => setDescription(e.target.value)}
            required
          />
        </div>
        
        {error && <div className="error-message">{error}</div>}
        
        <button className="submit-button" type="submit">
          Skicka ärende
        </button>
      </form>
    </div>
  );
}

export default Home;