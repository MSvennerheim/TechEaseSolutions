import {useParams} from "react-router-dom";
import {useSearchParams} from "react-router";

export default function customerLogin() {
    const {chatId} = useParams()
    const [searchParams] = useSearchParams()
    const email = searchParams.get("email")

    const handleSubmit = async (e) => {
        e.preventDefault();
        setError('');

        if (!email || !chatId) {
            setError('Email and chatId are required');
            return;
        }

        try {
            const response = await fetch('/api/guestlogin', {
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
                navigate(`/chat/${data.user.chat}`)
            } else {
                setError(data.message || 'Login failed. Please check your credentials.');
            }

            if (!data) {
                setError('An unexpected error occurred. Please try again.');
                return;
            }

            return handleSubmit()

        } catch (error) {
            setError('An error occurred during login. Please try again.');
            console.error('Login error:', error);
        }
    }
}