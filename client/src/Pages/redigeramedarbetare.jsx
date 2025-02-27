import { useParams } from "react-router-dom";
import { useEffect, useState } from "react";
import plusSign from "../images/plus-button.png"

function Redigeramedarbetare() {
    const { companyName } = useParams();
    const [data, setData] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);

    useEffect(() => {
        const fetchCoWorkers = async () => {
            try {
                const response = await fetch(`/api/editCoWorker`);
                if (!response.ok) throw new Error("Failed to fetch Employees");

                const responseData = await response.json();
                
                if (responseData && typeof responseData === "object" && !Array.isArray(responseData)) {
                    setData([responseData]);
                } else if (Array.isArray(responseData)) {
                    setData(responseData);
                } else {
                    console.warn("Unexpected response format:", responseData);
                    setData([]);
                }
            } catch (error) {
                console.error("An error has occurred:", error);
                setError(error.message);
            } finally {
                setLoading(false);
            }
        };

        fetchCoWorkers();
    }, []);



    if (loading) return <p>Loading employees...</p>;
    if (error) return <p>Error: {error}</p>;

    return (
        <div>
            <h1>Edit Employees</h1>
           
            <div className="EmployeeLayout">
                {data.length > 0 ? (
                    data.map((item, index) => (
                        <div key={index}>
                            <div className="Employee">
                                <p><strong>Id:</strong> {item.id ?? "N/A"}</p>
                                <p><strong>Email:</strong> {item.email ?? "N/A"}</p>
                                <p><strong>Company:</strong> {item.companyName ?? "N/A"}</p>
                                <p><strong>Customer Service User:</strong> {item.isCustomerServiceUser ? "Yes" : "No"}
                                </p>
                                <p><strong>Admin:</strong> {item.isAdmin ? "Yes" : "No"}</p>
                            </div>
                        </div>
                    ))
                ) : (
                    <p>No employees found.</p>
                )}
                <div className="addEmployee">
                    <img src={plusSign} className="plusImage"/>
                </div>
            </div>
        </div>
    );


}

export default Redigeramedarbetare;
