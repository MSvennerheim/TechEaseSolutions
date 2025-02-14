import React, { useState, useEffect } from "react";
import { useParams } from "react-router-dom";

function CaseEditor() {
    const { companyId } = useParams();
    const [topics, setTopics] = useState([]);
    const [newTopic, setNewTopic] = useState("");

    useEffect(() => {
        fetch(`http://localhost:5000/api/casetypes?companyId=${companyId}`)
            .then(response => response.json())
            .then(data => setTopics(data))
            .catch(error => console.error("❌ Error fetching data:", error));
    }, [companyId]);

    const handleAddTopic = () => {
        setTopics([...topics, { id: null, text: newTopic, company: parseInt(companyId) }]);
        setNewTopic("");
    };

    const handleSave = () => {
        const validTopics = topics.filter(t => t.id !== null); // Filtrera bort ogiltiga poster

        console.log("📤 Skickar till backend:", validTopics);

        fetch("http://localhost:5000/api/casetypes", {
            method: "PUT",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(validTopics),
        })
            .then(response => {
                console.log("🔍 API-respons status:", response.status);
                return response.text(); // Ändrat från .json() till .text() för att se exakt vad som returneras
            })
            .then(data => {
                console.log("✅ Svar från backend:", data);
                try {
                    const jsonData = JSON.parse(data); // Försöker tolka JSON manuellt
                    console.log("📌 Parsed JSON:", jsonData);
                } catch (error) {
                    console.error("❌ JSON-parse-fel:", error);
                }
            })
            .catch(error => console.error("❌ Fel vid anrop:", error));
    };


    return (
        <div>
            <h2>Redigera ämnen för företag {companyId}</h2>
            <textarea value={newTopic} onChange={(e) => setNewTopic(e.target.value)} />
            <button onClick={handleAddTopic}>Lägg till</button>
            <ul>
                {topics.map((t, index) => (
                    <li key={index}>
                        <input
                            type="text"
                            value={t.text}
                            onChange={(e) => {
                                const newTopics = [...topics];
                                newTopics[index].text = e.target.value;
                                setTopics(newTopics);
                            }}
                        />
                    </li>
                ))}
            </ul>
            <button onClick={handleSave}>Spara i databasen</button>
        </div>
    );
}

export default CaseEditor;
