import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { checkAuth } from "./authService"; 


// funktion för att skydda routes och endast göra dem tillgängliga för autentiserade användare
export default function ProtectedRoute({ children }) {
  const [isAuthenticated, setIsAuthenticated] = useState(false);
  const [isLoading, setIsLoading] = useState(true);
  const navigate = useNavigate();

  useEffect(() => {
  async function verifyAuth() {
    const user = await checkAuth();
    console.log("Authenticated User:", user); // debugging För att se om användaren är autentiserad

    if (!user) {
      navigate("/login"); // redireta till login sidan om ingen användare är autentiserad
    } else {
      setIsAuthenticated(true);
    }
    setIsLoading(false);
  }
  verifyAuth();
}, [navigate]);


  if (isLoading) {
    return <div>Loading...</div>; // vissa att sidan laddas in
  
  }

  return isAuthenticated ? children : null; // rendera children om användaren är autentiserad annars ingenting
}