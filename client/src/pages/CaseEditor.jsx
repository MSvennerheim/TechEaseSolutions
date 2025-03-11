import React, { useState, useEffect } from "react";
import {data, useParams} from "react-router-dom";

function CaseEditor() {
    const { companyId } = useParams(); // Hämta companyId från URL
    const [topics, setTopics] = useState([]);
    const [newTopic, setNewTopic] = useState("");
    const [error, setError] = useState(null);
    const [loading, setLoading] = useState(false);
    

    // 🟢 Hämta casetypes från backend
    useEffect(() => {
        setLoading(true);
        fetch(`api/casetypes`) 
            .then(response => response.json())
            .then(data => setTopics(data))
            .catch(error => {
                console.error("❌ Fel vid hämtning av casetypes:", error);
                setError("Kunde inte hämta casetypes. Kontrollera backend.");
            })
            .finally(() => setLoading(false));
    }, []);

    // 🟢 Lägg till ett nytt ämne i UI (ej i databasen än)
    const handleAddTopic = async () => {
        console.log(newTopic);
        if (newTopic != "") {                
            const response = await fetch("/api/NewCaseType", {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify({
                caseType: newTopic
            }),
                })
            ;}
    };

    //  Ta bort ett ämne
        const handleDeleteTopic = async (caseId) => {
            
            const response = await fetch("/api/deleteCaseType", { // No ID in the URL
                method: "DELETE",
                headers: {
                    "Content-Type": "application/json",
                },
                body: JSON.stringify({ caseId }), // Send ID as JSON
            });
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
                    className="topic"
                />
                <button onClick={handleAddTopic}>Lägg till</button>

                <div className="caseTypeLayout">
                    {topics.map((t, index) => (
                        <div key={index}>
                            <p value={t.caseType}><strong>CaseType</strong> {t.caseType ?? "N/A"}</p>
                            <p value={t.caseId}>ID: {t.caseId}</p>
                            <button onClick={() => {
                                handleDeleteTopic(t.caseId);
                            }}>🗑Ta bort
                            </button>
                        </div>
                    ))}
                </div>

            </div>
        </div>
    );
}

export default CaseEditor;
