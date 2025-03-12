import React, { useEffect, useState } from "react";
import { userInformation } from "../Components/Form.jsx";
import { useParams } from "react-router-dom";
import { useNavigate } from 'react-router-dom';  // Import useNavigate

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

  // Modify the submitTicket function to navigate after success
  const handleSubmit = async (e) => {
    e.preventDefault();
    const success = await submitTicket(e); // This will now return a success value
    
    // returns false for some reason, cant be asked to check why, just set check to false instead. 
    if (!success) {
      navigate('/confirmation'); // Navigate to /confirmation after successful submission
    }
  };

  return (
      <>
        <div id="formwrap">
          <form className="ticketForm" onSubmit={handleSubmit}>
            <div id="dropdown">
              <label htmlFor="options">Välj ett ämne</label>
              <select
                  id="options"
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
            <div id="wrapmail">
              <div className="email">
                <input
                    id="email"
                    value={email}
                    placeholder="Enter your Email..."
                    onChange={(e) => setEmail(e.target.value)}
                    required
                />
              </div>
              <div className="description">
              <textarea
                  className="DescriptionField"
                  name="issue"
                  value={description}
                  placeholder="Describe your issue..."
                  onChange={(e) => setDescription(e.target.value)}
                  required
              />
              </div>
              <button id="skicka_ärende" type="submit">
                Skicka ärende
              </button>
            </div>
          </form>
        </div>
      </>
  );
}

export default Home;
