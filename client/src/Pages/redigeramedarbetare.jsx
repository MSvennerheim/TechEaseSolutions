import { useParams } from "react-router-dom";
import {use, useEffect, useState} from "react";
import plusSign from "../images/plus-button.png"

function Redigeramedarbetare() {
    const { company } = useParams();
    const [data, setData] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);
    const [isVisible, setIsVisible] = useState(false);
    const [Email, setEmail] = useState('');
    const [updateTicker, setUpdateTicker] = useState(0)

    const fetchCoWorkers = async () => {
        try {
            const response = await fetch(`/api/GetCoWorker`);
            if (!response.ok) throw new Error("Failed to fetch Employees");

            const responseData = await response.json();
            const data = responseData;
            console.log(data)
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


    useEffect(() => {
        fetchCoWorkers();
    }, [updateTicker]);

    const updateSite = () => {
        setTimeout(() => {
            setUpdateTicker(updateTicker + 1)
        }, 200);
    }

    const handleCoworkerSubmit = (e) => {
        e.preventDefault();
        console.log(Email)
        fetch('/api/NewCustomerSupport', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({
                Email
            }),
        });
        newCoworkerToggle()
        updateSite()
    };
    
    const handleCoworkerDelete = async (email) =>{
        //e.preventDefault();
        await fetch('/api/deleteCsRep', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({
                Email: email,
            }),
        })
        updateSite()
    };

    
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
                                <p value={item.id}><strong>Id:</strong> {item.id ?? "N/A"}</p>
                                <p value={item.email}><strong>Email:</strong> {item.email ?? "N/A"}</p>
                                <p value={item.companyName}><strong>Company:</strong> {item.companyName ?? "N/A"}</p>
                                <p value={item.csRep}><strong>Customer Service User:</strong> {item.csRep ? "Yes" : "No"}</p>
                                <p value={item.isAdmin}><strong>Admin:</strong> {item.isAdmin ? "Yes" : "No"}</p>
                                <button onClick={() => handleCoworkerDelete(item.email)}>Remove coworker</button>
                            </div>
                        </div>
                    ))
                ) : (
                    <p>No employees found.</p>
                )}
                
                <div className="addEmployee" onClick={newCoworkerToggle}>
                    <img src={plusSign} className="plusImage"/>
                </div>

                {/* Confirmation Popup */}
                {isVisible && (
                    <div id="confirmation-popup" className="popup-overlay"  onClick={newCoworkerToggle}>
                        <div id="newCoworker" className="popup-content" onClick={(e) => e.stopPropagation()}>
                            <h2 className='text-center mb-4'>New customer support</h2>
                            <p id="CloseWindow" onClick={newCoworkerToggle}>X</p>

                            {error && <div className='alert alert-danger' role='alert'>{error}</div>}

                            <form onSubmit={handleCoworkerSubmit}>
                                <div className='mb-3'>
                                    <label htmlFor='email' className='form-label'>Email</label>
                                    <input
                                        type='Email'
                                        className='form-control'
                                        id='Email'
                                        placeholder='Enter email'
                                        value={Email}
                                        required
                                        onChange={(e) => setEmail(e.target.value)}
                                    />
                                </div>

                                <div className='d-grid'>
                                    <button
                                        type="submit"
                                        className="btn btn-primary btn-lg"
                                        onClick={() => {
                                            fetchCoWorkers();
                                        }}>Confirm</button>
                                </div>
                            </form>
                        </div>
                    </div>
                )}
            </div>
        </div>

    );
}

export default Redigeramedarbetare;
