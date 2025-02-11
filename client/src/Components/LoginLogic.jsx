import { useState } from 'react';

export function useLogin() { // här definerar vi variablerna som ska användas i komponeten
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError('');

    if (!email || !password) {
      setError('Email and password are required');
      return;
    }

    try {
      const response = await fetch('/api/login', { // Här skickas förfrågan till servern med anvöndarens mail och lösenord 
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({
          Email: email,
          Password: password,
        }),
      });


      
      const data = await response.json(); //Här hämtar vi svaret från servern som är en token. Om det går bra så redirectar vi användaren till dashboard.

      if (response.ok) {
        localStorage.setItem('token', data.token); // lägger till en token i localstorage så att inforamatioenen kan sparas
        window.location.href = '/arbetarsida'; // skickar dig till arbetarsidan.
      } else {
        setError(data.message || 'Login failed');
      }
    } catch (err) {
      setError('An error occurred during login');
      console.error('Login error:', err);
    }
  };

  return {
    email,
    setEmail,
    password,
    setPassword,
    error,
    handleSubmit
  };
}