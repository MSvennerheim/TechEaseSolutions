import {Link, useParams} from "react-router-dom";
import {useEffect, useState} from "react";

function Adminsida() {
    
    const { companyName } = useParams();
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
        //function som hämtar company namn/*
    }, []);
    
  return( 
      <>
        <div>
            <h1>Admin site</h1>
            <Link to={`/redigeramedarbetare`}><button>edit coworkers</button></Link>
            <button>Redigera Formulär</button>
            <button>Arbetarsida</button>
        </div>
    </>
  );
  

}

export default Adminsida;