import React, { useEffect, useState } from "react";
import { useParams, useLocation } from "react-router-dom";

const CaseView = () => {
  const { token } = useParams();
  const location = useLocation();
  const [caseData, setCaseData] = useState(null);
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(true);

  const queryParams = new URLSearchParams(location.search);
  const email = queryParams.get('email');

  useEffect(() => {
    const fetchCase = async () => {
      try {
        const url = `http://localhost:5000/case/${token}?email=${encodeURIComponent(email)}`;

        const response = await fetch(url, {
          method: "GET",
          headers: {
            "Content-Type": "application/json",
            "Referer": "mail.google.com" 
          },
          credentials: 'include'
        });

        if (!response.ok) {
          const errorData = await response.json();
          throw new Error(errorData.message || "Failed to load case details");
        }

        const data = await response.json();
        setCaseData(data);
      } catch (error) {
        setError(error.message);
      } finally {
        setLoading(false);
      }
    };

    if (token && email) {
      fetchCase();
    } else {
      setError("Invalid access attempt");
      setLoading(false);
    }
  }, [token, email]);

  if (loading) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-gray-900"></div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="p-4 bg-red-100 border border-red-400 text-red-700 rounded">
        <p>{error}</p>
      </div>
    );
  }

  if (!caseData) {
    return null;
  }

  return (
    <div className="max-w-2xl mx-auto p-6">
      <h1 className="text-2xl font-bold mb-6">Case Details</h1>
      <div className="space-y-4 bg-white shadow rounded-lg p-6">
        <div>
          <h2 className="font-semibold text-gray-700">Message</h2>
          <p className="mt-1">{caseData.message}</p>
        </div>
        <div className="grid grid-cols-2 gap-4">
          <div>
            <h2 className="font-semibold text-gray-700">Sender</h2>
            <p className="mt-1">{caseData.sender}</p>
          </div>
          <div>
            <h2 className="font-semibold text-gray-700">Timestamp</h2>
            <p className="mt-1">{new Date(caseData.timestamp).toLocaleString()}</p>
          </div>
          <div>
            <h2 className="font-semibold text-gray-700">Company</h2>
            <p className="mt-1">{caseData.company}</p>
          </div>
          <div>
            <h2 className="font-semibold text-gray-700">Case Type</h2>
            <p className="mt-1">{caseData.casetype}</p>
          </div>
        </div>
      </div>
    </div>
  );
};

export default CaseView;
