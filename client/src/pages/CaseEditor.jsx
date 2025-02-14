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

       const removeTopic = (topicText) => {
        setTopics(topics.filter(topic => topic.text !== topicText));
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
        <div id="modifywrapper">
            <h1>Redigera ämnen för företag {companyId}</h1>
            <div id="text_button">
                <textarea value={newTopic} onChange={(e) => setNewTopic(e.target.value)} />
                <button onClick={handleAddTopic}>Lägg till</button>
            </div>
            <div id="topics-list">
                {topics.length === 0 ? (
                    <p>Inga ämnen tillagda</p>
                ) : (
                    topics.map((topic, index) => (
                        <div key={index} id="topic-container">
                            <input 
                                id="topic-input"
                                type="text" 
                                value={topic.text} 
                                onChange={(e) => {
                                    const newTopics = [...topics];
                                    newTopics[index].text = e.target.value;
                                    setTopics(newTopics);
                                }} 
                            />
                            <button id="delete-button" onClick={() => removeTopic(topic.text)}>🗑️</button>
                        </div>
                    ))
                )}
            </div>

export default CaseEditor;
