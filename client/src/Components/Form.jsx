import {useEffect, useState} from "react";
import {useParams} from "react-router-dom";


/*
const ChatHistory = () => {
  const { chatId } = useParams()
  const [data, setData] = useState([])
  useEffect(() => {
    const GetChat = async () => {
      const response = await fetch(`http://localhost:5000/Chat/${chatId}`)
      const responseData = await response.json()
      setData(responseData)
      //console.log(responseData)
    }
    */

export function userInformation() {
    const [email, setEmail] = useState('');
    const [selectedOption, setOption] = useState('');
    const [description, setDescription] = useState('');
    const [error, setError] = useState('');

    const submitTicket = async (e) => {
        e.preventDefault();
        setError('');

        if (!email || !selectedOption || !description) {
            setError('Email, Description and option is required');
            return;
        }

        try {
            console.log({
                email,
                option: selectedOption,
                description
            });
            
            const response = await fetch('/api/form', { 
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({
                    email,
                    option: selectedOption,
                    description
                }),
            });
            const data = await response.json();
            // console.log(data);
        } catch (error) {
            setError("noob jävel")
        }
        
    };

    return {
        email,
        setEmail,
        selectedOption,
        setOption,
        description,
        setDescription,
        error,
        submitTicket
    };
}