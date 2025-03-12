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
            <h1>Edit Form</h1>

            {loading && <p>🔄 Laddar data...</p>}
            {error && <p style={{ color: "red" }}>❌ {error}</p>}

            <div id="modifywrapper">
                <div className="addNewTopic">                
                    <input
                        value={newTopic}
                        onChange={(e) => setNewTopic(e.target.value)}
                        placeholder="Skriv ett nytt ämne..."
                        className="topic"
                    />
                    <button onClick={handleAddTopic} className="newTopicButton">Lägg till</button>
                </div>



                    {topics.map((t, index) => (
                        <div key={index} id="caseTypeLayout">
                            <p value={t.caseType} className="caseTypeText"> {t.caseType ?? "N/A"}</p>
                            <button onClick={() => {
                                handleDeleteTopic(t.caseId);
                            }} id="delete-button">🗑Ta bort
                            </button>
                        </div>
                    ))}

            </div>
        </div>
    );
}

export default CaseEditor;
