import { useParams } from "react-router-dom";
import { useEffect, useState } from "react";
import plusSign from "../images/plus-button.png"

function Redigeramedarbetare() {
    const { company } = useParams();
    const [data, setData] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);
    const [isVisible, setIsVisible] = useState(false);
    const [email, setEmail] = useState('');

    useEffect(() => {
        const fetchCoWorkers = async () => {
            try {
                const response = await fetch(`/api/GetCoWorker`);
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
    
    const handleCoworkerSubmit = async () => {
        const response = await fetch('/api/NewCustomerSupport', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({
                email,
            }),
        });
    }

    const newCoworkerToggle = () => {
        setIsVisible(!isVisible);
    };
    
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
                <div className="addEmployee" onClick={newCoworkerToggle}>
                    <img src={plusSign} className="plusImage" />
                </div>


                {/* Pop-up window to handle the creation of a new csrep */}
                
                <div id="newCoworker" style={{display: isVisible ? "block" : "none"}}>
                    <div className='bg-white p-4 rounded shadow-sm' style={{width: '400px'}}>
                        <h2 className='text-center mb-4'>New customer support</h2>
                        <p id="CloseWindow" onClick={newCoworkerToggle}>X</p>

                        {error && <div className='alert alert-danger' role='alert'>{error}</div>}
                        <form onSubmit={handleCoworkerSubmit}>
                            <div className='mb-3'>
                                <label 
                                    htmlFor='email' 
                                    className='form-label'
                                    
                                >Email</label>
                                <input
                                    type='email'
                                    className='form-control'
                                    id='email'

                                    placeholder='Enter email'
                                    required
                                    onChange={(e) => {setEmail}}
                                />
                            </div>

                            <div className='d-grid'>
                                <button type='submit' className='btn btn-primary btn-lg'>Confirm</button>
                            </div>
                        </form>
                    </div>
                </div>
            </div>
        </div>
    );


}

export default Redigeramedarbetare;
