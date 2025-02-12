import {useState} from "react";

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
            const response = await fetch('/api/form', { 
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({
                    Email: email,
                    Option: selectedOption,
                    Description: description,
                }),
            });
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