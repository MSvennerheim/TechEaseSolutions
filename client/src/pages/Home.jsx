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

  const handleChange = (event) => {
    setSelectedOption(event.target.value);
  };

  return (
    <>
    <div id="formwrap">
      <div id="dropdown">
        <label htmlFor="options">Välj ett ämne</label>
        <select id="options" value={selectedOption} onChange={handleChange}>
          <option value="">--Välj ett ämne--</option>
          <option value="option1">Sprucken skärm</option>
          <option value="option2">Dator startar ej</option>
          <option value="option3">Problem med abonnemang</option>
        </select>
      </div>
      <div id="wrapmail">
        <div>
         <form>
         <textarea id="email-input" placeholder="Enter your Email..."></textarea></form>
        </div>
        <div>
        <form>
        <textarea name="issue" id="describe" placeholder="Describe your issue..."></textarea>
        </form>
        </div>
        <button id="skicka_ärende">Skicka ärende</button>
      </div>  
    </div>
    </>
  );
};
