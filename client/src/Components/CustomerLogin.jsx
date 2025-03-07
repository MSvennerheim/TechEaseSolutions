import {useParams, useNavigate, useSearchParams} from "react-router-dom";
import {useState, useEffect} from "react";

export default function CustomerLogin() {
    const {chatId} = useParams()
    const [searchParams] = useSearchParams()
    const email = searchParams.get("email")
    const navigate = useNavigate();
    const [error, setError] = useState('');

    useEffect(() => {

        if (email && chatId) {
        handleSubmit();
            }
        }, [email, chatId])

    const handleSubmit = async (e) => {
        setError('');

        if (!email || !chatId) {
            setError('Email and chatId are required');
            return;
        }

        try {
            const response = await fetch('/api/guestLogin', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({
                    Email: email,
                    ChatId: chatId,
                }),
            });

            const data = await response.json().catch(() => null); // Hantera json fel här
            console.log('Login Response:', data); // Logga svaret från servern

            if (response.ok) {
                navigate(`/chat/${data.user.chatId}`)
            } else {
                setError(data.message || 'Login failed. Please check your credentials.');
            }

            if (!data) {
                setError('An unexpected error occurred. Please try again.');
                return;
            }
            
        } catch (error) {
            setError('An error occurred during login. Please try again.');
            console.error('Login error:', error);
        }
    }
}