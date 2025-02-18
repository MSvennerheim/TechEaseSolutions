import React, { useEffect, useState } from "react";
import { useParams } from "react-router-dom";

const CaseView = () => {
  const { token } = useParams();
  const [caseData, setCaseData] = useState(null);
  const [error, setError] = useState("");

  useEffect(() => {
    const fetchCase = async () => {
      try {
        const response = await fetch(`http://localhost:5000/case/${token}`); // Ensure the correct URL
        if (!response.ok) {
          throw new Error("Failed to fetch case details");
        }
        const data = await response.json(); // Parse the response as JSON
        setCaseData(data);
      } catch (error) {
        console.error("Error fetching case details:", error);
        setError("Failed to load case details. Please try again later.");
      }
    };

    fetchCase();
  }, [token]);

  if (error) {
    return <div>{error}</div>;
  }

  if (!caseData) {
    return <div>Loading...</div>;
  }

  return (
    <div>
      <h1>Case Details</h1>
      <p><strong>Message:</strong> {caseData.message}</p>
      <p><strong>Sender:</strong> {caseData.sender}</p>
      <p><strong>Timestamp:</strong> {caseData.timestamp}</p>
      <p><strong>Company:</strong> {caseData.company}</p>
      <p><strong>Case Type:</strong> {caseData.casetype}</p>
    </div>
  );
};

export default CaseView;