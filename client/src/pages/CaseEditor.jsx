import React, { useState, useEffect } from "react";
import { useParams } from "react-router-dom";

function CaseEditor() {
    const { companyId } = useParams(); // Hämta companyId från URL
    const [topics, setTopics] = useState([]);
    const [newTopic, setNewTopic] = useState("");

    useEffect(() => {
        fetch(`http://localhost:5000/api/casetypes?companyId=${companyId}`)
            .then(response => response.json())
            .then(data => setTopics(data));
    }, [companyId]);

    const handleAddTopic = () => {
        setTopics([...topics, { id: null, text: newTopic, company: parseInt(companyId) }]);
        setNewTopic("");
    };

    const handleSave = () => {
        fetch("http://localhost:5000/api/casetypes", {
            method: "PUT",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(topics),
        }).then(() => alert("Ämnen uppdaterade"));
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
