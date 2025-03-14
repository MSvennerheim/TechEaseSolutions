# TeachEaseSolutions Anvandarguide

## Prerequisites
1. Skapa databasen från en dump.
2. Skapa en `.env`-fil för databasanslutning.
3. Skapa en admin i `users`-tabellen för ett nytt företag.
   - Markera även `csrep` som `true`, då detta krävs för att komma åt arbetarsidan.
   - Det finns ännu inget verktyg för att lägga till nya admins som CRM-owner.

---

## Sidor

### Logga in (/ eller /login)
- Inloggning för företagsadmins och kundtjänstmedarbetare.
- Redirectar dig till adminsidan om du är admin, eller arbetarsidan om du är kundtjänstmedarbetare.

### Adminsida (/admin)
- Portal med länkar till adminfunktioner:
  1. Redigera medarbetare
  2. Case Editor
  3. Arbetarsida
  4. Redigera mall

### Redigera medarbetare (/redigeramedarbetare)
- Hämtar medarbetare för ditt företag och visar dem i en lista.
- Medarbetare kan läggas till och tas bort.
- Ny medarbetare får ett mejl med en länk för att byta lösenord (`/reset-password/{resetToken}`).
  - De kan inte logga in förrän lösenordet är ändrat.

### Case Editor (/caseEditor)
- Låter admins redigera ärendetyper som kunder kan välja vid inskickning av ärenden.
- Ärendetyper kan läggas till och tas bort.
- Borttagna ärendetyper inaktiveras i databasen men kan återaktiveras genom att lägga till en ny typ med samma namn.

### Redigera mall (/redigeramall)
- Låter admins redigera det första automatiska utskicket vid ärendeskapande.
- Förhandsgranskning av utskicket visas.
- Vissa kravfunktioner (t.ex. ärendenummer, kundens chattlänk, kundens meddelande) är hårdkodade och kan inte modifieras.

### Arbetarsida (/arbetarsida)
- Visar en lista över otilldelade ärenden där senaste svaret inte är från en kundtjänstmedarbetare.
- Filtreringsalternativ:
  - Samtliga ärenden.
  - Obesvarade med tilldelad kundtjänstmedarbetare.
  - Ärenden där senaste svaret är från kundtjänst.
- Låter kundtjänstmedarbetare:
  - Öppna chattar utan att tilldela ärendet.
  - Öppna chatt och tilldela sig själv ärendet.

### Chatt (/chat/{chatid})
- Visar en specifik chatt.
- Endast tillgänglig för:
  - Kundtjänstmedarbetare hos aktuellt företag.
  - Kunden som har skickat in ärendet.
- Visar hela chathistoriken (avsändare, meddelande, timestamp).
- Kundtjänstmedarbetare kan:
  - Skicka meddelanden.
  - Använda "Send and take next open ticket" för att hoppa till nästa otilldelade ärende.
- Kunden kan endast skicka meddelanden.

### Kontakta oss (/kontaktaoss/{företagsnamn})
- Formulär för att skicka in ett ärende till ett företag.
- Kräver:
  - Ärendetyp.
  - E-postadress.
  - Meddelande.
- Efter inskick skickas kunden till en bekräftelsesida och får ett mejl med en länk till sin chatt (`/guestlogin/{chatid}?email={customerEmail}`).
  - Länken loggar in kunden och leder dem till `/chat/{chatid}`.

### Password Reset (/reset-password/{resetToken})
- Tillåter användare att byta lösenord.
- Efter lösenordsbyte omdirigeras användaren till `/login`.
