import React, {useEffect, useState} from 'react';
import {userInformation} from '../Components/Form.jsx'
import {useParams} from "react-router-dom";
function Home() {
  
  const { email, setEmail, selectedOption, setOption, description, setDescription, error, submitTicket } = userInformation();
  
  const companies = () => {
    const { name } = useParams()
    const [data, setData] = useState([])
    useEffect(() => {
      const getCompanyName = async () => {
        const response = await fetch(`/api/kontaktaoss/${name}`)
        const responseData = await response.json()
        setData(responseData)
      }
      
  //Make some improvements on the email. Make sure that the user inputs it in a correct format.
  //Make the options dynamic by fetching the unique companies "settings" for the dropdown.
  return (
      <>
        <div id="formwrap">
          <form onSubmit={submitTicket}>
            <div id="dropdown">
              <label htmlFor="options">Välj ett ämne</label>
              <select id="options" value={selectedOption} onChange={(e) => setOption(e.target.value)}>
                <option value="">--Välj ett ämne--</option>
                <option value="Sprucken skärm">Sprucken skärm</option>
                <option value="Desktop">Dator startar ej</option>
                <option value="Abonnemang">Problem med abonnemang</option>
              </select>
            </div>
            <div id="wrapmail">
              <div>
                <input id="email" value={email} placeholder="Enter your Email..." onChange={(e) => setEmail(e.target.value)}></input>
              </div>
              <div>
                  <input name="issue" value={description} placeholder="Describe your issue..." onChange={(e) => setDescription(e.target.value)}></input>
              </div>
              <button id="skicka_ärende" type="submit">Skicka ärende</button>
            </div>
          </form>
        </div>
      </>
  );
}

export default Home;