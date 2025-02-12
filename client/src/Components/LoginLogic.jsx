import { useState } from 'react';
import { useNavigate } from 'react-router';
export function useLogin() { // här definerar vi variablerna som ska användas i komponeten
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
        localStorage.setItem('token', data.token);// lägger till en token i localstorage så att inforamatioenen kan sparas
        localStorage.setItem('userRole', data.user.isAdmin ? 'admin' : 'Worker')

 console.log('IsAdmin:', data.user.isAdmin);

        if (data.user.isAdmin) {
          navigate('/admin'); // om man är admin så redirectar vi till dashboard
        } else {
          navigate('/arbetarsida'); // annars redirectar vi till arbetarsidan
        }
      } else {
        setError(data.message || 'Login failed');
      }  
    } catch (error) {
      setError('An error occurred during login');
      console.error('Login error:', error);
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