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
  
  <form>
    <div id="formwrap">
      <div id="dropdown">
        <label htmlFor="options">Choose a topic</label>  {/* Byt ut texten inuti elementen till {variabel} för att göra topics dynamiska till varje sida*/}
        <select id="options" value={selectedOption} onChange={handleChange}>
          <option value="">--Choose a topic--</option>
          <option value="option1">Cracked screen</option>
          <option value="option2">Computer not starting</option>
          <option value="option3">Problem med abonnemang</option>
        </select>
      </div>
      <div id="wrapmail">
        <div>
        <textarea id="email-input" placeholder="Enter your Email..."/>
        </div>
        <div>
        <textarea name="issue" id="describe" placeholder="Describe your issue..."/>
        </div>
        <button id="skicka_ärende">Send ticket</button>
      </div>
    </div>
  </form>
  );
};
