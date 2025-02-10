import React, { useState } from 'react';

export default function Home() {
  return (
    <div id='header'>
      <h1>Welcome to Home Page</h1>
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
        <label htmlFor="options">Choose an option:</label>
        <select id="options" value={selectedOption} onChange={handleChange}>
          <option value="">--Please choose an option--</option>
          <option value="option1">Option 1</option>
          <option value="option2">Option 2</option>
          <option value="option3">Option 3</option>
        </select>
        {selectedOption && <p>You selected: {selectedOption}</p>}
      </div>
      <div id="wrapmail">
        <div>
         <form>
          <label>
            Email:
            <input type="text" name="email" />
          </label>
        </form>
        </div>
        <div>
        <form>
          <label>
            Describe your issue
            <input type="text" name="issue" id="describe" />
          </label>
        </form>
        </div>
        <button id="skicka_ärende">Skicka ärende</button>
      </div>  
    </div>
    </>
  );
};
