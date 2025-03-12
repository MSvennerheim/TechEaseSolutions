import {useEffect, useState} from "react";
import {useParams} from "react-router-dom";
import { useNavigate } from 'react-router-dom';  // Import useNavigate


export function userInformation() {
    const [email, setEmail] = useState('');
    const [selectedOption, setOption] = useState('');
    const [description, setDescription] = useState('');
    const [error, setError] = useState('');

    const submitTicket = async (e) => {
        e.preventDefault();
        setError('');

        if (!email || !selectedOption || !description) {
            alert('Email, Description, and option are required');
            return false;  // Return false if validation fails
        }

        try {
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
                console.log("data: " + data)
            if (data.ok) {
                return true;  // Return true if submission is successful
            } else {
                throw new Error("Failed to submit the ticket");
            }
        } catch (error) {
            setError("Something went wrong, please try again.");
            return false;  // Return false if an error occurs
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
