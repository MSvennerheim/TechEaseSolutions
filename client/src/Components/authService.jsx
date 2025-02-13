export async function checkAuth() {
  try {
    const response = await fetch("/api/check-session");
    console.log("Check Auth Response:", response); // Log the response
    if (response.ok) {
      const user = await response.json();
      console.log("User:", user); // Log the user data
      return user;
    }
  } catch (error) {
    console.error("Auth check error:", error);
  }
  return null;
}

export const logout = async () => {
  try {
    await fetch('/api/logout', { method: 'POST' });
  } catch (error) {
    console.error('Logout error:', error);
  }
};