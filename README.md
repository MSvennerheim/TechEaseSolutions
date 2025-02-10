# Git hjälp
Liten tutorial på hur vi kommer igång och arbetar med git.
## Så här skapar vi ett nytt repo
Antingen kan du göra det via Rider.
Det går att göra det på Github, där kan du välja att lägga till ignore,readme och liscens.
Det går att skippa det steget och då får du göra det via terminalen (bash).
Om repot finns så måste du sedan bjuda in de som ska arbeta på repot i github.
Sedan kan de klona repot via github och sen i terminalen eller rider klona det till din dator.
Se till att ni sitter i den mappen ni vill repot ska connectas med.
Kolla var du är genom commandot ls (du får nu upp en lista på filer och mappar som ligger i mappen) och gå in i den mappen du vill med kommandot cd "directory".

## Så här gör vi en ny branch
git checkout -b fancy_feature (skapar och switchar till en ny branch),
git branch cool_feature (skapar en ny branch)
git checkout cool_feature (byter till branch "cool_feature"),
Glöm inte göra en first commit.
git add Cool.cs,
git commit -m "added cool file",
git push, om du får error så står det vad du ska göra oftast brukar det vara -->
git push --set-upstream origin cool_feature,
Döp era features (branches) till feature/"namn" för att underlätta. Detta är praxis!
Använd git status ofta för att se var du är och hur dina commits ligger till.
git branch -a (kolla vilka branches som finns).

## branches i praktiken
gör en branch för varje task.
Testa den med main genom merge. (Merga inte den till main, merge main med branch).
Sedan kommunicera det med gruppen och när alla är överens så mergea den till main.

## Gitignore
För att göra en ignore manuellt behöver du göra en vanlig fil i repot som heter gitignore.
Sedan gå in i benjamins dokumentation som heter gitignore på bloggen i artikeln "kom igång med git".
Kopiera sedan in det template du behöver för ditt projekt. Vi använder C#.

Svåra vägen är att radera filerna med rm -Rf obj och rm -Rf bin.
Var försiktig med detta kommandot eftersom om du använder det på hela datorn så försvinner dina system filer.

terminal:
touch gitignore (gör fil),
git add gitignore,
git commit ....,
git push ,
rm -Rf obj,
rm -Rf bin (raderar filerna lokalt),
git push.
Kan finnas något extra steg som behövs. Kolla i dokumentation för att vara säker.

## Lägg till på olika sätt
git add . (. = current directory),
git add * (utan gömda filer),
git add "filename",
git add Mapp/. (allting i denna mappen),
git add (hela sökvägen),
GLÖM INTE att använda "git commit -m "meddelande"" sedan "git push"
för att det ska läggas till i repository.

## divergent branches
Glöm inte att läsa dina error messages!

Får du error med divergent branches använd:

git config pull.rebase false

Testa sedan git pull, sedan fixa de ändringar du skulle göra och använd git add och git commit sedan git push. Får du merge problem igen så kan det vara bra att prata med teamet.

## Merge strategy
git pull (kolla om du är up to date),

git checkout "branch" ,

git pull (kolla om du är up to date),

git merge main (för att mergea in main i din branch),

kolla med git status

commit changes and push changes,

git checkout main,

git merge "branch" (för att mergea din branch med main),

# Npgsql
för att ladda ner/ använda en sql databas så behövs ett package som heter Nuget package (Npgsql)
Detta görs inne i Riders terminal för det projektet du vill använda det hos.
"dotnet add package Npgsql --version 9.0.1"

# Import SQL database into DataGrip
to import a database with .sql format into Datagrip, follow these steps:

1. Create a new database with the name of the database in the file (if the database is named fluffykittens, then the database should be named fluffykittens).
2. Right click on the database (so not the elephant, which is the server) and select 'restore with pg_restore/psql'
3. A window will pop up. In the upper left corner select 'psql'.
4. In 'path to executable' search for where the psql.exe program is on your computer. It's usually in C:/Program Files/PostgreSQL/16/bin/psql.exe.
5. In the 'path to dump' field locate the .sql file and select it.
6. Click 'run'.
7. This should load the database. If the database is empty and only show a little box with '...' then that means the database has no schema. To fix this, click the dots and a tiny menu will appear. Make sure 'default schema' is checked. This should solve the problem.
