-- Droppa tabellen om den finns (var försiktig med detta i produktion!)
DROP TABLE IF EXISTS casetypes;

-- Skapa tabellen med rätt struktur
CREATE TABLE casetypes (
    id SERIAL PRIMARY KEY,  -- Använd SERIAL för auto-increment
    text VARCHAR(255) NOT NULL,
    company INTEGER NOT NULL
);

-- Skapa index för company för bättre prestanda
CREATE INDEX idx_casetypes_company ON casetypes(company); 