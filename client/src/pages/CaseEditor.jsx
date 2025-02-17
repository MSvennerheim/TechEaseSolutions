import React, { useState, useEffect } from "react";
import { useParams } from "react-router-dom";

function CaseEditor() {
    const { companyId } = useParams(); // Hämta companyId från URL
    const [topics, setTopics] = useState([]);
    const [newTopic, setNewTopic] = useState("");
    const [error, setError] = useState(null);
    const [loading, setLoading] = useState(false);

    // 🟢 Hämta casetypes från backend
    useEffect(() => {
        setLoading(true);
        fetch(`http://localhost:5000/api/casetypes?companyId=${companyId}`)
            .then(response => response.json())
            .then(data => setTopics(data))
            .catch(error => {
                console.error("❌ Fel vid hämtning av casetypes:", error);
                setError("Kunde inte hämta casetypes. Kontrollera backend.");
            })
            .finally(() => setLoading(false));
    }, [companyId]);

    // 🟢 Lägg till ett nytt ämne i UI (ej i databasen än)
    const handleAddTopic = () => {
        if (newTopic.trim() === "") return; // Förhindra tomma ämnen
        setTopics([...topics, { id: null, text: newTopic, company: parseInt(companyId) }]); // 🟢 Lägg till company ID
        setNewTopic("");
    };

    // 🟢 Spara alla ändringar (uppdateringar + nya)
    const handleSave = () => {
        const newTopics = topics.filter(t => t.id === null); // 🆕 Filtrera nya cases
        const existingTopics = topics.filter(t => t.id !== null); // 🔄 Befintliga cases

        console.log("📤 Skickar PUT för uppdatering:", existingTopics);
        console.log("📤 Skickar POST för nya ämnen:", newTopics);

        // 🟢 Skicka PUT för uppdateringar
        if (existingTopics.length > 0) {
            fetch("http://localhost:5000/api/casetypes", {
                method: "PUT",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(existingTopics),
            })
            .then(response => response.json())
            .then(data => console.log("✅ Uppdaterade ämnen:", data))
            .catch(error => console.error("❌ Fel vid uppdatering:", error));
        }

        // 🆕 Skicka POST för nya cases
        newTopics.forEach(topic => {
            console.log("🟢 Försöker lägga till nytt ämne:", topic);

            fetch("http://localhost:5000/api/casetypes", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(topic),
            })
            .then(response => response.json())
            .then(data => {
                console.log("✅ Nytt case tillagt:", data);
                topic.id = data.id; // 🆕 Uppdatera ID i frontend
            })
            .catch(error => console.error("❌ Fel vid tillägg:", error));
        });

        alert("Ämnen uppdaterade!");
    };

    return (
        <div>
            <h2>Redigera ämnen för företag {companyId}</h2>

            {loading && <p>🔄 Laddar data...</p>}
            {error && <p style={{ color: "red" }}>❌ {error}</p>}

            <div id="modifywrapper">
                <textarea 
                    value={newTopic} 
                    onChange={(e) => setNewTopic(e.target.value)}
                    placeholder="Skriv ett nytt ämne..."
                />
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
        </div>
    );
}

export default CaseEditor;
