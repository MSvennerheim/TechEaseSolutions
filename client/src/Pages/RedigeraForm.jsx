import { useState, useEffect } from "react";

const RedigeraForm = () => {
    const [fields, setFields] = useState([]);
    const [newField, setNewField] = useState("");

    // Hämta formulärfält från backend
    useEffect(() => {
        fetch("http://localhost:5000/api/formular")
            .then((res) => res.json())
            .then((data) => setFields(data));
    }, []);

    // Uppdatera ett formulärfält
    const handleUpdate = (id, text) => {
        fetch(`http://localhost:5000/api/formular/${id}`, {
            method: "PUT",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ text }),
        }).then(() => {
            setFields(fields.map((field) => (field.id === id ? { ...field, text } : field)));
        });
    };

    // Lägg till nytt fält
    const handleAdd = () => {
        if (!newField) return;
        fetch("http://localhost:5000/api/formular", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ text: newField }),
        }).then((res) => res.json())
          .then((data) => setFields([...fields, data]));
        setNewField("");
    };

    // Radera ett fält
    const handleDelete = (id) => {
        fetch(`http://localhost:5000/api/formular/${id}`, { method: "DELETE" }).then(() => {
            setFields(fields.filter((field) => field.id !== id));
        });
    };

    return (
        <div className="form-container">
            <h2>Redigera Formulär</h2>
            <ul>
                {fields.map((field) => (
                    <li key={field.id}>
                        <input
                            type="text"
                            value={field.text}
                            onChange={(e) => handleUpdate(field.id, e.target.value)}
                        />
                        <button onClick={() => handleDelete(field.id)}>❌</button>
                    </li>
                ))}
            </ul>
            <input
                type="text"
                value={newField}
                onChange={(e) => setNewField(e.target.value)}
                placeholder="Lägg till nytt fält..."
            />
            <button onClick={handleAdd}>➕ Lägg till</button>
        </div>
    );
};

export default RedigeraForm;
