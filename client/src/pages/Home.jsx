import React, { useState, useRef, useEffect } from 'react';

// ... rest of your existing code remains the same

const Dropdown = () => {
  const [selectedOption, setSelectedOption] = useState('');
  const describeRef = useRef(null);

  const handleChange = (event) => {
    setSelectedOption(event.target.value);
  };

  useEffect(() => {
    const textarea = describeRef.current;
    const autoResize = () => {
      textarea.style.height = 'auto';
      textarea.style.height = textarea.scrollHeight + 'px';
    };

    textarea.addEventListener('input', autoResize);
    return () => textarea.removeEventListener('input', autoResize);
  }, []);

  return (
    <>
    <div id="formwrap">
      {/* ... other code remains the same */}
      <div id="wrapmail">
        {/* ... other code remains the same */}
        <div>
        <form>
        <textarea 
          ref={describeRef}
          name="issue" 
          id="describe" 
          placeholder="Describe your issue..."
        ></textarea>
        </form>
        </div>
        <button id="skicka_ärende">Skicka ärende</button>
      </div>  
    </div>
    </>
  );
};