import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { checkAuth } from "./authService";


// funktion för att skydda routes och endast göra dem tillgängliga för autentiserade användare.
// och skyddar så att ENDAST administratörer kan komma åt Administrations sidan.
export default function ProtectedRoute({ children, adminOnly = false }) {
  const [isAuthenticated, setIsAuthenticated] = useState(false);
  const [isAdmin, setIsAdmin] = useState(false);
  const [isLoading, setIsLoading] = useState(true);
  const navigate = useNavigate();

  useEffect(() => {
    async function verifyAuth() {
      const user = await checkAuth();
      console.log("Authenticated User:", user); // debugging

      if (!user) {
        navigate("/login/"); // skicka till login om ej inloggad
      } else {
        setIsAuthenticated(true);
        setIsAdmin(user.isAdmin);

        if (adminOnly && !user.isAdmin) {
          navigate(`/arbetarsida/`); // skicka arbetare till sin sida om de försöker gå till admin
        }
      }
      setIsLoading(false);
    }
    verifyAuth();
  }, [navigate]);

  if (isLoading) {
    return <div>Loading...</div>;
  }

  return isAuthenticated ? children : null;
}
