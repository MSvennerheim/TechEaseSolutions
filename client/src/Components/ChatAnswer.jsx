import { useState } from "react";
import { useParams } from "react-router-dom";

// need to get company, casetyoe and sender id in BE before we get write to DB.. Probably the easiest way..

export function useSendChatAnswer() {
  const [message, setMessage] = useState("");
  const { chatId } = useParams();

  const sendToBackend = async (e) => {
    await fetch(`/api/ChatResponse/${chatId}`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({message}),
    });

    // Clear fields after sent
    setMessage("");
  };

  return { message, setMessage, sendToBackend };
}
