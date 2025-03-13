import {Link, useParams} from "react-router-dom";
import {useEffect, useState} from "react";
import cogwheel from '../images/cogwheel.png'
import form from '../images/contact-form.png'
import customerservice from '../images/customer-support.png'
import email from '../images/email.png'

function Adminsida() {
    
    const { company } = useParams();
    const [data, setData] = useState([]);

    useEffect(() => {
        const getCompanyCaseTypes = async () => {
            try {
                const response = await fetch(`/api/admin/${company}`);
                if (!response.ok) throw new Error("Failed to fetch case types");
                const responseData = await response.json()
                // console.log(responseData)
                setData(responseData)
                // Console.log(data)
            } catch (error) {
                console.error("An error has occured:", error);
            }
        };
    }, []);
    
  return( 
      <>
        <div>
            <h1>Admin site</h1>
            <div className="adminNavBar">
                <div>
                    <Link to={`/redigeramedarbetare`}><img src={cogwheel} className="adminIcons"/></Link>
                    <p className="adminLayoutP">Employees</p>
                </div>
                <div>
                    <Link to={`/caseEditor`}><img src={form} className="adminIcons"/></Link>
                    <p className="adminLayoutP">Edit form</p>
                </div>
                <div>
                    <Link to={`/arbetarsida`}><img src={customerservice} className="adminIcons"/></Link>
                    <p className="adminLayoutP">Issues</p>
                </div>
                <div>
                    <Link to={`/Redigeramall`}><img src={email} className="adminIcons"/></Link>
                    <p className="adminLayoutP">Edit email <br/>template</p>
                </div>
            </div>
        </div>
      </>
  );


}

export default Adminsida;