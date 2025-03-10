export default function assignTicket() {
    return async () => {
        (chatid, navigate) => {
            
            try {
            await fetch(`api/assignticket`, {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                },
                body: JSON.stringify({ChatId: chatid}),
            })
                if (!response.ok) {
                    throw new Error("Failed to assign ticket");
                }
                navigate(`/Chat/${chatid}`)
            } catch (error){
                console.log("couldn't assign ticket: ", error)
            }
        }
    }
}