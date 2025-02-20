import { useState, useEffect } from "react";

function RedigeraMedarbetare() {
    const [data, setData] = useState([]);

    useEffect(() => {
        const fetchCoWorkers = async () => {
            try {
                console.log("fetchCoWorkers");
                const response = await fetch(`/api/redigeramedarbetare`);
                const responseData = await response.json();
                setData(responseData);
                console.log(responseData);
            } catch (error) {
                console.error("An error occurred:", error);
            }
        };
        fetchCoWorkers();
    }, []);

    return (
        <>
            <h1>Edit Coworkers</h1>
            <ul>
                {data.map((coworker, index) => (
                    <li key={index}>{coworker.name}</li>
                ))}
            </ul>
        </>
    );
}

export default RedigeraMedarbetare;