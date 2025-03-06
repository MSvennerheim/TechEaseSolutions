import { useState } from 'react';
import { useNavigate } from 'react-router';


export function useLogin() {

  // komponenter för att hantera inloggningsfält och felmeddelande
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const navigate = useNavigate();

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError('');

    if (!email || !password) {
      setError('Email and password are required');
      return;
    }

    try {
      const response = await fetch('/api/login', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({
          Email: email,
          Password: password,
        }),
      });

      const data = await response.json().catch(() => null); // Hantera json fel här
      console.log('Login Response:', data); // Logga svaret från servern

      if (!data) {
        setError('An unexpected error occurred. Please try again.');
        return;
      }

      if (response.ok) {
        console.log('Login Successful. User:', data.user); // logga användaren som loggat in
        if (data.user.isAdmin) {
          navigate(`/admin`);
        } else {
          navigate(`/arbetarsida`);
        }
      } else {
        setError(data.message || 'Login failed. Please check your credentials.');
      }
    } catch (error) {
      setError('An error occurred during login. Please try again.');
      console.error('Login error:', error);
    }
  };

  return {
    email,
    setEmail,
    password,
    setPassword,
    error,
    handleSubmit,
  };
}