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
        fetch(`/api/casetypes/${companyId}`)
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
        const trimmedTopic = newTopic.trim();
        if (trimmedTopic === "") return; // Förhindra tomma ämnen
        
        // Kontrollera om ämnet redan finns (case-insensitive)
        const topicExists = topics.some(t => 
            t.text.trim().toLowerCase() === trimmedTopic.toLowerCase()
        );
        
        if (topicExists) {
            setError("Detta ämne finns redan!");
            return;
        }
        
        setTopics([...topics, { id: null, text: trimmedTopic, company: parseInt(companyId) }]);
        setNewTopic("");
        setError(null); // Rensa eventuella tidigare fel
    };

    // 🟢 Spara alla ändringar (uppdateringar + nya)
    const handleSave = async () => {
        const newTopics = topics.filter(t => t.id === null);
        const existingTopics = topics.filter(t => t.id !== null);

        try {
            // 🟢 Skicka PUT för uppdateringar
            if (existingTopics.length > 0) {
                const updateResponse = await fetch("/api/casetypes", {
                    method: "PUT",
                    headers: { "Content-Type": "application/json" },
                    body: JSON.stringify(existingTopics),
                });
                
                if (!updateResponse.ok) {
                    throw new Error('Fel vid uppdatering av befintliga ämnen');
                }
                
                const updatedData = await updateResponse.json();
                console.log("✅ Uppdaterade ämnen:", updatedData);
            }

            // 🆕 Skicka POST för nya ämnen och uppdatera UI
            const updatedTopics = [...existingTopics];
            
            for (const topic of newTopics) {
                const requestData = {
                    text: topic.text.trim(),
                    company: parseInt(topic.company)
                };
                
                console.log("🔍 Försöker skicka data:", JSON.stringify(requestData, null, 2));
                
                try {
                    const response = await fetch("/api/casetypes", {
                        method: "POST",
                        headers: { 
                            "Content-Type": "application/json"
                        },
                        body: JSON.stringify(requestData),
                    });

                    if (!response.ok) {
                        const errorText = await response.text();
                        console.error("❌ Server svarade med:", response.status, errorText);
                        throw new Error(`Fel vid tillägg av nytt ämne: ${errorText}`);
                    }

                    const newData = await response.json();
                    console.log("✅ Nytt ämne tillagt:", newData);
                    
                    // Uppdatera topics-listan med det nya ID:t från servern
                    updatedTopics.push({
                        ...topic,
                        id: newData.id
                    });
                } catch (error) {
                    console.error("❌ Fel vid anrop:", error);
                    throw error;
                }
            }

            // Uppdatera hela topics-listan med de nya ID:na
            setTopics(updatedTopics);
            alert("Ämnen uppdaterade!");
        } catch (error) {
            console.error("❌ Fel vid sparande:", error);
            setError(error.message);
        }
    };

    //  Ta bort ett ämne
        const handleDeleteTopic = async (id) => {
            if (!id) {
                setTopics(topics.filter(t => t.id !== null)); // Ta bort lokala, ej sparade topics
                return;
            }

            try {
                const response = await fetch(`/api/casetypes/${id}`, {
                 method: "DELETE",
                });

                if (!response.ok) {
                    throw new Error("Misslyckades med att ta bort ämnet.");
                }

                setTopics(topics.filter(t => t.id !== id));
                alert("Ämnet har tagits bort!");
            } catch (error) {
                console.error("❌ Fel vid borttagning:", error);
                alert("❌ Kunde inte ta bort ämnet.");
            }
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
                            <button onClick={() => handleDeleteTopic(t.id)}>🗑 Ta bort</button>
                        </li>
                    ))}
                </ul>

                <button onClick={handleSave}>Spara i databasen</button>
            </div> 
        </div>
    );
}

export default CaseEditor;
