import React, { useEffect, useState } from "react";
import { userInformation } from "../Components/Form.jsx";
import { useParams } from "react-router-dom";

function Home() {
  const { companyName } = useParams();
  const [data, setData] = useState([]);
  
  useEffect(() => {
    const getCompanyCaseTypes = async () => {
      try {
        const response = await fetch(`/api/kontaktaoss/${companyName}`);
        if (!response.ok) throw new Error("Failed to fetch case types");
        const responseData = await response.json()
        // console.log(responseData)
        setData(responseData)
        // Console.log(data)

      } catch (error) {
        console.error("An error has occured:", error);
      }
    };
      getCompanyCaseTypes();
  }, []);

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

  return (
      <>
        <div id="formwrap">
          <form className="ticketForm" onSubmit={submitTicket}>
            <div id="dropdown">
              <label htmlFor="options">Välj ett ämne</label>
              <select
                  id="options"
                  value={selectedOption}
                  onChange={(e) => setOption(e.target.value)}
              >
                <option value="">--Välj ett ämne--</option>
                {data.length > 0 ? (
                    data.map((caseType, index) => (
                        <option key={index} value={caseType.caseId} onChange={(e) => setOption(e.target.value)}>
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
                />
              </div>
              <div className="description">
                <textarea
                    className="DescriptionField"
                    name="issue"
                    value={description}
                    placeholder="Describe your issue..."
                    onChange={(e) => setDescription(e.target.value)}
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
