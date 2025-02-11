import React, { useState } from 'react';

export default function Home() {
  return (
      <div id='header'>
        <h1>TechEaseSolutions</h1>
        <Dropdown />
      </div>
  );
}

const Dropdown = () => {
  const [selectedOption, setSelectedOption] = useState('');
  const [email, setEmail] = useState('');
  const [issue, setIssue] = useState('');
  const [loading, setLoading] = useState(false);
  const [message, setMessage] = useState('');

  const handleSubmit = async (event) => {
    event.preventDefault();

    if (!email || !selectedOption || !issue) {
      alert('Please fill in all fields before submitting.');
      return;
    }

    setLoading(true);

    const formData = {
      email,
      issue,
      selectedOption,
    };

    try {
      // Use relative path so Vite can handle proxy
      const response = await fetch('/api/tickets/submit', {  // Use '/api/tickets/submit' (without full URL)
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(formData),
      });

      const result = await response.json();

      if (response.ok) {
        setMessage('Your issue has been submitted successfully!');
        setEmail('');
        setIssue('');
        setSelectedOption('');
      } else {
        setMessage(result.message || 'Something went wrong.');
      }
    } catch (error) {
      setMessage('Error submitting the form. Please try again.');
      console.error('Error:', error);
    } finally {
      setLoading(false);
    }
  };

  return (
      <div id="formwrap">
        <div id="dropdown">
          <label htmlFor="options">Välj ett ämne</label>
          <select id="options" value={selectedOption} onChange={(e) => setSelectedOption(e.target.value)}>
            <option value="">--Välj ett ämne--</option>
            <option value="Sprucken skärm">Sprucken skärm</option>
            <option value="Datorn startar ej">Dator startar ej</option>
            <option value="Abonemang">Problem med abonnemang</option>
          </select>
        </div>

        <form onSubmit={handleSubmit}>
          <div id="wrapmail">
            <div>
            <textarea
                id="email-input"
                placeholder="Enter your Email..."
                value={email}
                onChange={(e) => setEmail(e.target.value)}
            />
            </div>
            <div>
            <textarea
                name="issue"
                id="issue-description"
                placeholder="Describe your issue..."
                value={issue}
                onChange={(e) => setIssue(e.target.value)}
            />
            </div>
            <button id="skicka_ärende" type="submit" disabled={loading}>
              {loading ? 'Submitting...' : 'Skicka ärende'}
            </button>
          </div>
        </form>

        {message && <p>{message}</p>}
      </div>
  );
};