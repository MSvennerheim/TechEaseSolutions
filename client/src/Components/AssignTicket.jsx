const assignTicket = async (chatid, navigate) => {
        console.log("got here, chatid: " + chatid)
        
        const response = await fetch(`api/assignticket`, {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
            },
            body: JSON.stringify({ChatId: chatid}),
        })
    if (response.ok){
        console.log("ticket assigned")
        navigate(`/Chat/${chatid}`)
    } else {
        console.log("ticket could not be assigned")
    }
}

export default assignTicket