import { useState } from "react";
import { useParams } from "react-router-dom";

// need to get company, casetyoe and sender id in BE before we get write to DB.. Probably the easiest way..

export function useSendChatAnswer() {
  const [message, setMessage] = useState("");
  const [email, setEmail] = useState("");
  const [csrep, setCsrep] = useState(false);
  const { chatId } = useParams();

  const sendToBackend = async (e) => {
    e.preventDefault();

    await fetch(`/api/ChatResponse/${chatId}`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ message, email, csrep }),
    });

    // Clear fields after sent
    setMessage("");
    setEmail("");
    setCsrep(false);
  };

  return { message, setMessage, email, setEmail, csrep, setCsrep, sendToBackend };
}
