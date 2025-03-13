import React, { useEffect, useState } from 'react';

const EmailTemplateEditor = () => {
  const [loading, setLoading] = useState(true);
  const [success, setSuccess] = useState(false);
  const [title, setTitle] = useState('');
  const [greeting, setGreeting] = useState('');
  const [mainContent, setMainContent] = useState('');
  const [signature, setSignature] = useState('');

  useEffect(() => {
    fetchTemplate();
  }, []);

  // här hämtar e-postmallen från api
  const fetchTemplate = async () => {
    try {
      const response = await fetch('/api/get-email-template');
      const data = await response.json();
      parseTemplate(data); // Extraherar och sätter in innehållet i formuläret
    } catch (error) {
      console.error('Error fetching the email template:', error);
    } finally {
      setLoading(false); 
    }
  };
    // extraherar innehållet från HTML-mallen och sätter tillstånd
  const parseTemplate = (data) => {
    setTitle(data.title)
    setGreeting(data.greeting)
    setMainContent(data.content)
    setSignature(data.signature)
    
    /*
    const tempDiv = document.createElement('div');
    tempDiv.innerHTML = template; // skapar ett tillfälligt DOM-element för att extrahera innehåll

    // hämtar titeln från en rubrik alltså (h1, h2 eller h3)
    setTitle(tempDiv.querySelector('h1, h2, h3')?.textContent || '');

    //hämtar alla <p>-element 
    const paragraphs = tempDiv.querySelectorAll('p'); //första paragrafen är en hälsning
    setGreeting(paragraphs[0]?.textContent || '');
    setMainContent(Array.from(paragraphs).slice(1, -1).map(p => p.textContent).join('\n\n')); // detta är mitten av texten
    setSignature(paragraphs[paragraphs.length - 1]?.textContent || ''); // och detta är signaturen så man kan skriva sitt företags namn
    */
  };


  // Här har vi HTML-mallen från variablerna, har även lagt till så att länken till chatten måste följa med och redan är hrefed.
  // men senare skulle man kunna ta bort själva texten och bara ha länken till ordet.
  const buildTemplate = () => `
    <html>
    <body>
      <h2>${title}</h2>
      <p>${greeting}</p>
      ${mainContent.split('\n\n').map(para => `<p>${para}</p>`).join('\n')}
      <p>Du kan följa ditt ärende <a href='http://localhost:5173/guestlogin/{chatid}?email={encodedEmail}'>HÄR</a>.</p>
      <p>${signature}</p> 
      <p>Svara inte på detta mejlet, det är autogenererat</p>
    </body>
    </html>`;

  //sparar den uppfaterade e-postmallen via api
  const handleSave = async () => {
    const templateToSave = buildTemplate();
    // detta måste finnas i mallen
    /*
    const requiredElements = ['{chatid}', '{description}', '{encodedEmail}'];
    const missingElements = requiredElements.filter(el => !templateToSave.includes(el));
   
    // om inte variablerna finns med så ska en varning dyk upp
    if (missingElements.length > 0) {
      alert(`Följande element saknas och måste vara med: ${missingElements.join(', ')}`);
      return;
      */
    
    }

    try {
      setLoading(true);
      const response = await fetch('/api/post-email-template', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({  }),
      });

      if (response.ok) {
        setSuccess(true);
        setTimeout(() => setSuccess(false), 3000); // här ska den visa "sparat" i  sekunder
      } else {
        alert('Misslyckades att uppdatera mallen');
      }
    } catch (error) {
      console.error('Error updating email template:', error);
      alert('Ett fel uppstod vid uppdatering av mallen');
    } finally {
      setLoading(false);
    }
  };

  // här har jag skapat en förhandsgranskning av den genererade mallen så att man kan see sina ändringar innan man sparar dem
  const previewTemplate = buildTemplate();

  return (
    <div className="editor-container">
      <h1 className="editor-title">Redigera ärende mail</h1>
      {loading ? (
        <div className="loading-spinner">
          <div className="spinner"></div>
          <p>Laddar...</p>
        </div>
      ) : (
        <>
          <form className="editor-form">
            <InputField label="Rubrik" value={title} onChange={setTitle} />
            <InputField label="Hälsning" value={greeting} onChange={setGreeting} />
            <TextAreaField label="Huvudtext" value={mainContent} onChange={setMainContent} />
            <InputField label="Signatur" value={signature} onChange={setSignature} />
          </form>
          <PreviewSection template={previewTemplate} />
          <ActionButtons onSave={handleSave} success={success} loading={loading} />
        </>
      )}
    </div>
  );
};

const InputField = ({ label, value, onChange }) => (
  <div className="input-field">
    <label>{label}</label>
    <input type="text" value={value} onChange={(e) => onChange(e.target.value)} />
  </div>
);

const TextAreaField = ({ label, value, onChange }) => (
  <div className="input-field">
    <label>{label}</label>
    <textarea value={value} onChange={(e) => onChange(e.target.value)} rows={6} />
  </div>
);

const PreviewSection = ({ template }) => (
  <div className="preview-section">
    <h3>Förhandsgranskning</h3>
    <div className="preview-content" dangerouslySetInnerHTML={{ __html: template }} />
  </div>
);

const ActionButtons = ({ onSave, success, loading }) => (
  <div className="action-buttons">
    <button onClick={onSave} disabled={loading}>Spara</button>
    {success && <div className="success-message">Mallen har sparats!</div>}
  </div>
);

export default EmailTemplateEditor;
